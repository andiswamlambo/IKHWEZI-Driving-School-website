using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;

namespace WebApplicationM3
{
    public partial class PayFastNotify : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                Response.ContentType = "text/plain";
                Response.TrySkipIisCustomErrors = true;

                if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
                { ReplySafe(405, "POST required"); return; }

                string raw;
                using (StreamReader reader = new StreamReader(Request.InputStream, Encoding.UTF8))
                { raw = reader.ReadToEnd(); }

                // Handle both application/x-www-form-urlencoded and multipart/form-data
                NameValueCollection form;
                if (Request.ContentType != null && Request.ContentType.Contains("multipart/form-data"))
                {
                    form = Request.Form;
                }
                else
                {
                    form = HttpUtility.ParseQueryString(raw);
                }

                string merchantId = ConfigurationManager.AppSettings["PayFastMerchantId"] ?? "";
                if (!string.Equals(form["merchant_id"], merchantId, StringComparison.Ordinal))
                { ReplySafe(400, "Merchant mismatch"); return; }

                List<KeyValuePair<string, string>> fields = new List<KeyValuePair<string, string>>();
                foreach (string key in form.AllKeys)
                {
                    if (string.Equals(key, "signature", StringComparison.OrdinalIgnoreCase)) continue;
                    fields.Add(new KeyValuePair<string, string>(key, form[key]));
                }

                string passphrase = ConfigurationManager.AppSettings["PayFastPassphrase"] ?? "";

                // Uses the special signature method that includes empty values
                string expectedSignature = SignWithEmptyValues(fields, passphrase);
                string receivedSignature = (form["signature"] ?? "").ToLowerInvariant();

                if (!FixedEquals(expectedSignature, receivedSignature))
                {
                    System.Diagnostics.Trace.TraceWarning("PayFast ITN: Signature mismatch.");
                    ReplySafe(400, "Invalid signature"); return;
                }

                System.Diagnostics.Trace.TraceInformation("PayFast ITN: Signature verified.");

                string paymentStatus = form["payment_status"];

                if (!string.Equals(paymentStatus, "COMPLETE", StringComparison.OrdinalIgnoreCase))
                {
                    string reference = form["m_payment_id"];
                    string providerId = form["pf_payment_id"];
                    MarkAttemptFailed(reference, providerId, paymentStatus);
                    ReplySafe(200, "Acknowledged");
                    return;
                }

                decimal amount;
                if (!decimal.TryParse(form["amount_gross"], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out amount) || amount <= 0m)
                { ReplySafe(400, "Invalid payment amount"); return; }

                string refId = form["m_payment_id"];
                string provId = form["pf_payment_id"];

                string result = PayFastPackageProcessor.Apply(refId, provId, amount);

                System.Diagnostics.Trace.TraceInformation("PayFast ITN Processing Result: " + result);

                if (result == "Payment successfully applied") { ReplySafe(200, "Payment applied"); return; }
                if (result == "Already processed") { ReplySafe(200, "Already processed"); return; }
                if (result == "Payment requires manual review") { ReplySafe(200, "Payment held for review"); return; }
                if (result == "Already awaiting manual review") { ReplySafe(200, "Already awaiting review"); return; }

                ReplySafe(500, "Unknown processing result");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("PayFast ITN FATAL ERROR: " + ex.ToString());
                ReplySafe(500, "Payment processing failed");
            }
        }

        private static string SignWithEmptyValues(IEnumerable<KeyValuePair<string, string>> fields, string passphrase)
        {
            var sb = new StringBuilder();

            foreach (var pair in fields)
            {
                if (string.IsNullOrEmpty(pair.Key) || pair.Key == "signature")
                    continue;

                if (sb.Length > 0) sb.Append("&");

                // PayFast expects empty values to be included in the signature string
                sb.Append(pair.Key).Append("=").Append(Encode(pair.Value));
            }

            if (!string.IsNullOrWhiteSpace(passphrase))
            {
                if (sb.Length > 0) sb.Append("&");
                sb.Append("passphrase=").Append(Encode(passphrase));
            }

            using (var md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        private static string Encode(string value)
        {
            string encoded = HttpUtility.UrlEncode((value ?? "").Trim(), Encoding.UTF8);
            return System.Text.RegularExpressions.Regex.Replace(encoded, "%[0-9a-fA-F]{2}", m => m.Value.ToUpperInvariant());
        }

        private void MarkAttemptFailed(string reference, string providerId, string reason)
        {
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                using (var c = new SqlConnection(cs))
                using (var cmd = new SqlCommand(
                    @"UPDATE dbo.PayFastAttempts
                      SET Status = 'Failed',
                          ProviderPaymentID = COALESCE(ProviderPaymentID, @Provider),
                          ErrorMessage = @Reason
                      WHERE Reference = @Ref
                        AND Status IN ('Pending','Verified')", c))
                {
                    cmd.Parameters.AddWithValue("@Provider", (object)providerId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Reason", "PayFast status: " + reason);
                    cmd.Parameters.AddWithValue("@Ref", reference);
                    c.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch { }
        }

        private static bool FixedEquals(string a, string b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int difference = 0;
            for (int i = 0; i < a.Length; i++) difference |= a[i] ^ b[i];
            return difference == 0;
        }

        private void ReplySafe(int status, string message)
        {
            Response.StatusCode = status;
            Response.Write(message);
            Response.Flush();
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}