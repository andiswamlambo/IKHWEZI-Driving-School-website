using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;

namespace WebApplicationM3
{
    public class ChatApi : IHttpHandler
    {
        // =====================================================
        // DTOs - property names match the Gemini API JSON
        // exactly (JavaScriptSerializer uses them as-is)
        // =====================================================

        private class GeminiPart { public string text { get; set; } }
        private class GeminiContent
        {
            public string role { get; set; }
            public List<GeminiPart> parts { get; set; }
        }
        private class GeminiSystem { public List<GeminiPart> parts { get; set; } }
        private class GeminiGenConfig
        {
            public double temperature { get; set; }
            public int maxOutputTokens { get; set; }
        }
        private class GeminiRequest
        {
            public GeminiSystem system_instruction { get; set; }
            public List<GeminiContent> contents { get; set; }
            public GeminiGenConfig generationConfig { get; set; }
        }
        private class GeminiResponse { public List<GeminiCandidate> candidates { get; set; } }
        private class GeminiCandidate { public GeminiContent content { get; set; } }

        private class ChatTurn { public string role { get; set; } public string text { get; set; } }
        private class ChatRequest
        {
            public string message { get; set; }
            public List<ChatTurn> history { get; set; }
        }


        // =====================================================
        // MAIN ENTRY
        // =====================================================

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);

            ChatRequest req = ReadRequest(context);

            string question = (req.message ?? "").Trim();
            if (question.Length > 500)
            {
                question = question.Substring(0, 500);
            }

            if (question.Length == 0)
            {
                context.Response.Write(
                    "{\"ok\":false,\"error\":\"Empty question\"}");
                return;
            }

            string reply;
            string source;

            try
            {
                // AI answer, grounded on live database prices
                reply = AskAi(question, req.history);
                source = "ai";
            }
            catch (Exception ex)
            {
                // AI unavailable (no key, quota, network...)
                // -> fall back to the rule-based answers so
                //    the chatbot NEVER goes silent
                LogChatError(question, ex);

                reply = RuleBasedReply(question);
                source = "rules";
            }

