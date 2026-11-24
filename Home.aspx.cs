using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApplicationM3
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnContactSubmit_Click(object sender, EventArgs e)
        {
            lblContactMsg.Visible = false;
            lblContactMsg.CssClass = "form-message";

            string name = txtContactName.Text.Trim();
            string email = txtContactEmail.Text.Trim().ToLowerInvariant();
            string phone = txtContactPhone.Text.Trim();
            string message = txtContactMessage.Text.Trim();

            if (name.Length < 2)
            {
                ShowContactMsg("Please enter your name.", false);
                return;
            }

            if (!IsValidEmail(email))
            {
                ShowContactMsg("Please enter a valid email address.", false);
                return;
            }

            if (phone.Length < 7)
            {
                ShowContactMsg("Please enter your phone number so we can reach you.", false);
                return;
            }

            if (message.Length < 10)
            {
                ShowContactMsg("Please describe your enquiry (at least 10 characters).", false);
                return;
            }

            try
            {
                EmailHelper.SendContactMessage(name, email, phone, message);
            }
            catch
            {
            }

            ShowContactMsg(
                "Thank you, " + Server.HtmlEncode(name) +
                ". Your message has been received — we'll be in touch shortly.",
                true);

            txtContactName.Text = "";
            txtContactEmail.Text = "";
            txtContactPhone.Text = "";
            txtContactMessage.Text = "";
        }

        private void ShowContactMsg(string html, bool ok)
        {
            lblContactMsg.CssClass = "form-message " + (ok ? "ok" : "err");
            lblContactMsg.Text = html;
            lblContactMsg.Visible = true;
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