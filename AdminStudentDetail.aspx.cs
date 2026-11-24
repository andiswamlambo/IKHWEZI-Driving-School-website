
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;

namespace WebApplicationM3
{
    public partial class AdminStudentDetail : Page
    {
        private string ConnectionString
        {
            get
            {
                return ConfigurationManager
                    .ConnectionStrings["KhweziConnection"]
                    .ConnectionString;
            }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            if (Session != null)
                ViewStateUserKey = Session.SessionID;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!StaffAuth.CheckAccess("Manager"))
                return;

            if (!IsPostBack)
                LoadStudent();
        }

        // DATABASE HELPERS

        private DataTable GetData(
            SqlConnection connection,
            string sql,
            SqlParameter parameter)
        {
            using (SqlCommand command =
                new SqlCommand(sql, connection))
            {
                command.CommandTimeout = 30;
                command.Parameters.Add(parameter);

                using (SqlDataAdapter adapter =
                    new SqlDataAdapter(command))
                {
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    return table;
                }
            }
        }

        private SqlParameter StudentParameter(int number)
        {
            return new SqlParameter(
                "@StudNumber", SqlDbType.Int)
            {
                Value = number
            };
        }

        private SqlParameter EmailParameter(string email)
        {
            return new SqlParameter(
                "@Email", SqlDbType.VarChar, 256)
            {
                Value = email
            };
        }

        private string Safe(object value)
        {
            if (value == null || value == DBNull.Value)
                return "Not provided";

            string text = Convert.ToString(value);

            return HttpUtility.HtmlEncode(
                string.IsNullOrWhiteSpace(text)
                    ? "Not provided"
                    : text);
        }

        private string TextValue(DataRow row, string column)
        {
            if (row[column] == DBNull.Value)
                return "";

            return Convert.ToString(row[column]);
        }

        private int ToInt(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0;

            return Convert.ToInt32(value);
        }

        private decimal ToMoney(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0m;

            return Convert.ToDecimal(value);
        }

        private string Money(decimal amount)
        {
            return "R" + amount.ToString("N2");
        }

        private string DateText(object value)
        {
            if (value == null || value == DBNull.Value)
                return "Not provided";

            return Convert.ToDateTime(value)
                .ToString("dd MMM yyyy");
        }

        // LOAD STUDENT PROFILE

