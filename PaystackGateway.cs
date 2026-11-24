using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace WebApplicationM3
{
    public static class PaystackGateway
    {
        private static string Cs
        {
            get { return ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString; }
        }

        private static string SecretKey
        {
            get { return ConfigurationManager.AppSettings["PaystackSecretKey"]; }
        }

        public static string Start(int reservationId, int studentNumber, decimal amount, string email, string description)
        {
            // 1. Generate our internal reference
            string reference = "IKHWEZI-PS-" + Guid.NewGuid().ToString("N").Substring(0, 12);

            // 2. Record the attempt in the database to lock out double payments
            using (var c = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(
                @"INSERT INTO dbo.PayFastAttempts (Reference, FlowKind, FlowID, StudNumber, Amount, Status)
                  VALUES (@Ref, 'Reservation', @FlowID, @Student, @Amount, 'Pending')", c))
            {
                cmd.Parameters.AddWithValue("@Ref", reference);
                cmd.Parameters.AddWithValue("@FlowID", reservationId);
                cmd.Parameters.AddWithValue("@Student", studentNumber);
                cmd.Parameters.AddWithValue("@Amount", amount);
                c.Open();
                cmd.ExecuteNonQuery();
            }

            // 3. Initialize transaction with Paystack API
            // Paystack expects amounts in cents (kobo), so R800.00 becomes 80000
            long amountInCents = (long)(amount * 100);

            var payload = new
            {
                email = email,
                amount = amountInCents,
                reference = reference,
                callback_url = ConfigurationManager.AppSettings["PaystackCallbackUrl"],
                metadata = new
                {
                    reservation_id = reservationId,
                    custom_fields = new[] {
                        new { display_name = "Description", variable_name = "description", value = description }
                    }
                }
            };

            var json = new JavaScriptSerializer().Serialize(payload);

            var request = (HttpWebRequest)WebRequest.Create("https://api.paystack.co/transaction/initialize");
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", "Bearer " + SecretKey);

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(json);
            }

            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var responseText = reader.ReadToEnd();
                    var result = new JavaScriptSerializer().Deserialize<dynamic>(responseText);

                    if (result["status"] == true)
                    {
                        // Return the URL to redirect the user to Paystack Checkout
                        return result["data"]["authorization_url"];
                    }
                    else
                    {
                        throw new Exception("Paystack error: " + result["message"]);
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    using (var reader = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        string errorText = reader.ReadToEnd();
                        throw new Exception("Paystack API error: " + errorText);
                    }
                }
                throw;
            }
        }
    }
}