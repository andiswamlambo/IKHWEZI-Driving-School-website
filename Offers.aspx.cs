using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace WebApplicationM3
{
    public partial class Offers : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // PUBLIC page: no login required.
            bool isLoggedIn = (Session["StudNumber"] != null);

            phStudentNav.Visible = isLoggedIn;   // student sidebar only when logged in
            phUserChip.Visible = isLoggedIn;     // name + avatar only when logged in
            phGuestNotice.Visible = !isLoggedIn; // sign-up prompt only for guests

            if (isLoggedIn) LoadTopBar();

            if (!IsPostBack)
            {
                BindOffers();
            }
        }

        private void LoadTopBar()
        {
            string fullName = Session["FullName"] != null ? Session["FullName"].ToString() : "Student";
            litStudentName.Text = fullName;
            litInitials.Text = GetInitials(fullName);
        }

        private string GetInitials(string name)
        {
            string[] parts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string initials = "";
            foreach (string p in parts)
                initials += char.ToUpper(p[0]);
            return initials.Length > 0 ? initials.Substring(0, Math.Min(2, initials.Length)) : "?";
        }

        private void BindOffers()
        {
            repOffers.DataSource = LoadOffers();
            repOffers.DataBind();
        }

        private List<OfferItem> LoadOffers()
        {
            var list = new List<OfferItem>();
            try
            {
                string cs = ConfigurationManager
                    .ConnectionStrings["KhweziConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT PackageName, PackageType, VehicleCategory, Amount,
                             TotalLessons, DepositPercentage, PaymentPeriodMonths,
                             FreeLearnersLicence, PackageDescription
                      FROM Packages
                      WHERE PackageType <> 'Learners'
                      ORDER BY CASE PackageType
                                   WHEN 'Code 08'   THEN 1
                                   WHEN 'Automatic' THEN 2
                                   WHEN 'Code 10'   THEN 3
                                   ELSE 4
                               END,
                               TotalLessons", conn))
                {
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                        while (r.Read())
                        {
                            string desc = (r["PackageDescription"] == DBNull.Value ||
                                           string.IsNullOrWhiteSpace(r["PackageDescription"].ToString()))
                                ? r["TotalLessons"] + " " + r["PackageType"].ToString() +
                                  " driving lessons · " + r["VehicleCategory"]
                                : r["PackageDescription"].ToString();

                            var extras = new List<string>();
                            extras.Add(r["TotalLessons"] + " lessons");

                            if (r["DepositPercentage"] != DBNull.Value &&
                                Convert.ToDecimal(r["DepositPercentage"]) > 0)
                                extras.Add(Convert.ToDecimal(r["DepositPercentage"]).ToString("0") + "% deposit");

                            if (r["PaymentPeriodMonths"] != DBNull.Value &&
                                Convert.ToInt32(r["PaymentPeriodMonths"]) > 0)
                                extras.Add(r["PaymentPeriodMonths"] + "-month payment plan");

                            if (r["FreeLearnersLicence"] != DBNull.Value &&
                                Convert.ToInt32(r["FreeLearnersLicence"]) == 1)
                                extras.Add("Free learners licence included");

                            list.Add(new OfferItem
                            {
                                Title = r["PackageName"].ToString(),
                                Description = desc,
                                Price = Convert.ToDecimal(r["Amount"]),
                                Extra = string.Join(" · ", extras)
                            });
                        }
                }

                if (list.Count > 0) return list;
            }
            catch { }

            return DemoOffers();
        }

        private List<OfferItem> DemoOffers()
        {
            return new List<OfferItem>
            {
                new OfferItem { Title="Code 08 - 10 Lessons",
                    Description="10 Code 08 driving lessons. Car hire available at R700. Learners licence R500.",
                    Price=1600, Extra="10 lessons · 50% deposit · 2-month payment plan" },
                new OfferItem { Title="Code 10 - 10 Lessons",
                    Description="10 Code 10 driving lessons. Truck hire available at R1000. Learners licence R500.",
                    Price=1800, Extra="10 lessons · 50% deposit · 2-month payment plan" },
                new OfferItem { Title="Automatic - 10 Lessons",
                    Description="10 automatic driving lessons. Car hire available at R700. Learners licence R500.",
                    Price=2000, Extra="10 lessons · 50% deposit · 2-month payment plan" }
            };
        }

        protected void btnLogoutSide_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Account/Login.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }

    public class OfferItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Extra { get; set; }

        public string PriceText { get { return "R " + Price.ToString("N0"); } }
    }
}