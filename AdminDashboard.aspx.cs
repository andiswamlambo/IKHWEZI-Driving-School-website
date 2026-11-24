
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApplicationM3
{
    public partial class AdminDashboard : System.Web.UI.Page
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            if (Session != null) ViewStateUserKey = Session.SessionID ?? "anon";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!StaffAuth.CheckAccess("Admin")) return;

            if (StaffAuth.IsManagerOrHigher())
            {
                Response.Redirect("~/ManagerDashboard.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            phManagerOnly.Visible = false;

            if (!IsPostBack)
            {
                LoadTopBar();
                LoadCards();
                LoadBookings();
            }
        }

        private void LoadTopBar()
        {
            string fullName = StaffAuth.Name;
            if (string.IsNullOrEmpty(fullName)) fullName = "Reception";
            litStaffName.Text = fullName;
            litInitials.Text = GetInitials(fullName);
            litRoleBadge.Text = StaffAuth.Role;
        }

        private string GetInitials(string name)
        {
            var parts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string ini = "";
            foreach (var p in parts) ini += char.ToUpper(p[0]);
            return ini.Length > 0 ? ini.Substring(0, Math.Min(2, ini.Length)) : "?";
        }

        private void LoadCards()
        {
            lblCardUnsettled.Text = "0";
            lblCardPending.Text = "0";
            lblCardConfirmed.Text = "0";
            lblCardToday.Text = "0";

            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Booking WHERE BookingStatus = 'Unsettled'", conn))
                        lblCardUnsettled.Text = cmd.ExecuteScalar().ToString();

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Booking WHERE BookingStatus = 'Pending'", conn))
                        lblCardPending.Text = cmd.ExecuteScalar().ToString();

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Booking WHERE BookingStatus = 'Confirmed'", conn))
                        lblCardConfirmed.Text = cmd.ExecuteScalar().ToString();

                    using (SqlCommand cmd = new SqlCommand(
                        @"SELECT COUNT(*) FROM Booking
                          WHERE CAST(ScheduledDateTime AS DATE) = @Today
                            AND BookingStatus <> 'Cancelled'", conn))
                    {
                        cmd.Parameters.AddWithValue("@Today", LessonProgress.TodaySa());
                        lblCardToday.Text = cmd.ExecuteScalar().ToString();
                    }
                }
            }
            catch { }
        }

        private void LoadBookings()
        {
            litEmpty.Visible = false;

            string filter = ddlFilter.SelectedValue;
            string where;
            string order;

            switch (filter)
            {
                case "unsettled": where = "AND b.BookingStatus = 'Unsettled'"; order = "b.ScheduledDateTime"; break;
                case "pending": where = "AND b.BookingStatus = 'Pending'"; order = "b.ScheduledDateTime"; break;
                case "confirmed": where = "AND b.BookingStatus = 'Confirmed'"; order = "b.ScheduledDateTime"; break;
                case "today": where = "AND CAST(b.ScheduledDateTime AS DATE) = @Today"; order = "b.ScheduledDateTime"; break;
                case "completed": where = "AND b.BookingStatus = 'Completed'"; order = "b.ScheduledDateTime DESC"; break;
                case "all": where = ""; order = "b.ScheduledDateTime DESC"; break;
                default: where = "AND b.BookingStatus IN ('Unsettled','Pending','Confirmed')"; order = "b.ScheduledDateTime"; break;
            }

            string search = txtSearch.Text.Trim();
            if (search.Length > 0)
            {
                where += @" AND (
                    s.Name LIKE @Search OR
                    s.Surname LIKE @Search OR
                    s.StudEmailAddress LIKE @Search OR
                    CAST(b.BookingID AS VARCHAR(20)) LIKE @Search
                )";
            }

            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                string sql =
                    @"SELECT b.BookingID, b.ScheduledDateTime, b.BookingStatus,
                             b.StudEmailAddress AS StudEmail,
                             LTRIM(RTRIM(ISNULL(s.Name,'') + ' ' + ISNULL(s.Surname,''))) AS StudentName,
                             ISNULL(p.PackageName, 'Learners / Theory') AS PackageName,
                             ISNULL(b.StafEmailAddress, 'To be assigned') AS InstructorName,
                             r.RequestType AS InstructorRequestStatus,
                             r.Reason AS InstructorRequestReason
                      FROM Booking b
                      LEFT JOIN Students s ON s.StudEmailAddress = b.StudEmailAddress
                      LEFT JOIN Packages p ON p.PackageID = b.PackageID
                      LEFT JOIN dbo.InstructorRequests r ON r.BookingID = b.BookingID AND r.Status = 'Pending'
                      WHERE 1=1 " + where + " ORDER BY " + order;

                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                {
                    da.SelectCommand.Parameters.AddWithValue("@Today", LessonProgress.TodaySa());
                    if (search.Length > 0)
                        da.SelectCommand.Parameters.AddWithValue("@Search", "%" + search + "%");

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    repBookings.DataSource = dt;
                    repBookings.DataBind();
                    litEmpty.Visible = dt.Rows.Count == 0;
                }
            }
            catch (Exception ex)
            {
                ShowMsg("Could not load bookings: " + Server.HtmlEncode(ex.Message), false);
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e) { LoadBookings(); }
        protected void btnClearSearch_Click(object sender, EventArgs e) { txtSearch.Text = ""; LoadBookings(); }
        protected void ddlFilter_SelectedIndexChanged(object sender, EventArgs e) { LoadBookings(); }

        protected bool IsUnsettled(string status)
        {
            return status != null && status.Equals("Unsettled", StringComparison.OrdinalIgnoreCase);
        }

        protected bool IsPending(string status)
        {
            return status != null && status.Equals("Pending", StringComparison.OrdinalIgnoreCase);
        }

        protected bool IsCancellable(string status)
        {
            if (string.IsNullOrEmpty(status)) return false;
            return status.Equals("Unsettled", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Pending", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Overdue", StringComparison.OrdinalIgnoreCase);
        }

        protected bool HasRequest(object requestStatus)
        {
            return requestStatus != null &&
                   requestStatus != DBNull.Value &&
                   !string.IsNullOrEmpty(requestStatus.ToString());
        }

        protected string Chip(string status)
        {
            if (string.IsNullOrEmpty(status)) return "pending";
            string s = status.ToLowerInvariant();
            if (s == "completed" || s == "confirmed" || s == "cancelled" ||
                s == "rescheduled" || s == "unsettled" || s == "overdue" ||
                s == "suspended")
                return s;
            return "pending";
        }

        protected void btnViewStudent_Command(object sender, CommandEventArgs e)
        {
            string email = Convert.ToString(e.CommandArgument);
            // Note: This now points to ReceptionistStudentDetail.aspx
            Response.Redirect("~/ReceptionistStudentDetail.aspx?email=" + Server.UrlEncode(email), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void btnConfirm_Command(object sender, CommandEventArgs e)
        {
            int bookingId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out bookingId)) return;

            string msg;
            bool ok = ConfirmBooking(bookingId, StaffAuth.Email, out msg);
            ShowMsg(msg, ok);
            LoadCards();
            LoadBookings();
        }

        protected void btnCancel_Command(object sender, CommandEventArgs e)
        {
            int bookingId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out bookingId)) return;

            string msg;
            bool ok = LessonProgress.CancelBooking(bookingId, StaffAuth.Email, out msg);
            ShowMsg(msg, ok);
            LoadCards();
            LoadBookings();
        }

        protected void btnDeclineRequest_Command(object sender, CommandEventArgs e)
        {
            int bookingId = Convert.ToInt32(e.CommandArgument);
            try
            {
                string instructorEmail = null, instructorName = "Instructor";
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT b.StafEmailAddress, ISNULL(i.Name + ' ' + i.Surname, 'Instructor') AS InsName
                      FROM Booking b
                      LEFT JOIN Instructor i ON i.InstruEmailAddress = b.StafEmailAddress
                      WHERE b.BookingID = @B", conn))
                {
                    cmd.Parameters.AddWithValue("@B", bookingId);
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            instructorEmail = r["StafEmailAddress"] != DBNull.Value
                                ? r["StafEmailAddress"].ToString()
                                : null;
                            instructorName = r["InsName"].ToString();
                        }
                    }
                }

                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"UPDATE dbo.InstructorRequests
                      SET Status = 'Declined'
                      WHERE BookingID = @B AND Status = 'Pending'", conn))
                {
                    cmd.Parameters.AddWithValue("@B", bookingId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                if (!string.IsNullOrEmpty(instructorEmail))
                {
                    EmailHelper.SendInstructorRequestResponse(
                        instructorEmail, instructorName, bookingId,
                        "Request", false,
                        "Reception has declined your request. Please contact the office.");
                }

                ShowMsg("Request declined. The instructor's schedule remains unchanged.", true);
            }
            catch (Exception ex)
            {
                ShowMsg("Error: " + ex.Message, false);
            }

            LoadBookings();
        }

        protected void btnApproveRequest_Command(object sender, CommandEventArgs e)
        {
            int bookingId = Convert.ToInt32(e.CommandArgument);
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                string reqType = null;
                string instructorEmail = null;
                string instructorName = "Instructor";

                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT r.RequestType, b.StafEmailAddress,
                             ISNULL(i.Name + ' ' + i.Surname, 'Instructor') AS InsName
                      FROM dbo.InstructorRequests r
                      INNER JOIN Booking b ON b.BookingID = r.BookingID
                      LEFT JOIN Instructor i ON i.InstruEmailAddress = b.StafEmailAddress
                      WHERE r.BookingID = @B AND r.Status = 'Pending'", conn))
                {
                    cmd.Parameters.AddWithValue("@B", bookingId);
                    conn.Open();

                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            reqType = r["RequestType"].ToString();
                            instructorEmail = r["StafEmailAddress"] != DBNull.Value
                                ? r["StafEmailAddress"].ToString()
                                : null;
                            instructorName = r["InsName"].ToString();
                        }
                    }
                }

                if (string.IsNullOrEmpty(reqType)) return;

                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"UPDATE dbo.InstructorRequests
                      SET Status = 'Approved'
                      WHERE BookingID = @B AND Status = 'Pending'", conn))
                {
                    cmd.Parameters.AddWithValue("@B", bookingId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                if (reqType == "Cancel")
                {
                    string msg;
                    LessonProgress.CancelBooking(bookingId, StaffAuth.Email, out msg);

                    if (!string.IsNullOrEmpty(instructorEmail))
                        EmailHelper.SendInstructorRequestResponse(
                            instructorEmail, instructorName, bookingId,
                            "Cancellation", true,
                            "Reception has cancelled the lesson for the student.");

                    ShowMsg("Request Approved: Booking has been cancelled.", true);
                }
                else if (reqType == "Replace")
                {
                    DateTime scheduled = DateTime.MinValue;
                    int durationMins = 60;

                    using (SqlConnection conn = new SqlConnection(cs))
                    using (SqlCommand cmd = new SqlCommand(
                        @"SELECT ScheduledDateTime,
                                 ISNULL(DurationMinutes, 60) AS DurationMinutes
                          FROM Booking WHERE BookingID = @B", conn))
                    {
                        cmd.Parameters.AddWithValue("@B", bookingId);
                        conn.Open();

                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                scheduled = Convert.ToDateTime(r["ScheduledDateTime"]);
                                durationMins = Convert.ToInt32(r["DurationMinutes"]);
                            }
                        }
                    }

                    string newInstructor = GetAvailableInstructor(
                        scheduled, durationMins, bookingId);

                    if (!string.IsNullOrEmpty(newInstructor))
                    {
                        using (SqlConnection conn = new SqlConnection(cs))
                        using (SqlCommand cmd = new SqlCommand(
                            @"UPDATE Booking
                              SET StafEmailAddress = @NewIns,
                                  BookingStatus = 'Confirmed'
                              WHERE BookingID = @B", conn))
                        {
                            cmd.Parameters.AddWithValue("@NewIns", newInstructor);
                            cmd.Parameters.AddWithValue("@B", bookingId);
                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }

                        if (!string.IsNullOrEmpty(instructorEmail))
                            EmailHelper.SendInstructorRequestResponse(
                                instructorEmail, instructorName, bookingId,
                                "Replacement", true,
                                "Reception has removed you from the lesson. A new instructor has been assigned.");

                        ShowMsg(
                            "Request Approved: A new instructor was found and assigned automatically.",
                            true);
                    }
                    else
                    {
                        using (SqlConnection conn = new SqlConnection(cs))
                        using (SqlCommand cmd = new SqlCommand(
                            @"UPDATE Booking
                              SET StafEmailAddress = NULL,
                                  BookingStatus = 'Pending'
                              WHERE BookingID = @B", conn))
                        {
                            cmd.Parameters.AddWithValue("@B", bookingId);
                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }

                        if (!string.IsNullOrEmpty(instructorEmail))
                            EmailHelper.SendInstructorRequestResponse(
                                instructorEmail, instructorName, bookingId,
                                "Replacement", true,
                                "Reception has removed you from the lesson.");

                        ShowMsg(
                            "Request Approved: Instructor removed. No replacement was available automatically. Please assign one manually.",
                            false);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMsg("Error: " + ex.Message, false);
            }

            LoadCards();
            LoadBookings();
        }

        private string GetAvailableInstructor(
            DateTime scheduled,
            int durationMins,
            int currentBookingId)
        {
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT TOP 1 i.InstruEmailAddress
                      FROM Instructor i
                      WHERE i.InstruEmailAddress NOT IN (
                          SELECT StafEmailAddress FROM Booking
                          WHERE CAST(ScheduledDateTime AS DATE) = @D
                            AND BookingStatus IN ('Unsettled','Pending','Confirmed')
                            AND StafEmailAddress IS NOT NULL
                            AND BookingID <> @CurrentB
                            AND CAST(ScheduledDateTime AS TIME) < @End
                            AND DATEADD(MINUTE, ISNULL(DurationMinutes, 60),
                                CAST(ScheduledDateTime AS TIME)) > @Start
                      )
                      AND i.InstruEmailAddress NOT IN (
                          SELECT InstruEmailAddress FROM Lesson
                          WHERE CAST(LessonDateTime AS DATE) = @D
                            AND Status <> 'Cancelled'
                            AND InstruEmailAddress IS NOT NULL
                            AND CAST(LessonDateTime AS TIME) < @End
                            AND DATEADD(MINUTE, ISNULL(Duration, 60),
                                CAST(LessonDateTime AS TIME)) > @Start
                      )
                      AND i.InstruEmailAddress NOT IN (
                          SELECT InstruEmailAddress
                          FROM InstructorBlockOuts
                          WHERE BlockedDate = @D
                      )
                      ORDER BY NEWID()", conn))
                {
                    cmd.Parameters.AddWithValue("@D", scheduled.Date);
                    cmd.Parameters.AddWithValue("@CurrentB", currentBookingId);
                    cmd.Parameters.AddWithValue("@Start", scheduled.TimeOfDay);
                    cmd.Parameters.AddWithValue("@End",
                        scheduled.TimeOfDay.Add(TimeSpan.FromMinutes(durationMins)));

                    conn.Open();
                    object r = cmd.ExecuteScalar();
                    return r != null ? r.ToString() : null;
                }
            }
            catch
            {
                return null;
            }
        }

        private bool ConfirmBooking(int bookingId, string actorEmail, out string message)
        {
            message = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(
                    ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString))
                {
                    conn.Open();

                    using (SqlTransaction tx = conn.BeginTransaction())
                    {
                        try
                        {
                            string currentStatus;

                            using (SqlCommand cmd = new SqlCommand(
                                @"SELECT BookingStatus
                                  FROM Booking WITH (UPDLOCK, ROWLOCK)
                                  WHERE BookingID = @B", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@B", bookingId);
                                object r = cmd.ExecuteScalar();

                                if (r == null)
                                {
                                    tx.Rollback();
                                    message = "Booking not found.";
                                    return false;
                                }

                                currentStatus = r.ToString();
                            }

                            if (currentStatus.Equals("Confirmed", StringComparison.OrdinalIgnoreCase))
                            {
                                tx.Rollback();
                                message = "Already confirmed.";
                                return false;
                            }

                            if (currentStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                            {
                                tx.Rollback();
                                message = "Cannot confirm a cancelled booking.";
                                return false;
                            }

                            if (currentStatus.Equals("Unsettled", StringComparison.OrdinalIgnoreCase))
                            {
                                tx.Rollback();
                                message = "Booking is Unsettled — record the deposit first.";
                                return false;
                            }

                            using (SqlCommand cmd = new SqlCommand(
                                @"UPDATE Booking
                                  SET BookingStatus = 'Confirmed'
                                  WHERE BookingID = @B", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@B", bookingId);
                                cmd.ExecuteNonQuery();
                            }

                            using (SqlCommand cmd = new SqlCommand(
                                @"INSERT INTO BookingAudit
                                  (BookingID, Action, ActorEmail, ActorRole, ActionTime, Notes)
                                  VALUES
                                  (@B, 'Confirmed', @E, @R, @T, 'Confirmed at reception')",
                                conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@B", bookingId);
                                cmd.Parameters.AddWithValue("@E", actorEmail);
                                cmd.Parameters.AddWithValue("@R", StaffAuth.Role);
                                cmd.Parameters.AddWithValue("@T", LessonProgress.NowSa());
                                cmd.ExecuteNonQuery();
                            }

                            tx.Commit();
                            message = "Booking #" + bookingId + " confirmed.";
                            return true;
                        }
                        catch
                        {
                            tx.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                message = "Error: " + ex.Message;
                return false;
            }
        }

        private void ShowMsg(string html, bool ok)
        {
            lblMsg.CssClass = ok ? "msg ok" : "msg err";
            lblMsg.Text = html;
            lblMsg.Visible = true;
        }

        protected void btnLogoutSide_Click(object sender, EventArgs e)
        {
            StaffAuth.SignOutStaff();
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Account/Login.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
