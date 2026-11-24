using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Web.UI;

namespace WebApplicationM3.Account
{
    public partial class ForgotPassword : System.Web.UI.Page
    {
        private const int CodeExpiryMinutes = 15;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["StudNumber"] != null)
                {
                    Response.Redirect("~/StudentDashboard.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }
            }
        }

        protected void btnSendCode_Click(object sender, EventArgs e)
        {
            lblMsg.Visible = false;

            string email = txtEmail.Text.Trim().ToLowerInvariant();

            if (email.Length == 0 || !IsValidEmail(email))
            {
                ShowMsg("Please enter a valid email address.", false);
                return;
            }

            string role, fullName;
            bool found = LookupUser(email, out role, out fullName);

            // Always show the same message whether the email exists or not.
            pnlRequest.Visible = false;
            pnlSent.Visible = true;
            litSentEmail.Text = email;

            if (!found) return;

            string code = GenerateCode();

            try
            {
                string cs = ConfigurationManager
                    .ConnectionStrings["KhweziConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(cs))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(
                        @"UPDATE PasswordReset SET UsedOn = GETDATE()
                          WHERE Email = @E AND UsedOn IS NULL", conn))
                    {
                        cmd.Parameters.AddWithValue("@E", email);
                        cmd.ExecuteNonQuery();
                    }

                    using (SqlCommand cmd = new SqlCommand(
                        @"INSERT INTO PasswordReset
                          (UserRole, Email, Code, ExpiresOn, UsedOn,
                           Attempts, RequestedOn, RequestedIP)
                          VALUES (@R, @E, @C, @X, NULL, 0, GETDATE(), @IP)", conn))
                    {
                        cmd.Parameters.AddWithValue("@R", role);
                        cmd.Parameters.AddWithValue("@E", email);
                        cmd.Parameters.AddWithValue("@C", code);
                        cmd.Parameters.AddWithValue("@X",
                            DateTime.Now.AddMinutes(CodeExpiryMinutes));
                        cmd.Parameters.AddWithValue("@IP",
                            Request.UserHostAddress ?? (object)DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Reset persist error: " + ex.Message);
                return;
            }

            try
            {
                string firstName = (fullName ?? "").Split(' ')[0];
                EmailHelper.SendPasswordResetCode(email, firstName, code, CodeExpiryMinutes);
            }
            catch { }
        }

        protected void btnRetry_Click(object sender, EventArgs e)
        {
            pnlSent.Visible = false;
            pnlRequest.Visible = true;
            txtEmail.Text = "";
        }

        private bool LookupUser(string email, out string role, out string fullName)
        {
            role = null;
            fullName = null;

            try
            {
                string cs = ConfigurationManager
                    .ConnectionStrings["KhweziConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(cs))
                {
                    conn.Open();

                    // ---- 1. Students ----
                    using (SqlCommand cmd = new SqlCommand(
                        @"SELECT TOP 1 Name, Surname FROM Students
                          WHERE LOWER(StudEmailAddress) = @E", conn))
                    {
                        cmd.Parameters.AddWithValue("@E", email);
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                role = "Student";
                                fullName = (r["Name"].ToString() + " " + r["Surname"].ToString()).Trim();
                                return true;
                            }
                        }
                    }

                    // ---- 2. Admin_Staff ----
                    using (SqlCommand cmd = new SqlCommand(
                        @"SELECT TOP 1 Name, Surname FROM Admin_Staff
                          WHERE LOWER(StafEmailAddress) = @E", conn))
                    {
                        cmd.Parameters.AddWithValue("@E", email);
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                role = "AdminStaff";
                                fullName = (r["Name"].ToString() + " " + r["Surname"].ToString()).Trim();
                                return true;
                            }
                        }
                    }

                    // ---- 3. Instructor ----
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(
                            @"SELECT TOP 1 Name, Surname FROM Instructor
                              WHERE LOWER(InstruEmailAddress) = @E", conn))
                        {
                            cmd.Parameters.AddWithValue("@E", email);
                            using (SqlDataReader r = cmd.ExecuteReader())
                            {
                                if (r.Read())
                                {
                                    role = "Instructor";
                                    fullName = (r["Name"].ToString() + " " + r["Surname"].ToString()).Trim();
                                    return true;
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }

            return false;
        }

        private string GenerateCode()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] bytes = new byte[4];
                rng.GetBytes(bytes);
                uint value = BitConverter.ToUInt32(bytes, 0);
                int code = (int)(value % 1000000);
                return code.ToString("D6");
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch { return false; }
        }

        private void ShowMsg(string html, bool ok)
        {
            lblMsg.CssClass = "message-label";
            lblMsg.ForeColor = ok
                ? System.Drawing.Color.FromArgb(20, 102, 63)
                : System.Drawing.Color.FromArgb(163, 20, 31);
            lblMsg.Text = Server.HtmlEncode(html);
            lblMsg.Visible = true;
        }
    }
}