        private void LoadStudent()
        {
            string email = Request.QueryString["email"];

            if (string.IsNullOrWhiteSpace(email))
            {
                ShowError("No student was selected.");
                return;
            }

            try
            {
                using (SqlConnection connection =
                    new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    string sql = @"
                        SELECT TOP 1
                            StudNumber,
                            StudEmailAddress,
                            Name,
                            Surname,
                            PhoneNum,
                            DateOfBirth,
                            StreetAddress,
                            Suburb,
                            City,
                            RegisterDate,
                            Status,
                            HasLearnersLicence
                        FROM dbo.Students
                        WHERE StudEmailAddress = @Email";

                    DataTable students = GetData(
                        connection,
                        sql,
                        EmailParameter(email));

                    if (students.Rows.Count == 0)
                    {
                        ShowError("Student not found.");
                        return;
                    }

                    DataRow student = students.Rows[0];

                    int studentNumber =
                        ToInt(student["StudNumber"]);

                    string studentEmail =
                        TextValue(student, "StudEmailAddress");

                    string firstName =
                        TextValue(student, "Name");

                    string surname =
                        TextValue(student, "Surname");

                    litName.Text = Safe(
                        (firstName + " " + surname).Trim());

                    litNumber.Text = Safe(studentNumber);
                    litEmail.Text = Safe(studentEmail);
                    litPhone.Text = Safe(student["PhoneNum"]);
                    litStatus.Text = Safe(student["Status"]);

                    litDOB.Text = Safe(
                        DateText(student["DateOfBirth"]));

                    litRegistered.Text = Safe(
                        DateText(student["RegisterDate"]));

                    StringBuilder address = new StringBuilder();

                    foreach (string part in new[]
                    {
                        TextValue(student, "StreetAddress"),
                        TextValue(student, "Suburb"),
                        TextValue(student, "City")
                    })
                    {
                        if (string.IsNullOrWhiteSpace(part))
                            continue;

                        if (address.Length > 0)
                            address.Append(", ");

                        address.Append(part);
                    }

                    litAddress.Text = Safe(address.ToString());

                    if (student["HasLearnersLicence"] ==
                        DBNull.Value)
                    {
                        litLearners.Text = "Not recorded";
                    }
                    else
                    {
                        litLearners.Text =
                            Convert.ToBoolean(
                                student["HasLearnersLicence"])
                            ? "Yes" : "No";
                    }

                    string initials = "";

                    if (!string.IsNullOrWhiteSpace(firstName))
                        initials += char.ToUpperInvariant(firstName[0]);

                    if (!string.IsNullOrWhiteSpace(surname))
                        initials += char.ToUpperInvariant(surname[0]);

                    litInitials.Text = Safe(initials);

                    hlEmailStudent.NavigateUrl =
                        "mailto:" + studentEmail;

                    LoadPhoto(connection, studentNumber);
                    LoadPackages(connection, studentNumber);
                    LoadFinancialSummary(connection, studentNumber);
                    LoadPackagePayments(connection, studentNumber);
                    LoadBookings(connection, studentEmail);
                    LoadBookingPayments(connection, studentEmail);

                    pnlDetails.Visible = true;
                    lblMessage.Visible = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "Student profile error: " + ex);

                pnlDetails.Visible = false;

                ShowError(
                    "We couldn't load this student's information. " +
                    "Please try again later.");
            }
        }

        private void ShowError(string message)
        {
            lblMessage.Text = Safe(message);
            lblMessage.Visible = true;
        }

        // STUDENT PROFILE PHOTO

        private void LoadPhoto(
            SqlConnection connection,
            int studentNumber)
        {
            imgStudentPhoto.Visible = false;
            pnlNoPhoto.Visible = true;

            try
            {
                string sql = @"
                    SELECT TOP 1 FilePath
                    FROM dbo.StudentDocuments
                    WHERE StudNumber = @StudNumber
                      AND DocumentType = 'Photo'
                      AND Status = 'Approved'
                    ORDER BY UploadedOn DESC";

                DataTable table = GetData(
                    connection,
                    sql,
                    StudentParameter(studentNumber));

                if (table.Rows.Count == 0 ||
                    table.Rows[0]["FilePath"] == DBNull.Value)
                    return;

                string storedPath =
                    Convert.ToString(table.Rows[0]["FilePath"]);

                if (string.IsNullOrWhiteSpace(storedPath))
                    return;

                string root = Path.GetFullPath(
                    Server.MapPath("~/Uploads/Documents/"));

                string fullPath = Path.GetFullPath(
                    Server.MapPath(storedPath));

                string prefix = root.TrimEnd(
                    Path.DirectorySeparatorChar) +
                    Path.DirectorySeparatorChar;

                if (!fullPath.StartsWith(
                    prefix,
                    StringComparison.OrdinalIgnoreCase))
                    return;

                if (!File.Exists(fullPath))
                    return;

                FileInfo file = new FileInfo(fullPath);

                if (file.Length <= 0 ||
                    file.Length > 2 * 1024 * 1024)
                    return;

                byte[] bytes = File.ReadAllBytes(fullPath);

                bool jpeg =
                    bytes.Length >= 3 &&
                    bytes[0] == 0xFF &&
                    bytes[1] == 0xD8 &&
                    bytes[2] == 0xFF;

                bool png =
                    bytes.Length >= 8 &&
                    bytes[0] == 0x89 &&
                    bytes[1] == 0x50 &&
                    bytes[2] == 0x4E &&
                    bytes[3] == 0x47 &&
                    bytes[4] == 0x0D &&
                    bytes[5] == 0x0A &&
                    bytes[6] == 0x1A &&
                    bytes[7] == 0x0A;

                if (!jpeg && !png)
                    return;

                string mime = jpeg
                    ? "image/jpeg"
                    : "image/png";

                imgStudentPhoto.ImageUrl =
                    "data:" + mime + ";base64," +
                    Convert.ToBase64String(bytes);

                imgStudentPhoto.Visible = true;
                pnlNoPhoto.Visible = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning(
                    "Student photo unavailable: " + ex);

                imgStudentPhoto.Visible = false;
                pnlNoPhoto.Visible = true;
            }
        }

