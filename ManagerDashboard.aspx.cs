
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApplicationM3
{
    public partial class ManagerDashboard : Page
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
            {
                LoadTopBar();
                LoadCards();

                string filter = Request.QueryString["filter"];

                if (!string.IsNullOrEmpty(filter) &&
                    ddlFilter.Items.FindByValue(filter) != null)
                {
                    ddlFilter.SelectedValue = filter;
                }

                LoadBookings();
                LoadPaymentAlerts();
            }
        }

        private void LoadTopBar()
        {
            string name = StaffAuth.Name;

            if (string.IsNullOrWhiteSpace(name))
                name = "Manager";

            string initials = "";

            foreach (string part in name.Split(
                new[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries))
            {
                initials += char.ToUpperInvariant(part[0]);
            }

            litInitials.Text = initials.Length > 2
                ? initials.Substring(0, 2)
                : initials;
        }

        private int Count(SqlConnection conn, string sql)
        {
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private DataTable GetTable(
            SqlConnection conn,
            string sql)
        {
            using (SqlDataAdapter adapter =
                new SqlDataAdapter(sql, conn))
            {
                DataTable table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        private void LoadCards()
        {
            try
            {
                using (SqlConnection conn =
                    new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(@"
                        SELECT ISNULL(SUM(AmountPaid), 0)
                        FROM dbo.Payment
                        WHERE PaymentStatus = 'Paid'
                          AND PaymentDate >=
                              DATEFROMPARTS(
                                  YEAR(GETDATE()),
                                  MONTH(GETDATE()), 1)
                          AND PaymentDate <
                              DATEADD(MONTH, 1,
                                  DATEFROMPARTS(
                                      YEAR(GETDATE()),
                                      MONTH(GETDATE()), 1))",
                        conn))
                    {
                        decimal revenue =
                            Convert.ToDecimal(cmd.ExecuteScalar());

                        lblRevenueMonth.Text =
                            revenue.ToString("N2");
                    }

                    lblActiveUsers.Text = Count(conn, @"
                        SELECT COUNT(*)
                        FROM dbo.Students
                        WHERE Status = 'Active'").ToString();

                    lblPending.Text = Count(conn, @"
                        SELECT COUNT(*)
                        FROM dbo.Booking
                        WHERE BookingStatus = 'Pending'").ToString();

                    using (SqlCommand cmd = new SqlCommand(@"
                        SELECT COUNT(*)
                        FROM dbo.Lesson
                        WHERE CAST(LessonDateTime AS DATE) = @Today
                          AND Status <> 'Cancelled'", conn))
                    {
                        cmd.Parameters.Add("@Today", SqlDbType.Date)
                            .Value = LessonProgress.TodaySa();

                        lblToday.Text =
                            Convert.ToString(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());

                lblError.Text =
                    "Dashboard statistics are temporarily unavailable.";

                lblError.Visible = true;
            }
        }

        private void LoadBookings()
        {
            string where;
            string order = "b.ScheduledDateTime";

            switch (ddlFilter.SelectedValue)
            {
                case "unsettled":
                    where = "b.BookingStatus = 'Unsettled'";
                    break;

                case "pending":
                    where = "b.BookingStatus = 'Pending'";
                    break;

                case "confirmed":
                    where = "b.BookingStatus = 'Confirmed'";
                    break;

                case "today":
                    where =
                        "CAST(b.ScheduledDateTime AS DATE) = @Today";
                    break;

                case "revenue":
                    where = @"
                        EXISTS (
                            SELECT 1
                            FROM dbo.Payment p
                            WHERE p.BookingID = b.BookingID
                              AND p.PaymentStatus = 'Paid'
                              AND p.AmountPaid > 0
                              AND p.PaymentDate >=
                                  DATEFROMPARTS(
                                      YEAR(GETDATE()),
                                      MONTH(GETDATE()), 1)
                              AND p.PaymentDate <
                                  DATEADD(MONTH, 1,
                                      DATEFROMPARTS(
                                          YEAR(GETDATE()),
                                          MONTH(GETDATE()), 1))
                        )";
                    order = "b.ScheduledDateTime DESC";
                    break;

                default:
                    where = @"
                        b.BookingStatus IN
                        ('Unsettled','Pending','Confirmed')";
                    break;
            }

            string sql = @"
                SELECT
                    b.BookingID,
                    b.ScheduledDateTime,
                    b.BookingStatus,
                    b.StudEmailAddress AS StudEmail,
                    LTRIM(RTRIM(
                        ISNULL(s.Name, '') + ' ' +
                        ISNULL(s.Surname, '')
                    )) AS StudentName
                FROM dbo.Booking b
                LEFT JOIN dbo.Students s
                    ON s.StudEmailAddress = b.StudEmailAddress
                WHERE " + where + @"
                ORDER BY " + order;

            try
            {
                using (SqlConnection conn =
                    new SqlConnection(ConnectionString))
                using (SqlDataAdapter adapter =
                    new SqlDataAdapter(sql, conn))
                {
                    adapter.SelectCommand.Parameters
                        .Add("@Today", SqlDbType.Date)
                        .Value = LessonProgress.TodaySa();

                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    repBookings.DataSource = table;
                    repBookings.DataBind();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());

                lblError.Text =
                    "Booking information is temporarily unavailable.";

                lblError.Visible = true;
            }
        }

        // Payment monitoring is read-only.
        // Existing PayFast processing is not modified.
        private void LoadPaymentAlerts()
        {
            try
            {
                using (SqlConnection conn =
                    new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    LoadRecentPayments(conn);
                    LoadAwaitingPayments(conn);
                    LoadPaymentReviews(conn);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "Manager payment alerts: " + ex);

                lblPaymentAlertsError.Visible = true;
            }
        }

        private void LoadRecentPayments(SqlConnection conn)
        {
            // Ledger entries represent recorded package payments.
            // The seven-day count is not an unread count.

            DataTable table = GetTable(conn, @"
                SELECT TOP 8
                    l.LedgerID,
                    l.Amount,
                    l.PaidOn,
                    p.PackageName,
                    s.StudEmailAddress,
                    LTRIM(RTRIM(
                        ISNULL(s.Name, '') + ' ' +
                        ISNULL(s.Surname, '')
                    )) AS StudentName
                FROM dbo.PackagePaymentLedger l
                INNER JOIN dbo.PackageReservation r
                    ON r.ReservationID = l.ReservationID
                INNER JOIN dbo.Packages p
                    ON p.PackageID = r.PackageID
                INNER JOIN dbo.Students s
                    ON s.StudNumber = r.StudNumber
                WHERE l.PaidOn >= DATEADD(
                    DAY, -7, SYSUTCDATETIME())
                ORDER BY l.PaidOn DESC, l.LedgerID DESC");

            repPackagePayments.DataSource = table;
            repPackagePayments.DataBind();

            lblNoPackagePayments.Visible =
                table.Rows.Count == 0;

            litNewPaymentCount.Text = Count(conn, @"
                SELECT COUNT(*)
                FROM dbo.PackagePaymentLedger
                WHERE PaidOn >= DATEADD(
                    DAY, -7, SYSUTCDATETIME())").ToString("N0");
        }

        private void LoadAwaitingPayments(SqlConnection conn)
        {
            // Existing reservation status: Reserved.
            // Only unexpired reservations are counted.

            DataTable table = GetTable(conn, @"
                SELECT TOP 8
                    r.ReservationID,
                    r.PackagePrice,
                    r.ReservedOn,
                    p.PackageName,
                    s.StudEmailAddress,
                    LTRIM(RTRIM(
                        ISNULL(s.Name, '') + ' ' +
                        ISNULL(s.Surname, '')
                    )) AS StudentName
                FROM dbo.PackageReservation r
                INNER JOIN dbo.Packages p
                    ON p.PackageID = r.PackageID
                INNER JOIN dbo.Students s
                    ON s.StudNumber = r.StudNumber
                WHERE r.Status = 'Reserved'
                  AND r.ExpiresOn > GETDATE()
                ORDER BY
                    r.ReservedOn DESC,
                    r.ReservationID DESC");

            repAwaitingPayments.DataSource = table;
            repAwaitingPayments.DataBind();

            lblNoAwaiting.Visible =
                table.Rows.Count == 0;

            litAwaitingCount.Text = Count(conn, @"
                SELECT COUNT(*)
                FROM dbo.PackageReservation
                WHERE Status = 'Reserved'
                  AND ExpiresOn > GETDATE()").ToString("N0");
        }

        private void LoadPaymentReviews(SqlConnection conn)
        {
            DataTable table = GetTable(conn, @"
                SELECT TOP 8
                    r.ReservationID,
                    p.PackageName,
                    s.StudEmailAddress,
                    LTRIM(RTRIM(
                        ISNULL(s.Name, '') + ' ' +
                        ISNULL(s.Surname, '')
                    )) AS StudentName,
                    b.TotalPaid,
                    b.BalanceDue
                FROM dbo.PackagePaymentBalance b
                INNER JOIN dbo.PackageReservation r
                    ON r.ReservationID = b.ReservationID
                INNER JOIN dbo.Packages p
                    ON p.PackageID = r.PackageID
                INNER JOIN dbo.Students s
                    ON s.StudNumber = r.StudNumber
                WHERE b.PaymentStatus = 'ReviewRequired'
                ORDER BY b.BalanceID DESC");

            repPaymentReviews.DataSource = table;
            repPaymentReviews.DataBind();

            lblNoReviews.Visible =
                table.Rows.Count == 0;

            litReviewCount.Text = Count(conn, @"
                SELECT COUNT(*)
                FROM dbo.PackagePaymentBalance
                WHERE PaymentStatus = 'ReviewRequired'")
                .ToString("N0");
        }

        protected void ddlFilter_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            LoadBookings();
        }

        protected void btnViewStudent_Command(
            object sender,
            CommandEventArgs e)
        {
            string email = Convert.ToString(e.CommandArgument);

            if (string.IsNullOrWhiteSpace(email))
                return;

            Response.Redirect(
                "~/AdminStudentDetail.aspx?email=" +
                Server.UrlEncode(email),
                false);

            Context.ApplicationInstance.CompleteRequest();
        }

        protected void btnLogout_Click(
            object sender,
            EventArgs e)
        {
            StaffAuth.SignOutStaff();

            Session.Clear();
            Session.Abandon();

            Response.Redirect(
                "~/Account/Login.aspx",
                false);

            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
