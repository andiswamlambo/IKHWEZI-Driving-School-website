using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;

namespace WebApplicationM3
{
    public partial class LearnersClass : System.Web.UI.Page
    {
        private static readonly string[] Times =
            { "07:00", "08:00", "09:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00", "16:00" };

        private static readonly TimeSpan SaturdayClose = new TimeSpan(13, 0, 0);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["StudNumber"] == null)
            {
                Response.Redirect("~/Account/Login.aspx?return=LearnersClass", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (StaffAuth.IsSignedIn)
            {
                string role = StaffAuth.Role ?? "";
                Response.Redirect(
                    role.Equals("Instructor", StringComparison.OrdinalIgnoreCase)
                        ? "~/InstructorDashboard.aspx"
                        : "~/AdminDashboard.aspx",
                    false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                LoadTopBar();

                int studNumber = (int)Session["StudNumber"];

                string repeatReason = GetLearnersRepeatBookingReason(studNumber);
                if (repeatReason != null) { ShowAlreadyBooked(studNumber); return; }

                string blockReason = GetLearnerClassBlockReason(studNumber);
                if (blockReason != null) { ShowBlocked(); return; }

                if (Request.QueryString["nolicense"] == "1")
                    pnlNotice.Visible = true;

                DateTime def = DateTime.Today.AddDays(1);
                if (def.DayOfWeek == DayOfWeek.Sunday) def = def.AddDays(1);
                txtDate.Text = def.ToString("yyyy-MM-dd");

                if (ddlTime.Items.Count == 0)
                    foreach (string t in Times)
                        ddlTime.Items.Add(t);

                litPrice.Text = "R " + GetLearnersPrice().ToString("N0");
            }
        }

        private string GetLearnersRepeatBookingReason(int studNumber)
        {
            try
            {
                string cs = ConfigurationManager
                    .ConnectionStrings["KhweziConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT TOP 1 b.BookingID, b.BookingStatus
                      FROM Booking b
                      INNER JOIN Packages p ON p.PackageID = b.PackageID
                      WHERE b.StudEmailAddress =
                            (SELECT StudEmailAddress FROM Students WHERE StudNumber = @S)
                        AND p.PackageType = 'Learners'
                        AND b.BookingStatus IN ('Unsettled','Pending','Confirmed','Completed')
                      ORDER BY b.ScheduledDateTime DESC",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@S", studNumber);
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            string status = r["BookingStatus"].ToString();
                            if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                                return "completed";
                            return "active";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("GetLearnersRepeatBookingReason: " + ex);
            }
            return null;
        }

        private void ShowAlreadyBooked(int studNumber)
        {
            pnlAlreadyBooked.Visible = true;
            pnlBook.Visible = false;
            pnlNotice.Visible = false;
            pnlSuccess.Visible = false;
            pnlBlocked.Visible = false;
            pnlCancelLearners.Visible = false;
            pnlConfirmLearnersCancel.Visible = false;
            btnCancelLearnersBooking.Visible = true;

            try
            {
                string cs = ConfigurationManager
                    .ConnectionStrings["KhweziConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT TOP 1 b.BookingID, b.BookingStatus, b.ScheduledDateTime
                      FROM Booking b
                      INNER JOIN Packages p ON p.PackageID = b.PackageID
                      WHERE b.StudEmailAddress =
                            (SELECT StudEmailAddress FROM Students WHERE StudNumber = @S)
                        AND p.PackageType = 'Learners'
                        AND b.BookingStatus IN ('Unsettled','Pending','Confirmed','Completed')
                      ORDER BY b.ScheduledDateTime DESC",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@S", studNumber);
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            int bookingId = Convert.ToInt32(r["BookingID"]);
                            string status = r["BookingStatus"].ToString();
                            DateTime scheduled = Convert.ToDateTime(r["ScheduledDateTime"]);

                            ViewState["CancelLearnersBookingId"] = bookingId;
                            litExistingBookingId.Text = bookingId.ToString();

                            if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                            {
                                litAlreadyBookedTitle.Text = "Learners-Licence Class Already Completed";
                                litAlreadyBookedMessage.Text =
                                    "Our records show that you attended a K53 theory class on " +
                                    scheduled.ToString("dddd, d MMMM yyyy") + ". " +
                                    "Learners-licence classes cannot be re-booked once attended. " +
                                    "If you have lost your certificate or need assistance, please contact reception.";
                                litExistingBookingInfo.Text = "Completed on " + scheduled.ToString("dd MMM yyyy");
                            }
                            else
                            {
                                litAlreadyBookedTitle.Text = "Learners-Licence Class Already Booked";
                                litAlreadyBookedMessage.Text =
                                    "You have an active learners-licence booking on your account. " +
                                    "Only one learners-licence class is permitted per student.";
                                litExistingBookingInfo.Text =
                                    "Scheduled for " + scheduled.ToString("dddd, d MMM yyyy 'at' HH:mm");

                                int workingDays = WorkingDaysBetween(DateTime.Today, scheduled);
                                if (workingDays >= 3)
                                    pnlCancelLearners.Visible = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("ShowAlreadyBooked: " + ex);
            }
        }

        protected void btnCancelLearnersBooking_Click(object sender, EventArgs e)
        {
            pnlConfirmLearnersCancel.Visible = true;
            btnCancelLearnersBooking.Visible = false;
        }

        protected void btnConfirmYes_Click(object sender, EventArgs e)
        {
            int bookingId = 0;
            if (ViewState["CancelLearnersBookingId"] != null)
                bookingId = Convert.ToInt32(ViewState["CancelLearnersBookingId"]);

            if (bookingId <= 0)
            {
                Response.Redirect("~/LearnersClass.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            string email = null;
            try { email = LessonProgress.GetStudentEmail((int)Session["StudNumber"]); }
            catch (Exception ex) { System.Diagnostics.Trace.TraceError("GetStudentEmail: " + ex); }

            if (string.IsNullOrEmpty(email))
            {
                Session["DashboardMessage"] = "We couldn't identify your account. Please sign in again.";
                Session["DashboardMessageOk"] = false;
                Response.Redirect("~/StudentDashboard.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            string msg;
            bool ok = LessonProgress.CancelLearnersBookingByStudent(bookingId, email, out msg);

            Session["DashboardMessage"] = msg ?? DbFriendly.GenericMessage;
            Session["DashboardMessageOk"] = ok;

            Response.Redirect("~/StudentDashboard.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void btnConfirmNo_Click(object sender, EventArgs e)
        {
            pnlConfirmLearnersCancel.Visible = false;
            btnCancelLearnersBooking.Visible = true;
        }

        private string GetLearnerClassBlockReason(int studNumber)
        {
            var activePackage = LessonProgress.GetActiveStudentPackage(studNumber);
            if (activePackage != null && activePackage.RemainingLessons > 0)
            {
                return "You already have an active lesson package ('" +
                       activePackage.PackageName + "' with " +
                       activePackage.RemainingLessons + " lesson(s) remaining).";
            }

            var reservation = PackageReservationService.GetCurrent(studNumber);
            if (reservation != null && reservation.IsActive)
            {
                return "You have a lesson package reserved ('" +
                       reservation.PackageName + "', expiring " +
                       reservation.ExpiresOn.ToString("d MMM yyyy HH:mm") + ").";
            }

            return null;
        }

        private void ShowBlocked()
        {
            pnlBlocked.Visible = true;
            pnlBook.Visible = false;
            pnlNotice.Visible = false;
            pnlSuccess.Visible = false;
            pnlAlreadyBooked.Visible = false;
        }

        private void LoadTopBar()
        {
            string fullName = Session["FullName"] != null ? Session["FullName"].ToString() : "";
            if (fullName.Length == 0) fullName = "Student";
            litStudentName.Text = fullName;
            litInitials.Text = GetInitials(fullName);
        }

        private string GetInitials(string name)
        {
            string[] parts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string initials = "";
            foreach (string p in parts) initials += char.ToUpper(p[0]);
            return initials.Length > 0 ? initials.Substring(0, Math.Min(2, initials.Length)) : "?";
        }

        private static int WorkingDaysBetween(DateTime from, DateTime to)
        {
            if (to <= from) return 0;
            int count = 0;
            for (DateTime d = from.Date.AddDays(1); d <= to.Date; d = d.AddDays(1))
            {
                if (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
                    continue;
                count++;
            }
            return count;
        }

        private bool StudentsHasLicenceColumn()
        {
            try
            {
                string cs = ConfigurationManager
                    .ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT COUNT(1) FROM INFORMATION_SCHEMA.COLUMNS
                      WHERE TABLE_NAME = 'Students' AND COLUMN_NAME = 'HasLearnersLicence'", conn))
                {
                    conn.Open();
                    return ((int)cmd.ExecuteScalar()) > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("StudentsHasLicenceColumn: " + ex);
                return false;
            }
        }

        private void SaveLicenceFlag(bool hasLicence)
        {
            if (!StudentsHasLicenceColumn()) return;
            try
            {
                string cs = ConfigurationManager
                    .ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"UPDATE Students SET HasLearnersLicence = @Flag
                      WHERE StudNumber = @StudNumber", conn))
                {
                    cmd.Parameters.AddWithValue("@Flag", hasLicence);
                    cmd.Parameters.AddWithValue("@StudNumber", (int)Session["StudNumber"]);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("SaveLicenceFlag: " + ex);
            }
        }

        protected void btnDeclareLicence_Click(object sender, EventArgs e)
        {
            SaveLicenceFlag(true);
            Response.Redirect("~/BookSession.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private decimal GetLearnersPrice()
        {
            try
            {
                string cs = ConfigurationManager
                    .ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT TOP 1 Amount FROM Packages
                      WHERE PackageType = 'Learners' OR PackageName LIKE '%Learner%'
                      ORDER BY CASE WHEN PackageType = 'Learners' THEN 0 ELSE 1 END, PackageID", conn))
                {
                    conn.Open();
                    object r = cmd.ExecuteScalar();
                    if (r != null) return Convert.ToDecimal(r);
                }
            }
            catch (Exception ex) { System.Diagnostics.Trace.TraceError("GetLearnersPrice: " + ex); }
            return 500m;
        }

        private int GetLearnersPackageId()
        {
            try
            {
                string cs = ConfigurationManager
                    .ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT TOP 1 PackageID FROM Packages
                      WHERE PackageType = 'Learners' OR PackageName LIKE '%Learner%'
                      ORDER BY CASE WHEN PackageType = 'Learners' THEN 0 ELSE 1 END, PackageID", conn))
                {
                    conn.Open();
                    object r = cmd.ExecuteScalar();
                    if (r != null) return (int)r;
                }
            }
            catch (Exception ex) { System.Diagnostics.Trace.TraceError("GetLearnersPackageId: " + ex); }
            return -1;
        }

        protected void btnBook_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;
            pnlSuccess.Visible = false;

            int studNumber = (int)Session["StudNumber"];

            string repeatCheck = GetLearnersRepeatBookingReason(studNumber);
            if (repeatCheck != null) { ShowAlreadyBooked(studNumber); return; }

            string reCheck = GetLearnerClassBlockReason(studNumber);
            if (reCheck != null) { ShowBlocked(); return; }

            int packageId = GetLearnersPackageId();
            if (packageId < 0)
            {
                ShowErr("Learners package not found. Please contact reception on +27 83 544 8544.");
                return;
            }

            DateTime date;
            if (!DateTime.TryParse(txtDate.Text, out date) || date < DateTime.Today)
            { ShowErr("Please choose today or a future date."); return; }

            if (date.DayOfWeek == DayOfWeek.Sunday)
            { ShowErr("We are closed on Sundays. Please choose Monday to Saturday."); return; }

            TimeSpan start;
            if (!TimeSpan.TryParse(ddlTime.SelectedValue, out start))
            { ShowErr("Please choose a valid time slot."); return; }

            TimeSpan end = start.Add(new TimeSpan(1, 0, 0));

            if (date.DayOfWeek == DayOfWeek.Saturday && end > SaturdayClose)
            { ShowErr("Saturday classes run 08:00 to 13:00. Please pick an earlier slot."); return; }

            DateTime scheduled = date.Add(start);

            try
            {
                string cs = ConfigurationManager
                    .ConnectionStrings["KhweziConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(cs))
                {
                    conn.Open();

                    string studEmail;
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT StudEmailAddress FROM Students WHERE StudNumber = @StudNumber", conn))
                    {
                        cmd.Parameters.AddWithValue("@StudNumber", studNumber);
                        object r = cmd.ExecuteScalar();
                        if (r == null || r == DBNull.Value)
                        { ShowErr("We could not find your student email. Please contact reception."); return; }
                        studEmail = r.ToString().Trim();
                    }

                    string block = LessonProgress.GetBookingBlockReason(studNumber, studEmail, packageId);
                    if (block != null) { ShowErr(block); return; }

                    using (SqlCommand cmd = new SqlCommand(
                        @"SELECT COUNT(1) FROM Booking
                          WHERE StudEmailAddress = @Email
                            AND BookingStatus <> 'Cancelled'
                            AND CAST(ScheduledDateTime AS DATE) = @Date
                            AND CAST(ScheduledDateTime AS TIME) < @End
                            AND DATEADD(HOUR, 1, CAST(ScheduledDateTime AS TIME)) > @Start", conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", studEmail);
                        cmd.Parameters.AddWithValue("@Date", date);
                        cmd.Parameters.AddWithValue("@Start", start);
                        cmd.Parameters.AddWithValue("@End", end);

                        if ((int)cmd.ExecuteScalar() > 0)
                        {
                            ShowErr("You already have a booking that overlaps this time. Please choose a different slot or date.");
                            return;
                        }
                    }

                    string assignedEmail, assignedName;
                    if (!TryAssignInstructor(conn, date, start, end, ddlCode.SelectedValue,
                                             out assignedEmail, out assignedName))
                    {
                        ShowErr("All instructors are fully booked at that time. Please choose another slot or date.");
                        return;
                    }

                    int bookingId;
                    using (SqlCommand cmd = new SqlCommand(
                        @"INSERT INTO Booking
                          (BookingDate, StudEmailAddress, StafEmailAddress, BookingStatus,
                           PackageID, ScheduledDateTime)
                          OUTPUT INSERTED.BookingID
                          VALUES (CAST(GETDATE() AS DATE), @StudEmail, @StafEmail, 'Unsettled',
                                  @PackageID, @Scheduled)", conn))
                    {
                        cmd.Parameters.AddWithValue("@StudEmail", studEmail);
                        cmd.Parameters.AddWithValue("@StafEmail", assignedEmail);
                        cmd.Parameters.AddWithValue("@PackageID", packageId);
                        cmd.Parameters.AddWithValue("@Scheduled", scheduled);
                        bookingId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    LessonProgress.EnsureStudentPackage(studNumber, packageId);

                    try
                    {
                        string firstName = Session["FullName"] != null
                            ? Session["FullName"].ToString().Split(' ')[0]
                            : "Student";

                        EmailHelper.SendLearnersConfirmation(
                            studEmail, firstName, bookingId,
                            ddlCode.SelectedValue, date, start, assignedName);
                    }
                    catch (Exception mailEx)
                    {
                        System.Diagnostics.Trace.TraceError("LearnersClass email failed: " + mailEx);
                    }

                    litSuccess.Text =
                        "&#10004; <strong>Learners class booked!</strong><br/>" +
                        "<span style='font-weight:400;font-size:13.5px;color:#14663f;'>" +
                        "Booking reference: <strong>#" + bookingId + "</strong><br/>" +
                        Server.HtmlEncode(ddlCode.SelectedValue) + " K53 theory on " +
                        date.ToString("dddd, d MMMM yyyy") + " at " + ddlTime.SelectedValue +
                        " with <strong>" + Server.HtmlEncode(assignedName) + "</strong>.<br/>" +
                        "Please arrive 15 minutes early with your ID document." +
                        "</span>";

                    lnkPayOnline.NavigateUrl = "~/PayDeposit.aspx?id=" + bookingId;
                    lnkPayOnline.Visible = true;

                    pnlNotice.Visible = false;
                    pnlBook.Visible = false;
                    pnlSuccess.Visible = true;
                }
            }
            catch (Exception ex)
            {
                ShowErr(DbFriendly.Message(ex, "saving your learners booking"));
            }
        }

        private void ShowErr(string msg) { lblError.Text = msg; lblError.Visible = true; }

        private bool TryAssignInstructor(SqlConnection conn, DateTime date,
            TimeSpan start, TimeSpan end, string licenceCode,
            out string instructorEmail, out string instructorName)
        {
            instructorEmail = null;
            instructorName = null;

            var admin = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string emailCol = GetAdminStaffEmailColumn(conn);
            if (emailCol != null)
            {
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT LTRIM(RTRIM(CAST([" + emailCol + "] AS NVARCHAR(200)))) FROM Admin_Staff", conn))
                using (SqlDataReader r = cmd.ExecuteReader())
                    while (r.Read())
                        if (!r.IsDBNull(0))
                            admin.Add(r.GetString(0));
            }

            var instructors = new List<InstructorRow>();
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT InstruEmailAddress, Name, Surname, LicenceType FROM Instructor", conn))
            using (SqlDataReader r = cmd.ExecuteReader())
                while (r.Read())
                    instructors.Add(new InstructorRow
                    {
                        Email = r["InstruEmailAddress"].ToString().Trim(),
                        Name = r["Name"].ToString().Trim() + " " + r["Surname"].ToString().Trim(),
                        Licence = r["LicenceType"] == DBNull.Value ? "" : r["LicenceType"].ToString().Trim()
                    });

            var busy = new List<Interval>();

            using (SqlCommand cmd = new SqlCommand(
                @"SELECT StafEmailAddress, CAST(ScheduledDateTime AS TIME) AS StartTime
                  FROM Booking
                  WHERE CAST(ScheduledDateTime AS DATE) = @Date
                    AND BookingStatus <> 'Cancelled'", conn))
            {
                cmd.Parameters.AddWithValue("@Date", date);
                using (SqlDataReader r = cmd.ExecuteReader())
                    while (r.Read())
                    {
                        if (r.IsDBNull(0)) continue;
                        TimeSpan s = (TimeSpan)r["StartTime"];
                        busy.Add(new Interval { Who = r.GetString(0).Trim(), Start = s, End = s.Add(new TimeSpan(1, 0, 0)) });
                    }
            }

            using (SqlCommand cmd = new SqlCommand(
                @"SELECT InstruEmailAddress, CAST(LessonDateTime AS TIME) AS StartTime, Duration
                  FROM Lesson
                  WHERE CAST(LessonDateTime AS DATE) = @Date AND Status <> 'Cancelled'", conn))
            {
                cmd.Parameters.AddWithValue("@Date", date);
                using (SqlDataReader r = cmd.ExecuteReader())
                    while (r.Read())
                    {
                        if (r.IsDBNull(0)) continue;
                        TimeSpan s = (TimeSpan)r["StartTime"];
                        int mins = r.IsDBNull(2) ? 60 : Convert.ToInt32(r["Duration"]);
                        busy.Add(new Interval { Who = r.GetString(0).Trim(), Start = s, End = s.Add(TimeSpan.FromMinutes(mins)) });
                    }
            }

            string want = NormCode(licenceCode);
            InstructorRow best = null;
            int bestScore = int.MaxValue;

            foreach (var ins in instructors)
            {
                if (admin.Count > 0 && !admin.Contains(ins.Email)) continue;

                bool clash = busy.Any(b =>
                    string.Equals(b.Who, ins.Email, StringComparison.OrdinalIgnoreCase) &&
                    b.Start < end && b.End > start);
                if (clash) continue;

                int load = busy.Count(b => string.Equals(b.Who, ins.Email, StringComparison.OrdinalIgnoreCase));
                int score = (NormCode(ins.Licence).Equals(want, StringComparison.OrdinalIgnoreCase) ? 0 : 1000) + load;

                if (score < bestScore) { bestScore = score; best = ins; }
            }

            if (best == null) return false;

            instructorEmail = best.Email;
            instructorName = best.Name;
            return true;
        }

        private string GetAdminStaffEmailColumn(SqlConnection conn)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT TOP 1 COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
                      WHERE TABLE_NAME = 'Admin_Staff'
                        AND (COLUMN_NAME LIKE '%Email%' OR COLUMN_NAME LIKE '%Mail%')", conn))
                {
                    object r = cmd.ExecuteScalar();
                    if (r == null) return null;
                    string col = r.ToString();
                    return Regex.IsMatch(col, @"^[A-Za-z0-9_]+$") ? col : null;
                }
            }
            catch { return null; }
        }

        private static string NormCode(string s)
        {
            Match m = Regex.Match(s ?? "", @"(?i)code\s*0*(\d+)");
            return m.Success ? "Code " + m.Groups[1].Value : (s ?? "").Trim();
        }

        protected void btnLogoutSide_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Account/Login.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private class InstructorRow
        {
            public string Email;
            public string Name;
            public string Licence;
        }

        private class Interval
        {
            public string Who;
            public TimeSpan Start;
            public TimeSpan End;
        }
    }
}