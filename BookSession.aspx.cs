using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;

namespace WebApplicationM3
{
    public partial class BookSession : System.Web.UI.Page
    {
        private static readonly string[] SlotTimes =
            { "07:00", "08:00", "09:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00", "16:00" };

        private BookingDraft Draft
        {
            get
            {
                var d = Session["BookingDraft"] as BookingDraft;
                if (d == null) { d = new BookingDraft(); Session["BookingDraft"] = d; }
                return d;
            }
            set { Session["BookingDraft"] = value; }
        }

        private LinkButton[] SlotButtons
        {
            get
            {
                return new[] { slot0700, slot0800, slot0900, slot1000, slot1100,
                               slot1200, slot1300, slot1400, slot1500, slot1600 };
            }
        }

        // Static-cached licence-column check
        private static bool? _hasLicenceCol;
        private static readonly object _licenceColLock = new object();
        private bool StudentsHasLicenceColumn
        {
            get
            {
                lock (_licenceColLock)
                {
                    if (!_hasLicenceCol.HasValue)
                    {
                        try
                        {
                            string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                            using (SqlConnection conn = new SqlConnection(cs))
                            using (SqlCommand cmd = new SqlCommand(
                                @"SELECT COUNT(1) FROM INFORMATION_SCHEMA.COLUMNS
                                  WHERE TABLE_NAME='Students' AND COLUMN_NAME='HasLearnersLicence'", conn))
                            { conn.Open(); _hasLicenceCol = ((int)cmd.ExecuteScalar()) > 0; }
                        }
                        catch { _hasLicenceCol = false; }
                    }
                    return _hasLicenceCol.Value;
                }
            }
        }

        private bool? GetStoredLicenceFlag()
        {
            if (!StudentsHasLicenceColumn) return null;
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT HasLearnersLicence FROM Students WHERE StudNumber=@S", conn))
                {
                    cmd.Parameters.AddWithValue("@S", (int)Session["StudNumber"]);
                    conn.Open();
                    object r = cmd.ExecuteScalar();
                    if (r == null || r == DBNull.Value) return null;
                    return Convert.ToBoolean(r);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("BookSession.GetStoredLicenceFlag: " + ex);
                return null;
            }
        }

        private void SaveLicenceFlag(bool hasLicence)
        {
            if (!StudentsHasLicenceColumn) return;
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE Students SET HasLearnersLicence=@F WHERE StudNumber=@S", conn))
                {
                    cmd.Parameters.AddWithValue("@F", hasLicence);
                    cmd.Parameters.AddWithValue("@S", (int)Session["StudNumber"]);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("BookSession.SaveLicenceFlag: " + ex);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (StaffAuth.IsSignedIn)
            {
                ShowStaffBlocked();
                return;
            }

            if (Session["StudNumber"] == null)
            {
                Response.Redirect("~/Account/Login.aspx?return=BookSession", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                // SHOW SUCCESS BANNER IF RETURNING FROM PAYMENT
                if (Request.QueryString["newpayment"] == "1")
                {
                    pnlSuccessBanner.Visible = true;
                }

                if (Session["BookingDraft"] == null)
                    Draft = new BookingDraft();

                LoadTopBar();

                bool? licence = GetStoredLicenceFlag();
                if (licence == false)
                {
                    Response.Redirect("~/LearnersClass.aspx?nolicense=1", false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }

                LoadPackages();
                PrefillStudentDetails();

                if (licence == true)
                {
                    int studNumber = (int)Session["StudNumber"];

                    var reservation = PackageReservationService.GetCurrent(studNumber);
                    var activePackage = LessonProgress.GetActiveStudentPackage(studNumber);

                    if (activePackage != null && activePackage.RemainingLessons > 0)
                    {
                        Draft.PackageId = activePackage.PackageID;
                        Draft.PackageName = activePackage.PackageName;
                        var pkgDef = GetPackages().FirstOrDefault(p => p.Id == activePackage.PackageID);
                        if (pkgDef != null) ApplyPackageDef(pkgDef);

                        LockToExistingPackageIfAny();
                        ShowStep(3);
                        return;
                    }

                    if (reservation != null && reservation.IsActive)
                    {
                        LockToExistingPackageIfAny();
                        ShowStep(2);
                        ShowReservationActiveBanner(reservation);
                        return;
                    }

                    if (reservation != null && !reservation.IsActive && !reservation.IsPaid)
                        pnlReservationExpired.Visible = true;

                    LockToExistingPackageIfAny();
                    ShowStep(2);
                    return;
                }

                PrefillGate();
                ShowStep(1);
            }
        }

        // =========================================================
        // STAFF BLOCKED
        // =========================================================
        private void ShowStaffBlocked()
        {
            mvWizard.Visible = false;
            litStaffBlocked.Visible = true;
            litStepTitle.Text = "Staff account — booking not permitted";
            litStepSubtitle.Text = "This page is for students only";

            string fullName = StaffAuth.Name;
            string role = StaffAuth.Role;
            string dashUrl = role != null &&
                role.Equals("Instructor", StringComparison.OrdinalIgnoreCase)
                    ? ResolveUrl("~/InstructorDashboard.aspx")
                    : ResolveUrl("~/AdminDashboard.aspx");

            var sb = new StringBuilder();
            sb.Append("<div style='background:#fff;border-radius:12px;");
            sb.Append("box-shadow:0 4px 15px rgba(13,43,110,.08);padding:38px 32px;");
            sb.Append("text-align:center;'>");

            sb.Append("<div style='font-size:11px;font-weight:800;letter-spacing:3px;");
            sb.Append("color:#e8283f;text-transform:uppercase;margin-bottom:10px;'>Staff Account</div>");

            sb.Append("<div style='font-size:22px;font-weight:800;color:#0d2b6e;margin-bottom:14px;'>");
            sb.Append("You're signed in as staff</div>");

            sb.Append("<p style='color:#445;font-size:14px;line-height:1.7;max-width:520px;");
            sb.Append("margin:0 auto 22px;'>");
            sb.Append("Hi ").Append(Server.HtmlEncode(string.IsNullOrEmpty(fullName) ? "there" : fullName));
            sb.Append(", this page is only available to student accounts. Staff members cannot book lessons. ");
            sb.Append("If you need to make a booking for yourself, please sign out and log in with your student account.");
            sb.Append("</p>");

            sb.Append("<div style='display:flex;gap:12px;flex-wrap:wrap;justify-content:center;margin-top:6px;'>");
            sb.Append("<a href='").Append(dashUrl);
            sb.Append("' style='display:inline-block;padding:12px 30px;background:#0d2b6e;color:#fff;");
            sb.Append("border-radius:24px;font-weight:800;text-decoration:none;font-size:14px;'>Go to my dashboard</a>");

            sb.Append("<a href='").Append(ResolveUrl("~/Account/Login.aspx?logout=1"));
            sb.Append("' style='display:inline-block;padding:12px 30px;background:#e8283f;color:#fff;");
            sb.Append("border-radius:24px;font-weight:800;text-decoration:none;font-size:14px;'>");
            sb.Append("Sign out and log in as a student</a></div>");

            sb.Append("<div style='margin-top:24px;font-size:12.5px;color:#7787a5;'>");
            sb.Append("Need help? Contact reception on ");
            sb.Append("<a href='tel:+27835448544' style='color:#0d2b6e;font-weight:700;'>");
            sb.Append("+27 83 544 8544</a>.</div></div>");

            litStaffBlocked.Text = sb.ToString();
        }

        private void ShowReservationActiveBanner(PackageReservationService.Reservation r)
        {
            pnlReservationActive.Visible = true;
            litReservationMsg.Text =
                "Package: <strong>" + Server.HtmlEncode(r.PackageName) + "</strong><br/>" +
                "Deposit due: <strong>R " + r.DepositAmount.ToString("N2") + "</strong><br/>" +
                "Expires: <strong>" + r.ExpiresOn.ToString("dddd, d MMMM yyyy HH:mm") +
                "</strong> (" + r.DaysRemaining + " day" + (r.DaysRemaining == 1 ? "" : "s") + " remaining)" +
                "<br/><br/><a class='pay-btn' href='PayDeposit.aspx?id=" + r.ReservationID + "'>" +
                "&#128179; Pay 50% Deposit Now</a>" +
                " <span style='margin-left:12px;color:#7787a5;font-size:12px;'>" +
                "(You can also pay later from your dashboard)</span>";
        }

        private void LockToExistingPackageIfAny()
        {
            int studNumber = (int)Session["StudNumber"];
            var active = LessonProgress.GetActiveStudentPackage(studNumber);
            if (active == null) return;

            string reason;
            bool canSwitch = LessonProgress.CanStudentSwitchPackage(studNumber, out reason);

            if (canSwitch)
            {
                lblExistingPackageInfo.Text =
                    "You currently have <strong>" + Server.HtmlEncode(active.PackageName) +
                    "</strong> reserved (not yet started). You may choose a different package below — " +
                    "your old reservation will be released automatically.";
                lblExistingPackageInfo.Visible = true;
                ddlPackage.Enabled = true;
                return;
            }

            var toRemove = ddlPackage.Items.Cast<ListItem>()
                .Where(i => i.Value != active.PackageID.ToString()).ToList();
            foreach (var item in toRemove) ddlPackage.Items.Remove(item);

            if (ddlPackage.Items.Count > 0)
            {
                ddlPackage.SelectedValue = active.PackageID.ToString();
                var pkg = GetPackages().FirstOrDefault(p => p.Id == active.PackageID);
                if (pkg != null) ApplyPackageDef(pkg);
            }

            ddlPackage.Enabled = false;
            lblExistingPackageInfo.Text =
                "You already have an active package: <strong>" +
                Server.HtmlEncode(active.PackageName) + "</strong> with " +
                active.RemainingLessons + " lesson(s) remaining. " +
                (string.IsNullOrEmpty(reason)
                    ? "If you need a different package, please contact reception."
                    : Server.HtmlEncode(reason));
            lblExistingPackageInfo.Visible = true;
        }

        private void ApplyPackageDef(PackageOption pkg)
        {
            Draft.PackageId = pkg.Id;
            Draft.PackageName = pkg.Name;
            Draft.PackageType = pkg.PackageType;
            Draft.Price = pkg.Price;
            Draft.VehicleCategory = pkg.VehicleCategory;
            Draft.LicenceCode = NormCode(pkg.LicenceCode);
            Draft.Lessons = pkg.Lessons;
        }

        private void ShowStep(int step)
        {
            mvWizard.ActiveViewIndex = step - 1;
            switch (step)
            {
                case 1:
                    litStepTitle.Text = "Before You Begin";
                    litStepSubtitle.Text = "A few details we need before your first booking"; break;
                case 2:
                    litStepTitle.Text = "New Booking";
                    litStepSubtitle.Text = "Confirm your details and choose a package";
                    RestoreStep2UI(); UpdatePackageSummary(); break;
                case 3:
                    litStepTitle.Text = "Schedule";
                    litStepSubtitle.Text = "Choose a date, time and lesson type";
                    RestoreStep3UI(); break;
                case 4:
                    litStepTitle.Text = "Instructor & Vehicle Selection";
                    litStepSubtitle.Text = "Qualified and available at the selected time";
                    BindInstructorsVehicles(); break;
                case 5:
                    litStepTitle.Text = "Confirm Booking";
                    litStepSubtitle.Text = "Review the details and confirm";
                    BuildConfirmation(); break;
                case 6:
                    litStepTitle.Text = "Booking Confirmed!";
                    litStepSubtitle.Text = "Your lesson has been saved"; break;
            }
            Page.Title = litStepTitle.Text + " - Khwezi Driving School";
        }

        private void RestoreStep2UI()
        {
            if (Draft.PackageId > 0 && ddlPackage.Items.Count > 0)
            {
                var item = ddlPackage.Items.FindByValue(Draft.PackageId.ToString());
                if (item != null) ddlPackage.SelectedValue = Draft.PackageId.ToString();
            }
        }

        private void RestoreStep3UI()
        {
            if (Draft.Date == DateTime.MinValue)
                txtDate.Text = LessonProgress.TodaySa().AddDays(1).ToString("yyyy-MM-dd");
            else
                txtDate.Text = Draft.Date.ToString("yyyy-MM-dd");

            Draft.Date = GetSelectedDate();

            if (Draft.DurationHours > 0)
            {
                var durItem = ddlDuration.Items.FindByValue(Draft.DurationHours.ToString());
                if (durItem != null) ddlDuration.SelectedValue = Draft.DurationHours.ToString();
            }

            if (!string.IsNullOrEmpty(Draft.LessonType))
            {
                var t = ddlLessonType.Items.FindByText(Draft.LessonType);
                if (t != null) ddlLessonType.SelectedIndex = ddlLessonType.Items.IndexOf(t);
                else ApplyLessonTypeDefault();
            }
            else ApplyLessonTypeDefault();

            BindSlots();
        }

        private void ApplyLessonTypeDefault()
        {
            string suggestion = (!string.IsNullOrEmpty(Draft.PackageType) &&
                Draft.PackageType.StartsWith("Automatic", StringComparison.OrdinalIgnoreCase))
                ? "Automatic Driving" : "Manual Driving";

            if (ddlLessonType.Items.FindByText(suggestion) != null)
                ddlLessonType.SelectedValue = suggestion;

            Draft.LessonType = suggestion;
        }

        private void PrefillGate()
        {
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT DateOfBirth, StreetAddress, Suburb, City
                      FROM Students WHERE StudNumber=@S", conn))
                {
                    cmd.Parameters.AddWithValue("@S", (int)Session["StudNumber"]);
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            if (r["DateOfBirth"] != DBNull.Value)
                                txtDob.Text = Convert.ToDateTime(r["DateOfBirth"]).ToString("yyyy-MM-dd");
                            txtStreet.Text = r["StreetAddress"].ToString();
                            txtSuburb.Text = r["Suburb"].ToString();
                            txtCity.Text = r["City"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("BookSession.PrefillGate: " + ex);
            }
        }

        protected void btnGateBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/StudentDashboard.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void btnGateNext_Click(object sender, EventArgs e)
        {
            lblGateError.Visible = false;

            DateTime dob;
            if (txtDob.Text.Trim().Length == 0 || !DateTime.TryParse(txtDob.Text, out dob))
            { GateErr("Please provide your date of birth."); return; }
            if (dob > LessonProgress.TodaySa())
            { GateErr("Date of birth cannot be in the future."); return; }

            int age = LessonProgress.TodaySa().Year - dob.Year;
            if (dob.Date > LessonProgress.TodaySa().AddYears(-age)) age--;
            if (age < 16) { GateErr("You must be at least 16 years old to enrol."); return; }

            string street = txtStreet.Text.Trim(), suburb = txtSuburb.Text.Trim(), city = txtCity.Text.Trim();
            if (street.Length == 0 || suburb.Length == 0 || city.Length == 0)
            { GateErr("Please complete your street address, suburb and city."); return; }

            if (rblLearners.SelectedIndex < 0)
            { GateErr("Please tell us whether you have a learner's licence."); return; }

            SaveStudentDetails(dob, street, suburb, city);

            if (rblLearners.SelectedValue == "No")
            {
                SaveLicenceFlag(false);
                Response.Redirect("~/LearnersClass.aspx?nolicense=1", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            SaveLicenceFlag(true);
            ShowStep(2);
        }

        private void GateErr(string msg) { lblGateError.Text = msg; lblGateError.Visible = true; }

        private void SaveStudentDetails(DateTime dob, string street, string suburb, string city)
        {
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"UPDATE Students SET DateOfBirth=@D, StreetAddress=@A,
                              Suburb=@S, City=@C WHERE StudNumber=@N", conn))
                {
                    cmd.Parameters.AddWithValue("@D", dob.Date);
                    cmd.Parameters.AddWithValue("@A", street);
                    cmd.Parameters.AddWithValue("@S", suburb);
                    cmd.Parameters.AddWithValue("@C", city);
                    cmd.Parameters.AddWithValue("@N", (int)Session["StudNumber"]);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("BookSession.SaveStudentDetails: " + ex);
            }
        }

        private void LoadPackages()
        {
            ddlPackage.Items.Clear();
            foreach (var p in GetPackages())
                ddlPackage.Items.Add(new ListItem(
                    p.Name + "  —  R " + p.Price.ToString("N0") + "  (" + p.Lessons + " lessons)",
                    p.Id.ToString()));
        }

        protected void ddlPackage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlPackage.SelectedIndex < 0) return;
            int pkgId;
            if (!int.TryParse(ddlPackage.SelectedValue, out pkgId)) return;
            var pkg = GetPackages().FirstOrDefault(p => p.Id == pkgId);
            if (pkg == null) return;

            ApplyPackageDef(pkg);
            UpdatePackageSummary();
            ApplyLessonTypeDefault();
        }

        private void UpdatePackageSummary()
        {
            if (Draft.PackageId > 0)
            {
                int studNum = (int)Session["StudNumber"];
                var stats = LessonProgress.GetPackageStats(studNum, Draft.PackageId);

                litPkgName.Text = Draft.PackageName;
                litPkgCompleted.Text = stats.CompletedLessons.ToString();
                litPkgUpcoming.Text = stats.UpcomingLessons.ToString();
                litPkgRemaining.Text = Draft.Lessons.ToString();
                pnlPackageSummary.Visible = true;
            }
            else pnlPackageSummary.Visible = false;
        }

        private List<PackageOption> GetPackages()
        {
            var list = new List<PackageOption>();
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT PackageID, PackageName, PackageType, Amount, TotalLessons, VehicleCategory
                      FROM Packages WHERE PackageType <> 'Learners' ORDER BY Amount", conn))
                {
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                        while (r.Read())
                            list.Add(new PackageOption
                            {
                                Id = (int)r["PackageID"],
                                Name = r["PackageName"].ToString(),
                                PackageType = r["PackageType"].ToString(),
                                Price = Convert.ToDecimal(r["Amount"]),
                                Lessons = Convert.ToInt32(r["TotalLessons"]),
                                VehicleCategory = r["VehicleCategory"].ToString()
                            });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("BookSession.GetPackages: " + ex);
            }

            return list;
        }

        private void PrefillStudentDetails()
        {
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT Name, Surname, StudEmailAddress, PhoneNum
                      FROM Students WHERE StudNumber=@S", conn))
                {
                    cmd.Parameters.AddWithValue("@S", (int)Session["StudNumber"]);
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            string name = r["Name"].ToString();
                            string sur = r["Surname"].ToString();
                            string email = r["StudEmailAddress"].ToString().Trim().ToLowerInvariant();
                            string phone = r["PhoneNum"] == DBNull.Value ? "" : r["PhoneNum"].ToString();

                            txtFirstName.Text = name;
                            txtSurname.Text = sur;
                            txtEmail.Text = email;
                            txtPhone.Text = phone;
                            Session["StudEmail"] = email;

                            Draft.FirstName = name;
                            Draft.Surname = sur;
                            Draft.Phone = phone;
                            Draft.Email = email;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("BookSession.PrefillStudentDetails: " + ex);
            }
        }

        protected void btnDetailsBack_Click(object sender, EventArgs e) { ShowStep(1); }

        protected void btnDetailsNext_Click(object sender, EventArgs e)
        {
            lblDetailsError.Visible = false;

            string first = txtFirstName.Text.Trim();
            string sur = txtSurname.Text.Trim();
            string phone = txtPhone.Text.Replace(" ", "");
            string email = txtEmail.Text.Trim().ToLowerInvariant();

            if (first.Length == 0 || sur.Length == 0)
            { DetailsErr("Please enter the student's first name and surname."); return; }
            if (!Regex.IsMatch(phone, @"^0\d{9}$"))
            { DetailsErr("Phone must be 10 digits starting with 0."); return; }
            if (!IsValidEmail(email))
            { DetailsErr("Please enter a valid email address."); return; }
            if (ddlPackage.SelectedIndex < 0)
            { DetailsErr("Please select a package."); return; }

            int pkgId;
            if (!int.TryParse(ddlPackage.SelectedValue, out pkgId))
            { DetailsErr("Please select a package."); return; }

            var pkg = GetPackages().FirstOrDefault(p => p.Id == pkgId);
            if (pkg == null) { DetailsErr("Package no longer available."); return; }

            int studNumber = (int)Session["StudNumber"];

            var existing = PackageReservationService.GetCurrent(studNumber);
            if (existing != null && existing.IsActive && existing.PackageID == pkgId)
            {
                Response.Redirect("~/PayDeposit.aspx?id=" + existing.ReservationID, false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            var active = LessonProgress.GetActiveStudentPackage(studNumber);
            if (active != null && active.PackageID == pkgId && active.RemainingLessons > 0)
            {
                ApplyPackageDef(pkg);
                Draft.FirstName = first; Draft.Surname = sur;
                Draft.Phone = phone; Draft.Email = email;
                SavePhoneIfEmpty(phone);
                ShowStep(3);
                return;
            }

            var currentActive = LessonProgress.GetActiveStudentPackage(studNumber);
            if (currentActive != null && currentActive.PackageID != pkgId)
            {
                string switchReason;
                bool canSwitch = LessonProgress.CanStudentSwitchPackage(studNumber, out switchReason);
                if (!canSwitch) { DetailsErr(switchReason); return; }
                LessonProgress.DeactivateStudentPackage(studNumber, currentActive.PackageID);
            }

            string block = LessonProgress.GetBookingBlockReasonMulti(studNumber, email, pkgId, 5);
            if (block != null) { DetailsErr(block); return; }

            Draft.FirstName = first; Draft.Surname = sur;
            Draft.Phone = phone; Draft.Email = email;
            ApplyPackageDef(pkg);
            Draft.LessonType = null;

            decimal deposit = decimal.Round(pkg.Price * 0.50m, 2);
            string msg;
            int resId = PackageReservationService.Create(
                studNumber, pkgId, pkg.Price, deposit, out msg);

            if (resId <= 0)
            {
                DetailsErr("We couldn't create your reservation right now. Please try again, or contact reception on +27 83 544 8544.");
                return;
            }

            SavePhoneIfEmpty(phone);

            try { EmailHelper.SendPackageReservationReminder(studNumber, resId, 0); }
            catch (Exception mailEx) { System.Diagnostics.Trace.TraceError("Reservation reminder email failed: " + mailEx); }

            Response.Redirect("~/PayDeposit.aspx?id=" + resId, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void DetailsErr(string msg) { lblDetailsError.Text = msg; lblDetailsError.Visible = true; }

        private void SavePhoneIfEmpty(string phone)
        {
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"UPDATE Students SET PhoneNum=@P
                      WHERE StudNumber=@S AND (PhoneNum IS NULL OR PhoneNum='')", conn))
                {
                    cmd.Parameters.AddWithValue("@P", phone);
                    cmd.Parameters.AddWithValue("@S", (int)Session["StudNumber"]);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("BookSession.SavePhoneIfEmpty: " + ex);
            }
        }

        private DateTime GetSelectedDate()
        {
            DateTime d;
            if (DateTime.TryParse(txtDate.Text, out d)) return d.Date;
            return DateTime.MinValue;
        }

        protected void txtDate_TextChanged(object sender, EventArgs e) { Draft.Date = GetSelectedDate(); BindSlots(); }

        protected void ddlDuration_SelectedIndexChanged(object sender, EventArgs e)
        {
            int dur;
            if (!int.TryParse(ddlDuration.SelectedValue, out dur) || dur < 1) dur = 1;
            if (dur > 4) dur = 4;
            Draft.DurationHours = dur;
            BindSlots();
        }

        protected void Slot_Command(object sender, CommandEventArgs e)
        {
            TimeSpan slot;
            if (!TimeSpan.TryParse(e.CommandArgument.ToString(), out slot)) return;
            Draft.Start = (Draft.Start == slot) ? (TimeSpan?)null : slot;
            BindSlots();
        }

        private void BindSlots()
        {
            int dur = Draft.DurationHours <= 0 ? 1 : Draft.DurationHours;
            int durMin = dur * 60;

            List<SlotBooking> day = (Draft.Date == DateTime.MinValue)
                ? new List<SlotBooking>() : GetDayBookings(Draft.Date);
            List<SlotBooking> lessons = (Draft.Date == DateTime.MinValue)
                ? new List<SlotBooking>() : GetDayLessons(Draft.Date);

            string wantedSpec = (!string.IsNullOrEmpty(Draft.LessonType) &&
                Draft.LessonType.StartsWith("Automatic", StringComparison.OrdinalIgnoreCase))
                ? "Automatic" : "Manual";
            var qualified = GetQualifiedInstructors(wantedSpec);

            bool studentBlocked = Draft.Date != DateTime.MinValue &&
                GetSameDayBookingBlockReason(Draft.Date) != null;

            DateTime now = LessonProgress.NowSa();
            bool isToday = Draft.Date.Date == now.Date;

            int free = 0, taken = 0;
            LinkButton[] buttons = SlotButtons;

            for (int i = 0; i < SlotTimes.Length; i++)
            {
                TimeSpan start = TimeSpan.Parse(SlotTimes[i]);
                TimeSpan end = start.Add(TimeSpan.FromMinutes(durMin));

                bool isPast = isToday && start <= now.TimeOfDay;

                var busySet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var b in day)
                    if (b.Start < end && b.End > start && !string.IsNullOrEmpty(b.StaffEmail))
                        busySet.Add(b.StaffEmail.Trim());
                foreach (var l in lessons)
                    if (l.Start < end && l.End > start && !string.IsNullOrEmpty(l.InstructorEmail))
                        busySet.Add(l.InstructorEmail.Trim());

                int freeIns = qualified.Count(q => !busySet.Contains(q.Email));
                bool isTaken = freeIns == 0;
                bool isSelected = Draft.Start == start;

                if (studentBlocked || isPast || (isTaken && !isSelected))
                {
                    buttons[i].CssClass = "slot taken";
                    buttons[i].Enabled = false;
                    taken++;
                }
                else
                {
                    buttons[i].CssClass = isSelected ? "slot selected" : "slot";
                    buttons[i].Enabled = true;
                    free++;
                }
            }

            if (studentBlocked)
                litSlotCounts.Text = "0 available — you already have a booking on this day";
            else if (isToday && taken == SlotTimes.Length)
                litSlotCounts.Text = "0 available — today's slots have all passed";
            else
                litSlotCounts.Text = free + " available | " + taken + " taken";
        }

        protected void btnScheduleBack_Click(object sender, EventArgs e)
        {
            Draft.Date = GetSelectedDate();
            int dur;
            if (int.TryParse(ddlDuration.SelectedValue, out dur) && dur > 0)
                Draft.DurationHours = dur;
            Draft.LessonType = ddlLessonType.SelectedValue;
            ShowStep(2);
        }

        protected void btnScheduleNext_Click(object sender, EventArgs e)
        {
            lblScheduleError.Visible = false;
            Draft.Date = GetSelectedDate();
            int dur;
            if (int.TryParse(ddlDuration.SelectedValue, out dur) && dur > 0)
                Draft.DurationHours = dur;
            Draft.LessonType = ddlLessonType.SelectedValue;

            if (Draft.Date == DateTime.MinValue) { ScheduleErr("Please choose a date."); return; }
            if (Draft.Date < LessonProgress.TodaySa()) { ScheduleErr("Please choose today or a future date."); return; }
            if (Draft.Start == null) { ScheduleErr("Please select a time slot."); return; }

            DateTime scheduled = Draft.Date.Add(Draft.Start.Value);
            int durMin = Math.Max(1, Draft.DurationHours) * 60;
            string slotErr = LessonProgress.ValidateSlot(scheduled, durMin);
            if (slotErr != null) { ScheduleErr(slotErr); return; }

            string sameDayBlock = GetSameDayBookingBlockReason(Draft.Date);
            if (sameDayBlock != null) { ScheduleErr(sameDayBlock); return; }

            ShowStep(4);
        }

        private void ScheduleErr(string msg) { lblScheduleError.Text = msg; lblScheduleError.Visible = true; }

        private string GetSameDayBookingBlockReason(DateTime date)
        {
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT TOP 1 BookingID, CAST(ScheduledDateTime AS TIME) AS T
                      FROM Booking
                      WHERE StudEmailAddress = (SELECT StudEmailAddress FROM Students WHERE StudNumber=@S)
                        AND CAST(ScheduledDateTime AS DATE) = @D
                        AND BookingStatus IN ('Unsettled','Pending','Confirmed')
                      ORDER BY ScheduledDateTime", conn))
                {
                    cmd.Parameters.AddWithValue("@S", (int)Session["StudNumber"]);
                    cmd.Parameters.AddWithValue("@D", date.Date);
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            int otherId = Convert.ToInt32(r["BookingID"]);
                            TimeSpan otherTime = (TimeSpan)r["T"];
                            return "You already have a booking (#" + otherId + ") on " +
                                   date.ToString("dddd, d MMMM yyyy") + " at " + FormatTime(otherTime) +
                                   ". Only one lesson per day is allowed.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("BookSession.GetSameDayBookingBlockReason: " + ex);
            }
            return null;
        }

        private void BindInstructorsVehicles()
        {
            bool wantsAutomatic = !string.IsNullOrEmpty(Draft.LessonType) &&
                Draft.LessonType.StartsWith("Automatic", StringComparison.OrdinalIgnoreCase);
            string wantedSpecialization = wantsAutomatic ? "Automatic" : "Manual";
            var qualified = GetQualifiedInstructors(wantedSpecialization);

            lstInstructors.Items.Clear();
            lstVehicles.Items.Clear();

            if (Draft.Start == null)
            {
                litAvailNote.Text = "Pick any green time below for " +
                    Draft.Date.ToString("dddd, d MMMM yyyy") + " — everything else stays the same.";
                ShowSuggestions(qualified);
                pnlNoInstructors.Visible = true;
                return;
            }

            TimeSpan start = Draft.Start.Value;
            TimeSpan end = start.Add(TimeSpan.FromMinutes(Math.Max(1, Draft.DurationHours) * 60));

            var busyInstructors = GetBusyInstructors(Draft.Date, start, end);
            var busyVehicles = GetBusyVehicles(Draft.Date, start, end);

            int insCount = 0, vehCount = 0;

            foreach (var ins in qualified.Where(i => !busyInstructors.Contains(i.Email)))
            {
                lstInstructors.Items.Add(new ListItem(ins.Display, ins.Email));
                insCount++;
            }
            if (lstInstructors.Items.Count > 0) lstInstructors.SelectedIndex = 0;

            foreach (var v in GetVehiclesForCategory(Draft.VehicleCategory))
            {
                if (busyVehicles.Contains(v.Key, StringComparer.OrdinalIgnoreCase)) continue;
                lstVehicles.Items.Add(new ListItem(v.Value, v.Key));
                vehCount++;
            }
            if (lstVehicles.Items.Count > 0) lstVehicles.SelectedIndex = 0;

            string typeText = wantsAutomatic ? "automatic" : "manual";
            if (insCount == 0)
            {
                litAvailNote.Text = "No " + typeText + " instructors are free on " +
                    Draft.Date.ToString("dddd, d MMMM yyyy") + " at " + FormatTime(start) +
                    ". No need to go back — choose any green time below.";
                ShowSuggestions(qualified);
                pnlNoInstructors.Visible = true;
            }
            else
            {
                pnlNoInstructors.Visible = false;
                litAvailNote.Text = insCount + " " + typeText + " instructor(s) and " + vehCount +
                    " " + typeText + " vehicle(s) available on " + Draft.Date.ToString("dddd, d MMMM yyyy") +
                    " at " + FormatTime(start) + ".";
            }
        }

        private List<InstructorOption> GetQualifiedInstructors(string wanted)
        {
            var admin = GetAdminStaffEmails();
            return GetInstructors()
                .Where(i => !string.IsNullOrEmpty(i.Specialization) &&
                    i.Specialization.IndexOf(wanted, StringComparison.OrdinalIgnoreCase) >= 0)
                .Where(i => !admin.Any() || admin.Contains(i.Email))
                .ToList();
        }

        // =========================================================
        // SUGGESTIONS — one query for the whole 7-day window
        // =========================================================
        private class WeekBusyItem
        {
            public DateTime Date;
            public string StaffEmail;
            public TimeSpan Start;
            public TimeSpan End;
        }

        private List<WeekBusyItem> GetWeekBusy(DateTime start, DateTime end)
        {
            var list = new List<WeekBusyItem>();
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT CAST(ScheduledDateTime AS DATE) AS D,
                             StafEmailAddress AS Email,
                             CAST(ScheduledDateTime AS TIME) AS T,
                             ISNULL(DurationMinutes, 60) AS Dur
                      FROM Booking
                      WHERE CAST(ScheduledDateTime AS DATE) BETWEEN @From AND @To
                        AND BookingStatus <> 'Cancelled'
                      UNION ALL
                      SELECT CAST(LessonDateTime AS DATE),
                             InstruEmailAddress,
                             CAST(LessonDateTime AS TIME),
                             ISNULL(Duration, 60)
                      FROM Lesson
                      WHERE CAST(LessonDateTime AS DATE) BETWEEN @From AND @To
                        AND Status <> 'Cancelled'", conn))
                {
                    cmd.Parameters.AddWithValue("@From", start.Date);
                    cmd.Parameters.AddWithValue("@To", end.Date);
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                        while (r.Read())
                        {
                            TimeSpan s = (TimeSpan)r["T"];
                            int dur = Convert.ToInt32(r["Dur"]);
                            list.Add(new WeekBusyItem
                            {
                                Date = Convert.ToDateTime(r["D"]).Date,
                                StaffEmail = r["Email"] == DBNull.Value ? null : r["Email"].ToString().Trim(),
                                Start = s,
                                End = s.Add(TimeSpan.FromMinutes(dur))
                            });
                        }
                }
            }
            catch (Exception ex) { System.Diagnostics.Trace.TraceError("BookSession.GetWeekBusy: " + ex); }
            return list;
        }

        private void ShowSuggestions(List<InstructorOption> qualified)
        {
            litSuggestDate.Text = Draft.Date.ToString("dddd, d MMMM yyyy");
            int durMin = Math.Max(1, Draft.DurationHours) * 60;
            DateTime weekStart = Draft.Date.Date;

            var weekBusy = GetWeekBusy(weekStart, weekStart.AddDays(6));
            var dayBusy = weekBusy.Where(x => x.Date == weekStart).ToList();

            var slotItems = new List<SlotSuggestion>();
            foreach (string t in SlotTimes)
            {
                TimeSpan s = TimeSpan.Parse(t);
                TimeSpan e = s.Add(TimeSpan.FromMinutes(durMin));
                bool free = qualified.Any(i =>
                    !dayBusy.Any(b => b.Start < e && b.End > s &&
                        !string.IsNullOrEmpty(b.StaffEmail) &&
                        string.Equals(b.StaffEmail, i.Email, StringComparison.OrdinalIgnoreCase)));
                bool isSel = Draft.Start == s;
                slotItems.Add(new SlotSuggestion
                {
                    TimeText = t,
                    TimeArg = t,
                    IsAvailable = free || isSel,
                    CssClass = isSel ? "slot selected" : (free ? "slot" : "slot taken")
                });
            }
            repSlotSuggestions.DataSource = slotItems;
            repSlotSuggestions.DataBind();

            var dayItems = new List<DaySuggestion>();
            for (int off = 0; off < 7; off++)
            {
                DateTime d = weekStart.AddDays(off);
                var dBusy = weekBusy.Where(x => x.Date == d).ToList();
                int freeSlots = CountFreeSlotsWithData(dBusy, durMin, qualified);
                dayItems.Add(new DaySuggestion
                {
                    DateArg = d.ToString("yyyy-MM-dd"),
                    DateText = d.ToString("ddd, d MMM") + " · " + freeSlots +
                               (freeSlots == 1 ? " slot" : " slots"),
                    HasAvailability = freeSlots > 0,
                    CssClass = freeSlots > 0 ? "day-chip" : "day-chip empty"
                });
            }
            repDaySuggestions.DataSource = dayItems;
            repDaySuggestions.DataBind();
        }

        private int CountFreeSlotsWithData(
            List<WeekBusyItem> dayBusy, int durMin, List<InstructorOption> qualified)
        {
            int n = 0;
            foreach (string t in SlotTimes)
            {
                TimeSpan s = TimeSpan.Parse(t);
                TimeSpan e = s.Add(TimeSpan.FromMinutes(durMin));
                bool free = qualified.Any(i =>
                    !dayBusy.Any(b => b.Start < e && b.End > s &&
                        !string.IsNullOrEmpty(b.StaffEmail) &&
                        string.Equals(b.StaffEmail, i.Email, StringComparison.OrdinalIgnoreCase)));
                if (free) n++;
            }
            return n;
        }

        protected void SuggestionSlot_Command(object sender, CommandEventArgs e)
        {
            TimeSpan slot;
            if (!TimeSpan.TryParse(e.CommandArgument.ToString(), out slot)) return;
            Draft.Start = slot;
            BindInstructorsVehicles();
        }

        protected void SuggestionDay_Command(object sender, CommandEventArgs e)
        {
            DateTime d;
            if (!DateTime.TryParse(e.CommandArgument.ToString(), out d)) return;
            Draft.Date = d; Draft.Start = null;
            BindInstructorsVehicles();
        }

        private List<InstructorOption> GetInstructors()
        {
            var list = new List<InstructorOption>();
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT InstruEmailAddress, Name, Surname, LicenceType, Specialization
                      FROM Instructor ORDER BY Name", conn))
                {
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                        while (r.Read())
                            list.Add(new InstructorOption
                            {
                                Email = r["InstruEmailAddress"].ToString().Trim().ToLowerInvariant(),
                                Name = r["Name"].ToString().Trim() + " " + r["Surname"].ToString().Trim(),
                                LicenceType = r["LicenceType"].ToString().Trim(),
                                Specialization = r["Specialization"].ToString().Trim()
                            });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("BookSession.GetInstructors: " + ex);
            }
            return list;
        }

        private HashSet<string> GetAdminStaffEmails()
        {
            var set = new HashSet<string>();
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT LOWER(LTRIM(RTRIM(StafEmailAddress))) FROM Admin_Staff", conn))
                {
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                        while (r.Read()) set.Add(r.GetString(0));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("BookSession.GetAdminStaffEmails: " + ex);
            }
            return set;
        }

        private Dictionary<string, string> GetVehiclesForCategory(string category)
        {
            var dict = new Dictionary<string, string>();
            string cat = string.IsNullOrEmpty(category) ? "Light Motor Vehicle" : category;
            bool wantsAuto = cat.StartsWith("Automatic", StringComparison.OrdinalIgnoreCase);
            bool wantsManual = cat.StartsWith("Manual", StringComparison.OrdinalIgnoreCase);

            if (!wantsAuto && !wantsManual && !string.IsNullOrEmpty(Draft.LessonType))
            {
                if (Draft.LessonType.StartsWith("Automatic", StringComparison.OrdinalIgnoreCase)) wantsAuto = true;
                else wantsManual = true;
            }

            string core = cat.Replace("Automatic", "").Replace("Manual", "").Trim();

            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT VehicleID, VehicleType, TransmissionType FROM Vehicles
                      WHERE Status='Operational' ORDER BY VehicleType, VehicleID", conn))
                {
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                        while (r.Read())
                        {
                            string type = r["VehicleType"].ToString().Trim();
                            string trans = r["TransmissionType"].ToString().Trim();

                            bool transMatch;
                            if (wantsAuto) transMatch = trans.Equals("Automatic", StringComparison.OrdinalIgnoreCase);
                            else if (wantsManual) transMatch = trans.Equals("Manual", StringComparison.OrdinalIgnoreCase);
                            else transMatch = true;
                            if (!transMatch) continue;

                            bool typeMatch = string.IsNullOrEmpty(core) ||
                                type.Equals(core, StringComparison.OrdinalIgnoreCase) ||
                                (core.Equals("Light Motor Vehicle", StringComparison.OrdinalIgnoreCase) &&
                                 type.Equals("Light Vehicle", StringComparison.OrdinalIgnoreCase)) ||
                                (core.Equals("Medium Truck", StringComparison.OrdinalIgnoreCase) &&
                                 type.Equals("Heavy Truck", StringComparison.OrdinalIgnoreCase));
                            if (!typeMatch) continue;

                            dict[r["VehicleID"].ToString()] =
                                r["VehicleID"] + "  —  " + type + " (" + trans + ")";
                        }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("BookSession.GetVehiclesForCategory: " + ex);
            }

            if (dict.Count == 0)
                dict["TBD"] = "No matching vehicles available — reception will assign one";
            return dict;
        }

        private HashSet<string> GetBusyInstructors(DateTime date, TimeSpan start, TimeSpan end)
        {
            var busy = new HashSet<string>();
            foreach (var b in GetDayBookings(date))
                if (b.Start < end && b.End > start && b.StaffEmail != null)
                    busy.Add(b.StaffEmail.ToLowerInvariant());
            foreach (var l in GetDayLessons(date))
                if (l.Start < end && l.End > start && !string.IsNullOrEmpty(l.InstructorEmail))
                    busy.Add(l.InstructorEmail.ToLowerInvariant());
            return busy;
        }

        private HashSet<string> GetBusyVehicles(DateTime date, TimeSpan start, TimeSpan end)
        {
            var busy = new HashSet<string>();
            foreach (var b in GetDayBookings(date))
                if (b.Start < end && b.End > start && !string.IsNullOrEmpty(b.VehicleId))
                    busy.Add(b.VehicleId.Trim());
            foreach (var l in GetDayLessons(date))
                if (l.Start < end && l.End > start && !string.IsNullOrEmpty(l.VehicleId))
                    busy.Add(l.VehicleId.Trim());
            return busy;
        }

        protected void btnAssignBack_Click(object sender, EventArgs e) { ShowStep(3); }

        protected void btnAssignNext_Click(object sender, EventArgs e)
        {
            lblAssignError.Visible = false;
            if (lstInstructors.SelectedIndex < 0) { AssignErr("Please select an instructor."); return; }
            if (lstVehicles.Items.Count > 0 && lstVehicles.SelectedIndex < 0) { AssignErr("Please select a vehicle."); return; }

            Draft.InstructorEmail = lstInstructors.SelectedValue;
            Draft.InstructorName = lstInstructors.SelectedItem.Text;
            Draft.VehicleId = lstVehicles.SelectedIndex >= 0 ? lstVehicles.SelectedValue : null;
            Draft.VehicleText = lstVehicles.SelectedIndex >= 0 ? lstVehicles.SelectedItem.Text : "";
            ShowStep(5);
        }

        private void AssignErr(string msg) { lblAssignError.Text = msg; lblAssignError.Visible = true; }

        private void BuildConfirmation()
        {
            TimeSpan start = Draft.Start ?? TimeSpan.Zero;
            TimeSpan end = start.Add(TimeSpan.FromMinutes(Math.Max(1, Draft.DurationHours) * 60));
            decimal deposit = Draft.Price * 0.50m;

            var sb = new StringBuilder();
            Row(sb, "Student", Draft.FirstName + " " + Draft.Surname);
            Row(sb, "Email", Draft.Email);
            Row(sb, "Package", Draft.PackageName);
            Row(sb, "Date & Time", Draft.Date.ToString("dddd, d MMMM yyyy") + "  ·  " + FormatTime(start) + " to " + FormatTime(end));
            Row(sb, "Duration", Draft.DurationHours + (Draft.DurationHours == 1 ? " hour" : " hours"));
            Row(sb, "Vehicle", string.IsNullOrEmpty(Draft.VehicleText) ? "To be assigned" : Draft.VehicleText);
            Row(sb, "Instructor", Draft.InstructorName);
            Row(sb, "Package Total", "R " + Draft.Price.ToString("N2"));
            Row(sb, "Deposit Already Paid", "R " + deposit.ToString("N2"));
            Row(sb, "Booked On", LessonProgress.NowSa().ToString("dd MMM yyyy, HH:mm"));

            litConfirmDetails.Text = sb.ToString();
        }

        private void Row(StringBuilder sb, string label, string value)
        {
            sb.Append("<div class='c-row'><span class='c-label'>").Append(label)
              .Append("</span><span class='c-value'>").Append(Server.HtmlEncode(value ?? ""))
              .Append("</span></div>");
        }

        protected void btnConfirmBack_Click(object sender, EventArgs e) { ShowStep(4); }

        protected void btnConfirm_Click(object sender, EventArgs e)
        {
            lblConfirmError.Visible = false;

            if (StaffAuth.IsSignedIn) { ShowStaffBlocked(); return; }

            if (GetStoredLicenceFlag() == false)
            {
                Response.Redirect("~/LearnersClass.aspx?nolicense=1", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (string.IsNullOrEmpty(Draft.Email) ||
                string.IsNullOrEmpty(Draft.FirstName) ||
                string.IsNullOrEmpty(Draft.InstructorEmail) ||
                Draft.PackageId <= 0 ||
                Draft.Date == DateTime.MinValue ||
                Draft.Start == null)
            {
                lblConfirmError.Text = "Some booking details were lost. Please start the booking again.";
                lblConfirmError.Visible = true;
                ShowStep(2);
                return;
            }

            int studNumber = (int)Session["StudNumber"];
            var active = LessonProgress.GetActiveStudentPackage(studNumber);
            if (active == null || active.RemainingLessons <= 0)
            {
                lblConfirmError.Text = "You need a paid package to book lessons. Redirecting…";
                lblConfirmError.Visible = true;
                Response.Redirect("~/BookSession.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            int durationMinutes = Math.Max(1, Draft.DurationHours) * 60;
            TimeSpan start = Draft.Start.Value;
            TimeSpan end = start.Add(TimeSpan.FromMinutes(durationMinutes));
            DateTime scheduled = Draft.Date.Add(start);

            string err;
            int bookingId = LessonProgress.CreateBooking(
                Draft.Email,
                Draft.InstructorEmail,
                Draft.PackageId,
                scheduled,
                durationMinutes,
                Draft.VehicleId,
                out err,
                paidInFull: true,
                sendEmail: false);

            if (bookingId <= 0)
            {
                lblConfirmError.Text = err ?? "Could not save the booking.";
                lblConfirmError.Visible = true;

                if (!string.IsNullOrEmpty(err) &&
                    (err.IndexOf("instructor", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     err.IndexOf("vehicle", StringComparison.OrdinalIgnoreCase) >= 0))
                    ShowStep(4);
                else if (!string.IsNullOrEmpty(err) &&
                    (err.IndexOf("slot", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     err.IndexOf("one lesson per day", StringComparison.OrdinalIgnoreCase) >= 0))
                    ShowStep(3);
                return;
            }

            try
            {
                string studentName = Draft.FirstName + " " + Draft.Surname;
                EmailHelper.SendBookingReceivedEmail(Draft.Email, studentName, bookingId,
                    Draft.PackageName, Draft.Date, start, end,
                    Draft.InstructorName, Draft.VehicleText);
            }
            catch (Exception mailEx)
            {
                System.Diagnostics.Trace.TraceError("Booking email failed: " + mailEx);
            }

            string pkgName = Draft.PackageName;
            DateTime bookedDate = Draft.Date;
            string instructorName = Draft.InstructorName;

            Session["BookingDraft"] = null;

            litSuccess.Text =
                "<div class='success-icon'>&#10004;</div>" +
                "<h3>Booking #" + bookingId + " confirmed!</h3>" +
                "<p>" + Server.HtmlEncode(pkgName) + " on " +
                    bookedDate.ToString("dddd, d MMMM yyyy") + " at " + FormatTime(start) +
                    " with " + Server.HtmlEncode(instructorName) + ".</p>" +
                "<p class='success-sub'>Your package is paid, so no further payment is needed for this lesson. " +
                    "You have <strong>" + Math.Max(0, active.RemainingLessons - 1) + "</strong> lesson(s) left.</p>";

            ShowStep(6);
        }

        protected void btnGoDashboard_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/StudentDashboard.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void btnLogoutSide_Click(object sender, EventArgs e)
        {
            Session.Clear(); Session.Abandon();
            Response.Redirect("~/Account/Login.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private List<SlotBooking> GetDayBookings(DateTime date)
        {
            var list = new List<SlotBooking>();
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT StafEmailAddress, VehicleID,
                             CAST(ScheduledDateTime AS TIME) AS StartTime,
                             ISNULL(DurationMinutes, 60) AS DurationMinutes
                      FROM Booking
                      WHERE CAST(ScheduledDateTime AS DATE) = @D
                        AND BookingStatus <> 'Cancelled'", conn))
                {
                    cmd.Parameters.AddWithValue("@D", date);
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                        while (r.Read())
                        {
                            TimeSpan s = (TimeSpan)r["StartTime"];
                            int mins = Convert.ToInt32(r["DurationMinutes"]);
                            list.Add(new SlotBooking
                            {
                                StaffEmail = r["StafEmailAddress"] == DBNull.Value
                                    ? null : r["StafEmailAddress"].ToString(),
                                VehicleId = r["VehicleID"] == DBNull.Value
                                    ? null : r["VehicleID"].ToString(),
                                Start = s,
                                End = s.Add(TimeSpan.FromMinutes(mins))
                            });
                        }
                }
            }
            catch (Exception ex) { System.Diagnostics.Trace.TraceError("GetDayBookings: " + ex); }
            return list;
        }

        private List<SlotBooking> GetDayLessons(DateTime date)
        {
            var list = new List<SlotBooking>();
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT InstruEmailAddress, VehicleID,
                             CAST(LessonDateTime AS TIME) AS StartTime, Duration
                      FROM Lesson
                      WHERE CAST(LessonDateTime AS DATE) = @D AND Status <> 'Cancelled'", conn))
                {
                    cmd.Parameters.AddWithValue("@D", date);
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                        while (r.Read())
                        {
                            TimeSpan s = (TimeSpan)r["StartTime"];
                            int mins = r["Duration"] == DBNull.Value
                                ? 60 : Convert.ToInt32(r["Duration"]);
                            list.Add(new SlotBooking
                            {
                                InstructorEmail = r["InstruEmailAddress"] == DBNull.Value
                                    ? null : r["InstruEmailAddress"].ToString(),
                                VehicleId = r["VehicleID"] == DBNull.Value
                                    ? null : r["VehicleID"].ToString(),
                                Start = s,
                                End = s.Add(TimeSpan.FromMinutes(mins))
                            });
                        }
                }
            }
            catch (Exception ex) { System.Diagnostics.Trace.TraceError("GetDayLessons: " + ex); }
            return list;
        }

        private void LoadTopBar()
        {
            string fullName = Session["FullName"] != null ? Session["FullName"].ToString() : "";
            if (fullName.Length == 0) fullName = "Student";
            litStudentName.Text = fullName;
            litInitials.Text = GetInitials(fullName);

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

        private string FormatTime(TimeSpan t) { return t.ToString("hh\\:mm"); }

        private bool IsValidEmail(string email)
        {
            try { var addr = new System.Net.Mail.MailAddress(email); return addr.Address == email; }
            catch { return false; }
        }

        private static string NormCode(string s)
        {
            Match m = Regex.Match(s ?? "", @"(?i)code\s*0*(\d+)");
            return m.Success ? "Code " + m.Groups[1].Value : (s ?? "").Trim();
        }
    }

    [Serializable]
    public class BookingDraft
    {
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int PackageId { get; set; }
        public string PackageName { get; set; }
        public string PackageType { get; set; }
        public decimal Price { get; set; }
        public int Lessons { get; set; }
        public string VehicleCategory { get; set; }
        public string LicenceCode { get; set; }
        public string LessonType { get; set; }
        public int DurationHours { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan? Start { get; set; }
        public string InstructorEmail { get; set; }
        public string InstructorName { get; set; }
        public string VehicleId { get; set; }
        public string VehicleText { get; set; }

        public BookingDraft() { DurationHours = 1; Date = DateTime.MinValue; }
    }

    public class SlotBooking
    {
        public string StaffEmail { get; set; }
        public string InstructorEmail { get; set; }
        public string VehicleId { get; set; }
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
    }

    public class InstructorOption
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string LicenceType { get; set; }
        public string Specialization { get; set; }
        public string Display { get { return Name + "  —  " + LicenceType + " (" + Specialization + ")"; } }
    }

    public class PackageOption
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PackageType { get; set; }
        public decimal Price { get; set; }
        public int Lessons { get; set; }
        public string VehicleCategory { get; set; }
        public string LicenceCode { get; set; }
    }

    public class SlotSuggestion
    {
        public string TimeText { get; set; }
        public string TimeArg { get; set; }
        public bool IsAvailable { get; set; }
        public string CssClass { get; set; }
    }

    public class DaySuggestion
    {
        public string DateArg { get; set; }
        public string DateText { get; set; }
        public bool HasAvailability { get; set; }
        public string CssClass { get; set; }
    }
}