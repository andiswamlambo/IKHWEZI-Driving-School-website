using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApplicationM3
{
    public partial class InstructorDashboard : System.Web.UI.Page
    {
        private static readonly TimeSpan RequestNotice = TimeSpan.FromHours(1);
        private static readonly TimeSpan CompletionLeadWindow = TimeSpan.FromMinutes(15);

        private string StaffEmail { get { return StaffAuth.Email; } }

        private int PendingCompleteBookingId
        {
            get { return ViewState["PendingCompleteBookingId"] == null ? 0 : Convert.ToInt32(ViewState["PendingCompleteBookingId"]); }
            set { ViewState["PendingCompleteBookingId"] = value; }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            if (Session != null) ViewStateUserKey = Session.SessionID ?? "anon";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!StaffAuth.CheckAccess("Instructor")) return;

            if (!IsPostBack)
            {
                LoadTopBar();
                LoadTimeSlots();
                LoadCards();
                ApplyActiveCardHighlight();
                LoadSessions();
            }
        }

        private void LoadTopBar()
        {
            string fullName = StaffAuth.Name;
            if (string.IsNullOrEmpty(fullName) || fullName == "Staff") fullName = "Instructor";
            litStaffName.Text = fullName;
            litInitials.Text = GetInitials(fullName);
        }

        private string GetInitials(string name)
        {
            string[] parts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string initials = "";
            foreach (string p in parts) initials += char.ToUpper(p[0]);
            return initials.Length > 0 ? initials.Substring(0, Math.Min(2, initials.Length)) : "?";
        }

        private void LoadTimeSlots()
        {
            if (ddlNewTime.Items.Count == 0)
            {
                for (int h = 7; h <= 16; h++) ddlNewTime.Items.Add(h.ToString("00") + ":00");
            }
        }

        protected void btnBlockDate_Click(object sender, EventArgs e)
        {
            DateTime d;
            if (!DateTime.TryParse(txtBlockDate.Text, out d)) { ShowMsg("Please select a valid date.", false); return; }
            if (d.Date < LessonProgress.TodaySa()) { ShowMsg("Cannot block a past date.", false); return; }

            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"IF NOT EXISTS (SELECT 1 FROM dbo.InstructorBlockOuts WHERE InstruEmailAddress = @E AND BlockedDate = @D)
                      BEGIN
                          INSERT INTO dbo.InstructorBlockOuts (InstruEmailAddress, BlockedDate, Reason)
                          VALUES (@E, @D, @R)
                      END", conn))
                {
                    cmd.Parameters.AddWithValue("@E", StaffEmail);
                    cmd.Parameters.AddWithValue("@D", d.Date);
                    cmd.Parameters.AddWithValue("@R", (object)txtBlockReason.Text.Trim() ?? DBNull.Value);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                ShowMsg("Success! You are blocked out on " + d.ToString("dd MMM yyyy") + ".", true);
                txtBlockDate.Text = ""; txtBlockReason.Text = "";
            }
            catch (Exception ex) { ShowMsg("Error: " + ex.Message, false); }
        }

        private void LoadCards()
        {
            lblCardStudents.Text = "0"; lblCardToday.Text = "0"; lblCardUnsettled.Text = "0"; lblCardCompleted.Text = "0";
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(DISTINCT StudEmailAddress) FROM Booking WHERE StafEmailAddress = @E AND BookingStatus <> 'Cancelled'", conn))
                    { cmd.Parameters.AddWithValue("@E", StaffEmail); lblCardStudents.Text = cmd.ExecuteScalar().ToString(); }

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Booking WHERE StafEmailAddress = @E AND BookingStatus <> 'Cancelled' AND CAST(ScheduledDateTime AS DATE) = @Today", conn))
                    { cmd.Parameters.AddWithValue("@E", StaffEmail); cmd.Parameters.AddWithValue("@Today", LessonProgress.TodaySa()); lblCardToday.Text = cmd.ExecuteScalar().ToString(); }

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Booking WHERE StafEmailAddress = @E AND BookingStatus = 'Unsettled'", conn))
                    { cmd.Parameters.AddWithValue("@E", StaffEmail); lblCardUnsettled.Text = cmd.ExecuteScalar().ToString(); }

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Booking WHERE StafEmailAddress = @E AND BookingStatus = 'Completed'", conn))
                    { cmd.Parameters.AddWithValue("@E", StaffEmail); lblCardCompleted.Text = cmd.ExecuteScalar().ToString(); }
                }
            }
            catch { }
        }

        private void ApplyActiveCardHighlight()
        {
            string f = ddlFilter.SelectedValue;
            cardStudents.CssClass = "stat-card";
            cardToday.CssClass = "stat-card hot";
            cardAwaiting.CssClass = "stat-card warn";
            cardCompleted.CssClass = "stat-card green";

            if (f == "today") cardToday.CssClass += " active";
            if (f == "unsettled") cardAwaiting.CssClass += " active";
            if (f == "completed") cardCompleted.CssClass += " active";
        }

        protected void cardToday_Click(object sender, EventArgs e) { ddlFilter.SelectedValue = "today"; ApplyActiveCardHighlight(); LoadSessions(); }
        protected void cardAwaiting_Click(object sender, EventArgs e) { ddlFilter.SelectedValue = "unsettled"; ApplyActiveCardHighlight(); LoadSessions(); }
        protected void cardCompleted_Click(object sender, EventArgs e) { ddlFilter.SelectedValue = "completed"; ApplyActiveCardHighlight(); LoadSessions(); }
        protected void cardStudents_Click(object sender, EventArgs e) { ddlFilter.SelectedValue = "all"; ApplyActiveCardHighlight(); LoadSessions(); }

        private void LoadSessions()
        {
            litEmpty.Visible = false;
            string filter = ddlFilter.SelectedValue;
            string where;
            string order;

            switch (filter)
            {
                case "today": where = "AND CAST(b.ScheduledDateTime AS DATE) = @Today"; order = "b.ScheduledDateTime"; break;
                case "completed": where = "AND b.BookingStatus = 'Completed'"; order = "b.ScheduledDateTime DESC"; break;
                case "unsettled": where = "AND b.BookingStatus = 'Unsettled'"; order = "b.ScheduledDateTime"; break;
                case "all": where = ""; order = "b.ScheduledDateTime DESC"; break;
                default: where = "AND b.BookingStatus IN ('Unsettled','Pending','Confirmed','Overdue')"; order = "b.ScheduledDateTime"; break;
            }

            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                string sql =
                    @"SELECT b.BookingID, b.ScheduledDateTime, b.BookingStatus, 
                             r.RequestType AS InstructorRequestStatus, r.Reason AS InstructorRequestReason,
                             LTRIM(RTRIM(ISNULL(s.Name,'') + ' ' + ISNULL(s.Surname,''))) AS StudentName,
                             ISNULL(s.PhoneNum, '') AS StudentPhone, ISNULL(s.StudEmailAddress,'') AS StudentEmail,
                             ISNULL(s.StreetAddress,'') AS Street, ISNULL(s.Suburb,'') AS Suburb, ISNULL(s.City,'') AS City,
                             ISNULL(s.Name, '?') AS FirstName, ISNULL(s.Surname, '?') AS LastName,
                             (SELECT TOP 1 FilePath FROM StudentDocuments d WHERE d.StudNumber = s.StudNumber AND d.DocumentType = 'Photo' AND d.Status <> 'Rejected' ORDER BY d.UploadedOn DESC) AS ProfilePhotoUrl
                      FROM Booking b
                      INNER JOIN Students s ON s.StudEmailAddress = b.StudEmailAddress
                      LEFT JOIN dbo.InstructorRequests r ON r.BookingID = b.BookingID AND r.Status = 'Pending'
                      WHERE b.StafEmailAddress = @E " + where + " ORDER BY " + order;

                // THE FIX: We fill the DataTable inside the 'using' block, 
                // then exit the block. This closes the connection and releases the DataReader.
                DataTable dt = new DataTable();
                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                {
                    da.SelectCommand.Parameters.AddWithValue("@E", StaffEmail);
                    da.SelectCommand.Parameters.AddWithValue("@Today", LessonProgress.TodaySa());
                    da.Fill(dt);
                }

                // Now that the DataReader is closed, we can safely manipulate the DataTable
                dt.Columns.Add("Initials", typeof(string));
                foreach (DataRow row in dt.Rows)
                {
                    string first = row["FirstName"].ToString().Trim();
                    string last = row["LastName"].ToString().Trim();
                    string fullName = (first + " " + last).Trim();
                    if (string.IsNullOrEmpty(fullName)) fullName = "Student";
                    row["Initials"] = GetInitials(fullName);
                    if (row["ProfilePhotoUrl"] == DBNull.Value) row["ProfilePhotoUrl"] = null;
                }

                repSessions.DataSource = dt;
                repSessions.DataBind();
                litEmpty.Visible = dt.Rows.Count == 0;
            }
            catch (Exception ex) { ShowMsg("Error loading sessions: " + ex.Message, false); }
        }

        protected void ddlFilter_SelectedIndexChanged(object sender, EventArgs e) { ApplyActiveCardHighlight(); LoadSessions(); }

        protected bool IsPhotoAvailable(object photoObj)
        {
            return photoObj != null && photoObj != DBNull.Value && !string.IsNullOrEmpty(photoObj.ToString());
        }

        protected string BuildStudentMeta(object dataItem)
        {
            var row = dataItem as DataRowView;
            if (row == null) return "";
            string status = Convert.ToString(row["BookingStatus"]);
            if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase) || status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                return "<div class='student-meta'>Contact details hidden.</div>";

            string phone = Convert.ToString(row["StudentPhone"]); string email = Convert.ToString(row["StudentEmail"]);
            string street = Convert.ToString(row["Street"]); string suburb = Convert.ToString(row["Suburb"]); string city = Convert.ToString(row["City"]);

            string address = "";
            if (!string.IsNullOrWhiteSpace(street) || !string.IsNullOrWhiteSpace(city))
            {
                address = street;
                if (!string.IsNullOrWhiteSpace(suburb)) address += (address.Length > 0 ? ", " : "") + suburb;
                if (!string.IsNullOrWhiteSpace(city)) address += (address.Length > 0 ? ", " : "") + city;
            }

            var sb = new System.Text.StringBuilder();
            sb.Append("<div class='student-meta'>");
            if (!string.IsNullOrWhiteSpace(phone)) sb.Append("&#128222; <a href='tel:").Append(Server.HtmlEncode(phone)).Append("'>").Append(Server.HtmlEncode(phone)).Append("</a> ");
            if (!string.IsNullOrWhiteSpace(email)) sb.Append("&#9993; <a href='mailto:").Append(Server.HtmlEncode(email)).Append("'>").Append(Server.HtmlEncode(email)).Append("</a>");
            if (!string.IsNullOrWhiteSpace(address)) sb.Append("<br/>&#128205; ").Append(Server.HtmlEncode(address));
            sb.Append("</div>");
            return sb.ToString();
        }

        protected bool HasPendingRequestUI(object requestStatus)
        {
            return requestStatus != null && requestStatus != DBNull.Value && !string.IsNullOrEmpty(requestStatus.ToString());
        }

        protected string GetRequestLabel(object requestStatus)
        {
            if (requestStatus == null || requestStatus == DBNull.Value) return "";
            string s = requestStatus.ToString();
            if (s.Equals("Cancel", StringComparison.OrdinalIgnoreCase)) return "Cancel Requested";
            if (s.Equals("Replace", StringComparison.OrdinalIgnoreCase)) return "Replacement Requested";
            return "Request Pending";
        }

        protected bool CanBeMarkedCompleted(string status, object scheduledObj, object requestStatus)
        {
            if (string.IsNullOrEmpty(status) || HasPendingRequestUI(requestStatus)) return false;
            bool statusOk = status.Equals("Pending", StringComparison.OrdinalIgnoreCase) || status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase) || status.Equals("Overdue", StringComparison.OrdinalIgnoreCase);
            if (!statusOk) return false;
            DateTime scheduled;
            try { scheduled = Convert.ToDateTime(scheduledObj); } catch { return false; }
            return (scheduled - LessonProgress.NowSa()) <= CompletionLeadWindow;
        }

        protected bool IsReschedulable(string status, object requestStatus)
        {
            if (string.IsNullOrEmpty(status) || HasPendingRequestUI(requestStatus)) return false;
            return status.Equals("Unsettled", StringComparison.OrdinalIgnoreCase) || status.Equals("Pending", StringComparison.OrdinalIgnoreCase) || status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase) || status.Equals("Overdue", StringComparison.OrdinalIgnoreCase);
        }

        protected bool CanRequestCancel(string status, object requestStatus, object scheduledObj)
        {
            if (string.IsNullOrEmpty(status) || HasPendingRequestUI(requestStatus)) return false;
            bool active = status.Equals("Unsettled", StringComparison.OrdinalIgnoreCase) || status.Equals("Pending", StringComparison.OrdinalIgnoreCase) || status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase);
            return active && HasEnoughNotice(scheduledObj);
        }

        protected bool CanRequestReplace(string status, object requestStatus, object scheduledObj)
        {
            if (string.IsNullOrEmpty(status) || HasPendingRequestUI(requestStatus)) return false;
            bool active = status.Equals("Unsettled", StringComparison.OrdinalIgnoreCase) || status.Equals("Pending", StringComparison.OrdinalIgnoreCase) || status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase);
            return active && HasEnoughNotice(scheduledObj);
        }

        private bool HasEnoughNotice(object scheduledObj)
        {
            try { return (Convert.ToDateTime(scheduledObj) - LessonProgress.NowSa()) >= RequestNotice; } catch { return false; }
        }

        protected string Chip(string status)
        {
            if (string.IsNullOrEmpty(status)) return "pending";
            string s = status.ToLowerInvariant();
            return (s == "completed" || s == "confirmed" || s == "cancelled" || s == "rescheduled" || s == "unsettled" || s == "overdue" || s == "suspended") ? s : "pending";
        }

        protected bool IsPendingComplete(object dataItem)
        {
            var row = dataItem as DataRowView;
            return row != null && Convert.ToInt32(row["BookingID"]) == PendingCompleteBookingId;
        }

        protected void btnComplete_ShowConfirm(object sender, CommandEventArgs e)
        {
            int bookingId; if (!int.TryParse(e.CommandArgument.ToString(), out bookingId)) return;
            PendingCompleteBookingId = bookingId; HideActionPanels(); LoadSessions();
        }

        protected void btnComplete_Confirm(object sender, CommandEventArgs e)
        {
            int bookingId; if (!int.TryParse(e.CommandArgument.ToString(), out bookingId)) return;
            PendingCompleteBookingId = 0;
            string msg; bool ok = LessonProgress.MarkBookingCompleted(bookingId, StaffEmail, out msg);
            ShowMsg(msg, ok); LoadCards(); LoadSessions();
        }

        protected void btnComplete_Dismiss(object sender, CommandEventArgs e) { PendingCompleteBookingId = 0; LoadSessions(); }

        protected void btnResched_Command(object sender, CommandEventArgs e)
        {
            int bookingId; if (!int.TryParse(e.CommandArgument.ToString(), out bookingId)) return;
            hfReschedBooking.Value = bookingId.ToString(); litReschedTarget.Text = "#" + bookingId;
            DateTime def = LessonProgress.TodaySa().AddDays(1); if (def.DayOfWeek == DayOfWeek.Sunday) def = def.AddDays(1);
            txtNewDate.Text = def.ToString("yyyy-MM-dd");
            HideActionPanels(); pnlReschedule.Visible = true;
        }

        protected void btnDoResched_Click(object sender, EventArgs e)
        {
            int bookingId; if (!int.TryParse(hfReschedBooking.Value, out bookingId)) { ShowMsg("No booking selected.", false); return; }
            DateTime d; TimeSpan t;
            if (!DateTime.TryParse(txtNewDate.Text, out d) || !TimeSpan.TryParse(ddlNewTime.SelectedValue, out t)) { ShowMsg("Invalid date/time.", false); return; }
            string msg; bool ok = LessonProgress.RescheduleBooking(bookingId, d.Date.Add(t), StaffEmail, out msg);
            ShowMsg(msg, ok); pnlReschedule.Visible = false; LoadSessions();
        }

        protected void btnCloseResched_Click(object sender, EventArgs e) { pnlReschedule.Visible = false; }

        protected void btnRequestCancel_Command(object sender, CommandEventArgs e)
        {
            hfActionBooking.Value = e.CommandArgument.ToString(); litRequestCancelTarget.Text = "#" + e.CommandArgument.ToString();
            txtRequestCancelReason.Text = ""; HideActionPanels(); pnlRequestCancel.Visible = true;
        }

        protected void btnDoRequestCancel_Click(object sender, EventArgs e)
        {
            int bookingId; if (!int.TryParse(hfActionBooking.Value, out bookingId)) return;
            if (string.IsNullOrWhiteSpace(txtRequestCancelReason.Text)) { ShowMsg("Please provide a reason.", false); return; }
            string msg; bool ok = SubmitInstructorRequest(bookingId, "Cancel", txtRequestCancelReason.Text.Trim(), out msg);
            ShowMsg(msg, ok); pnlRequestCancel.Visible = false; LoadSessions();
        }

        protected void btnCloseRequestCancel_Click(object sender, EventArgs e) { pnlRequestCancel.Visible = false; }

        protected void btnRequestReplace_Command(object sender, CommandEventArgs e)
        {
            hfActionBooking.Value = e.CommandArgument.ToString(); litRequestReplaceTarget.Text = "#" + e.CommandArgument.ToString();
            txtRequestReplaceReason.Text = ""; HideActionPanels(); pnlRequestReplace.Visible = true;
        }

        protected void btnDoRequestReplace_Click(object sender, EventArgs e)
        {
            int bookingId; if (!int.TryParse(hfActionBooking.Value, out bookingId)) return;
            if (string.IsNullOrWhiteSpace(txtRequestReplaceReason.Text)) { ShowMsg("Please provide a reason.", false); return; }
            string msg; bool ok = SubmitInstructorRequest(bookingId, "Replace", txtRequestReplaceReason.Text.Trim(), out msg);
            ShowMsg(msg, ok); pnlRequestReplace.Visible = false; LoadSessions();
        }

        protected void btnCloseRequestReplace_Click(object sender, EventArgs e) { pnlRequestReplace.Visible = false; }

        private void HideActionPanels()
        {
            pnlReschedule.Visible = false; pnlRequestCancel.Visible = false; pnlRequestReplace.Visible = false;
        }

        private bool SubmitInstructorRequest(int bookingId, string requestType, string reason, out string message)
        {
            message = null;
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                DateTime scheduled = DateTime.MinValue;
                string studentName = "Student";
                string studentEmail = "";
                string packageName = "Driving Lesson";

                using (SqlConnection conn = new SqlConnection(cs))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(
                        @"SELECT b.ScheduledDateTime, 
                                 ISNULL(p.PackageName, 'Driving Lesson') AS PackageName,
                                 LTRIM(RTRIM(ISNULL(s.Name,'') + ' ' + ISNULL(s.Surname,''))) AS StudentName,
                                 ISNULL(s.StudEmailAddress, '') AS StudentEmail
                          FROM Booking b
                          LEFT JOIN Students s ON s.StudEmailAddress = b.StudEmailAddress
                          LEFT JOIN Packages p ON p.PackageID = b.PackageID
                          WHERE b.BookingID = @B", conn))
                    {
                        cmd.Parameters.AddWithValue("@B", bookingId);
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                scheduled = Convert.ToDateTime(r["ScheduledDateTime"]);
                                studentName = r["StudentName"].ToString();
                                studentEmail = r["StudentEmail"].ToString();
                                packageName = r["PackageName"].ToString();
                            }
                        }
                    }

                    using (SqlCommand cmd = new SqlCommand(
                        @"INSERT INTO dbo.InstructorRequests (BookingID, InstruEmailAddress, RequestType, Reason, Status)
                          VALUES (@B, @E, @Type, @R, 'Pending')", conn))
                    {
                        cmd.Parameters.AddWithValue("@B", bookingId);
                        cmd.Parameters.AddWithValue("@E", StaffEmail);
                        cmd.Parameters.AddWithValue("@Type", requestType);
                        cmd.Parameters.AddWithValue("@R", reason);
                        cmd.ExecuteNonQuery();
                    }
                }

                try
                {
                    EmailHelper.SendInstructorRequestToReception(
                        requestType, StaffEmail, StaffAuth.Name,
                        bookingId, scheduled, packageName, studentName, studentEmail, reason
                    );
                }
                catch (Exception mailEx)
                {
                    System.Diagnostics.Trace.TraceError("Failed to email reception: " + mailEx.Message);
                }

                message = "Request sent to reception successfully! They have been notified.";
                return true;
            }
            catch (Exception ex)
            {
                message = "Error: " + ex.Message;
                return false;
            }
        }


        private void ShowMsg(string html, bool ok)
        {
            lblMsg.CssClass = ok ? "msg ok" : "msg err"; lblMsg.Text = html; lblMsg.Visible = true;
        }

        protected void btnLogoutSide_Click(object sender, EventArgs e)
        {
            try { StaffAuth.SignOutStaff(); } catch { }
            try { Session.Clear(); } catch { }
            try { Session.Abandon(); } catch { }
            Response.Redirect("~/Account/Login.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}