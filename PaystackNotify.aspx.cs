using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace WebApplicationM3
{
    public partial class PaystackNotify : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string logPath = Server.MapPath("~/itn_log.txt");
            string logMessage = "\r\n--- PAYSTACK WEBHOOK " + DateTime.Now.ToString() + " ---\r\n";

            try
            {
                string json;
                using (StreamReader reader = new StreamReader(Request.InputStream, Encoding.UTF8))
                {
                    json = reader.ReadToEnd();
                }

                logMessage += "Raw JSON: " + json + "\r\n";

                string secretKey = ConfigurationManager.AppSettings["PaystackSecretKey"];
                string paystackSignature = Request.Headers["x-paystack-signature"];

                if (string.IsNullOrEmpty(paystackSignature))
                {
                    logMessage += "FAILED: No signature provided.\r\n";
                    File.AppendAllText(logPath, logMessage);
                    ReplySafe(400, "No signature provided");
                    return;
                }

                using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secretKey)))
                {
                    byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(json));
                    string computedSignature = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

                    if (computedSignature != paystackSignature.ToLowerInvariant())
                    {
                        logMessage += "FAILED: Signature mismatch.\r\n";
                        File.AppendAllText(logPath, logMessage);
                        ReplySafe(401, "Invalid signature");
                        return;
                    }
                }

                logMessage += "Signature verified successfully.\r\n";

                var serializer = new JavaScriptSerializer();
                var result = serializer.Deserialize<dynamic>(json);

                string eventType = result["event"];
                logMessage += "Event Type: " + eventType + "\r\n";

                if (eventType == "charge.success")
                {
                    string reference = result["data"]["reference"];
                    decimal amount = Convert.ToDecimal(result["data"]["amount"]) / 100m;
                    string providerId = "PAYSTACK-" + result["data"]["id"];

                    logMessage += "Applying payment to DB...\r\n";
                    string processResult = PayFastPackageProcessor.Apply(reference, providerId, amount);
                    logMessage += "DB Result: " + processResult + "\r\n";
                }

                File.AppendAllText(logPath, logMessage);
                ReplySafe(200, "OK");
            }
            catch (Exception ex)
            {
                logMessage += "FATAL ERROR: " + ex.ToString() + "\r\n";
                File.AppendAllText(logPath, logMessage);
                ReplySafe(500, "Error");
            }
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