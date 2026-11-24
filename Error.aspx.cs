using System;
using System.Web;

namespace WebApplicationM3
{
    public partial class Error : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // 1. Generate a unique Reference ID for the student to give to reception
                string errorId = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
                litErrorId.Text = errorId;
                pnlRef.Visible = true;

                // 2. Log the actual error to Azure Trace logs using that same ID
                Exception ex = Server.GetLastError();
                if (ex != null)
                {
                    System.Diagnostics.Trace.TraceError(
                        "Unhandled Error [Ref: " + errorId + "] - " +
                        ex.ToString());

                    // Clear the error so it doesn't trigger this page again in a loop
                    Server.ClearError();
                }

                // 3. Check if user is logged in to decide which button to show
                if (StaffAuth.IsSignedIn)
                {
                    btnGoDashboard.Text = "Staff Dashboard";
                    btnGoDashboard.Visible = true;
                }
                else if (Session["StudNumber"] != null)
                {
                    btnGoDashboard.Text = "Student Dashboard";
                    btnGoDashboard.Visible = true;
                }
                else
                {
                    btnGoDashboard.Visible = false; // Hide dashboard button for guests
                }
            }
        }

        protected void btnGoHome_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Home.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void btnGoDashboard_Click(object sender, EventArgs e)
        {
            if (StaffAuth.IsSignedIn)
            {
                string role = StaffAuth.Role;
                string dashUrl = role != null && role.Equals("Instructor", StringComparison.OrdinalIgnoreCase)
                    ? "~/InstructorDashboard.aspx"
                    : "~/AdminDashboard.aspx";

                Response.Redirect(dashUrl, false);
            }
            else
            {
                Response.Redirect("~/StudentDashboard.aspx", false);
            }

            Context.ApplicationInstance.CompleteRequest();
        }
    }
}