        // LESSON PACKAGES

        private void LoadPackages(
            SqlConnection connection,
            int studentNumber)
        {
            string sql = @"
                SELECT
                    sp.StudentPackageID,
                    p.PackageName,
                    sp.TotalLessons,
                    sp.UsedLessons,
                    CASE
                        WHEN sp.TotalLessons - sp.UsedLessons < 0
                            THEN 0
                        ELSE sp.TotalLessons - sp.UsedLessons
                    END AS Remaining,
                    sp.IsActive,
                    sp.PurchasedOn
                FROM dbo.StudentPackage sp
                INNER JOIN dbo.Packages p
                    ON p.PackageID = sp.PackageID
                WHERE sp.StudNumber = @StudNumber
                ORDER BY sp.PurchasedOn DESC,
                         sp.StudentPackageID DESC";

            DataTable packages = GetData(
                connection,
                sql,
                StudentParameter(studentNumber));

            gvPackages.DataSource = packages;
            gvPackages.DataBind();

            int total = 0;
            int used = 0;
            int remaining = 0;

            foreach (DataRow row in packages.Rows)
            {
                total += ToInt(row["TotalLessons"]);
                used += ToInt(row["UsedLessons"]);
                remaining += ToInt(row["Remaining"]);
            }

            litTotalLessons.Text = total.ToString("N0");
            litUsedLessons.Text = used.ToString("N0");
            litRemainingLessons.Text = remaining.ToString("N0");

            int percentage = total > 0
                ? (int)Math.Round(
                    100.0 * Math.Min(Math.Max(used, 0), total)
                    / total)
                : 0;

            litProgressPercent.Text = percentage.ToString();

            litProgressText.Text =
                used.ToString("N0") + " of " +
                total.ToString("N0") + " lessons used";

            progressFill.Style["width"] =
                percentage.ToString() + "%";
        }

        // PACKAGE FINANCIAL SUMMARY

