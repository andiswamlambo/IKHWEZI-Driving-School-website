using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace WebApplicationM3
{
    public static class PayFastGateway
    {
        internal static string Cs
        {
            get { return ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString; }
        }

        public static bool Sandbox
        {
            get { return string.Equals(ConfigurationManager.AppSettings["PayFastSandbox"], "true", StringComparison.OrdinalIgnoreCase); }
        }

        public static string CheckoutUrl
        {
            get
            {
                return Sandbox
                    ? "https://sandbox.payfast.co.za/eng/process"
                    : "https://www.payfast.co.za/eng/process";
            }
        }

        private static string Setting(string name)
        {
            return (ConfigurationManager.AppSettings[name] ?? "").Trim();
        }

        private static string Encode(string value)
        {
            string encoded = HttpUtility.UrlEncode((value ?? "").Trim(), Encoding.UTF8);
            return Regex.Replace(encoded, "%[0-9a-fA-F]{2}", m => m.Value.ToUpperInvariant());
        }

        public static string Sign(IEnumerable<KeyValuePair<string, string>> fields, string passphrase)
        {
            var sb = new StringBuilder();

            foreach (var pair in fields)
            {
                if (string.IsNullOrEmpty(pair.Key) || pair.Key == "signature" || string.IsNullOrEmpty(pair.Value))
                    continue;

                if (sb.Length > 0) sb.Append("&");
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

        private static void ValidateConfiguration()
        {
            if (string.Equals(Setting("PayFastFakeMode"), "true", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Disable fake payments before checkout.");

            if (string.IsNullOrWhiteSpace(Setting("PayFastMerchantId")) || string.IsNullOrWhiteSpace(Setting("PayFastMerchantKey")))
                throw new InvalidOperationException("PayFast merchant credentials are missing.");

            string[] urlKeys = { "PayFastReturnUrl", "PayFastCancelUrl", "PayFastNotifyUrl" };
            foreach (string key in urlKeys)
            {
                Uri uri;
                if (!Uri.TryCreate(Setting(key), UriKind.Absolute, out uri) || uri.Scheme != Uri.UriSchemeHttps || string.IsNullOrWhiteSpace(uri.Host))
                    throw new InvalidOperationException("Invalid HTTPS configuration: " + key);
            }
        }

        public static string Start(string kind, int flowId, int studentNumber, decimal amount, string description)
        {
            ValidateConfiguration();

            if (kind != "Reservation" && kind != "Balance")
                throw new ArgumentException("Unsupported payment type.");

            if (flowId <= 0 || studentNumber <= 0 || amount <= 0m)
                throw new ArgumentException("Invalid payment details.");

            amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
            string reference = "IKHWEZI-" + Guid.NewGuid().ToString("N");

            using (var c = new SqlConnection(Cs))
            {
                c.Open();

                using (var tx = c.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        decimal price;
                        decimal minimum;
                        decimal alreadyPaid = 0m;
                        string balanceStatus = "";
                        string reservationStatus;
                        DateTime expires;

                        using (var cmd = new SqlCommand(@"
                            SELECT PackagePrice, DepositAmount, Status, ExpiresOn
                            FROM dbo.PackageReservation WITH (UPDLOCK, HOLDLOCK)
                            WHERE ReservationID = @R AND StudNumber = @S", c, tx))
                        {
                            cmd.Parameters.Add("@R", SqlDbType.Int).Value = flowId;
                            cmd.Parameters.Add("@S", SqlDbType.Int).Value = studentNumber;

                            using (var rd = cmd.ExecuteReader())
                            {
                                if (!rd.Read()) throw new InvalidOperationException("Reservation not found.");
                                price = Convert.ToDecimal(rd["PackagePrice"]);
                                minimum = Convert.ToDecimal(rd["DepositAmount"]);
                                reservationStatus = Convert.ToString(rd["Status"]);
                                expires = Convert.ToDateTime(rd["ExpiresOn"]);
                            }
                        }

                        if (price <= 0m) throw new InvalidOperationException("Invalid package price.");

                        using (var cmd = new SqlCommand(@"
                            SELECT TotalPaid, PaymentStatus
                            FROM dbo.PackagePaymentBalance WITH (UPDLOCK, HOLDLOCK)
                            WHERE ReservationID = @R", c, tx))
                        {
                            cmd.Parameters.Add("@R", SqlDbType.Int).Value = flowId;
                            using (var rd = cmd.ExecuteReader())
                            {
                                if (rd.Read())
                                {
                                    alreadyPaid = Convert.ToDecimal(rd["TotalPaid"]);
                                    balanceStatus = Convert.ToString(rd["PaymentStatus"]);
                                }
                            }
                        }

                        if (kind == "Reservation")
                        {
                            decimal half = decimal.Round(price * 0.50m, 2, MidpointRounding.AwayFromZero);
                            decimal required = Math.Max(half, minimum);

                            if (reservationStatus != "Reserved" || expires <= LessonProgress.NowSa() || alreadyPaid != 0m || amount < required || amount > price)
                                throw new InvalidOperationException("Initial payment must be at least 50% of the package price and the reservation must be active.");
                        }
                        else
                        {
                            decimal remaining = price - alreadyPaid;

                            if (balanceStatus == "ReviewRequired")
                                throw new InvalidOperationException("Reception must verify your historical payment balance first.");

                            if (reservationStatus != "Paid" || balanceStatus != "PartiallyPaid" || alreadyPaid <= 0m || remaining <= 0m || amount > remaining)
                                throw new InvalidOperationException("Invalid outstanding-balance payment.");

                            using (var cmd = new SqlCommand(@"
                                SELECT COUNT(*)
                                FROM dbo.ReservationPackageActivation a
                                INNER JOIN dbo.StudentPackage sp ON sp.StudentPackageID = a.StudentPackageID
                                INNER JOIN dbo.PackageReservation r ON r.ReservationID = a.ReservationID
                                WHERE a.ReservationID = @R AND r.StudNumber = @S AND sp.StudNumber = @S AND sp.PackageID = r.PackageID", c, tx))
                            {
                                cmd.Parameters.Add("@R", SqlDbType.Int).Value = flowId;
                                cmd.Parameters.Add("@S", SqlDbType.Int).Value = studentNumber;

                                if (Convert.ToInt32(cmd.ExecuteScalar()) != 1)
                                    throw new InvalidOperationException("Package activation needs review.");
                            }
                        }

                        using (var cmd = new SqlCommand(@"
                            UPDATE dbo.PayFastAttempts
                            SET Status = 'Failed', ErrorMessage = 'Checkout abandoned'
                            WHERE FlowID = @R
                              AND FlowKind IN ('Reservation', 'Balance')
                              AND Status IN ('Pending', 'Verified')
                              AND StartedUtc < DATEADD(MINUTE, -15, SYSUTCDATETIME())", c, tx))
                        {
                            cmd.Parameters.Add("@R", SqlDbType.Int).Value = flowId;
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd = new SqlCommand(@"
                            SELECT COUNT(*)
                            FROM dbo.PayFastAttempts WITH (UPDLOCK, HOLDLOCK)
                            WHERE FlowID = @R
                              AND FlowKind IN ('Reservation', 'Balance')
                              AND Status IN ('Pending', 'Verified', 'Review')", c, tx))
                        {
                            cmd.Parameters.Add("@R", SqlDbType.Int).Value = flowId;

                            if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                                throw new InvalidOperationException("Another payment is pending or requires review. Contact reception before starting another checkout.");
                        }

                        using (var cmd = new SqlCommand(@"
                            INSERT INTO dbo.PayFastAttempts (Reference, FlowKind, FlowID, StudNumber, Amount, Status)
                            VALUES (@Reference, @Kind, @FlowID, @Student, @Amount, 'Pending')", c, tx))
                        {
                            cmd.Parameters.Add("@Reference", SqlDbType.VarChar, 100).Value = reference;
                            cmd.Parameters.Add("@Kind", SqlDbType.VarChar, 30).Value = kind;
                            cmd.Parameters.Add("@FlowID", SqlDbType.Int).Value = flowId;
                            cmd.Parameters.Add("@Student", SqlDbType.Int).Value = studentNumber;

                            var p = cmd.Parameters.Add("@Amount", SqlDbType.Decimal);
                            p.Precision = 12; p.Scale = 2; p.Value = amount;

                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        try { tx.Rollback(); } catch { }
                        throw;
                    }
                }
            }

            var fields = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("merchant_id", Setting("PayFastMerchantId")),
                new KeyValuePair<string, string>("merchant_key", Setting("PayFastMerchantKey")),
                new KeyValuePair<string, string>("return_url", Setting("PayFastReturnUrl")),
                new KeyValuePair<string, string>("cancel_url", Setting("PayFastCancelUrl")),
                new KeyValuePair<string, string>("notify_url", Setting("PayFastNotifyUrl")),
                new KeyValuePair<string, string>("m_payment_id", reference),
                new KeyValuePair<string, string>("amount", amount.ToString("0.00", CultureInfo.InvariantCulture)),
                new KeyValuePair<string, string>("item_name", "IKHWEZI Driving School"),
                new KeyValuePair<string, string>("item_description", description ?? "Driving package")
            };

            string signature = Sign(fields, Setting("PayFastPassphrase"));

            var html = new StringBuilder();
            html.Append("<!DOCTYPE html><html><head><meta charset='utf-8' /><title>PayFast Checkout</title></head><body>");
            html.Append("<p>Redirecting to secure PayFast checkout...</p>");
            html.Append("<form id='payfastForm' method='post' action='");
            html.Append(HttpUtility.HtmlAttributeEncode(CheckoutUrl));
            html.Append("'>");

            foreach (var field in fields)
            {
                html.Append("<input type='hidden' name='");
                html.Append(HttpUtility.HtmlAttributeEncode(field.Key));
                html.Append("' value='");
                html.Append(HttpUtility.HtmlAttributeEncode(field.Value ?? ""));
                html.Append("' />");
            }

            html.Append("<input type='hidden' name='signature' value='");
            html.Append(HttpUtility.HtmlAttributeEncode(signature));
            html.Append("' />");
            html.Append("</form>");
            html.Append("<script>document.getElementById('payfastForm').submit();</script>");
            html.Append("<noscript><button type='submit' form='payfastForm'>Continue to PayFast</button></noscript>");
            html.Append("</body></html>");

            return html.ToString();
        }
    }
}