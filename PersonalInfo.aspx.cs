using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web.UI;

namespace WebApplicationM3
{
    public partial class PersonalInfo : System.Web.UI.Page
    {
        private bool? _hasLicenceCol;
        private bool StudentsHasLicenceColumn
        {
            get
            {
                if (!_hasLicenceCol.HasValue)
                {
                    try
                    {
                        string cs = ConfigurationManager
                            .ConnectionStrings["KhweziConnection"].ConnectionString;
                        using (SqlConnection conn = new SqlConnection(cs))
                        using (SqlCommand cmd = new SqlCommand(
                            @"SELECT COUNT(1) FROM INFORMATION_SCHEMA.COLUMNS
                              WHERE TABLE_NAME = 'Students'
                                AND COLUMN_NAME = 'HasLearnersLicence'", conn))
                        {
                            conn.Open();
                            _hasLicenceCol = ((int)cmd.ExecuteScalar()) > 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.TraceError("PersonalInfo.StudentsHasLicenceColumn: " + ex);
                        _hasLicenceCol = false;
                    }
                }
                return _hasLicenceCol.Value;
            }
        }

        private bool IsLicenceLocked(int studNumber)
        {
            try
            {
                string cs = ConfigurationManager
                    .ConnectionStrings["KhweziConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT COUNT(1)
                      FROM Booking b
                      INNER JOIN Packages p ON p.PackageID = b.PackageID
                      WHERE b.StudEmailAddress =
                            (SELECT StudEmailAddress FROM Students WHERE StudNumber = @S)
                        AND p.PackageType = 'Learners'
                        AND b.BookingStatus NOT IN ('Cancelled','Canceled')",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@S", studNumber);
                    conn.Open();
                    return ((int)cmd.ExecuteScalar()) > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("PersonalInfo.IsLicenceLocked: " + ex);
                return true;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["StudNumber"] == null)
            {
                Response.Redirect("~/Account/Login.aspx?return=PersonalInfo", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                LoadTopBar();
                LoadProfile();
            }
        }

        private void LoadTopBar()
        {
            string fullName = Session["FullName"] != null ? Session["FullName"].ToString() : "";
            if (fullName.Length == 0) fullName = "Student";

            litStudentName.Text = fullName;
            litInitials.Text = GetInitials(fullName);

            // NEW: Load profile photo
            int studNumber = (int)Session["StudNumber"];
            string photoUrl = MyDocuments.GetProfilePhotoUrl(studNumber);

            if (!string.IsNullOrEmpty(photoUrl))
            {
                imgTopAvatar.ImageUrl = ResolveUrl(photoUrl);
                imgTopAvatar.Visible = true;
                litInitials.Visible = false;
            }
            else
            {
                imgTopAvatar.Visible = false;
                litInitials.Visible = true;
            }
        }

        private string GetInitials(string name)
        {
            string[] parts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string initials = "";
            foreach (string p in parts) initials += char.ToUpper(p[0]);
            return initials.Length > 0 ? initials.Substring(0, Math.Min(2, initials.Length)) : "?";
        }

        private void LoadProfile()
        {
            try
            {
                int studNumber = (int)Session["StudNumber"];
                string cs = ConfigurationManager
                    .ConnectionStrings["KhweziConnection"].ConnectionString;

                string sql = StudentsHasLicenceColumn
                    ? @"SELECT Name, Surname, PhoneNum, StudEmailAddress,
                               StreetAddress, Suburb, City, HasLearnersLicence
                        FROM Students WHERE StudNumber = @N"
                    : @"SELECT Name, Surname, PhoneNum, StudEmailAddress,
                               StreetAddress, Suburb, City
                        FROM Students WHERE StudNumber = @N";

                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@N", studNumber);
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            txtName.Text = r["Name"].ToString();
                            txtSurname.Text = r["Surname"].ToString();
                            txtPhone.Text = r["PhoneNum"] == DBNull.Value ? "" : r["PhoneNum"].ToString();
                            txtEmail.Text = r["StudEmailAddress"].ToString();
                            txtStreet.Text = r["StreetAddress"] == DBNull.Value ? "" : r["StreetAddress"].ToString();
                            txtSuburb.Text = r["Suburb"] == DBNull.Value ? "" : r["Suburb"].ToString();
                            txtCity.Text = r["City"] == DBNull.Value ? "" : r["City"].ToString();

                            if (StudentsHasLicenceColumn && r["HasLearnersLicence"] != DBNull.Value)
                            {
                                bool hasLicence = Convert.ToBoolean(r["HasLearnersLicence"]);
                                rblLicence.SelectedValue = hasLicence ? "Yes" : "No";
                            }
                            else
                            {
                                rblLicence.ClearSelection();
                            }
                        }
                    }
                }

                txtName.ReadOnly = true;
                txtSurname.ReadOnly = true;
                txtEmail.ReadOnly = true;

                txtName.CssClass = "f-input readonly-field";
                txtSurname.CssClass = "f-input readonly-field";
                txtEmail.CssClass = "f-input readonly-field";

                bool licenceLocked = IsLicenceLocked(studNumber);

                if (licenceLocked)
                {
                    rblLicence.Enabled = false;
                    pnlLicenceLocked.Visible = true;
                }
                else
                {
                    rblLicence.Enabled = true;
                    pnlLicenceLocked.Visible = false;
                }

                pnlNameLocked.Visible = true;
            }
            catch (Exception ex)
            {
                ShowMsg(DbFriendly.Message(ex, "loading your profile"), false);
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int studNumber = (int)Session["StudNumber"];

            string phone = txtPhone.Text.Replace(" ", "").Trim();
            string street = txtStreet.Text.Trim();
            string suburb = txtSuburb.Text.Trim();
            string city = txtCity.Text.Trim();

            if (!Regex.IsMatch(phone, @"^0\d{9}$"))
            {
                ShowMsg("Phone must be 10 digits starting with 0 (e.g. 0723456789).", false);
                return;
            }

            if (street.Length == 0 || suburb.Length == 0 || city.Length == 0)
            {
                ShowMsg("Street address, suburb and city are required.", false);
                return;
            }

            bool licenceLocked = IsLicenceLocked(studNumber);
            bool updateLicence =
                StudentsHasLicenceColumn &&
                !licenceLocked &&
                rblLicence.SelectedIndex >= 0;

            if (StudentsHasLicenceColumn && !licenceLocked && rblLicence.SelectedIndex < 0)
            {
                ShowMsg("Please tell us whether you have a learner's licence.", false);
                return;
            }

            try
            {
                string cs = ConfigurationManager
                    .ConnectionStrings["KhweziConnection"].ConnectionString;

                string sql;
                if (updateLicence)
                {
                    sql = @"UPDATE Students
                              SET PhoneNum = @P,
                                  StreetAddress = @Street,
                                  Suburb = @Suburb,
                                  City = @City,
                                  HasLearnersLicence = @HasLic
                            WHERE StudNumber = @Num";
                }
                else
                {
                    sql = @"UPDATE Students
                              SET PhoneNum = @P,
                                  StreetAddress = @Street,
                                  Suburb = @Suburb,
                                  City = @City
                            WHERE StudNumber = @Num";
                }

                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@P", phone);
                    cmd.Parameters.AddWithValue("@Street", street);
                    cmd.Parameters.AddWithValue("@Suburb", suburb);
                    cmd.Parameters.AddWithValue("@City", city);
                    cmd.Parameters.AddWithValue("@Num", studNumber);

                    if (updateLicence)
                        cmd.Parameters.AddWithValue("@HasLic", rblLicence.SelectedValue == "Yes");

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                if (!string.IsNullOrEmpty(txtName.Text) && !string.IsNullOrEmpty(txtSurname.Text))
                    Session["FullName"] = txtName.Text.Trim() + " " + txtSurname.Text.Trim();

                ShowMsg("Your contact details have been updated.", true);
                LoadTopBar(); // refresh topbar just in case
                LoadProfile();
            }
            catch (Exception ex)
            {
                ShowMsg(DbFriendly.Message(ex, "saving your profile"), false);
            }
        }

        protected void btnLogoutSide_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Account/Login.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void ShowMsg(string html, bool ok)
        {
            lblMsg.CssClass = ok ? "msg ok" : "msg err";
            lblMsg.Text = Server.HtmlEncode(html);
            lblMsg.Visible = true;
        }
    }
}