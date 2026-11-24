
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;

namespace WebApplicationM3
{
    public partial class PaymentSuccess : System.Web.UI.Page
    {
        private string Cs
        {
            get
            {
                return ConfigurationManager
                    .ConnectionStrings["KhweziConnection"]
                    .ConnectionString;
            }
        }

        private class ReceiptInfo
        {
            public int ReservationID;
            public string PackageName;
            public decimal PackagePrice;
            public decimal AmountPaid;
            public decimal TotalPaid;
            public decimal BalanceDue;
            public string Reference;
            public DateTime PaymentDate;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
                return;

            pnlSuccess.Visible = false;
            pnlProcessing.Visible = false;
            pnlError.Visible = false;

            string reference =
                Request.QueryString["reference"];

            if (string.IsNullOrWhiteSpace(reference))
            {
                reference = Request.QueryString["trxref"];
            }

            if (string.IsNullOrWhiteSpace(reference))
            {
                ShowError(
                    "No payment reference was provided. " +
                    "Please check your payment status " +
                    "from your student dashboard.");
                return;
            }

            // Only handle references created by
            // IKHWEZI's Paystack integration.
            if (!reference.StartsWith(
                "IKHWEZI-PS-",
                StringComparison.Ordinal))
            {
                ShowError(
                    "The payment reference is not valid.");
                return;
            }

            try
            {
                // Verify that this reference belongs
                // to the signed-in student.
                if (!StudentOwnsAttempt(reference))
                {
                    ShowError(
                        "This payment could not be found " +
                        "for your student account.");
                    return;
                }

                string secretKey =
                    ConfigurationManager.AppSettings[
                        "PaystackSecretKey"];

                if (string.IsNullOrWhiteSpace(secretKey))
                {
                    throw new InvalidOperationException(
                        "Paystack configuration is missing.");
                }

                // Verify directly with Paystack.
                string verifyUrl =
                    "https://api.paystack.co/transaction/verify/" +
                    Uri.EscapeDataString(reference);

                var request =
                    (HttpWebRequest)WebRequest.Create(verifyUrl);

                request.Method = "GET";
                request.Headers.Add(
                    "Authorization",
                    "Bearer " + secretKey);

                request.Timeout = 10000;
                request.ReadWriteTimeout = 10000;

                string responseText;

                using (var response =
                    (HttpWebResponse)request.GetResponse())
                using (var reader =
                    new StreamReader(
                        response.GetResponseStream()))
                {
                    responseText = reader.ReadToEnd();
                }

                var serializer = new JavaScriptSerializer();

                var result =
                    serializer.Deserialize<dynamic>(
                        responseText);

                if (result == null ||
                    result["status"] != true ||
                    result["data"] == null)
                {
                    ShowProcessing(
                        "We could not confirm your payment " +
                        "with Paystack at this time.");
                    return;
                }

                var data = result["data"];

                string paymentStatus =
                    Convert.ToString(data["status"]);

                if (!string.Equals(
                    paymentStatus,
                    "success",
                    StringComparison.OrdinalIgnoreCase))
                {
                    ShowProcessing(
                        "Paystack has not confirmed a " +
                        "successful payment for this reference.");
                    return;
                }

                // Confirm the transaction belongs to
                // the requested reference.
                string verifiedReference =
                    Convert.ToString(data["reference"]);

                if (!string.Equals(
                    verifiedReference,
                    reference,
                    StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "Payment reference mismatch.");
                }

                // IKHWEZI's package prices are in ZAR.
                string currency =
                    Convert.ToString(data["currency"]);

                if (!string.Equals(
                    currency,
                    "ZAR",
                    StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "Unexpected payment currency.");
                }

                // Paystack returns the amount in cents.
                long cents =
                    Convert.ToInt64(data["amount"]);

                if (cents <= 0)
                {
                    throw new InvalidOperationException(
                        "Invalid verified payment amount.");
                }

                decimal amount = cents / 100m;

                string providerId =
                    "PAYSTACK-" +
                    Convert.ToString(data["id"]);

                // Existing transaction-safe processor:
                // - checks expected amount
                // - prevents duplicate applications
                // - records the payment
                // - activates the package
                // - updates the balance
                string applyResult =
                    PayFastPackageProcessor.Apply(
                        reference,
                        providerId,
                        amount);

                if (applyResult !=
                        "Payment successfully applied" &&
                    applyResult != "Already processed")
                {
                    ShowProcessing(
                        "Your payment was verified, " +
                        "but its package allocation " +
                        "requires additional review.");
                    return;
                }

                // Read the receipt from the database
                // after the payment has been applied.
                ReceiptInfo receipt =
                    GetReceipt(reference);

                if (receipt == null)
                {
                    ShowProcessing(
                        "Your payment was verified, " +
                        "but the receipt is not yet available.");
                    return;
                }

                ShowReceipt(receipt);
            }
            catch (WebException ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "Paystack verification error: " + ex);

                ShowProcessing(
                    "We are unable to confirm your " +
                    "payment with Paystack right now. " +
                    "Please check your payment status later.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "PaymentSuccess error: " + ex);

                ShowProcessing(
                    "We could not finish confirming " +
                    "your payment. Please check your " +
                    "payment status before trying again.");
            }
        }

