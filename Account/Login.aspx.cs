 using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;

namespace WebApplicationM3.Account
{
    public partial class Login : System.Web.UI.Page
    {
        private bool _dbUnreachable;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Opportunistic stale-payment cleanup.
            // Runs at most once every 30 minutes thanks to the app cache.
            TryCleanupStalePayments();

            if (IsPostBack) return;

            if (Session["StudNumber"] != null)
            {
                Response.Redirect("~/StudentDashboard.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (StaffAuth.IsSignedIn)
            {
                RouteStaff();
            }
        }

        // =========================================================
        // STALE PAYMENT CLEANUP (Option B)
        //
        // Runs the CleanupStalePayments stored procedure at most
        // once every 30 minutes across the whole application.
        // Cheap, out of band, and safe — the procedure itself
        // skips rows referenced by Transactions.
        // =========================================================
        private void TryCleanupStalePayments()
        {
            try
            {
                object last = Application["LastStalePaymentCleanup"];
                DateTime lastRun = last is DateTime ? (DateTime)last : DateTime.MinValue;

                if ((DateTime.Now - lastRun).TotalMinutes < 30) return;

                Application["LastStalePaymentCleanup"] = DateTime.Now;

                int deleted = LessonProgress.CleanupStalePayments(24);

                if (deleted > 0)
                {
                    System.Diagnostics.Trace.TraceInformation(
                        "Stale payment cleanup deleted " + deleted + " row(s).");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("TryCleanupStalePayments: " + ex);
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            lblMessage.Visible = false;
            _dbUnreachable = false;

            string email = txtEmail.Text.Trim().ToLowerInvariant();
            string password = txtPassword.Text;

            if (email.Length == 0 || password.Length == 0)
            {
                ShowError("Please enter your email and password.");
                return;
            }

            // ---- 1. Try Admin_Staff ----
            var admin = LookupAdminStaff(email, password);
            if (admin != null)
            {
                StaffAuth.SetStaffSession(admin.Value.Email, admin.Value.Name, admin.Value.Role);
                RouteStaff();
                return;
            }
            if (_dbUnreachable) { ShowDbUnreachable(); return; }

            // ---- 2. Try Instructor table ----
            var inst = LookupInstructor(email, password);
            if (inst != null)
            {
                StaffAuth.SetStaffSession(inst.Value.Email, inst.Value.Name, "Instructor");
                RouteStaff();
                return;
            }
            if (_dbUnreachable) { ShowDbUnreachable(); return; }

            // ---- 3. Try Students ----
            var student = LookupStudent(email, password);
            if (student != null)
            {
                Session["StudNumber"] = student.Value.StudNumber;
                Session["StudEmail"] = student.Value.Email;
                Session["FullName"] = student.Value.Name;

                string target = ResolveStudentReturnUrl();
                Response.Redirect(target, false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
            if (_dbUnreachable) { ShowDbUnreachable(); return; }

            // ---- 4. Nobody matched ----
            ShowError("Invalid email or password.");
        }

        private void ShowError(string msg)
        {
            lblMessage.Text = msg;
            lblMessage.Visible = true;
        }

        private void ShowDbUnreachable()
        {
            ShowError(
                "We can't reach our systems right now. Please try again in a moment. " +
                "If this keeps happening, contact reception on +27 83 544 8544.");
        }

        // =========================================================
        // RETURN URL
        // =========================================================
        private string ResolveStudentReturnUrl()
        {
            string ret = (Request.QueryString["return"] ?? "").Trim();
            if (ret.Length == 0) return "~/StudentDashboard.aspx";

            if (ret.Equals("BookSession", StringComparison.OrdinalIgnoreCase))
                return "~/BookSession.aspx";
            if (ret.Equals("LearnersClass", StringComparison.OrdinalIgnoreCase))
                return "~/LearnersClass.aspx";
            if (ret.Equals("StudentDashboard", StringComparison.OrdinalIgnoreCase))
                return "~/StudentDashboard.aspx";

            if (ret.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                ret.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                try { ret = new Uri(ret).AbsolutePath; }
                catch { return "~/StudentDashboard.aspx"; }
            }

            if (ret.StartsWith("~/")) return ret;
            if (ret.StartsWith("/")) return "~" + ret;
            return "~/" + ret.TrimStart('/');
        }

        // =========================================================
        // STAFF ROUTING
        // =========================================================
        private void RouteStaff()
        {
            string role = StaffAuth.Role ?? "";
            if (role.Equals("Instructor", StringComparison.OrdinalIgnoreCase))
                Response.Redirect("~/InstructorDashboard.aspx", false);
            else
                Response.Redirect("~/AdminDashboard.aspx", false);

            Context.ApplicationInstance.CompleteRequest();
        }

        // ================= MODELS =================
        private struct AdminRow { public string Email; public string Name; public string Role; }
        private struct InstructorRow { public string Email; public string Name; }
        private struct StudentRow { public int StudNumber; public string Email; public string Name; }

        // =========================================================
        // ADMIN_STAFF LOOKUP
        // =========================================================
        private AdminRow? LookupAdminStaff(string email, string password)
        {
            try
            {
                string cs = ConfigurationManager
                    .ConnectionStrings["KhweziConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT TOP 1 Name, Surname, Role, PasswordHash
                      FROM Admin_Staff
                      WHERE LOWER(LTRIM(RTRIM(StafEmailAddress))) = @E", conn))
                {
                    cmd.Parameters.AddWithValue("@E", email);
                    conn.Open();

                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return null;

                        string storedHash = r["PasswordHash"] == DBNull.Value
                            ? null : r["PasswordHash"].ToString();
                        if (string.IsNullOrEmpty(storedHash)) return null;
                        if (!PasswordHelper.Verify(password, storedHash)) return null;

                        string name = r["Name"].ToString().Trim();
                        string surname = r["Surname"].ToString().Trim();
                        string fullName = (name + " " + surname).Trim();
                        if (fullName.Length == 0) fullName = "Staff";

                        string role = r["Role"] == DBNull.Value
                            ? "Receptionist" : r["Role"].ToString().Trim();
                        if (string.IsNullOrEmpty(role)) role = "Receptionist";

                        return new AdminRow { Email = email, Name = fullName, Role = role };
                    }
                }
            }
            catch (Exception ex)
            {
                if (IsConnectionFailure(ex)) _dbUnreachable = true;
                System.Diagnostics.Trace.TraceError("Login.LookupAdminStaff: " + ex);
                return null;
            }
        }

        // =========================================================
        // INSTRUCTOR LOOKUP
        // =========================================================
        private InstructorRow? LookupInstructor(string email, string password)
        {
            try
            {
                string cs = ConfigurationManager
                    .ConnectionStrings["KhweziConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT TOP 1 Name, Surname, Password
                      FROM Instructor
                      WHERE LOWER(LTRIM(RTRIM(InstruEmailAddress))) = @E", conn))
                {
                    cmd.Parameters.AddWithValue("@E", email);
                    conn.Open();

                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return null;

                        string storedHash = r["Password"] == DBNull.Value
                            ? null : r["Password"].ToString();
                        if (string.IsNullOrEmpty(storedHash)) return null;
                        if (!PasswordHelper.Verify(password, storedHash)) return null;

                        string name = r["Name"].ToString().Trim();
                        string surname = r["Surname"].ToString().Trim();
                        string fullName = (name + " " + surname).Trim();
                        if (fullName.Length == 0) fullName = "Instructor";

                        return new InstructorRow { Email = email, Name = fullName };
                    }
                }
            }
            catch (Exception ex)
            {
                if (IsConnectionFailure(ex)) _dbUnreachable = true;
                System.Diagnostics.Trace.TraceError("Login.LookupInstructor: " + ex);
                return null;
            }
        }

        // =========================================================
        // STUDENT LOOKUP
        // =========================================================
        private StudentRow? LookupStudent(string email, string password)
        {
            try
            {
                string cs = ConfigurationManager
                    .ConnectionStrings["KhweziConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT TOP 1 StudNumber, Name, Surname, PasswordHash, Status
                      FROM Students
                      WHERE LOWER(LTRIM(RTRIM(StudEmailAddress))) = @E", conn))
                {
                    cmd.Parameters.AddWithValue("@E", email);
                    conn.Open();

                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return null;

                        string status = r["Status"] == DBNull.Value
                            ? "" : r["Status"].ToString();
                        if (!status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                            return null;

                        string storedHash = r["PasswordHash"] == DBNull.Value
                            ? null : r["PasswordHash"].ToString();
                        if (string.IsNullOrEmpty(storedHash)) return null;
                        if (!PasswordHelper.Verify(password, storedHash)) return null;

                        int studNumber = 0;
                        try
                        {
                            int ord = r.GetOrdinal("StudNumber");
                            if (!r.IsDBNull(ord)) studNumber = Convert.ToInt32(r.GetValue(ord));
                        }
                        catch { }
                        if (studNumber <= 0) return null;

                        string name = r["Name"].ToString().Trim();
                        string surname = r["Surname"].ToString().Trim();
                        string fullName = (name + " " + surname).Trim();
                        if (fullName.Length == 0) fullName = "Student";

                        return new StudentRow
                        {
                            StudNumber = studNumber,
                            Email = email,
                            Name = fullName
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                if (IsConnectionFailure(ex)) _dbUnreachable = true;
                System.Diagnostics.Trace.TraceError("Login.LookupStudent: " + ex);
                return null;
            }
        }

        // =========================================================
        // Detect connection failures so we say "DB down" rather than
        // "wrong password"
        // =========================================================
        private static bool IsConnectionFailure(Exception ex)
        {
            var sql = ex as SqlException;
            if (sql != null)
            {
                if (sql.Class >= 20) return true;
                switch (sql.Number)
                {
                    case -2:
                    case 53:
                    case 40:
                    case 4060:
                    case 18456:
                        return true;
                }
            }
            if (ex is System.Net.WebException) return true;
            if (ex is System.Net.Sockets.SocketException) return true;
            return false;
        }
    }
}