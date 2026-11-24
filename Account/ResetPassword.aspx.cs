using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;

namespace WebApplicationM3.Account
{
    public partial class ResetPassword : System.Web.UI.Page
    {
        private string Email
        {
            get { return Session["PwdResetEmail"] as string ?? ""; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                bool verified = Session["PwdResetVerified"] != null &&
                                (bool)Session["PwdResetVerified"];

                if (!verified || string.IsNullOrEmpty(Email))
                {
                    Response.Redirect("~/Account/ForgotPassword.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            lblMsg.Visible = false;

            string pwd = txtNew.Text;
            string confirm = txtConfirm.Text;

            if (pwd.Length < 8)
            { ShowMsg("Password must be at least 8 characters long.", false); return; }

            if (!HasLetterAndDigit(pwd))
            { ShowMsg("Password must contain at least one letter and one number.", false); return; }

            if (pwd != confirm)
            { ShowMsg("Passwords don't match.", false); return; }

            string hash = PasswordHelper.Hash(pwd);

            try
            {
                string cs = ConfigurationManager
                    .ConnectionStrings["KhweziConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(cs))
                {
                    conn.Open();

                    int totalRows = 0;

                    // Students
                    using (SqlCommand cmd = new SqlCommand(
                        @"UPDATE Students SET PasswordHash = @H
                          WHERE LOWER(StudEmailAddress) = @E", conn))
                    {
                        cmd.Parameters.AddWithValue("@H", hash);
                        cmd.Parameters.AddWithValue("@E", Email);
                        totalRows += cmd.ExecuteNonQuery();
                    }

                    // Admin_Staff
                    using (SqlCommand cmd = new SqlCommand(
                        @"UPDATE Admin_Staff SET PasswordHash = @H
                          WHERE LOWER(StafEmailAddress) = @E", conn))
                    {
                        cmd.Parameters.AddWithValue("@H", hash);
                        cmd.Parameters.AddWithValue("@E", Email);
                        totalRows += cmd.ExecuteNonQuery();
                    }

                    // Instructor
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(
                            @"UPDATE Instructor SET Password = @H
                              WHERE LOWER(InstruEmailAddress) = @E", conn))
                        {
                            cmd.Parameters.AddWithValue("@H", hash);
                            cmd.Parameters.AddWithValue("@E", Email);
                            totalRows += cmd.ExecuteNonQuery();
                        }
                    }
                    catch { }

                    if (totalRows == 0)
                    {
                        ShowMsg("Could not update your password. Please contact support.", false);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMsg("Error: " + ex.Message, false);
                return;
            }

            Session.Remove("PwdResetVerified");
            Session.Remove("PwdResetResetId");
            Session.Remove("PwdResetEmail");

            lblMsg.ForeColor = System.Drawing.Color.FromArgb(20, 102, 63);
            lblMsg.Text = "Your password has been reset. Redirecting to login...";
            lblMsg.Visible = true;

            Response.AddHeader("REFRESH", "2;URL=Login.aspx");
        }

        private bool HasLetterAndDigit(string pwd)
        {
            bool hasLetter = false, hasDigit = false;
            foreach (char c in pwd)
            {
                if (char.IsLetter(c)) hasLetter = true;
                if (char.IsDigit(c)) hasDigit = true;
            }
            return hasLetter && hasDigit;
        }

        private void ShowMsg(string html, bool ok)
        {
            lblMsg.ForeColor = ok
                ? System.Drawing.Color.FromArgb(20, 102, 63)
                : System.Drawing.Color.FromArgb(163, 20, 31);
            lblMsg.Text = Server.HtmlEncode(html);
            lblMsg.Visible = true;
        }
    }
}