using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace WebApplicationM3
{
    public static class EmailHelper
    {
        public static bool SendStaffAnnouncement(string toEmail, string subject, string message)
        {
            string safe = HttpUtility.HtmlEncode(message ?? "").Replace("\r\n", "<br/>").Replace("\n", "<br/>");
            return Send(toEmail, subject, "<h2>Staff announcement</h2><p>" + safe + "</p>");
        }

        // =========================================================
        // SEND EMAIL VIA BREVO HTTP API
        // =========================================================
        private static bool Send(string toEmail, string subject, string htmlBody)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(toEmail)) return false;

                string apiKey = ConfigurationManager.AppSettings["BrevoApiKey"];
                string senderEmail = ConfigurationManager.AppSettings["SmtpUser"];
                string fromName = ConfigurationManager.AppSettings["SmtpFromName"];

                if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(senderEmail))
                {
                    LogFailure(toEmail, subject, new Exception("Email settings missing from Web.config."));
                    return false;
                }
                if (string.IsNullOrWhiteSpace(fromName)) fromName = "IKHWEZI WEB";

                string json =
                    "{" +
                    "\"sender\":{\"name\":\"" + J(fromName) + "\",\"email\":\"" + J(senderEmail) + "\"}," +
                    "\"to\":[{\"email\":\"" + J(toEmail) + "\"}]," +
                    "\"subject\":\"" + J(subject) + "\"," +
                    "\"htmlContent\":\"" + J(BuildShell(htmlBody)) + "\"" +
                    "}";

                PostJson("https://api.brevo.com/v3/smtp/email", apiKey, json);
                return true;
            }
            catch (Exception ex)
            {
                LogFailure(toEmail, subject, ex);
                return false;
            }
        }

        private static string PostJson(string url, string apiKey, string json)
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.Headers["accept"] = "application/json";
                    client.Headers["api-key"] = apiKey;
                    return client.UploadString(url, "POST", json);
                }
            }
            catch (WebException wx)
            {
                string details = "";
                if (wx.Response != null)
                {
                    try
                    {
                        using (var reader = new StreamReader(wx.Response.GetResponseStream()))
                            details = reader.ReadToEnd();
                    }
                    catch { }
                }
                throw new Exception("Brevo API error: " + wx.Message + (string.IsNullOrEmpty(details) ? "" : " | " + details));
            }
        }

        private static string J(string value)
        {
            return HttpUtility.JavaScriptStringEncode(value ?? "");
        }

        private static void LogFailure(string toEmail, string subject, Exception ex)
        {
            try
            {
                string directory = HttpContext.Current.Server.MapPath("~/App_Data");
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                string file = Path.Combine(directory, "email_errors.txt");
                string error = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                    " | To=" + toEmail + " | Subject=" + subject +
                    " | Error=" + ex + Environment.NewLine;
                File.AppendAllText(file, error);
            }
            catch { }
        }

        // =========================================================
        // BRANDED SHELL & HELPERS
        // =========================================================
        private static string BuildShell(string innerHtml)
        {
            return
                "<html><body style='margin:0;padding:0;background:#eef2fa;" +
                "font-family:Segoe UI,Arial,sans-serif;'>" +
                "<table width='100%' cellpadding='0' cellspacing='0' " +
                "style='background:#eef2fa;padding:24px 0;'><tr><td align='center'>" +
                "<table width='600' cellpadding='0' cellspacing='0' " +
                "style='background:#ffffff;border-radius:12px;overflow:hidden;'>" +
                "<tr><td style='background:#0d2b6e;padding:22px 28px;'>" +
                "<span style='font-size:24px;font-weight:800;font-style:italic;" +
                "color:#ffffff;letter-spacing:1px;'>IKHWEZI" +
                "<span style='color:#e8283f;'>.</span></span><br/>" +
                "<span style='font-size:9px;letter-spacing:4px;color:#c3d2ee;'>DRIVING SCHOOL</span>" +
                "</td></tr>" +
                "<tr><td style='padding:28px;'>" + innerHtml + "</td></tr>" +
                "<tr><td style='background:#f4f7fd;padding:16px 28px;" +
                "border-top:3px solid #e8283f;'>" +
                "<span style='font-size:12px;color:#667;'>" +
                "IKHWEZI Driving School &middot; Durban UKZN Concourse &middot; " +
                "This is an automated message &mdash; please do not reply.</span>" +
                "</td></tr></table></td></tr></table></body></html>";
        }

        private static string Row(string label, string value)
        {
            return "<tr>" +
                "<td style='padding:8px 0;color:#0d2b6e;font-weight:700;" +
                "font-size:13px;width:150px;vertical-align:top;'>" +
                HttpUtility.HtmlEncode(label) + "</td>" +
                "<td style='padding:8px 0;color:#333;font-weight:600;font-size:13px;'>" +
                HttpUtility.HtmlEncode(value ?? "") + "</td>" +
                "</tr>";
        }

        private static string DetailTable(string rows)
        {
            return "<table width='100%' cellpadding='0' cellspacing='0' " +
                "style='background:#f7faff;border:1px solid #dbe4f6;" +
                "border-radius:8px;padding:14px 18px;'>" + rows + "</table>";
        }

        private static string Format(TimeSpan time) { return time.ToString(@"hh\:mm"); }

        // =========================================================
        // 1. REGISTRATION
        // =========================================================
        public static bool SendRegistrationWelcome(string toEmail, string fullName, int studNumber)
        {
            string firstName = fullName;
            if (!string.IsNullOrWhiteSpace(fullName))
            {
                string[] names = fullName.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (names.Length > 0) firstName = names[0];
            }

            string body =
                "<h2 style='color:#0d2b6e;margin:0 0 6px;'>Welcome to IKHWEZI, " +
                HttpUtility.HtmlEncode(firstName) + "! &#127891;</h2>" +
                "<p style='color:#555;font-size:14px;line-height:1.6;margin:0 0 18px;'>" +
                "Your student account has been created successfully. " +
                "Your journey to the driver's seat starts here!</p>" +
                DetailTable(
                    Row("Student Number", studNumber.ToString()) +
                    Row("Email", toEmail) +
                    Row("Status", "Active") +
                    Row("Registered On", DateTime.Now.ToString("dd MMM yyyy, HH:mm"))) +
                "<p style='color:#555;font-size:14px;line-height:1.6;margin:18px 0 0;'>" +
                "&#9993; Keep your student number safe &mdash; you'll use it whenever you contact reception." +
                "<br/>&#128663; Next step: browse our packages and book your first session from your dashboard." +
                "</p>";

            return Send(toEmail, "Welcome to IKHWEZI Driving School - Account Created", body);
        }

        // =========================================================
        // 2. BOOKING CONFIRMATION
        // =========================================================
        public static bool SendBookingConfirmation(
            string toEmail, string firstName, int bookingId, string packageName,
            DateTime date, TimeSpan start, TimeSpan end,
            string instructorName, string vehicleText)
        {
            string body =
                "<h2 style='color:#0d2b6e;margin:0 0 6px;'>Booking Confirmed! &#9989;</h2>" +
                "<p style='color:#555;font-size:14px;line-height:1.6;margin:0 0 18px;'>" +
                "Hi " + HttpUtility.HtmlEncode(firstName) + ", your driving lesson has been " +
                "successfully booked. Your lesson will be deducted from your paid package balance.</p>" +
                DetailTable(
                    Row("Booking Ref", "#" + bookingId) +
                    Row("Package", packageName) +
                    Row("Date", date.ToString("dddd, d MMMM yyyy")) +
                    Row("Time", Format(start) + " to " + Format(end)) +
                    Row("Instructor", instructorName) +
                    Row("Vehicle", string.IsNullOrWhiteSpace(vehicleText) ? "To be assigned" : vehicleText) +
                    Row("Status", "Confirmed")) +
                "<p style='color:#555;font-size:14px;line-height:1.6;margin:18px 0 0;'>" +
                "&#9203; Please arrive <strong>15 minutes early</strong> with your learner's licence and ID document." +
                "<br/>&#128197; You can manage, cancel or re-book this lesson at any time from your student dashboard.</p>";

            return Send(toEmail, "Booking #" + bookingId + " Confirmed - IKHWEZI Driving School", body);
        }

        // =========================================================
        // 3. LEARNERS CLASS EMAIL
        // =========================================================
        public static bool SendLearnersConfirmation(
            string toEmail, string firstName, int bookingId,
            string licenceCode, DateTime date, TimeSpan start, string instructorName)
        {
            string payUrl = BuildPayUrl(bookingId);

            string body =
                "<h2 style='color:#0d2b6e;margin:0 0 6px;'>Learners Class Booked! &#128218;</h2>" +
                "<p style='color:#555;font-size:14px;line-height:1.6;margin:0 0 18px;'>" +
                "Hi " + HttpUtility.HtmlEncode(firstName) + ", your K53 theory class has been scheduled.</p>" +
                DetailTable(
                    Row("Booking Ref", "#" + bookingId) +
                    Row("Licence Code", licenceCode) +
                    Row("Date", date.ToString("dddd, d MMMM yyyy")) +
                    Row("Time", Format(start) + " (1 hour)") +
                    Row("Instructor", string.IsNullOrWhiteSpace(instructorName) ? "To be assigned" : instructorName) +
                    Row("Status", "Unsettled - awaiting payment")) +
                "<div style='background:#eef4ff;border:1px solid #c9d9f3;border-radius:10px;" +
                "padding:18px 20px;margin:22px 0 0;'>" +
                "<div style='font-weight:800;color:#0d2b6e;font-size:15px;margin-bottom:10px;'>" +
                "&#128179; Payment Options</div>" +
                "<p style='margin:0 0 14px 0;font-size:13.5px;color:#445;line-height:1.6;'>" +
                "You can pay for this class in either of two convenient ways. " +
                "Your booking is held either way &mdash; pay whenever suits you best.</p>" +
                "<p style='margin:0 0 8px 0;font-size:13px;color:#22355f;'>" +
                "<strong>Option 1 &mdash; Pay online now:</strong></p>" +
                "<div style='text-align:center;margin:14px 0 20px 0;'>" +
                "<a href='" + HttpUtility.HtmlAttributeEncode(payUrl) + "' " +
                "style='display:inline-block;padding:13px 34px;background:#22b573;" +
                "color:#ffffff;border-radius:24px;text-decoration:none;font-weight:800;font-size:14px;'>" +
                "&#128179; Pay Now Online &rarr;</a></div>" +
                "<p style='margin:0 0 8px 0;font-size:13px;color:#22355f;'>" +
                "<strong>Option 2 &mdash; Pay at reception:</strong></p>" +
                "<p style='margin:0;font-size:13px;color:#445;line-height:1.7;'>" +
                "&#128222; <a href='tel:+27835448544' style='color:#0d2b6e;font-weight:700;text-decoration:none;'>" +
                "+27 83 544 8544</a><br/>" +
                "&#9993; <a href='mailto:info@khwezidrivingschool.co.za' " +
                "style='color:#0d2b6e;font-weight:700;text-decoration:none;'>" +
                "info@khwezidrivingschool.co.za</a></p></div>" +
                "<p style='color:#555;font-size:14px;line-height:1.6;margin:22px 0 0;'>" +
                "&#128214; Please prepare for your K53 theory class and bring your <strong>ID document</strong>." +
                "<br/>&#9203; Arrive <strong>15 minutes early</strong>.</p>";

            return Send(toEmail, "Learners Class #" + bookingId + " Booked - IKHWEZI Driving School", body);
        }

        // =========================================================
        // 4. BOOKING RECEIVED
        // =========================================================
        public static bool SendBookingReceivedEmail(
            string toEmail, string firstName, int bookingId, string packageName,
            DateTime date, TimeSpan start, TimeSpan end,
            string instructorName, string vehicleText)
        {
            string body =
                "<h2 style='color:#0d2b6e;margin:0 0 6px;'>Booking Received! &#128221;</h2>" +
                "<p style='color:#555;font-size:14px;line-height:1.6;margin:0 0 18px;'>" +
                "Hi " + HttpUtility.HtmlEncode(firstName) + ", your lesson has been reserved. " +
                "To <strong>confirm your slot</strong>, please pay a <strong>50% deposit</strong> " +
                "at reception or online.</p>" +
                DetailTable(
                    Row("Booking Ref", "#" + bookingId) +
                    Row("Package", packageName) +
                    Row("Date", date.ToString("dddd, d MMMM yyyy")) +
                    Row("Time", Format(start) + " to " + Format(end)) +
                    Row("Instructor", instructorName) +
                    Row("Vehicle", string.IsNullOrWhiteSpace(vehicleText) ? "To be assigned" : vehicleText) +
                    Row("Status", "Unsettled - awaiting 50% deposit")) +
                "<p style='color:#555;font-size:14px;line-height:1.6;margin:18px 0 0;'>" +
                "&#9200; <strong>Pay now</strong> to secure your spot. Bookings that remain unpaid may be released to other students." +
                "<br/>&#128179; Pay online from your dashboard, or at reception (Cash / Card / EFT).</p>";

            return Send(toEmail, "Booking #" + bookingId + " Reserved - Pay 50% Deposit", body);
        }

        // =========================================================
        // 5. PAYMENT RECEIVED
        // =========================================================
        public static bool SendPaymentReceivedEmail(
            string toEmail, string firstName, int bookingId,
            decimal amountPaid, DateTime scheduled)
        {
            string body =
                "<h2 style='color:#0d2b6e;margin:0 0 6px;'>Payment Received! &#127881;</h2>" +
                "<p style='color:#555;font-size:14px;line-height:1.6;margin:0 0 18px;'>" +
                "Hi " + HttpUtility.HtmlEncode(firstName) + ", we've received your deposit. " +
                "Your booking is now <strong>pending reception confirmation</strong>.</p>" +
                DetailTable(
                    Row("Booking Ref", "#" + bookingId) +
                    Row("Amount Paid", "R " + amountPaid.ToString("N2")) +
                    Row("Lesson Date", scheduled.ToString("dddd, d MMMM yyyy 'at' HH:mm")) +
                    Row("Status", "Pending - awaiting reception")) +
                "<p style='color:#555;font-size:14px;line-height:1.6;margin:18px 0 0;'>" +
                "&#9989; You'll receive a final confirmation email once reception has approved your booking.</p>";

            return Send(toEmail, "Payment Received - Booking #" + bookingId, body);
        }

        // =========================================================
        // 6. CANCELLATION NOTICE
        // =========================================================
        public static bool SendCancellationNotice(
            string toEmail, string firstName, int bookingId,
            DateTime scheduled, string cancelledBy)
        {
            string body =
                "<h2 style='color:#0d2b6e;margin:0 0 6px;'>Booking Cancelled</h2>" +
                "<p style='color:#555;font-size:14px;line-height:1.6;margin:0 0 18px;'>" +
                "Hi " + HttpUtility.HtmlEncode(firstName) + ", your booking has been " +
                "<strong>successfully cancelled</strong>. No lesson was deducted from your package.</p>" +
                DetailTable(
                    Row("Booking Ref", "#" + bookingId) +
                    Row("Was scheduled for", scheduled.ToString("dddd, d MMMM yyyy 'at' HH:mm")) +
                    Row("Cancelled by", cancelledBy) +
                    Row("Status", "Cancelled")) +
                "<p style='color:#555;font-size:14px;line-height:1.6;margin:18px 0 0;'>" +
                "You can book a new lesson anytime from your dashboard." +
                "<br/>If you did not request this cancellation, contact reception immediately.</p>";

            return Send(toEmail, "Booking #" + bookingId + " Cancelled - IKHWEZI Driving School", body);
        }

        // =========================================================
        // 7. RESCHEDULE NOTICE
        // =========================================================
        public static bool SendRescheduleNotice(
            string toEmail, string firstName, int bookingId,
            DateTime oldWhen, DateTime newWhen)
        {
            string body =
                "<h2 style='color:#0d2b6e;margin:0 0 6px;'>Booking Rescheduled</h2>" +
                "<p style='color:#555;font-size:14px;line-height:1.6;margin:0 0 18px;'>" +
                "Hi " + HttpUtility.HtmlEncode(firstName) + ", your lesson has been rescheduled. " +
                "Please note the new time below.</p>" +
                DetailTable(
                    Row("Booking Ref", "#" + bookingId) +
                    Row("Old Date", oldWhen.ToString("dddd, d MMM yyyy 'at' HH:mm")) +
                    Row("New Date", newWhen.ToString("dddd, d MMM yyyy 'at' HH:mm"))) +
                "<p style='color:#555;font-size:14px;line-height:1.6;margin:18px 0 0;'>" +
                "If the new time doesn't work, please log in to your dashboard to reschedule or contact reception.</p>";

            return Send(toEmail, "Booking #" + bookingId + " Rescheduled - IKHWEZI", body);
        }

        // =========================================================
        // 8. LESSON COMPLETED
        // =========================================================
        public static bool SendLessonCompletedNotice(
            string toEmail, string firstName, int bookingId, DateTime scheduled)
        {
            string body =
                "<h2 style='color:#0d2b6e;margin:0 0 6px;'>Lesson Completed &#127942;</h2>" +
                "<p style='color:#555;font-size:14px;line-height:1.6;margin:0 0 18px;'>" +
                "Hi " + HttpUtility.HtmlEncode(firstName) + ", your lesson on <b>" +
                scheduled.ToString("dddd, d MMMM yyyy") + "</b> has been marked as " +
                "<strong>Completed</strong>. One lesson has been deducted from your package balance.</p>" +
                DetailTable(
                    Row("Booking Ref", "#" + bookingId) +
                    Row("Completed On", DateTime.Now.ToString("dd MMM yyyy, HH:mm")) +
                    Row("Status", "Completed")) +
                "<p style='color:#555;font-size:14px;line-height:1.6;margin:18px 0 0;'>" +
                "Well done on your progress! You can book your next lesson from your dashboard anytime.</p>";

            return Send(toEmail, "Lesson Completed - Khwezi Driving School", body);
        }

        // =========================================================
        // 9. PASSWORD RESET CODE
        // =========================================================
        public static bool SendPasswordResetCode(
            string toEmail, string firstName, string code, int expiryMinutes)
        {
            string body =
                "<h2 style='color:#0d2b6e;margin:0 0 6px;'>Reset Your Password &#128274;</h2>" +
                "<p style='color:#555;font-size:14px;line-height:1.6;margin:0 0 18px;'>" +
                "Hi " + HttpUtility.HtmlEncode(firstName) + ", we received a request " +
                "to reset the password on your Khwezi account. Use the code below to continue:</p>" +
                "<div style='background:#f2f6fd;border-radius:10px;padding:24px;" +
                "margin:20px 0;text-align:center;border:1px solid #dbe4f6;'>" +
                "<div style='font-size:12px;color:#7787a5;letter-spacing:2px;" +
                "text-transform:uppercase;font-weight:700;margin-bottom:10px;'>Your Reset Code</div>" +
                "<div style='font-size:38px;font-weight:800;color:#0d2b6e;letter-spacing:8px;" +
                "font-family:Courier New,monospace;'>" + HttpUtility.HtmlEncode(code) + "</div></div>" +
                DetailTable(
                    Row("Valid For", expiryMinutes + " minutes") +
                    Row("Requested", DateTime.Now.ToString("dd MMM yyyy, HH:mm"))) +
                "<p style='color:#555;font-size:14px;line-height:1.6;margin:18px 0 0;'>" +
                "&#9200; This code expires in <strong>" + expiryMinutes + " minutes</strong>." +
                "<br/><br/>&#128274; If you did <strong>not</strong> request a password reset, " +
                "you can safely ignore this email &mdash; your password will remain unchanged.</p>";

            return Send(toEmail, "Your Password Reset Code - IKHWEZI Driving School", body);
        }

        // =========================================================
        // 10. STAFF PASSWORD RESET PROMPT
        // =========================================================
        public static bool SendStaffPasswordResetPrompt(string toEmail, string fullName)
        {
            if (string.IsNullOrWhiteSpace(toEmail)) return false;

            string firstName = (fullName ?? "").Split(' ')[0];
            if (string.IsNullOrEmpty(firstName)) firstName = "there";

            string resetUrl = BuildResetUrl();

            string body =
                "<h2 style='color:#0d2b6e;margin:0 0 6px;'>Reset Your IKHWEZI Password &#128274;</h2>" +
                "<p style='color:#555;font-size:14px;line-height:1.6;margin:0 0 18px;'>Hi " +
                HttpUtility.HtmlEncode(firstName) + ",</p>" +
                "<p style='color:#555;font-size:14px;line-height:1.6;margin:0 0 18px;'>" +
                "We recently upgraded our security. To keep your account safe, " +
                "please <b>set a new password</b> by clicking the button below.</p>" +
                "<div style='text-align:center;margin:26px 0;'>" +
                "<a href='" + HttpUtility.HtmlAttributeEncode(resetUrl) + "' " +
                "style='display:inline-block;padding:13px 34px;background:#007bff;color:#fff;" +
                "border-radius:24px;text-decoration:none;font-weight:800;font-size:15px;'>" +
                "Reset My Password &rarr;</a></div>" +
                "<p style='color:#555;font-size:13.5px;line-height:1.6;margin:18px 0 0;'>" +
                "Or paste this link into your browser:<br/>" +
                "<span style='color:#0d2b6e;font-size:12.5px;word-break:break-all;'>" +
                HttpUtility.HtmlEncode(resetUrl) + "</span></p>" +
                DetailTable(
                    Row("Your Email", toEmail) +
                    Row("Sent On", DateTime.Now.ToString("dd MMM yyyy, HH:mm"))) +
                "<p style='color:#99a3b5;font-size:12.5px;margin-top:22px;line-height:1.6;'>" +
                "&#128274; We never send passwords by email. The reset flow will ask you to " +
                "enter a 6-digit code first, then let you choose a new password.</p>" +
                "<p style='color:#99a3b5;font-size:12px;margin-top:14px;'>" +
                "If you weren't expecting this email, please contact your manager.</p>";

            return Send(toEmail, "Action Required: Reset your IKHWEZI password", body);
        }

        // =========================================================
        // URL BUILDERS
        // =========================================================
        private static string BuildResetUrl()
        {
            try
            {
                var ctx = HttpContext.Current;
                if (ctx != null && ctx.Request != null)
                {
                    string baseUrl = ctx.Request.Url.GetLeftPart(UriPartial.Authority);
                    string virtualPath = ctx.Request.ApplicationPath ?? "/";
                    if (!virtualPath.EndsWith("/")) virtualPath += "/";
                    return baseUrl + virtualPath + "Account/ForgotPassword.aspx";
                }
            }
            catch { }
            return "https://ist-wst-group-27.azurewebsites.net/Account/ForgotPassword.aspx";
        }

        private static string BuildPayUrl(int bookingId)
        {
            try
            {
                var ctx = HttpContext.Current;
                if (ctx != null && ctx.Request != null)
                {
                    string baseUrl = ctx.Request.Url.GetLeftPart(UriPartial.Authority);
                    string virtualPath = ctx.Request.ApplicationPath ?? "/";
                    if (!virtualPath.EndsWith("/")) virtualPath += "/";
                    return baseUrl + virtualPath + "PayDeposit.aspx?id=" + bookingId;
                }
            }
            catch { }
            return "https://ist-wst-group-27.azurewebsites.net/PayDeposit.aspx?id=" + bookingId;
        }

        private static string BuildBookUrl()
        {
            try
            {
                var ctx = HttpContext.Current;
                if (ctx != null && ctx.Request != null)
                {
                    string baseUrl = ctx.Request.Url.GetLeftPart(UriPartial.Authority);
                    string virtualPath = ctx.Request.ApplicationPath ?? "/";
                    if (!virtualPath.EndsWith("/")) virtualPath += "/";
                    return baseUrl + virtualPath + "BookSession.aspx";
                }
            }
            catch { }
            return "https://ist-wst-group-27.azurewebsites.net/BookSession.aspx";
        }

        // =========================================================
        // 11. PAYMENT REMINDER
        // =========================================================
        public static bool SendPaymentReminder(
            string toEmail, string studentName, int bookingId, string packageName,
            decimal totalPaid, decimal balanceDue, DateTime dueDate, int daysRemaining)
        {
            string urgency = daysRemaining < 0
                ? "<span style='color:#e8283f;font-weight:800;'>Your payment is " +
                  Math.Abs(daysRemaining) + " day(s) overdue.</span>"
                : "<span style='color:#e8890c;font-weight:800;'>You have " +
                  daysRemaining + " day(s) to pay the balance.</span>";

            string body =
                "<p style='color:#555;font-size:14px;line-height:1.6;margin:0 0 18px;'>Hi " +
                HttpUtility.HtmlEncode(studentName) + ",</p>" +
                "<p style='color:#555;font-size:14px;line-height:1.6;'>" +
                "This is a friendly reminder about the outstanding balance on your <b>" +
                HttpUtility.HtmlEncode(packageName) + "</b> package.</p>" +
                DetailTable(
                    Row("Booking", "#" + bookingId) +
                    Row("Package", packageName) +
                    Row("Total Paid", "R " + totalPaid.ToString("N2")) +
                    Row("Balance Due", "R " + balanceDue.ToString("N2")) +
                    Row("Due Date", dueDate.ToString("dddd, d MMMM yyyy"))) +
                "<div style='background:#fff3d6;border:1px solid #f0e0a0;border-radius:8px;" +
                "padding:14px 18px;margin:18px 0;text-align:center;font-size:14px;'>" +
                urgency + "</div>" +
                "<p style='color:#555;font-size:13.5px;line-height:1.6;'>" +
                "Please settle your balance at reception (Cash / Card / EFT). Once paid, the booking remains Confirmed.</p>" +
                "<p style='color:#99a3b5;font-size:12.5px;margin-top:22px;'>" +
                "If you have any questions, contact reception.</p>";

            string subject = daysRemaining < 0
                ? "Payment Overdue - Booking #" + bookingId
                : "Payment Reminder - Booking #" + bookingId;

            return Send(toEmail, subject, body);
        }

        // =========================================================
        // 12. CONTACT FORM
        // =========================================================
        public static bool SendContactMessage(
            string fromName, string fromEmail, string fromPhone, string message)
        {
            string toEmail = ConfigurationManager.AppSettings["ContactFormRecipient"]
                ?? "info@khwezidrivingschool.co.za";

            string body =
                "<h2 style='color:#0d2b6e;margin:0 0 6px;'>New Contact Message</h2>" +
                "<p style='color:#555;font-size:14px;line-height:1.6;margin:0 0 18px;'>" +
                "A visitor has submitted the contact form on your website. " +
                "Reply directly to their email address below.</p>" +
                DetailTable(
                    Row("Name", fromName) +
                    Row("Email", fromEmail) +
                    Row("Phone", fromPhone) +
                    Row("Sent", DateTime.Now.ToString("dd MMM yyyy, HH:mm"))) +
                "<div style='margin:22px 0 0;padding:18px 20px;background:#f7faff;" +
                "border-left:4px solid #e8283f;border-radius:8px;color:#22355f;" +
                "font-size:14px;line-height:1.65;white-space:pre-wrap;'>" +
                HttpUtility.HtmlEncode(message) + "</div>" +
                "<p style='color:#99a3b5;font-size:12.5px;margin-top:22px;'>" +
                "Reply directly to: <a href='mailto:" + HttpUtility.HtmlAttributeEncode(fromEmail) +
                "' style='color:#0d2b6e;font-weight:700;'>" +
                HttpUtility.HtmlEncode(fromEmail) + "</a></p>";

            return Send(toEmail, "Contact Form: " + fromName + " - IKHWEZI", body);
        }

        // =========================================================
        // 13. PACKAGE RESERVATION REMINDER
        // =========================================================
        public static void SendPackageReservationReminder(int studNumber, int reservationId, int dayNumber)
        {
            try
            {
                string email = LessonProgress.GetStudentEmail(studNumber);
                if (string.IsNullOrEmpty(email)) return;
                string name = LessonProgress.GetStudentDisplayName(email);

                var r = PackageReservationService.GetCurrent(studNumber);
                if (r == null || r.ReservationID != reservationId) return;

                string payUrl = BuildPayUrl(r.ReservationID);

                string subject;
                string body;

                if (dayNumber == 0)
                {
                    subject = "Your IKHWEZI package reservation — pay within 3 days";
                    body =
                        "<p style='color:#555;font-size:14px;line-height:1.6;'>Hi " +
                        HttpUtility.HtmlEncode(name) + ",</p>" +
                        "<p style='color:#555;font-size:14px;line-height:1.6;'>" +
                        "You have reserved the package <strong>" +
                        HttpUtility.HtmlEncode(r.PackageName) + "</strong>.</p>" +
                        DetailTable(
                            Row("Package", r.PackageName) +
                            Row("Deposit Due", "R " + r.DepositAmount.ToString("N2")) +
                            Row("Pay By", r.ExpiresOn.ToString("dddd, d MMMM yyyy HH:mm"))) +
                        "<div style='text-align:center;margin:22px 0;'>" +
                        "<a href='" + HttpUtility.HtmlAttributeEncode(payUrl) + "' " +
                        "style='display:inline-block;padding:13px 34px;background:#22b573;" +
                        "color:#fff;border-radius:24px;text-decoration:none;font-weight:800;font-size:14px;'>" +
                        "Pay Online Now &rarr;</a></div>" +
                        "<p style='color:#555;font-size:13.5px;line-height:1.6;'>" +
                        "If payment is not received within 3 days, the reservation will expire " +
                        "and the package will be released.</p>";
                }
                else if (dayNumber == 1)
                {
                    subject = "Reminder: pay your IKHWEZI deposit (" + r.DaysRemaining + " days left)";
                    body =
                        "<p style='color:#555;font-size:14px;line-height:1.6;'>Hi " +
                        HttpUtility.HtmlEncode(name) + ",</p>" +
                        "<p style='color:#555;font-size:14px;line-height:1.6;'>" +
                        "You have <strong>" + r.DaysRemaining + " day(s)</strong> left to pay " +
                        "the deposit for your reserved package.</p>" +
                        DetailTable(
                            Row("Package", r.PackageName) +
                            Row("Deposit", "R " + r.DepositAmount.ToString("N2"))) +
                        "<div style='text-align:center;margin:22px 0;'>" +
                        "<a href='" + HttpUtility.HtmlAttributeEncode(payUrl) + "' " +
                        "style='display:inline-block;padding:13px 34px;background:#22b573;" +
                        "color:#fff;border-radius:24px;text-decoration:none;font-weight:800;font-size:14px;'>" +
                        "Pay Online Now &rarr;</a></div>";
                }
                else
                {
                    subject = "URGENT: IKHWEZI reservation expires today";
                    body =
                        "<p style='color:#555;font-size:14px;line-height:1.6;'>Hi " +
                        HttpUtility.HtmlEncode(name) + ",</p>" +
                        "<p style='color:#e8283f;font-size:14px;font-weight:700;'>" +
                        "Your package reservation expires today. Pay now to avoid losing it.</p>" +
                        DetailTable(
                            Row("Package", r.PackageName) +
                            Row("Deposit", "R " + r.DepositAmount.ToString("N2"))) +
                        "<div style='text-align:center;margin:22px 0;'>" +
                        "<a href='" + HttpUtility.HtmlAttributeEncode(payUrl) + "' " +
                        "style='display:inline-block;padding:13px 34px;background:#e8283f;" +
                        "color:#fff;border-radius:24px;text-decoration:none;font-weight:800;font-size:14px;'>" +
                        "Pay Now &rarr;</a></div>";
                }

                Send(email, subject, body);
            }
            catch (Exception ex) { System.Diagnostics.Trace.TraceError("SendPackageReservationReminder: " + ex); }
        }

        // =========================================================
        // 14. PACKAGE PAID EMAIL
        // =========================================================
        public static void SendPackagePaidEmail(string toEmail, string name, string packageName, decimal amountPaid)
        {
            try
            {
                string bookUrl = BuildBookUrl();

                string body =
                    "<p style='color:#555;font-size:14px;line-height:1.6;'>Hi " +
                    HttpUtility.HtmlEncode(name) + ",</p>" +
                    "<p style='color:#22b573;font-size:15px;font-weight:700;'>" +
                    "&#9989; Your payment has been received!</p>" +
                    DetailTable(
                        Row("Package", packageName) +
                        Row("Amount Paid", "R " + amountPaid.ToString("N2")) +
                        Row("Paid On", DateTime.Now.ToString("dd MMM yyyy, HH:mm"))) +
                    "<p style='color:#555;font-size:14px;line-height:1.6;margin-top:18px;'>" +
                    "Your package is now active. You can start booking lessons immediately.</p>" +
                    "<div style='text-align:center;margin:22px 0;'>" +
                    "<a href='" + HttpUtility.HtmlAttributeEncode(bookUrl) + "' " +
                    "style='display:inline-block;padding:13px 34px;background:#22b573;" +
                    "color:#fff;border-radius:24px;text-decoration:none;font-weight:800;font-size:14px;'>" +
                    "Book Your First Lesson &rarr;</a></div>" +
                    "<p style='color:#555;font-size:13.5px;line-height:1.6;'>" +
                    "If you have any questions, contact reception on " +
                    "<a href='tel:+27835448544' style='color:#0d2b6e;font-weight:700;'>+27 83 544 8544</a>.</p>";

                Send(toEmail, "IKHWEZI — Package activated", body);
            }
            catch (Exception ex) { System.Diagnostics.Trace.TraceError("SendPackagePaidEmail: " + ex); }
        }

        // =========================================================
        // 15. LEARNERS CANCELLATION + REFUND
        // =========================================================
        public static bool SendLearnersCancellationEmail(
            string toEmail, string firstName, int bookingId,
            DateTime scheduledDate, decimal amountPaid)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(toEmail)) return false;

                string refundLine;
                if (amountPaid > 0)
                {
                    refundLine =
                        "<div style='background:#effaf4;border:1px solid #bfe8d4;" +
                        "border-radius:8px;padding:16px 20px;margin:18px 0;'>" +
                        "<div style='font-weight:800;color:#14663f;font-size:14px;margin-bottom:8px;'>" +
                        "&#128176; Refund In Progress</div>" +
                        "<p style='margin:0;font-size:13px;color:#14663f;line-height:1.6;'>" +
                        "A full refund of <strong>R " + amountPaid.ToString("N2") + "</strong> " +
                        "will be processed within <strong>3 working days</strong>. " +
                        "You will receive a separate confirmation once the refund has been issued.</p></div>";
                }
                else
                {
                    refundLine =
                        "<div style='background:#f4f7fd;border:1px solid #dbe4f6;" +
                        "border-radius:8px;padding:16px 20px;margin:18px 0;'>" +
                        "<p style='margin:0;font-size:13px;color:#445;'>" +
                        "No payment was received for this booking, so no refund is due.</p></div>";
                }

                string body =
                    "<h2 style='color:#0d2b6e;margin:0 0 6px;'>Learners Class Cancelled</h2>" +
                    "<p style='color:#555;font-size:14px;line-height:1.6;margin:0 0 18px;'>" +
                    "Hi " + HttpUtility.HtmlEncode(firstName) + ", your learners-licence class " +
                    "has been <strong>successfully cancelled</strong>.</p>" +
                    DetailTable(
                        Row("Booking Ref", "#" + bookingId) +
                        Row("Was scheduled for", scheduledDate.ToString("dddd, d MMMM yyyy 'at' HH:mm")) +
                        Row("Cancelled on", DateTime.Now.ToString("dddd, d MMMM yyyy 'at' HH:mm")) +
                        Row("Status", "Cancelled")) +
                    refundLine +
                    "<p style='color:#555;font-size:14px;line-height:1.6;margin:18px 0 0;'>" +
                    "You can now book a new learners class or a practical lesson package from your dashboard." +
                    "<br/><br/>If you did not request this cancellation, please contact reception immediately." +
                    "<br/>&#128222; <a href='tel:+27835448544' style='color:#0d2b6e;font-weight:700;'>" +
                    "+27 83 544 8544</a>" +
                    "<br/>&#9993; <a href='mailto:info@khwezidrivingschool.co.za' " +
                    "style='color:#0d2b6e;font-weight:700;'>info@khwezidrivingschool.co.za</a></p>";

                return Send(toEmail, "Learners Class #" + bookingId + " Cancelled - IKHWEZI", body);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("SendLearnersCancellationEmail: " + ex);
                return false;
            }
        }

        // =========================================================
        // 16. CANCELLATION EXCEPTION REQUEST → reception
        // =========================================================
        public static bool SendCancellationRequestToReception(
            string studentName, string studentEmail, int bookingId,
            DateTime scheduled, string reason)
        {
            try
            {
                List<string> recipients = GetReceptionistEmails();
                if (recipients.Count == 0) return false;

                TimeSpan timeUntil = scheduled - DateTime.Now;
                string hoursAway = timeUntil.TotalHours <= 0
                    ? "already started/passed"
                    : Math.Floor(timeUntil.TotalHours) + " hour(s) away";

                string body =
                    "<h2 style='color:#0d2b6e;margin:0 0 6px;'>Cancellation Exception Requested</h2>" +
                    "<p style='color:#555;font-size:14px;line-height:1.6;margin:0 0 18px;'>" +
                    "A student has asked to cancel a booking that is inside the 24-hour window. " +
                    "This is an exception request &mdash; please review and decide.</p>" +
                    "<div style='background:#fff7e0;border:1px solid #f0d999;border-left:4px solid #e8890c;" +
                    "border-radius:8px;padding:14px 18px;margin:0 0 18px;font-size:13.5px;color:#7a5a00;'>" +
                    "&#9200; The lesson is <strong>" + hoursAway + "</strong>.</div>" +
                    DetailTable(
                        Row("Booking Ref", "#" + bookingId) +
                        Row("Student", studentName) +
                        Row("Student Email", studentEmail) +
                        Row("Lesson Date & Time", scheduled.ToString("dddd, d MMMM yyyy 'at' HH:mm")) +
                        Row("Requested On", DateTime.Now.ToString("dddd, d MMM yyyy HH:mm"))) +
                    "<div style='margin:22px 0 0;padding:18px 20px;background:#f7faff;" +
                    "border-left:4px solid #e8283f;border-radius:8px;color:#22355f;" +
                    "font-size:14px;line-height:1.65;white-space:pre-wrap;'>" +
                    "<strong style='color:#0d2b6e;'>Student's reason:</strong><br/><br/>" +
                    HttpUtility.HtmlEncode(reason) + "</div>";

                bool anySuccess = false;
                foreach (string email in recipients)
                {
                    if (Send(email, "Cancellation Request: Booking #" + bookingId + " — " + studentName, body))
                        anySuccess = true;
                }
                return anySuccess;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("SendCancellationRequestToReception: " + ex);
                return false;
            }
        }

        // =========================================================
        // 17. INSTRUCTOR REQUEST → reception
        // =========================================================
        public static bool SendInstructorRequestToReception(
            string requestType, string instructorEmail, string instructorName,
            int bookingId, DateTime scheduled, string packageName,
            string studentName, string studentEmail, string reason)
        {
            try
            {
                List<string> recipients = GetReceptionistEmails();
                if (recipients.Count == 0) return false;

                bool isCancel = requestType.Equals("Cancel", StringComparison.OrdinalIgnoreCase);
                string heading = isCancel ? "Instructor Requested Cancellation" : "Instructor Requested Replacement";

                string body =
                    "<h2 style='color:#0d2b6e;margin:0 0 6px;'>" + heading + "</h2>" +
                    "<p style='color:#555;font-size:14px;line-height:1.6;margin:0 0 18px;'>An instructor has sent you a request. Please review and action it on the Reception Dashboard.</p>" +
                    DetailTable(
                        Row("Request Type", isCancel ? "Cancel lesson" : "Replace instructor") +
                        Row("Booking Ref", "#" + bookingId) +
                        Row("Instructor", instructorName + " (" + instructorEmail + ")") +
                        Row("Student", studentName + " (" + studentEmail + ")") +
                        Row("Lesson Date & Time", scheduled.ToString("dddd, d MMMM yyyy 'at' HH:mm"))) +
                    "<div style='margin:22px 0 0;padding:18px 20px;background:#f7faff;border-left:4px solid #e8283f;border-radius:8px;color:#22355f;font-size:14px;line-height:1.65;white-space:pre-wrap;'>" +
                    "<strong style='color:#0d2b6e;'>Instructor's reason:</strong><br/><br/>" + HttpUtility.HtmlEncode(reason) + "</div>";

                string subject = isCancel ? "Instructor Cancel Request: Booking #" + bookingId : "Instructor Replace Request: Booking #" + bookingId;

                bool anySuccess = false;
                foreach (string email in recipients)
                {
                    if (Send(email, subject, body)) anySuccess = true;
                }
                return anySuccess;
            }
            catch (Exception ex) { System.Diagnostics.Trace.TraceError("SendInstructorRequestToReception: " + ex); return false; }
        }

        // =========================================================
        // 18. INSTRUCTOR REQUEST RESPONSE
        // =========================================================
        public static void SendInstructorRequestResponse(string instructorEmail, string instructorName, int bookingId, string requestType, bool isApproved, string message)
        {
            try
            {
                string status = isApproved ? "APPROVED" : "DECLINED";
                string subject = $"Update on your request for Booking #{bookingId}";

                string body = $@"
                    <h2 style='color: #0d2b6e; margin: 0 0 6px;'>Request {status}</h2>
                    <p style='color: #555; font-size: 14px; line-height: 1.6;'>Hi {HttpUtility.HtmlEncode(instructorName)},</p>
                    <p style='color: #555; font-size: 14px; line-height: 1.6;'>Reception has reviewed your <strong>{requestType}</strong> request for booking <strong>#{bookingId}</strong>.</p>
                    <div style='background: {(isApproved ? "#ecfdf5" : "#fef2f2")}; border-left: 4px solid {(isApproved ? "#10b981" : "#ef4444")}; padding: 15px; margin: 20px 0;'>
                        <p style='margin: 0; font-weight: bold; color: {(isApproved ? "#065f46" : "#991b1b")};'>Status: {status}</p>
                        <p style='margin: 10px 0 0 0; color: #333;'>{HttpUtility.HtmlEncode(message)}</p>
                    </div>";

                Send(instructorEmail, subject, body);
            }
            catch (Exception ex) { System.Diagnostics.Trace.TraceError("SendInstructorRequestResponse: " + ex); }
        }
        // =========================================================
        // 19. NEW: STAFF INVITATION EMAIL
        // =========================================================
        public static bool SendStaffInvitation(string toEmail, string staffName, string role, string inviteLink)
        {
            try
            {
                string body =
                    "<h2 style='color:#0d2b6e;margin:0 0 6px;'>Welcome to IKHWEZI!</h2>" +
                    "<p style='color:#555;font-size:14px;line-height:1.6;margin:0 0 18px;'>" +
                    "Hi " + HttpUtility.HtmlEncode(staffName) + ",</p>" +
                    "<p style='color:#555;font-size:14px;line-height:1.6;margin:0 0 18px;'>" +
                    "You have been invited to join the IKHWEZI Driving School team as a <strong>" + HttpUtility.HtmlEncode(role) + "</strong>.</p>" +
                    "<p style='color:#555;font-size:14px;line-height:1.6;margin:0 0 18px;'>" +
                    "Please click the button below to set up your password and activate your account. This link is valid for 48 hours.</p>" +
                    "<div style='text-align:center;margin:26px 0;'>" +
                    "<a href='" + HttpUtility.HtmlAttributeEncode(inviteLink) + "' " +
                    "style='display:inline-block;padding:13px 34px;background:#22b573;color:#fff;" +
                    "border-radius:24px;text-decoration:none;font-weight:800;font-size:15px;'>" +
                    "Set Up My Account &rarr;</a></div>" +
                    "<p style='color:#555;font-size:13.5px;line-height:1.6;margin:18px 0 0;'>" +
                    "Or paste this link into your browser:<br/>" +
                    "<span style='color:#0d2b6e;font-size:12.5px;word-break:break-all;'>" +
                    HttpUtility.HtmlEncode(inviteLink) + "</span></p>" +
                    "<p style='color:#99a3b5;font-size:12px;margin-top:22px;'>" +
                    "If you weren't expecting this email, please contact management.</p>";

                return Send(toEmail, "Invitation to join IKHWEZI Driving School", body);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("SendStaffInvitation failed: " + ex.Message);
                return false;
            }
        }

        private static List<string> GetReceptionistEmails()
        {
            List<string> emails = new List<string>();
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand("SELECT StafEmailAddress FROM Admin_Staff WHERE Role IN ('Admin', 'Manager') AND StafEmailAddress IS NOT NULL AND StafEmailAddress <> ''", conn))
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            string email = r.GetString(0).Trim();
                            if (!emails.Contains(email)) emails.Add(email);
                        }
                    }
                }
            }

            catch (Exception ex) { System.Diagnostics.Trace.TraceError("GetReceptionistEmails: " + ex.Message); }

            if (emails.Count == 0)
            {
                string fallback = ConfigurationManager.AppSettings["ContactFormRecipient"];
                if (!string.IsNullOrEmpty(fallback)) emails.Add(fallback);
            }
            return emails;
        }
    }
}