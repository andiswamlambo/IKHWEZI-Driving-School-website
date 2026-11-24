using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;

namespace WebApplicationM3
{
    public partial class StaffSetup : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string token = Request.QueryString["token"];
                if (string.IsNullOrEmpty(token)) { pnlError.Visible = true; return; }

                try
                {
                    string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                    using (SqlConnection conn = new SqlConnection(cs))
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT StafEmailAddress, Name, Role FROM Admin_Staff WHERE InvitationToken = @Token AND InvitationExpires > GETDATE()", conn))
                    {
                        cmd.Parameters.AddWithValue("@Token", token);
                        conn.Open();
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                pnlSetup.Visible = true;
                                HiddenEmail.Value = r["StafEmailAddress"].ToString();
                                litName.Text = Server.HtmlEncode(r["Name"].ToString());
                                litRole.Text = Server.HtmlEncode(r["Role"].ToString());
                                Session["SetupToken"] = token;
                            }
                            else
                            {
                                pnlError.Visible = true;
                            }
                        }
                    }
                }
                catch { pnlError.Visible = true; }
            }
        }

        protected void btnSetPassword_Click(object sender, EventArgs e)
        {
            string token = Session["SetupToken"] as string;
            if (string.IsNullOrEmpty(token)) { pnlError.Visible = true; pnlSetup.Visible = false; return; }

            string pwd = txtPassword.Text;
            string confirm = txtConfirm.Text;

            if (pwd.Length < 8)
            {
                ShowError("Password must be at least 8 characters long."); return;
            }
            if (pwd != confirm)
            {
                ShowError("Passwords do not match."); return;
            }

            try
            {
                string email = HiddenEmail.Value;
                string hashed = PasswordHelper.Hash(pwd);

                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(
                        @"UPDATE Admin_Staff 
                          SET PasswordHash = @Pwd, AdminStatus = 'Employed', InvitationToken = NULL, InvitationExpires = NULL 
                          WHERE StafEmailAddress = @E AND InvitationToken = @Token", conn))
                    {
                        cmd.Parameters.AddWithValue("@Pwd", hashed);
                        cmd.Parameters.AddWithValue("@E", email);
                        cmd.Parameters.AddWithValue("@Token", token);

                        if (cmd.ExecuteNonQuery() == 1)
                        {
                            string role = litRole.Text;
                            if (role.Equals("Instructor", StringComparison.OrdinalIgnoreCase))
                            {
                                using (SqlCommand cmdIns = new SqlCommand(
                                    @"IF NOT EXISTS (SELECT 1 FROM Instructor WHERE InstruEmailAddress = @E)
                                      INSERT INTO Instructor (InstruEmailAddress, Name, Surname, LicenceType, Specialization)
                                      SELECT @E, Name, Surname, 'Code 8', 'Manual' FROM Admin_Staff WHERE StafEmailAddress = @E", conn))
                                {
                                    cmdIns.Parameters.AddWithValue("@E", email);
                                    cmdIns.ExecuteNonQuery();
                                }
                            }

                            lblMsg.CssClass = "msg ok";
                            lblMsg.Text = "Account activated successfully! You can now <a href='../Account/Login.aspx' style='color:#fff;text-decoration:underline;'>log in here</a>.";
                            lblMsg.Visible = true;
                            pnlSetup.Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Error: " + ex.Message);
            }
        }

        private void ShowError(string msg)
        {
            lblMsg.CssClass = "msg err";
            lblMsg.Text = msg;
            lblMsg.Visible = true;
        }
    }
}