using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;

namespace WebApplicationM3
{
    public partial class ForcePasswordChange : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Ensure the user is logged in. 
            // If they aren't, send them back to login.
            if (Session["StaffEmail"] == null)
            {
                Response.Redirect("~/Account/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            lblMsg.Visible = false;
            string pwd = txtNewPwd.Text;
            string confirm = txtConfirmPwd.Text;

            if (pwd.Length < 8)
            {
                ShowErr("Password must be at least 8 characters long.");
                return;
            }

            if (pwd != confirm)
            {
                ShowErr("Passwords do not match. Please try again.");
                return;
            }

            try
            {
                string email = Session["StaffEmail"].ToString();
                string hashed = PasswordHelper.Hash(pwd);
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"UPDATE Admin_Staff 
                      SET PasswordHash = @Pwd, MustChangePassword = 0 
                      WHERE StafEmailAddress = @E", conn))
                {
                    cmd.Parameters.AddWithValue("@Pwd", hashed);
                    cmd.Parameters.AddWithValue("@E", email);
                    conn.Open();

                    if (cmd.ExecuteNonQuery() == 1)
                    {
                        // Password successfully changed! 
                        lblMsg.CssClass = "msg ok";
                        lblMsg.Text = "Password updated successfully! Redirecting to your dashboard...";
                        lblMsg.Visible = true;

                        // Determine where to send them based on their role
                        string role = Session["StaffRole"] as string;

                        // Redirect after a short delay
                        ClientScript.RegisterStartupScript(this.GetType(), "redirect",
                            "setTimeout(function(){ window.location.href = '" + GetRedirectUrl(role) + "'; }, 1500);", true);
                    }
                    else
                    {
                        ShowErr("Could not update password. Please contact management.");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErr("Error: " + ex.Message);
            }
        }

        private string GetRedirectUrl(string role)
        {
            if (role == "Instructor")
                return ResolveUrl("~/InstructorDashboard.aspx");
            else if (role == "Manager")
                return ResolveUrl("~/ManagerDashboard.aspx");
            else
                return ResolveUrl("~/AdminDashboard.aspx");
        }

        private void ShowErr(string msg)
        {
            lblMsg.CssClass = "msg err";
            lblMsg.Text = msg;
            lblMsg.Visible = true;
        }
    }
}