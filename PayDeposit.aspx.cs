
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace WebApplicationM3
{
    public partial class PayDeposit : System.Web.UI.Page
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

        private int CurrentStudent
        {
            get
            {
                int student;

                if (Session["StudNumber"] == null ||
                    !int.TryParse(
                        Convert.ToString(Session["StudNumber"]),
                        out student))
                {
                    return 0;
                }

                return student;
            }
        }

        private int ReservationId
        {
            get
            {
                int id;

                return int.TryParse(
                    Request.QueryString["id"], out id)
                    ? id : 0;
            }
        }

        private class ReservationInfo
        {
            public int ReservationID;
            public int PackageID;
            public int StudNumber;
            public string PackageName;
            public decimal PackagePrice;
            public decimal DepositAmount;
            public string Status;
            public DateTime ExpiresOn;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (CurrentStudent <= 0)
            {
                Response.Redirect(
                    "~/Account/Login.aspx?return=BookSession",
                    false);

                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                LoadPaymentPage();
            }
        }

        private ReservationInfo GetReservation()
        {
            if (ReservationId <= 0)
                return null;

            const string sql = @"
                SELECT
                    r.ReservationID,
                    r.StudNumber,
                    r.PackageID,
                    r.PackagePrice,
                    r.DepositAmount,
                    r.Status,
                    r.ExpiresOn,
                    p.PackageName
                FROM dbo.PackageReservation r
                INNER JOIN dbo.Packages p
                    ON p.PackageID = r.PackageID
                WHERE r.ReservationID = @Reservation
                  AND r.StudNumber = @Student";

            using (SqlConnection c = new SqlConnection(Cs))
            using (SqlCommand cmd = new SqlCommand(sql, c))
            {
                cmd.Parameters.Add(
                    "@Reservation", SqlDbType.Int)
                    .Value = ReservationId;

                cmd.Parameters.Add(
                    "@Student", SqlDbType.Int)
                    .Value = CurrentStudent;

                c.Open();

                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    if (!rd.Read())
                        return null;

                    return new ReservationInfo
                    {
                        ReservationID = Convert.ToInt32(
                            rd["ReservationID"]),

                        StudNumber = Convert.ToInt32(
                            rd["StudNumber"]),

                        PackageID = Convert.ToInt32(
                            rd["PackageID"]),

                        PackageName = Convert.ToString(
                            rd["PackageName"]),

                        PackagePrice = Convert.ToDecimal(
                            rd["PackagePrice"]),

                        DepositAmount = Convert.ToDecimal(
                            rd["DepositAmount"]),

                        Status = Convert.ToString(
                            rd["Status"]),

                        ExpiresOn = Convert.ToDateTime(
                            rd["ExpiresOn"])
                    };
                }
            }
        }

        // Minimum payment is 50% or the reservation's
        // required deposit, whichever is greater.
        private decimal GetMinimumPayment(ReservationInfo r)
        {
            decimal half = decimal.Round(
                r.PackagePrice * 0.50m,
                2,
                MidpointRounding.AwayFromZero);

            return Math.Max(half, r.DepositAmount);
        }

        private void LoadPaymentPage()
        {
            try
            {
                ReservationInfo r = GetReservation();

                if (r == null)
                {
                    ShowMessage(
                        "Reservation not found.", "err");
                    return;
                }

                if (r.Status == "Paid")
                {
                    ShowPaid();
                    return;
                }

                if (r.Status != "Reserved" ||
                    r.ExpiresOn <= LessonProgress.NowSa())
                {
                    ShowMessage(
                        "Your reservation has expired or is no longer active.",
                        "warn");
                    return;
                }

                if (r.PackagePrice <= 0 ||
                    r.DepositAmount <= 0 ||
                    r.DepositAmount > r.PackagePrice)
                {
                    ShowMessage(
                        "Invalid package payment details.",
                        "err");
                    return;
                }

                decimal minimum = GetMinimumPayment(r);

                litRef.Text =
                    r.ReservationID.ToString();

                litPackage.Text =
                    Server.HtmlEncode(r.PackageName);

                litTotal.Text =
                    r.PackagePrice.ToString(
                        "N2", CultureInfo.InvariantCulture);

                litDeposit.Text =
                    minimum.ToString(
                        "N2", CultureInfo.InvariantCulture);

                litAmountLabel.Text =
                    "Minimum Initial Payment";

                int days = Math.Max(
                    0,
                    (int)Math.Ceiling(
                        (r.ExpiresOn - LessonProgress.NowSa())
                        .TotalDays));

                litDaysLeft.Text = days.ToString();

                litExpires.Text =
                    r.ExpiresOn.ToString("dd MMM yyyy HH:mm");

                // Values for the client-side preview.
                // These are display values only.
                // Server validation always uses the database.
                hfPackagePrice.Value =
                    r.PackagePrice.ToString(
                        "0.00", CultureInfo.InvariantCulture);

                hfMinimumPayment.Value =
                    minimum.ToString(
                        "0.00", CultureInfo.InvariantCulture);

                // Default to the minimum permitted amount.
                txtPaymentAmount.Text =
                    minimum.ToString(
                        "0.00", CultureInfo.InvariantCulture);

                txtPaymentAmount.Attributes["min"] =
                    minimum.ToString(
                        "0.00", CultureInfo.InvariantCulture);

                txtPaymentAmount.Attributes["max"] =
                    r.PackagePrice.ToString(
                        "0.00", CultureInfo.InvariantCulture);

                pnlPay.Visible = true;
                pnlTimer.Visible = true;
                pnlPaid.Visible = false;
                pnlConfirmCancel.Visible = false;
                btnCancel.Visible = true;
                lblMsg.Visible = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "PayDeposit.Load: " + ex);

                ShowMessage(
                    "Unable to load payment details.",
                    "err");
            }
        }

        private bool TryCalculatePayment(
            ReservationInfo r,
            out decimal amount)
        {
            amount = 0m;

            string enteredAmount =
                (txtPaymentAmount.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(enteredAmount))
            {
                ShowPaymentError(
                    "Please enter the amount you want to pay.");
                return false;
            }

            // Accept normal decimal rand amounts such as:
            // 2000
            // 2000.50
            // 2750.00
            // Reject currency symbols, commas and invalid text.
            decimal parsedAmount;

            if (!decimal.TryParse(
                enteredAmount,
                NumberStyles.AllowDecimalPoint |
                NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture,
                out parsedAmount))
            {
                ShowPaymentError(
                    "Please enter a valid amount in rands, " +
                    "for example 2000.00.");
                return false;
            }

            if (parsedAmount <= 0m)
            {
                ShowPaymentError(
                    "The payment amount must be greater than zero.");
                return false;
            }

            // Do not silently round a student's payment.
            // Only amounts with at most two decimal places
            // are accepted.
            if (decimal.Round(
                    parsedAmount,
                    2,
                    MidpointRounding.AwayFromZero)
                != parsedAmount)
            {
                ShowPaymentError(
                    "Please enter an amount with no more " +
                    "than two decimal places.");
                return false;
            }

            decimal minimum = GetMinimumPayment(r);

            if (parsedAmount < minimum)
            {
                ShowPaymentError(
                    "The minimum initial payment for this " +
                    "package is R " +
                    minimum.ToString("N2") +
                    ". Please enter this amount or more.");
                return false;
            }

            if (parsedAmount > r.PackagePrice)
            {
                ShowPaymentError(
                    "The payment amount cannot exceed " +
                    "the package price of R " +
                    r.PackagePrice.ToString("N2") + ".");
                return false;
            }

            amount = parsedAmount;
            return true;
        }

        // Keep the payment page visible when an amount
        // is rejected, so the student can correct it.
        private void ShowPaymentError(string message)
        {
            lblMsg.Text = Server.HtmlEncode(message);
            lblMsg.CssClass = "msg err";
            lblMsg.Visible = true;

            pnlPay.Visible = true;
            pnlTimer.Visible = true;
            pnlPaid.Visible = false;
        }

        protected void btnPay_Click(object sender, EventArgs e)
        {
            if (CurrentStudent <= 0)
            {
                ShowMessage(
                    "Please sign in again.", "err");
                return;
            }

            try
            {
                ReservationInfo r = GetReservation();

                if (r == null)
                {
                    ShowMessage(
                        "Reservation not found.", "err");
                    return;
                }

                if (r.Status != "Reserved" ||
                    r.ExpiresOn <= LessonProgress.NowSa())
                {
                    ShowMessage(
                        "This reservation is no longer eligible for payment.",
                        "warn");
                    return;
                }

                if (r.PackagePrice <= 0 ||
                    r.DepositAmount <= 0 ||
                    r.DepositAmount > r.PackagePrice)
                {
                    ShowMessage(
                        "Invalid package payment details.",
                        "err");
                    return;
                }

                decimal amount;

                if (!TryCalculatePayment(r, out amount))
                    return;

                bool useSimulator = string.Equals(
                    ConfigurationManager.AppSettings[
                        "UseSimulatedGateway"],
                    "true",
                    StringComparison.OrdinalIgnoreCase);

                bool usePaystack = string.Equals(
                    ConfigurationManager.AppSettings[
                        "UsePaystack"],
                    "true",
                    StringComparison.OrdinalIgnoreCase);

                if (useSimulator)
                {
                    // 1. SIMULATOR FLOW
                    string fakeRef =
                        "IKHWEZI-FAKE-" +
                        Guid.NewGuid().ToString("N");

                    using (SqlConnection c =
                        new SqlConnection(Cs))
                    using (SqlCommand cmd =
                        new SqlCommand(
                            @"INSERT INTO dbo.PayFastAttempts
                              (Reference, FlowKind, FlowID,
                               StudNumber, Amount, Status)
                              VALUES
                              (@Ref, 'Reservation', @FlowID,
                               @Student, @Amount, 'Pending')",
                            c))
                    {
                        cmd.Parameters.AddWithValue(
                            "@Ref", fakeRef);

                        cmd.Parameters.AddWithValue(
                            "@FlowID", r.ReservationID);

                        cmd.Parameters.AddWithValue(
                            "@Student", CurrentStudent);

                        cmd.Parameters.AddWithValue(
                            "@Amount", amount);

                        c.Open();
                        cmd.ExecuteNonQuery();
                    }

                    Response.Redirect(
                        "~/SandboxGateway.aspx?ref=" +
                        Uri.EscapeDataString(fakeRef) +
                        "&amount=" +
                        amount.ToString(
                            "0.00",
                            CultureInfo.InvariantCulture),
                        false);

                    Context.ApplicationInstance
                        .CompleteRequest();
                    return;
                }
                else if (usePaystack)
                {
                    // 2. PAYSTACK FLOW
                    string email =
                        Session["StudEmail"] != null
                        ? Session["StudEmail"].ToString()
                        : "student@ikhwezi.com";

                    string paystackUrl =
                        PaystackGateway.Start(
                            r.ReservationID,
                            CurrentStudent,
                            amount,
                            email,
                            "IKHWEZI Driving Package: " +
                            r.PackageName);

                    Response.Redirect(
                        paystackUrl, false);

                    Context.ApplicationInstance
                        .CompleteRequest();
                    return;
                }
                else
                {
                    // 3. PAYFAST FLOW
                    string redirectHtml =
                        PayFastGateway.Start(
                            "Reservation",
                            r.ReservationID,
                            CurrentStudent,
                            amount,
                            "IKHWEZI Driving Package: " +
                            r.PackageName);

                    Response.Clear();

                    Response.ContentType = "text/html";

                    Response.Cache.SetCacheability(
                        System.Web.HttpCacheability.NoCache);

                    Response.Cache.SetNoStore();

                    Response.Write(redirectHtml);

                    Context.ApplicationInstance
                        .CompleteRequest();
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "PayDeposit.Pay: " + ex);

                ShowMessage(
                    "Unable to start checkout. " +
                    ex.Message,
                    "err");
            }
        }

        protected void btnCancel_Click(
            object sender, EventArgs e)
        {
            if (GetReservation() == null)
            {
                ShowMessage(
                    "Reservation not found.", "err");
                return;
            }

            pnlConfirmCancel.Visible = true;
            btnCancel.Visible = false;
        }

        protected void btnCancelConfirmYes_Click(
            object sender, EventArgs e)
        {
            try
            {
                ReservationInfo r = GetReservation();

                if (r == null ||
                    r.Status != "Reserved")
                {
                    ShowMessage(
                        "This reservation cannot be cancelled.",
                        "warn");
                    return;
                }

                // Prevent cancellation while an
                // unresolved checkout is in progress.
                using (SqlConnection c =
                    new SqlConnection(Cs))
                {
                    c.Open();

                    using (SqlTransaction tx =
                        c.BeginTransaction(
                            IsolationLevel.Serializable))
                    {
                        using (SqlCommand cmd =
                            new SqlCommand(@"
                                UPDATE dbo.PackageReservation
                                SET Status = 'Cancelled',
                                    CancelledOn = @Now,
                                    Notes = 'Cancelled by student'
                                WHERE ReservationID = @R
                                  AND StudNumber = @S
                                  AND Status = 'Reserved'
                                  AND NOT EXISTS
                                  (
                                      SELECT 1
                                      FROM dbo.PayFastAttempts
                                          WITH (UPDLOCK, HOLDLOCK)
                                      WHERE FlowKind = 'Reservation'
                                        AND FlowID = @R
                                        AND Status IN
                                            ('Pending', 'Verified')
                                  )", c, tx))
                        {
                            cmd.Parameters.Add(
                                "@R", SqlDbType.Int)
                                .Value = r.ReservationID;

                            cmd.Parameters.Add(
                                "@S", SqlDbType.Int)
                                .Value = CurrentStudent;

                            cmd.Parameters.Add(
                                "@Now", SqlDbType.DateTime)
                                .Value = LessonProgress.NowSa();

                            if (cmd.ExecuteNonQuery() != 1)
                            {
                                tx.Rollback();

                                ShowMessage(
                                    "A payment may still be processing. " +
                                    "Please contact reception before cancelling.",
                                    "warn");
                                return;
                            }
                        }

                        tx.Commit();
                    }
                }

                Response.Redirect(
                    "~/StudentDashboard.aspx", false);

                Context.ApplicationInstance
                    .CompleteRequest();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "PayDeposit.Cancel: " + ex);

                ShowMessage(
                    "Unable to cancel the reservation.",
                    "err");
            }
        }

        protected void btnCancelConfirmNo_Click(
            object sender, EventArgs e)
        {
            pnlConfirmCancel.Visible = false;
            btnCancel.Visible = true;
        }

        protected void btnContinue_Click(
            object sender, EventArgs e)
        {
            Response.Redirect(
                "~/StudentDashboard.aspx", false);

            Context.ApplicationInstance
                .CompleteRequest();
        }

        private void ShowPaid()
        {
            pnlPay.Visible = false;
            pnlPaid.Visible = true;
            lblMsg.Visible = false;
        }

        private void ShowMessage(
            string message, string type)
        {
            lblMsg.Text =
                Server.HtmlEncode(message);

            lblMsg.CssClass =
                "msg " + type;

            lblMsg.Visible = true;

            pnlPay.Visible = false;
            pnlPaid.Visible = false;
        }
    }
}
