using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Web;

namespace WebApplicationM3
{
    public partial class StaffLogin : System.Web.UI.Page
    {
        private const int Iterations = 100000;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString["denied"] == "1")
                {
                    ShowMsg("Your role does not have access to that page.", false);
                }
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            lblMsg.Visible = false;

            string email = txtEmail.Text.Trim().ToLowerInvariant();
            string password = txtPassword.Text;

            if (email.Length == 0 || password.Length == 0)
            { ShowMsg("Please enter your staff email and password.", false); return; }

            try
            {
                string cs = ConfigurationManager
                    .ConnectionStrings["KhweziConnection"].ConnectionString;

                string name, role, storedPlain, storedHash;

                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT LTRIM(RTRIM(Name)) + ' ' + LTRIM(RTRIM(Surname)) AS FullName,
                             Role, Password, PasswordHash,
                             AdminStatus, EmploymentEndDate
                      FROM Admin_Staff
                      WHERE LOWER(LTRIM(RTRIM(StafEmailAddress))) = @E", conn))
                {
                    cmd.Parameters.AddWithValue("@E", email);
                    conn.Open();

                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (!r.Read())
                        { ShowMsg("Invalid staff email or password.", false); return; }

                        // Only CURRENT employees may sign in
                        if (!"Employed".Equals(r["AdminStatus"].ToString(), StringComparison.OrdinalIgnoreCase))
                        { ShowMsg("This account is no longer active. Please contact management.", false); return; }

                        object end = r["EmploymentEndDate"];
                        if (end != DBNull.Value && Convert.ToDateTime(end) < LessonProgress.TodaySa())
                        { ShowMsg("This account is no longer active. Please contact management.", false); return; }

                        name = r["FullName"].ToString();
                        role = r["Role"].ToString();
                        storedPlain = r["Password"] == DBNull.Value ? "" : r["Password"].ToString();
                        storedHash = r["PasswordHash"] == DBNull.Value ? "" : r["PasswordHash"].ToString();
                    }
                }

                // ---- Verify password (hash first, legacy fallback) ----
                bool ok;
                bool legacyUpgrade = false;

                if (storedHash.Length > 0)
                {
                    ok = VerifyHash(password, storedHash);
                }
                else
                {
                    ok = storedPlain.Length > 0 && password == storedPlain;
                    legacyUpgrade = ok;   // upgrade plaintext to a hash right now
                }

                if (!ok)
                { ShowMsg("Invalid staff email or password.", false); return; }

                // ---- Progressive migration: hash the legacy password ----
                if (legacyUpgrade)
                {
                    using (SqlConnection conn = new SqlConnection(cs))
                    using (SqlCommand cmd = new SqlCommand(
                        @"UPDATE Admin_Staff SET PasswordHash = @H
                          WHERE LOWER(LTRIM(RTRIM(StafEmailAddress))) = @E", conn))
                    {
                        cmd.Parameters.AddWithValue("@H", HashPassword(password));
                        cmd.Parameters.AddWithValue("@E", email);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                // ---- Create the staff session ----
                Session["StaffEmail"] = email;
                Session["StaffName"] = name;
                Session["StaffRole"] = role;

                // Safe redirect: only relative URLs (open-redirect guard)
                string ret = Request.QueryString["return"];
                if (!string.IsNullOrWhiteSpace(ret) &&
                    (ret.StartsWith("~/") ||
                     (ret.StartsWith("/") && !ret.StartsWith("//"))))
                {
                    Response.Redirect(ret, false);
                }
                else
                {
                    Response.Redirect(StaffAuth.HomeFor(role), false);
                }
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                ShowMsg("Login error: " + Server.HtmlEncode(ex.Message), false);
            }
        }

        // ================= PBKDF2 PASSWORD HASHING =================

        private static bool VerifyHash(string password, string stored)
        {
            try
            {
                string[] parts = stored.Split('.');
                if (parts.Length != 3) return false;

                int iterations = int.Parse(parts[0]);
                byte[] salt = Convert.FromBase64String(parts[1]);
                byte[] expected = Convert.FromBase64String(parts[2]);

                using (var deriver = new Rfc2898DeriveBytes(
                    password, salt, iterations, HashAlgorithmName.SHA256))
                {
                    byte[] actual = deriver.GetBytes(expected.Length);
                    return ConstantTimeEquals(expected, actual);
                }
            }
            catch { return false; }
        }

        private static string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            using (var deriver = new Rfc2898DeriveBytes(
                password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = deriver.GetBytes(32);
                return Iterations + "." +
                    Convert.ToBase64String(salt) + "." +
                    Convert.ToBase64String(hash);
            }
        }

        private static bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }

        private void ShowMsg(string text, bool ok)
        {
            lblMsg.CssClass = ok ? "ok" : "err";
            lblMsg.Text = text;
            lblMsg.Visible = true;
        }
    }
}