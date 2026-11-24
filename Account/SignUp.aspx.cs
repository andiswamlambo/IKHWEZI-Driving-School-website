using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;

namespace WebApplicationM3.Account
{
    public partial class Register : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["StudNumber"] != null)
            {
                Response.Redirect("~/StudentDashboard.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            string email = txtEmail.Text.Trim().ToLowerInvariant();

            if (fullName.Length == 0 || email.Length == 0 || txtPassword.Text.Length == 0)
            { ShowMessage("Please fill in all fields."); return; }

            string[] parts = fullName.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            { ShowMessage("Please enter your first name and surname (e.g. Andile Hlatshwayo)."); return; }

            string name = parts[0];
            string surname = parts[1];

            if (!IsValidEmail(email))
            { ShowMessage("Please enter a valid email address."); return; }

            if (txtPassword.Text.Length < 6)
            { ShowMessage("Password must be at least 6 characters long."); return; }

            if (!txtPassword.Text.Any(char.IsUpper))
            { ShowMessage("Password must contain at least one uppercase letter."); return; }

            if (!txtPassword.Text.Any(char.IsLower))
            { ShowMessage("Password must contain at least one lowercase letter."); return; }

            if (!txtPassword.Text.Any(char.IsDigit))
            { ShowMessage("Password must contain at least one number."); return; }

            if (!txtPassword.Text.Any(ch => !char.IsLetterOrDigit(ch)))
            { ShowMessage("Password must contain at least one special character, e.g. !, @, #, $, %."); return; }

            if (txtPassword.Text != txtConfirmPassword.Text)
            { ShowMessage("Passwords do not match."); return; }

            string connString = ConfigurationManager
                .ConnectionStrings["KhweziConnection"].ConnectionString;

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    using (SqlCommand check = new SqlCommand(
                        "SELECT COUNT(1) FROM Students WHERE LOWER(StudEmailAddress) = @Email", conn))
                    {
                        check.Parameters.AddWithValue("@Email", email);
                        if ((int)check.ExecuteScalar() > 0)
                        {
                            ShowMessage("An account with this email already exists. Please log in instead.");
                            return;
                        }
                    }

                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        int newStudNumber;
                        using (SqlCommand numCmd = new SqlCommand(
                            "SELECT ISNULL(MAX(StudNumber), 22000) + 1 FROM Students", conn, tran))
                        {
                            newStudNumber = (int)numCmd.ExecuteScalar();
                        }

                        using (SqlCommand cmd = new SqlCommand(
                            @"INSERT INTO Students
                              (StudNumber, StudEmailAddress, Name, Surname,
                               PasswordHash, RegisterDate, DeregisterDate, Status)
                              VALUES
                              (@StudNumber, @Email, @Name, @Surname,
                               @PasswordHash, GETDATE(), NULL, 'Active')", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@StudNumber", newStudNumber);
                            cmd.Parameters.AddWithValue("@Email", email);
                            cmd.Parameters.AddWithValue("@Name", name);
                            cmd.Parameters.AddWithValue("@Surname", surname);
                            cmd.Parameters.AddWithValue("@PasswordHash",
                                PasswordHelper.Hash(txtPassword.Text));

                            cmd.ExecuteNonQuery();
                        }

                        tran.Commit();

                        Session["StudNumber"] = newStudNumber;
                        Session["StudEmail"] = email;
                        Session["FullName"] = fullName;

                        try
                        {
                            EmailHelper.SendRegistrationWelcome(email, fullName, newStudNumber);
                        }
                        catch (Exception mailEx)
                        {
                            System.Diagnostics.Trace.TraceError("Registration email failed: " + mailEx);
                        }

                        Response.Redirect("~/StudentDashboard.aspx", false);
                        Context.ApplicationInstance.CompleteRequest();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage(DbFriendly.Message(ex, "registering a student account"));
            }
        }

        private void ShowMessage(string msg)
        {
            ErrorMessage.Text = msg;
            ErrorMessage.Visible = true;
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
    }
}