        private bool StudentOwnsAttempt(string reference)
        {
            int student;

            if (Session["StudNumber"] == null ||
                !int.TryParse(
                    Convert.ToString(
                        Session["StudNumber"]),
                    out student))
            {
                return false;
            }

            using (SqlConnection c =
                new SqlConnection(Cs))
            using (SqlCommand cmd =
                new SqlCommand(@"
                    SELECT COUNT(*)
                    FROM dbo.PayFastAttempts
                    WHERE Reference = @Reference
                      AND StudNumber = @Student",
                    c))
            {
                cmd.Parameters.Add(
                    "@Reference",
                    SqlDbType.VarChar, 100)
                    .Value = reference;

                cmd.Parameters.Add(
                    "@Student",
                    SqlDbType.Int)
                    .Value = student;

                c.Open();

                return Convert.ToInt32(
                    cmd.ExecuteScalar()) == 1;
            }
        }

        private ReceiptInfo GetReceipt(string reference)
        {
            int student;

            if (Session["StudNumber"] == null ||
                !int.TryParse(
                    Convert.ToString(
                        Session["StudNumber"]),
                    out student))
            {
                return null;
            }

            const string sql = @"
                SELECT
                    r.ReservationID,
                    p.PackageName,
                    r.PackagePrice,
                    a.Amount AS AmountPaid,
                    b.TotalPaid,
                    a.Reference,
                    l.PaidOn
                FROM dbo.PayFastAttempts a
                INNER JOIN dbo.PackageReservation r
                    ON r.ReservationID = a.FlowID
                   AND r.StudNumber = a.StudNumber
                INNER JOIN dbo.Packages p
                    ON p.PackageID = r.PackageID
                INNER JOIN dbo.PackagePaymentBalance b
                    ON b.ReservationID = r.ReservationID
                INNER JOIN dbo.PackagePaymentLedger l
                    ON l.AttemptID = a.AttemptID
                   AND l.ReservationID = r.ReservationID
                WHERE a.Reference = @Reference
                  AND a.StudNumber = @Student
                  AND a.FlowKind IN ('Reservation', 'Balance')
                  AND a.Status = 'Applied'";

            using (SqlConnection c =
                new SqlConnection(Cs))
            using (SqlCommand cmd =
                new SqlCommand(sql, c))
            {
                cmd.Parameters.Add(
                    "@Reference",
                    SqlDbType.VarChar, 100)
                    .Value = reference;

                cmd.Parameters.Add(
                    "@Student",
                    SqlDbType.Int)
                    .Value = student;

                c.Open();

                using (SqlDataReader rd =
                    cmd.ExecuteReader())
                {
                    if (!rd.Read())
                        return null;

                    decimal price =
                        Convert.ToDecimal(
                            rd["PackagePrice"]);

                    decimal totalPaid =
                        Convert.ToDecimal(
                            rd["TotalPaid"]);

                    return new ReceiptInfo
                    {
                        ReservationID =
                            Convert.ToInt32(
                                rd["ReservationID"]),

                        PackageName =
                            Convert.ToString(
                                rd["PackageName"]),

                        PackagePrice = price,

                        AmountPaid =
                            Convert.ToDecimal(
                                rd["AmountPaid"]),

                        TotalPaid = totalPaid,

                        BalanceDue =
                            Math.Max(
                                0m,
                                price - totalPaid),

                        Reference =
                            Convert.ToString(
                                rd["Reference"]),

                        PaymentDate =
                            Convert.ToDateTime(
                                rd["PaidOn"])
                    };
                }
            }
        }

        private void ShowReceipt(ReceiptInfo r)
        {
            pnlSuccess.Visible = true;
            pnlProcessing.Visible = false;
            pnlError.Visible = false;

            litReservation.Text =
                r.ReservationID.ToString();

            litPackage.Text =
                Server.HtmlEncode(r.PackageName);

            litPackagePrice.Text =
                FormatAmount(r.PackagePrice);

            litAmountPaid.Text =
                FormatAmount(r.AmountPaid);

            litTotalPaid.Text =
                FormatAmount(r.TotalPaid);

            litBalance.Text =
                FormatAmount(r.BalanceDue);

            litReference.Text =
                Server.HtmlEncode(r.Reference);

            litPaymentDate.Text =
                r.PaymentDate.ToString(
                    "dd MMM yyyy HH:mm");
        }

        private string FormatAmount(decimal amount)
        {
            return amount.ToString(
                "N2",
                CultureInfo.GetCultureInfo("en-ZA"));
        }

        private void ShowProcessing(string message)
        {
            pnlSuccess.Visible = false;
            pnlProcessing.Visible = true;
            pnlError.Visible = false;

            litProcessingInfo.Text =
                "<p class='subtitle' " +
                "style='margin-top:15px;'>" +
                Server.HtmlEncode(message) +
                "</p>";
        }

        private void ShowError(string message)
        {
            pnlSuccess.Visible = false;
            pnlProcessing.Visible = false;
            pnlError.Visible = true;

            litError.Text =
                Server.HtmlEncode(message);
        }
    }
}