        private void LoadFinancialSummary(
            SqlConnection connection,
            int studentNumber)
        {
            // A reservation can exist without payment.
            // A paid reservation may contain only a deposit.
            // Missing balance records must not be treated
            // as proof that nothing is owed.

            string sql = @"
                WITH LatestBalance AS
                (
                    SELECT
                        b.*,
                        ROW_NUMBER() OVER
                        (
                            PARTITION BY b.ReservationID
                            ORDER BY b.BalanceID DESC
                        ) AS rn
                    FROM dbo.PackagePaymentBalance b
                ),
                LedgerTotals AS
                (
                    SELECT
                        ReservationID,
                        SUM(Amount) AS LedgerPaid
                    FROM dbo.PackagePaymentLedger
                    GROUP BY ReservationID
                )
                SELECT
                    r.ReservationID,
                    p.PackageName,
                    r.PackagePrice,
                    r.Status AS ReservationStatus,
                    r.ExpiresOn,

                    CASE
                        WHEN b.BalanceID IS NOT NULL
                            THEN b.TotalPaid

                        WHEN r.Status = 'Reserved'
                             AND ISNULL(l.LedgerPaid, 0) = 0
                            THEN CAST(0 AS decimal(18,2))

                        ELSE NULL
                    END AS TotalPaid,

                    CASE
                        WHEN b.BalanceID IS NOT NULL
                            THEN b.BalanceDue

                        WHEN r.Status = 'Reserved'
                             AND ISNULL(l.LedgerPaid, 0) = 0
                            THEN r.PackagePrice

                        ELSE NULL
                    END AS BalanceDue,

                    CASE
                        WHEN b.BalanceID IS NOT NULL
                            THEN b.PaymentStatus

                        WHEN r.Status = 'Reserved'
                             AND ISNULL(l.LedgerPaid, 0) = 0
                            THEN 'Awaiting Payment'

                        ELSE 'ReviewRequired'
                    END AS PaymentStatus,

                    b.FirstPaidOn,
                    b.DueDate,
                    ISNULL(l.LedgerPaid, 0) AS LedgerPaid,
                    b.BalanceID

                FROM dbo.PackageReservation r

                INNER JOIN dbo.Packages p
                    ON p.PackageID = r.PackageID

                LEFT JOIN LatestBalance b
                    ON b.ReservationID = r.ReservationID
                   AND b.rn = 1

                LEFT JOIN LedgerTotals l
                    ON l.ReservationID = r.ReservationID

                WHERE r.StudNumber = @StudNumber
                  AND r.Status <> 'Cancelled'

                ORDER BY
                    r.ReservedOn DESC,
                    r.ReservationID DESC";

            DataTable balances = GetData(
                connection,
                sql,
                StudentParameter(studentNumber));

            gvBalances.DataSource = balances;
            gvBalances.DataBind();

            decimal totalPaid = 0m;
            decimal totalOutstanding = 0m;

            int awaitingCount = 0;
            int expiredCount = 0;
            int reviewCount = 0;
            int missingCount = 0;
            int overdueCount = 0;
            int recordedPayments = 0;

            foreach (DataRow row in balances.Rows)
            {
                string status =
                    TextValue(row, "PaymentStatus");

                bool awaiting = status.Equals(
                    "Awaiting Payment",
                    StringComparison.OrdinalIgnoreCase);

                bool review = status.Equals(
                    "ReviewRequired",
                    StringComparison.OrdinalIgnoreCase) ||
                    status.Equals(
                        "Review Required",
                        StringComparison.OrdinalIgnoreCase);

                bool complete =
                    row["TotalPaid"] != DBNull.Value &&
                    row["BalanceDue"] != DBNull.Value;

                if (awaiting)
                {
                    awaitingCount++;

                    if (row["ExpiresOn"] != DBNull.Value &&
                        Convert.ToDateTime(
                            row["ExpiresOn"]) < DateTime.Now)
                    {
                        expiredCount++;
                    }
                }

                if (review)
                    reviewCount++;

                if (!complete)
                {
                    missingCount++;
                    continue;
                }

                decimal paid = ToMoney(row["TotalPaid"]);
                decimal due = ToMoney(row["BalanceDue"]);

                totalPaid += paid;
                totalOutstanding += due;

                if (paid > 0m)
                    recordedPayments++;

                if (row["DueDate"] != DBNull.Value &&
                    due > 0m &&
                    Convert.ToDateTime(
                        row["DueDate"]).Date < DateTime.Today)
                {
                    overdueCount++;
                }
            }

            // A student may have lesson allocations without
            // corresponding package reservation records.
            // Do not infer a payment or debt from lessons alone.

            bool hasLessons =
                gvPackages.Rows.Count > 0;

            bool noReservations =
                balances.Rows.Count == 0;

            bool reliable =
                !noReservations &&
                missingCount == 0;

            litTotalPaid.Text = reliable
                ? Money(totalPaid)
                : "Not available";

            litOutstanding.Text = reliable
                ? Money(totalOutstanding)
                : "Not available";

            pnlOutstanding.Visible =
                reliable && totalOutstanding > 0m;

            StringBuilder warning = new StringBuilder();

            if (noReservations && hasLessons)
            {
                warning.Append(
                    "This student has lesson allocations, but " +
                    "no matching active package reservation " +
                    "financial records were found. " +
                    "Please verify the payment history. ");
            }

            if (missingCount > 0)
            {
                warning.Append(
                    missingCount +
                    " package reservation(s) have incomplete " +
                    "financial information. ");
            }

            if (awaitingCount > 0)
            {
                warning.Append(
                    awaitingCount +
                    " package selection(s) are awaiting payment. ");
            }

            if (expiredCount > 0)
            {
                warning.Append(
                    expiredCount +
                    " unpaid reservation(s) have expired. ");
            }

            if (reviewCount > 0)
            {
                warning.Append(
                    reviewCount +
                    " package payment record(s) require review. ");
            }

            if (overdueCount > 0)
            {
                warning.Append(
                    overdueCount +
                    " package balance(s) are overdue. ");
            }

            if (recordedPayments > 0)
            {
                warning.Append(
                    "Recorded package payments are available " +
                    "in the payment ledger below.");
            }

            pnlPaymentReview.Visible =
                noReservations && hasLessons ||
                missingCount > 0 ||
                awaitingCount > 0 ||
                reviewCount > 0 ||
                overdueCount > 0;

            litReviewMessage.Text =
                Safe(warning.ToString().Trim());

            if (noReservations)
            {
                lblFinanceNote.Text =
                    "No active package reservation financial " +
                    "records are available. Existing lessons " +
                    "do not establish payment status.";
            }
            else if (missingCount > 0)
            {
                lblFinanceNote.Text =
                    "Some financial records are incomplete. " +
                    "Totals are unavailable until the records " +
                    "have been checked.";
            }
            else
            {
                lblFinanceNote.Text =
                    "Financial totals are calculated from " +
                    "the latest package balances and unpaid " +
                    "package selections. Cancelled reservations " +
                    "are excluded.";
            }
        }