            context.Response.Write(
                "{\"ok\":true," +
                "\"source\":\"" + J(source) + "\"," +
                "\"reply\":\"" + J(reply) + "\"}");
        }


        // =====================================================
        // READ REQUEST (POST JSON body, or GET ?q= for quick
        // browser testing)
        // =====================================================

        private static ChatRequest ReadRequest(HttpContext context)
        {
            string qs = context.Request.QueryString["q"];

            if (!string.IsNullOrWhiteSpace(qs))
            {
                return new ChatRequest
                {
                    message = qs,
                    history = new List<ChatTurn>()
                };
            }

            if (context.Request.HttpMethod == "POST")
            {
                try
                {
                    using (var reader = new StreamReader(
                        context.Request.InputStream))
                    {
                        string body = reader.ReadToEnd();

                        var parsed = new JavaScriptSerializer()
                            .Deserialize<ChatRequest>(body);

                        if (parsed != null)
                        {
                            return parsed;
                        }
                    }
                }
                catch
                {
                    // Fall through to empty request
                }
            }

            return new ChatRequest
            {
                message = "",
                history = new List<ChatTurn>()
            };
        }


        // =====================================================
        // ASK GEMINI (HTTPS port 443 - campus-network safe)
        // =====================================================

        private static string AskAi(
            string question,
            List<ChatTurn> history)
        {
            string apiKey =
                ConfigurationManager.AppSettings["GeminiApiKey"];

            string model =
                ConfigurationManager.AppSettings["GeminiModel"];

            if (string.IsNullOrWhiteSpace(model))
            {
                model = "gemini-2.0-flash";
            }

            if (string.IsNullOrWhiteSpace(apiKey) ||
                apiKey.Contains("PASTE-YOUR"))
            {
                throw new Exception(
                    "GeminiApiKey is missing from Web.config.");
            }

            var request = new GeminiRequest
            {
                system_instruction = new GeminiSystem
                {
                    parts = new List<GeminiPart>
                    {
                        new GeminiPart { text = BuildSystemPrompt() }
                    }
                },
                contents = BuildContents(question, history),
                generationConfig = new GeminiGenConfig
                {
                    temperature = 0.4,
                    maxOutputTokens = 300
                }
            };

            string body =
                new JavaScriptSerializer().Serialize(request);

            string url =
                "https://generativelanguage.googleapis.com/v1beta/models/" +
                model +
                ":generateContent?key=" + apiKey;

            string responseJson = PostJson(url, body);

            var parsed = new JavaScriptSerializer()
                .Deserialize<GeminiResponse>(responseJson);

            string text = "";

            if (parsed != null &&
                parsed.candidates != null &&
                parsed.candidates.Count > 0 &&
                parsed.candidates[0].content != null &&
                parsed.candidates[0].content.parts != null &&
                parsed.candidates[0].content.parts.Count > 0)
            {
                text = parsed.candidates[0].content.parts[0].text ?? "";
            }

            text = CleanReply(text);

            if (text.Length == 0)
            {
                throw new Exception("AI returned an empty reply.");
            }

            return text;
        }


        // =====================================================
        // BUILD CONVERSATION (last few turns + new question)
        // =====================================================

        private static List<GeminiContent> BuildContents(
            string question,
            List<ChatTurn> history)
        {
            var contents = new List<GeminiContent>();

            if (history != null)
            {
                int count = 0;

                foreach (var turn in history)
                {
                    if (turn == null ||
                        string.IsNullOrWhiteSpace(turn.text))
                    {
                        continue;
                    }

                    if (count >= 8)
                    {
                        break;
                    }

                    // Widget roles: "user" / "bot" -> API: "user"/"model"
                    string role =
                        turn.role == "bot" ? "model" : "user";

                    string text = turn.text.Trim();
                    if (text.Length > 400)
                    {
                        text = text.Substring(0, 400);
                    }

                    contents.Add(new GeminiContent
                    {
                        role = role,
                        parts = new List<GeminiPart>
                        {
                            new GeminiPart { text = text }
                        }
                    });

                    count++;
                }
            }

            contents.Add(new GeminiContent
            {
                role = "user",
                parts = new List<GeminiPart>
                {
                    new GeminiPart { text = question }
                }
            });

            return contents;
        }


        // =====================================================
        // SYSTEM PROMPT - the AI's instructions, GROUNDED with
        // live prices pulled from the Packages table so it can
        // never invent prices or mention Code 14
        // =====================================================

        private static string BuildSystemPrompt()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("You are the friendly virtual assistant for ");
            sb.Append("IKHWEZI Driving School, based at the UKZN ");
            sb.Append("Concourse in Durban, South Africa.\n\n");

            sb.Append("STRICT RULES:\n");
            sb.Append("1. Only use the verified facts and prices below. ");
            sb.Append("Never invent prices, packages or policies.\n");
            sb.Append("2. We offer ONLY Code 08, Code 10, Automatic and ");
            sb.Append("the K53 Learners theory class. If asked about ");
            sb.Append("Code 14 or heavy trucks, politely say we do not ");
            sb.Append("offer that yet.\n");
            sb.Append("3. Keep replies short: 2 to 4 sentences, or a ");
            sb.Append("short bullet list.\n");
            sb.Append("4. Format replies as simple HTML only. Allowed ");
            sb.Append("tags: <b>, <i>, <br/>, <a>. Use &bull; for ");
            sb.Append("bullets. Never use markdown: no **, no ###, no ");
            sb.Append("dash bullets, no backticks.\n");
            sb.Append("5. Stay on topic: driving lessons, bookings, ");
            sb.Append("payments and our school. If asked something ");
            sb.Append("unrelated, politely steer back.\n\n");

            sb.Append("VERIFIED FACTS:\n");
            sb.Append("- Address: Student Union, UKZN Concourse, 269 ");
            sb.Append("Mazisi Kunene Rd, Glenwood, Durban, 4001.\n");
            sb.Append("- Phone/WhatsApp: +27 31 123 4567 or ");
            sb.Append("+27 81 234 5678. Email: info@khwezidriving.co.za.\n");
            sb.Append("- Hours: Mon-Fri 08:00-17:00, Sat 08:00-13:00, ");
            sb.Append("Sun closed.\n");
            sb.Append("- Payment: 50% deposit secures a booking; balance ");
            sb.Append("spread over 2 months; payments at reception for ");
            sb.Append("now (online payments coming soon).\n");
            sb.Append("- Full courses include a FREE learners licence. ");
            sb.Append("Vehicle hire for test day: car R700, truck R1000.\n");
            sb.Append("- Booking steps: 1) book on the website, ");
            sb.Append("2) confirmation email arrives, 3) pay the 50% ");
            sb.Append("deposit at reception, 4) reception confirms. ");
            sb.Append("Bring your learner's licence and ID on lesson day.\n");
            sb.Append("- Practical booking link: " +
                Url("~/BookSession") + "\n");
            sb.Append("- Learners class booking link: " +
                Url("~/LearnersClass") + "\n");
            sb.Append("- All packages page: " + Url("~/Offers") + "\n");
            sb.Append("When suggesting a booking, include the matching ");
            sb.Append("link as <a href='URL'>text</a>.\n\n");

            sb.Append("PACKAGES (live from our database - the ONLY ");
            sb.Append("prices you may quote):\n");

            try
            {
                string connStr =
                    ConfigurationManager
                    .ConnectionStrings["KhweziConnection"]
                    .ConnectionString;

                using (var conn = new SqlConnection(connStr))
                using (var cmd = new SqlCommand(
                    "SELECT PackageName, PackageType, TotalLessons, " +
                    "Amount, PackageDescription FROM Packages " +
                    "ORDER BY PackageType, TotalLessons", conn))
                {
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sb.Append("- " + reader["PackageName"] + " (");
                            sb.Append(reader["PackageType"] + "): ");
                            sb.Append(Convert.ToInt32(
                                reader["TotalLessons"]) + " lessons, R");
                            sb.Append(Convert.ToDecimal(reader["Amount"])
                                .ToString("0.00",
                                    CultureInfo.InvariantCulture));
                            sb.Append(". " +
                                reader["PackageDescription"] + "\n");
                        }
                    }
                }
            }
            catch
            {
                sb.Append("(Prices temporarily unavailable - direct the ");
                sb.Append("student to the Offers page.)\n");
            }

            return sb.ToString();
        }


        // =====================================================
        // HTTPS POST (same proven pattern as the Brevo email)
        // =====================================================

        private static string PostJson(string url, string json)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Timeout = 20000;
            request.ReadWriteTimeout = 20000;

            byte[] bytes = Encoding.UTF8.GetBytes(json);
            request.ContentLength = bytes.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }

            try
            {
                using (var response = request.GetResponse())
                using (var reader = new StreamReader(
                    response.GetResponseStream(), Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (WebException wx)
            {
                // Read the API's real error message so the log
                // tells us exactly what is wrong (bad key, quota...)
                string details = "";

                if (wx.Response != null)
                {
                    try
                    {
                        using (var r = new StreamReader(
                            wx.Response.GetResponseStream()))
                        {
                            details = r.ReadToEnd();
                        }
                    }
                    catch
                    {
                        // Best effort only
                    }
                }

                throw new Exception(
                    "AI API error: " + wx.Message +
                    (details.Length > 0 ? " | " + details : ""));
            }
        }


        // =====================================================
        // CLEAN AI OUTPUT (safety + fix stray markdown)
        // =====================================================

        private static string CleanReply(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return "";
            }

            html = Regex.Replace(html, "<script.*?</script>", "",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            html = Regex.Replace(html,
                "on\\w+\\s*=\\s*\"[^\"]*\"", "",
                RegexOptions.IgnoreCase);

            html = html.Replace("javascript:", "");

            // In case the model slips into markdown anyway
            html = html.Replace("```", "");
            html = Regex.Replace(html, "\\*\\*(.+?)\\*\\*", "<b>$1</b>");
            html = Regex.Replace(html, "(?m)^#{1,3}\\s*(.+)$", "<b>$1</b>");
            html = Regex.Replace(html, "(?m)^\\s*[-*]\\s+", "&bull; ");

            // Newlines must become <br/> for innerHTML rendering
            html = html.Replace("\n", "<br/>");

            return html.Trim();
        }


        // =====================================================
        // FALLBACK ANSWERS (used whenever the AI is unavailable)
        // =====================================================

        private static string RuleBasedReply(string q)
        {
            string ql = (q ?? "").ToLowerInvariant();

            string uBook = Url("~/BookSession");
            string uLearners = Url("~/LearnersClass");

            if (Regex.IsMatch(ql, "code ?14|heavy (motor|truck|vehicle)"))
            {
                return "We currently train for <b>Code 08</b> (light motor " +
                    "vehicles), <b>Code 10</b> (medium trucks) and " +
                    "<b>Automatic</b>, plus a <b>K53 learners theory " +
                    "class</b>. We do not offer Code 14 at the moment.";
            }

            if (Regex.IsMatch(ql, "book|booking|reserve|appointment|register|enrol"))
            {
                return "Booking is easy:<br/>&bull; <a href='" + uBook +
                    "'>Book a practical session</a> or a <a href='" +
                    uLearners + "'>learners class</a><br/>&bull; You'll " +
                    "get a confirmation email<br/>&bull; Pay the 50% " +
                    "deposit at reception<br/>&bull; Reception confirms " +
                    "your booking";
            }

            if (Regex.IsMatch(ql, "deposit|payment|instal|pay"))
            {
                return "We keep it affordable:<br/>&bull; <b>50% deposit</b> " +
                    "secures your booking<br/>&bull; Balance spread over " +
                    "<b>2 months</b><br/>&bull; Full courses include a " +
                    "<b>free learners licence</b><br/><br/>Payments are " +
                    "made at reception for now.";
            }

            if (Regex.IsMatch(ql, "where|location|address|located|map|direction"))
            {
                return "You'll find us at:<br/><b>IKHWEZI Driving School</b>" +
                    "<br/>Student Union, UKZN Concourse<br/>269 Mazisi " +
                    "Kunene Rd, Glenwood, Durban, 4001<br/><br/><a " +
                    "href='https://maps.app.goo.gl/aMAs4L6br3kr5W5bA' " +
                    "target='_blank' rel='noopener'>Open in Google Maps</a>.";
            }

            if (Regex.IsMatch(ql, "hour|opening|open|close|what time"))
            {
                return "Operating hours:<br/><b>Mon &ndash; Fri:</b> " +
                    "8:00 &ndash; 17:00<br/><b>Saturday:</b> 8:00 &ndash; " +
                    "13:00<br/><b>Sunday:</b> Closed";
            }

            if (Regex.IsMatch(ql, "contact|phone|whatsapp|email|call"))
            {
                return "You can reach us on:<br/><b>Phone / WhatsApp:</b> " +
                    "+27 31 123 4567 or +27 81 234 5678<br/><b>Email:</b> " +
                    "info@khwezidriving.co.za";
            }

            if (Regex.IsMatch(ql,
                "price|cost|how much|rate|package|lesson|code|automatic|truck|hire|learners|k53|theory"))
            {
                string prices = PricesReply(ql);
                if (prices != null)
                {
                    return prices;
                }
            }

            return "I'm having trouble answering that right now. Try asking " +
                "about <b>prices</b>, <b>booking</b>, <b>payment</b> or our " +
                "<b>location</b> &mdash; or email info@khwezidriving.co.za " +
                "and a human will help.";
        }


        // =====================================================
        // PRICES FROM DATABASE (fallback price answers)
        // =====================================================

        private static string PricesReply(string ql)
        {
            string filter = MapFilter(ql);

            try
            {
                string connStr =
                    ConfigurationManager
                    .ConnectionStrings["KhweziConnection"]
                    .ConnectionString;

                string uBook = Url("~/BookSession");
                string uLearners = Url("~/LearnersClass");
                string uOffers = Url("~/Offers");

                string html = "";

                using (var conn = new SqlConnection(connStr))
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = conn;

                    if (filter == null)
                    {
                        cmd.CommandText =
                            "SELECT PackageType, MIN(Amount) AS MinAmount " +
                            "FROM Packages GROUP BY PackageType " +
                            "ORDER BY PackageType";
                    }
                    else
                    {
                        cmd.CommandText =
                            "SELECT PackageName, TotalLessons, Amount " +
                            "FROM Packages WHERE PackageType = @pt " +
                            "ORDER BY TotalLessons";
                        cmd.Parameters.AddWithValue("@pt", filter);
                    }

                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        bool any = false;

                        if (filter == null)
                        {
                            html = "<b>Our packages (live prices):</b><br/>";

                            while (reader.Read())
                            {
                                any = true;
                                html += "&bull; <b>" +
                                    J(reader["PackageType"].ToString()) +
                                    "</b> packages from R" +
                                    Convert.ToDecimal(reader["MinAmount"])
                                        .ToString("0.00",
                                            CultureInfo.InvariantCulture) +
                                    "<br/>";
                            }
                        }
                        else
                        {
                            html = "<b>Our " + J(filter) +
                                " options (live prices):</b><br/>";

                            while (reader.Read())
                            {
                                any = true;
                                html += "&bull; " +
                                    J(reader["PackageName"].ToString()) +
                                    " &mdash; <b>R" +
                                    Convert.ToDecimal(reader["Amount"])
                                        .ToString("0.00",
                                            CultureInfo.InvariantCulture) +
                                    "</b> (" +
                                    Convert.ToInt32(
                                        reader["TotalLessons"]) +
                                    " lessons)<br/>";
                            }
                        }

                        if (!any)
                        {
                            return null;
                        }
                    }
                }

                html += "<br/><a href='" + uBook +
                    "'>Book a practical session</a> &nbsp;&bull;&nbsp; " +
                    "<a href='" + uLearners +
                    "'>Book a learners class</a> &nbsp;&bull;&nbsp; " +
                    "<a href='" + uOffers + "'>All packages</a>";

                return html;
            }
            catch
            {
                return null;
            }
        }


        private static string MapFilter(string q)
        {
            if (q.Contains("code 08") || q.Contains("code 8") ||
                q.Contains("code08"))
            {
                return "Code 08";
            }
            if (q.Contains("code 10") || q.Contains("code10"))
            {
                return "Code 10";
            }
            if (q.Contains("automatic") || q.Contains("auto"))
            {
                return "Automatic";
            }
            if (q.Contains("learner") || q.Contains("theory") ||
                q.Contains("k53"))
            {
                return "Learners";
            }
            return null;
        }


        // =====================================================
        // HELPERS
        // =====================================================

        private static string Url(string appPath)
        {
            try
            {
                return VirtualPathUtility.ToAbsolute(appPath);
            }
            catch
            {
                return appPath;
            }
        }

        private static string J(string value)
        {
            return HttpUtility.JavaScriptStringEncode(value ?? "");
        }

        private static void LogChatError(string question, Exception ex)
        {
            try
            {
                string directory =
                    HttpContext.Current.Server.MapPath("~/App_Data");

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string file =
                    Path.Combine(directory, "chat_errors.txt");

                string error =
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    + " | Q=" + question
                    + " | Error=" + ex.Message
                    + Environment.NewLine;

                File.AppendAllText(file, error);
            }
            catch
            {
                // Never let logging crash the handler
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}