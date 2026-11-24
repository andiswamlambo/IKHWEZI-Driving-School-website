using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApplicationM3
{
    public partial class StudentDashboard : System.Web.UI.Page
    {
        private int PendingCancelBookingId
        {
            get { return ViewState["PendingCancelBookingId"] == null ? 0 : Convert.ToInt32(ViewState["PendingCancelBookingId"]); }
            set { ViewState["PendingCancelBookingId"] = value; }
        }

        private int PendingExceptionBookingId
        {
            get { return ViewState["PendingExceptionBookingId"] == null ? 0 : Convert.ToInt32(ViewState["PendingExceptionBookingId"]); }
            set { ViewState["PendingExceptionBookingId"] = value; }
        }

        private string FeedbackMessage
        {
            get { return ViewState["FeedbackMessage"] as string; }
            set { ViewState["FeedbackMessage"] = value; }
        }

        private bool FeedbackIsOk
        {
            get { return ViewState["FeedbackIsOk"] == null || (bool)ViewState["FeedbackIsOk"]; }
            set { ViewState["FeedbackIsOk"] = value; }
        }

        private bool FeedbackIsWarn
        {
            get { return ViewState["FeedbackIsWarn"] != null && (bool)ViewState["FeedbackIsWarn"]; }
            set { ViewState["FeedbackIsWarn"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["StudNumber"] == null)
            {
                Response.Redirect("~/Account/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                LoadTopBar();
                LoadPackageCard();
                LoadNotifications();
                chkCompleted.Checked = true;

                string stashed = Session["DashboardMessage"] as string;
                if (!string.IsNullOrEmpty(stashed))
                {
                    FeedbackMessage = stashed;
                    FeedbackIsOk = Session["DashboardMessageOk"] == null || (bool)Session["DashboardMessageOk"];
                    Session["DashboardMessage"] = null;
                    Session["DashboardMessageOk"] = null;
                }

                BindLessons();
            }
        }

        // =========================================================
        // LOAD NOTIFICATIONS (Dropdown)
        // =========================================================
        private void LoadNotifications()
        {
            StringBuilder sb = new StringBuilder();
            int alertCount = 0;
            string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;

            try
            {
                using (SqlConnection conn = new SqlConnection(cs))
                {
                    conn.Open();

                    // 1. Check for Overdue Bookings / Outstanding Balance
                    using (SqlCommand cmd = new SqlCommand(
                        @"SELECT TOP 1 BookingID, PackagePrice, TotalPaid, PaymentDueDate
                          FROM Booking 
                          WHERE StudEmailAddress = (SELECT StudEmailAddress FROM Students WHERE StudNumber = @S)
                            AND BookingStatus = 'Overdue'", conn))
                    {
                        cmd.Parameters.AddWithValue("@S", (int)Session["StudNumber"]);
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                alertCount++;
                                decimal balance = Convert.ToDecimal(r["PackagePrice"]) - Convert.ToDecimal(r["TotalPaid"]);
                                sb.Append(BuildNotifHtml("danger", "&#9888;", "Payment Overdue",
                                    "You have an outstanding balance of R" + balance.ToString("N2") + ". Please settle this to continue booking.", "Action Required"));
                            }
                        }
                    }

                    // 2. Check for Upcoming Lessons (Next 3 days)
                    using (SqlCommand cmd = new SqlCommand(
                        @"SELECT TOP 2 BookingID, ScheduledDateTime, PackageName
                          FROM Booking b
                          LEFT JOIN Packages p ON b.PackageID = p.PackageID
                          WHERE StudEmailAddress = (SELECT StudEmailAddress FROM Students WHERE StudNumber = @S)
                            AND BookingStatus = 'Confirmed'
                            AND ScheduledDateTime BETWEEN GETDATE() AND DATEADD(DAY, 3, GETDATE())
                          ORDER BY ScheduledDateTime ASC", conn))
                    {
                        cmd.Parameters.AddWithValue("@S", (int)Session["StudNumber"]);
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                alertCount++;
                                DateTime when = Convert.ToDateTime(r["ScheduledDateTime"]);
                                string pkg = r["PackageName"].ToString();
                                sb.Append(BuildNotifHtml("info", "&#128663;", "Upcoming Lesson",
                                    "You have a lesson for " + pkg + " on " + when.ToString("ddd, d MMM 'at' HH:mm") + ".", when.ToString("ddd HH:mm")));
                            }
                        }
                    }

                    // 3. Check for Unsettled (Needs Deposit)
                    using (SqlCommand cmd = new SqlCommand(
                        @"SELECT TOP 1 BookingID, ScheduledDateTime
                          FROM Booking
                          WHERE StudEmailAddress = (SELECT StudEmailAddress FROM Students WHERE StudNumber = @S)
                            AND BookingStatus = 'Unsettled'", conn))
                    {
                        cmd.Parameters.AddWithValue("@S", (int)Session["StudNumber"]);
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                alertCount++;
                                sb.Append(BuildNotifHtml("warn", "&#128179;", "Deposit Required",
                                    "Your slot is held, but unpaid. Please pay the 50% deposit to confirm your lesson.", "Action Required"));
                            }
                        }
                    }

                    // If no alerts, show empty state
                    if (alertCount == 0)
                    {
                        sb.Append("<div class='notif-empty'>&#127881; You're all caught up! No new notifications.</div>");
                    }

                    litNotifications.Text = sb.ToString();

                    // Show the red dot on the bell if there are alerts
                    if (alertCount > 0)
                    {
                        var badge = FindControl("notifBadge") as System.Web.UI.HtmlControls.HtmlGenericControl;
                        if (badge != null)
                        {
                            badge.Attributes["class"] = "notif-badge show";
                        }
                    }
                }
            }
            catch
            {
                litNotifications.Text = "<div class='notif-empty'>Could not load notifications.</div>";
            }
        }

        private string BuildNotifHtml(string type, string icon, string title, string message, string time)
        {
            return $@"
            <a href='#' class='notif-item' onclick='clearNotifications(); return false;'>
                <div class='notif-icon {type}'>{icon}</div>
                <div class='notif-content'>
                    <h4>{title}</h4>
                    <p>{message}</p>
                    <span class='notif-time'>{time}</span>
                </div>
            </a>";
        }

        private void LoadPackageCard()
        {
            try
            {
                int studNumber = (int)Session["StudNumber"];
                litPackageCard.Text = LessonProgress.BuildProgressCardHtml(studNumber);

                var active = LessonProgress.GetActiveStudentPackage(studNumber);
                if (active != null && active.UsedLessons == 0)
                {
                    string reason;
                    bool canRelease = LessonProgress.CanStudentSwitchPackage(studNumber, out reason);
                    btnReleasePackage.Visible = canRelease;
                }
                else
                {
                    btnReleasePackage.Visible = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("StudentDashboard.LoadPackageCard: " + ex);
                litPackageCard.Text =
                    "<div style='background:#fff;border-radius:12px;padding:20px;" +
                    "box-shadow:0 4px 15px rgba(13,43,110,.08);color:#e8283f;" +
                    "font-size:13.5px;font-weight:600;'>" +
                    "We couldn't load your package balance. Please refresh the page.</div>";
                btnReleasePackage.Visible = false;
            }
        }

        private void LoadTopBar()
        {
            string fullName = Session["FullName"] != null ? Session["FullName"].ToString() : "";
            if (fullName.Length == 0) fullName = "Student";
            litStudentName.Text = fullName;
            litInitials.Text = GetInitials(fullName);

            // NEW: Load profile photo
            int studNumber = (int)Session["StudNumber"];
            string photoUrl = MyDocuments.GetProfilePhotoUrl(studNumber);

            if (!string.IsNullOrEmpty(photoUrl))
            {
                imgTopAvatar.ImageUrl = ResolveUrl(photoUrl);
                imgTopAvatar.Visible = true;
                litInitials.Visible = false;
            }
            else
            {
                imgTopAvatar.Visible = false;
                litInitials.Visible = true;
            }
        }

        private string GetInitials(string name)
        {
            string[] parts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string initials = "";
            foreach (string p in parts) initials += char.ToUpper(p[0]);
            return initials.Length > 0 ? initials.Substring(0, Math.Min(2, initials.Length)) : "?";
        }

        private void BindLessons()
        {
            List<LessonItem> lessons = LoadLessons();

            var wanted = new List<string>();
            if (chkAwaiting.Checked) { wanted.Add("Unsettled"); wanted.Add("Pending"); }
            if (chkCompleted.Checked) wanted.Add("Completed");
            if (chkConfirmed.Checked) wanted.Add("Confirmed");
            if (chkCanceled.Checked) { wanted.Add("Canceled"); wanted.Add("Cancelled"); }

            List<LessonItem> filtered = wanted.Count == 0
                ? lessons
                : lessons.Where(l => wanted.Contains(l.Status)).ToList();

            double totalHours = filtered.Sum(l => l.DurationHours);
            litHoursTotal.Text = totalHours.ToString("0") + " hours in total on this page";

            repLessons.DataSource = filtered;
            repLessons.DataBind();

            pnlEmpty.Visible = filtered.Count == 0;
            RenderFeedback();
        }

        private void RenderFeedback()
        {
            if (string.IsNullOrEmpty(FeedbackMessage))
            { pnlMessage.Visible = false; return; }

            string cls = FeedbackIsWarn
                ? "dashboard-msg warn"
                : (FeedbackIsOk ? "dashboard-msg ok" : "dashboard-msg err");

            litMessage.Text = "<div class='" + cls + "'>" + Server.HtmlEncode(FeedbackMessage) + "</div>";
            pnlMessage.Visible = true;
        }

        private List<LessonItem> LoadLessons()
        {
            try
            {
                var list = new List<LessonItem>();
                string cs = ConfigurationManager
                    .ConnectionStrings["KhweziConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT BookingID     = b.BookingID,
                             LessonType    = ISNULL(p.PackageName, 'Lesson'),
                             LessonDate    = CAST(b.ScheduledDateTime AS DATE),
                             StartTime     = CAST(b.ScheduledDateTime AS TIME),
                             EndTime       = CAST(DATEADD(HOUR, 1, b.ScheduledDateTime) AS TIME),
                             Location      = 'Khwezi Driving School',
                             InstructorName = ISNULL(b.StafEmailAddress, 'To be assigned'),
                             Transmission  = ISNULL(v.TransmissionType, ''),
                             Status        = b.BookingStatus
                      FROM Booking b
                      LEFT JOIN Package p ON p.PackageID = b.PackageID
                      LEFT JOIN Vehicles v ON v.VehicleID = b.VehicleID
                      WHERE b.StudEmailAddress =
                            (SELECT StudEmailAddress FROM Students WHERE StudNumber = @StudNumber)
                      ORDER BY b.ScheduledDateTime DESC", conn))
                {
                    cmd.Parameters.AddWithValue("@StudNumber", (int)Session["StudNumber"]);
                    conn.Open();

                    using (SqlDataReader r = cmd.ExecuteReader())
                        while (r.Read())
                            list.Add(new LessonItem
                            {
                                BookingID = Convert.ToInt32(r["BookingID"]),
                                Type = r["LessonType"].ToString(),
                                Status = r["Status"].ToString(),
                                LessonDate = Convert.ToDateTime(r["LessonDate"]),
                                Start = (TimeSpan)r["StartTime"],
                                End = (TimeSpan)r["EndTime"],
                                Location = r["Location"].ToString(),
                                Instructor = r["InstructorName"].ToString(),
                                Transmission = r["Transmission"].ToString()
                            });
                }
                return list;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("StudentDashboard.LoadLessons: " + ex);
                return new List<LessonItem>();
            }
        }

        protected bool IsPendingCancel(object dataItem)
        {
            var item = dataItem as LessonItem;
            return item != null && item.BookingID == PendingCancelBookingId;
        }

        protected void btnCancelLesson_ShowConfirm(object sender, CommandEventArgs e)
        {
            int bookingId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out bookingId))
            { SetFeedback("Invalid booking reference.", false); BindLessons(); return; }

            PendingCancelBookingId = bookingId;
            PendingExceptionBookingId = 0;
            SetFeedback("", true);
            BindLessons();
        }

        protected void btnCancelLesson_Confirm(object sender, CommandEventArgs e)
        {
            int bookingId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out bookingId))
            { SetFeedback("Invalid booking reference.", false); BindLessons(); return; }

            PendingCancelBookingId = 0;
            PendingExceptionBookingId = 0;

            string studentEmail = GetStudentEmail();
            if (string.IsNullOrEmpty(studentEmail))
            { SetFeedback("Could not identify your account. Please sign in again.", false); BindLessons(); return; }

            string msg;
            bool tooLate;
            bool ok = LessonProgress.CancelBookingByStudent(bookingId, studentEmail, out msg, out tooLate);

            if (ok) SetFeedback(msg ?? "Booking cancelled.", true);
            else if (tooLate) { PendingExceptionBookingId = bookingId; SetWarn(msg ?? "Cancellation requires 24 hours' notice."); }
            else SetFeedback(msg ?? "Could not cancel this booking.", false);

            LoadPackageCard();
            BindLessons();
        }

        protected void btnCancelLesson_Dismiss(object sender, CommandEventArgs e)
        {
            PendingCancelBookingId = 0;
            SetFeedback("", true);
            BindLessons();
        }

        protected void btnSubmitException_Click(object sender, EventArgs e)
        {
            int bookingId = PendingExceptionBookingId;
            if (bookingId <= 0)
            {
                SetWarn("Could not identify which booking to submit. Please try again.");
                BindLessons();
                return;
            }

            string reason = txtExceptionReason != null ? txtExceptionReason.Text : null;
            if (string.IsNullOrWhiteSpace(reason))
                reason = ReadPostedTextareaSuffix("txtExceptionReason");

            if (string.IsNullOrWhiteSpace(reason))
            {
                SetWarn("Please tell reception why you need to cancel at short notice.");
                UpdateExceptionPanel();
                BindLessons();
                return;
            }

            string studentEmail = GetStudentEmail();
            if (string.IsNullOrEmpty(studentEmail))
            { SetFeedback("Could not identify your account. Please sign in again.", false); BindLessons(); return; }

            string msg;
            bool ok = LessonProgress.SubmitCancellationRequest(
                bookingId, (int)Session["StudNumber"], studentEmail, reason, out msg);

            PendingExceptionBookingId = 0;
            SetFeedback(msg ?? (ok ? "Request sent to reception." : "Could not send request."), ok);
            UpdateExceptionPanel();
            BindLessons();
        }

        private string ReadPostedTextareaSuffix(string suffix)
        {
            string needle = "$" + suffix;
            foreach (string key in Request.Form.AllKeys)
            {
                if (key == null) continue;
                if (key.EndsWith(needle, StringComparison.OrdinalIgnoreCase))
                {
                    string val = Request.Form[key];
                    if (!string.IsNullOrWhiteSpace(val)) return val;
                }
            }
            return null;
        }

        protected void btnDismissException_Click(object sender, EventArgs e)
        {
            PendingExceptionBookingId = 0;
            SetFeedback("", true);
            UpdateExceptionPanel();
            BindLessons();
        }

        private void UpdateExceptionPanel()
        {
            if (PendingExceptionBookingId <= 0) { pnlException.Visible = false; return; }

            pnlException.Visible = true;

            var all = LoadLessons();
            var b = all.FirstOrDefault(x => x.BookingID == PendingExceptionBookingId);

            if (b != null)
                litExceptionLesson.Text =
                    "on <strong>" + Server.HtmlEncode(b.DateText) + "</strong> " +
                    "at <strong>" + Server.HtmlEncode(b.TimeText) + "</strong>";
            else
                litExceptionLesson.Text = "";
        }

        protected void btnPayDeposit_Command(object sender, CommandEventArgs e)
        {
            int bookingId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out bookingId))
            { SetFeedback("Invalid booking reference.", false); BindLessons(); return; }

            Response.Redirect("~/PayDeposit.aspx?id=" + bookingId, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void btnReleasePackage_Click(object sender, EventArgs e)
        {
            int studNumber = (int)Session["StudNumber"];

            string reason;
            if (!LessonProgress.CanStudentSwitchPackage(studNumber, out reason))
            { SetFeedback(reason ?? "This package cannot be released.", false); LoadPackageCard(); BindLessons(); return; }

            var active = LessonProgress.GetActiveStudentPackage(studNumber);
            if (active == null)
            { SetFeedback("You don't have an active package to release.", false); LoadPackageCard(); BindLessons(); return; }

            bool ok = LessonProgress.DeactivateStudentPackage(studNumber, active.PackageID);

            SetFeedback(
                ok ? "Your '" + active.PackageName + "' reservation has been released. You can now choose a new package whenever you're ready."
                   : "Could not release the package. Please try again or contact reception.",
                ok);

            LoadPackageCard();
            BindLessons();
        }

        protected void btnUpgrade_Click(object sender, EventArgs e)
        {
            SetFeedback("Premium membership is coming soon. Watch this space!", true);
            BindLessons();
        }

        protected bool IsUnsettled(string status)
        {
            return !string.IsNullOrEmpty(status) && status.Equals("Unsettled", StringComparison.OrdinalIgnoreCase);
        }

        protected bool IsCancellable(string status)
        {
            if (string.IsNullOrEmpty(status)) return false;
            return status.Equals("Unsettled", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Pending", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase);
        }

        private string GetStudentEmail()
        {
            string email = Session["StudEmail"] as string;
            if (!string.IsNullOrEmpty(email)) return email;
            return LessonProgress.GetStudentEmail((int)Session["StudNumber"]);
        }

        private void SetFeedback(string html, bool ok)
        {
            FeedbackMessage = html ?? "";
            FeedbackIsOk = ok;
            FeedbackIsWarn = false;
        }

        private void SetWarn(string html)
        {
            FeedbackMessage = html ?? "";
            FeedbackIsOk = false;
            FeedbackIsWarn = true;
        }

        protected void Filter_Changed(object sender, EventArgs e) { BindLessons(); }

        protected void btnNewLesson_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/BookSession.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void btnLogoutSide_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Account/Login.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }

    public class LessonItem
    {
        public int BookingID { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public DateTime LessonDate { get; set; }
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
        public string Location { get; set; }
        public string Instructor { get; set; }
        public string Transmission { get; set; }

        public string BadgeClass { get { return Type == "Exam" ? "badge-blue" : "badge-green"; } }

        public string StatusClass
        {
            get
            {
                switch (Status)
                {
                    case "Unsettled": return "status-orange";
                    case "Pending": return "status-orange";
                    case "Canceled":
                    case "Cancelled": return "status-red";
                    default: return "status-green";
                }
            }
        }

        public string DateText { get { return LessonDate.ToString("MMMM d, yyyy"); } }

        public string TimeText
        {
            get
            {
                string s = DateTime.Today.Add(Start).ToString("h tt").ToLower();
                string en = DateTime.Today.Add(End).ToString("h tt").ToLower();
                return s + " to " + en;
            }
        }

        public double DurationHours { get { return (End - Start).TotalHours; } }
    }
}