        // PACKAGE PAYMENT LEDGER

        private void LoadPackagePayments(
            SqlConnection connection,
            int studentNumber)
        {
            string sql = @"
                SELECT TOP 100
                    l.LedgerID,
                    r.ReservationID,
                    p.PackageName,
                    l.Amount,
                    l.PaidOn,
                    l.ProviderPaymentID,
                    r.Status AS ReservationStatus

                FROM dbo.PackagePaymentLedger l

                INNER JOIN dbo.PackageReservation r
                    ON r.ReservationID = l.ReservationID

                INNER JOIN dbo.Packages p
                    ON p.PackageID = r.PackageID

                WHERE r.StudNumber = @StudNumber

                ORDER BY
                    l.PaidOn DESC,
                    l.LedgerID DESC";

            gvPackagePayments.DataSource = GetData(
                connection,
                sql,
                StudentParameter(studentNumber));

            gvPackagePayments.DataBind();
        }

        // BOOKING HISTORY

        private void LoadBookings(
            SqlConnection connection,
            string email)
        {
            string sql = @"
                SELECT TOP 30
                    BookingID,
                    ScheduledDateTime,
                    BookingStatus

                FROM dbo.Booking

                WHERE StudEmailAddress = @Email

                ORDER BY ScheduledDateTime DESC";

            gvBookings.DataSource = GetData(
                connection,
                sql,
                EmailParameter(email));

            gvBookings.DataBind();
        }

        // BOOKING-LINKED PAYMENT HISTORY

        private void LoadBookingPayments(
            SqlConnection connection,
            string email)
        {
            string sql = @"
                SELECT TOP 30
                    p.PaymentID,
                    p.BookingID,
                    p.AmountPaid,
                    p.PaymentStatus,
                    p.PaymentDate

                FROM dbo.Payment p

                INNER JOIN dbo.Booking b
                    ON b.BookingID = p.BookingID

                WHERE b.StudEmailAddress = @Email

                ORDER BY
                    p.PaymentDate DESC,
                    p.PaymentID DESC";

            gvPayments.DataSource = GetData(
                connection,
                sql,
                EmailParameter(email));

            gvPayments.DataBind();
        }
    }
}
