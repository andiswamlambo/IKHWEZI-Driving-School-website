using System;
using System.Configuration;
using System.Data.SqlClient;

namespace WebApplicationM3
{
    public partial class ManagerRegisterStaff : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!StaffAuth.CheckAccess("Manager")) return;
        }

        protected void btnGenerateInvite_Click(object sender, EventArgs e)
        {
            lblMsg.Visible = false;

            string name = txtName.Text.Trim();
            string surname = txtSurname.Text.Trim();
            string email = txtEmail.Text.Trim().ToLowerInvariant();
            string phone = txtPhone.Text.Trim();
            string role = ddlRole.SelectedValue;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(surname) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(phone))
            {
                ShowMsg("Please fill in all fields.", false); return;
            }

            // Clean phone number for WhatsApp (remove leading 0 and add SA code 27)
            string waPhone = phone.TrimStart('0');
            waPhone = "27" + waPhone;

            string token = Guid.NewGuid().ToString("N");
            DateTime expires = DateTime.Now.AddHours(48);

            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(1) FROM Admin_Staff WHERE StafEmailAddress = @E", conn))
                    {
                        cmd.Parameters.AddWithValue("@E", email);
                        if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                        {
                            ShowMsg("A staff member with this email address already exists.", false);
                            return;
                        }
                    }

                    // Save phone number to the DB
                    using (SqlCommand cmd = new SqlCommand(
                        @"INSERT INTO Admin_Staff (StafEmailAddress, Name, Surname, PhoneNum, Role, PasswordHash, AdminStatus, EmploymentStartDate, InvitationToken, InvitationExpires, InvitedBy)
                          VALUES (@E, @N, @S, @Phone, @Role, 'PENDING_INVITE', 'Pending', GETDATE(), @Token, @Expiry, @InvitedBy)", conn))
                    {
                        cmd.Parameters.AddWithValue("@E", email);
                        cmd.Parameters.AddWithValue("@N", name);
                        cmd.Parameters.AddWithValue("@S", surname);
                        cmd.Parameters.AddWithValue("@Phone", phone);
                        cmd.Parameters.AddWithValue("@Role", role);
                        cmd.Parameters.AddWithValue("@Token", token);
                        cmd.Parameters.AddWithValue("@Expiry", expires);
                        cmd.Parameters.AddWithValue("@InvitedBy", StaffAuth.Email);

                        cmd.ExecuteNonQuery();
                    }
                }

                //string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
                //string setupUrl = baseUrl + ResolveUrl("~/Account/StaffSetup.aspx?token=" + token);

                string baseUrl = "https://ist-wst-group-27.azurewebsites.net";
                string setupUrl = baseUrl + ResolveUrl("~/Account/StaffSetup.aspx") + "?token=" + token;

                litInviteLink.Text = setupUrl;
                pnlForm.Visible = false;
                pnlSuccess.Visible = true;

                // 1. Send Email
                try
                {
                    string fullName = name + " " + surname;
                    EmailHelper.SendStaffInvitation(email, fullName, role, setupUrl);
                }
                catch { }

                // 2. Prepare WhatsApp Link
                string waMessage = "Hi " + name + ", you have been invited to join IKHWEZI Driving School as a " + role + ". Please click this link to set up your password: " + setupUrl;
                hlWhatsApp.NavigateUrl = "https://wa.me/" + waPhone + "?text=" + Server.UrlEncode(waMessage);
            }
            catch (Exception ex)
            {
                ShowMsg("Error creating invitation: " + ex.Message, false);
            }
        }

        protected void btnInviteAnother_Click(object sender, EventArgs e)
        {
            txtName.Text = ""; txtSurname.Text = ""; txtEmail.Text = ""; txtPhone.Text = "";
            pnlForm.Visible = true;
            pnlSuccess.Visible = false;
            lblMsg.Visible = false;
        }

        private void ShowMsg(string html, bool ok)
        {
            lblMsg.CssClass = "msg " + (ok ? "ok" : "err");
            lblMsg.Text = html;
            lblMsg.Visible = true;
        }
    }
}