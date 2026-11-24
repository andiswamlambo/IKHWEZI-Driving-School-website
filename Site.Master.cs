using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApplicationM3
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            bool isLoggedIn = (Session["StudNumber"] != null);

            // ---- Header: only shows user info when logged in ----
            // Guests have no login button up top anymore — the login link
            // lives in the footer Account column.

            // Find the placeholders by walking the control tree, so this
            // compiles even if the designer file hasn't been regenerated.
            var phUserLinks = FindControlRecursive(this, "phUserLinks") as PlaceHolder;
            var phFooterGuest = FindControlRecursive(this, "phFooterGuest") as PlaceHolder;
            var phFooterUser = FindControlRecursive(this, "phFooterUser") as PlaceHolder;
            var litUserName = FindControlRecursive(this, "litUserName") as Literal;

            if (phUserLinks != null) phUserLinks.Visible = isLoggedIn;
            if (phFooterGuest != null) phFooterGuest.Visible = !isLoggedIn;
            if (phFooterUser != null) phFooterUser.Visible = isLoggedIn;

            if (isLoggedIn && litUserName != null && Session["FullName"] != null)
            {
                litUserName.Text = Session["FullName"].ToString();
            }
        }

        protected void btnLogoutNav_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Account/Login.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        // =========================================================
        // Finds a control by ID anywhere in the master's control tree.
        // Used here so the code compiles even if the designer file is
        // out of sync (which happens occasionally in Web Forms projects).
        // =========================================================
        private static Control FindControlRecursive(Control root, string id)
        {
            if (root == null) return null;
            if (root.ID == id) return root;

            foreach (Control child in root.Controls)
            {
                Control found = FindControlRecursive(child, id);
                if (found != null) return found;
            }
            return null;
        }
    }
}