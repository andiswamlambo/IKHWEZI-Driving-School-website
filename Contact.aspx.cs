using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApplicationM3
{
    public partial class Contact : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        // This runs when the "Send Message" button is clicked
        protected void btnSend_Click(object sender, EventArgs e)
        {
            // 1. Show a success message using the name they typed in
            lblSuccess.Text = "Thank you, " + txtName.Text + "! Your message has been received. We will contact you shortly.";

            // 2. Clear the form fields so they are empty again
            txtName.Text = "";
            txtEmail.Text = "";
            txtPhone.Text = "";
            txtMessage.Text = "";
        }
    }
}