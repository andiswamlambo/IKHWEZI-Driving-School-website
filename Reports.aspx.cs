
using System;
using System.Configuration;
using System.Web;
using System.Web.UI;

namespace WebApplicationM3
{
    public partial class Reports : System.Web.UI.Page
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            if (Session != null)
            {
                ViewStateUserKey =
                    Session.SessionID ?? "anon";
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Same manager security as ManagerDashboard
            if (!StaffAuth.CheckAccess("Manager"))
            {
                Response.StatusCode = 403;
                Response.TrySkipIisCustomErrors = true;
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            LoadPowerBIReport();
        }

        private void LoadPowerBIReport()
        {
            string embedUrl = ConfigurationManager
                .AppSettings["PowerBIEmbedUrl"];

            Uri parsedUrl;

            if (string.IsNullOrWhiteSpace(embedUrl) ||
                !Uri.TryCreate(
                    embedUrl,
                    UriKind.Absolute,
                    out parsedUrl) ||
                parsedUrl.Scheme != Uri.UriSchemeHttps ||
                !string.Equals(
                    parsedUrl.Host,
                    "app.powerbi.com",
                    StringComparison.OrdinalIgnoreCase) ||
                parsedUrl.AbsolutePath != "/reportEmbed")
            {
                throw new ConfigurationErrorsException(
                    "Power BI embed URL is not configured correctly."
                );
            }

            powerBIFrame.Attributes["src"] = embedUrl;
        }
    }
}
