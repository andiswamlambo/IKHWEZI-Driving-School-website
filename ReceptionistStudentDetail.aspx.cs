
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;

namespace WebApplicationM3
{
    public partial class ReceptionistStudentDetail : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!StaffAuth.CheckAccess("Admin")) return;

            if (!IsPostBack)
            {
                LoadStudent();
            }
        }

        private void LoadStudent()
        {
            string email = Request.QueryString["email"];

            if (string.IsNullOrEmpty(email))
            {
                Response.Redirect("AdminStudents.aspx");
                return;
            }

            try
            {
                string cs = ConfigurationManager
                    .ConnectionStrings["KhweziConnection"]
                    .ConnectionString;

                using (SqlConnection conn = new SqlConnection(cs))
                {
                    conn.Open();

                    int studentNumber = 0;

                    // 1. Load Basic Info
                    using (SqlCommand cmd = new SqlCommand(
                        @"SELECT StudNumber, Name, Surname,
                                 StudEmailAddress, PhoneNum,
                                 ISNULL(Status, 'Active') AS Status
                          FROM Students
                          WHERE StudEmailAddress = @E", conn))
                    {
                        cmd.Parameters.AddWithValue("@E", email);

                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                studentNumber =
                                    Convert.ToInt32(r["StudNumber"]);

                                string firstName =
                                    Convert.ToString(r["Name"]);

                                string surname =
                                    Convert.ToString(r["Surname"]);

                                litName.Text = Server.HtmlEncode(
                                    firstName + " " + surname);

                                litNumber.Text =
                                    studentNumber.ToString();

                                litEmail.Text = Server.HtmlEncode(
                                    Convert.ToString(
                                        r["StudEmailAddress"]));

                                litPhone.Text = Server.HtmlEncode(
                                    r["PhoneNum"] != DBNull.Value
                                        ? Convert.ToString(r["PhoneNum"])
                                        : "N/A");

                                litStatus.Text = Server.HtmlEncode(
                                    Convert.ToString(r["Status"]));

                                // NEW: Default profile initials
                                string initials = "";

                                if (!string.IsNullOrWhiteSpace(firstName))
                                    initials += char.ToUpper(firstName[0]);

                                if (!string.IsNullOrWhiteSpace(surname))
                                    initials += char.ToUpper(surname[0]);

                                litPhotoInitials.Text =
                                    Server.HtmlEncode(
                                        initials.Length > 0
                                            ? initials
                                            : "?");
                            }
                            else
                            {
                                Response.Redirect(
                                    "AdminStudents.aspx",
                                    false);

                                Context.ApplicationInstance
                                    .CompleteRequest();

                                return;
                            }
                        }
                    }

                    // 2. Load Active Package Info
                    using (SqlCommand cmd = new SqlCommand(
                        @"SELECT TOP 1 p.PackageName,
                                 sp.TotalLessons, sp.UsedLessons
                          FROM StudentPackage sp
                          JOIN Packages p
                            ON p.PackageID = sp.PackageID
                          WHERE sp.StudNumber =
                            (SELECT StudNumber
                             FROM Students
                             WHERE StudEmailAddress = @E)
                            AND sp.IsActive = 1", conn))
                    {
                        cmd.Parameters.AddWithValue("@E", email);

                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                pnlPackage.Visible = true;

                                int total =
                                    Convert.ToInt32(r["TotalLessons"]);

                                int used =
                                    Convert.ToInt32(r["UsedLessons"]);

                                litPkgName.Text = Server.HtmlEncode(
                                    Convert.ToString(r["PackageName"]));

                                litPkgTotal.Text = total.ToString();
                                litPkgUsed.Text = used.ToString();

                                litPkgRemaining.Text =
                                    (total - used).ToString();
                            }
                            else
                            {
                                pnlNoPackage.Visible = true;
                            }
                        }
                    }

                    // 3. NEW: Load Student Profile Picture
                    LoadProfilePhoto(conn, studentNumber);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "ReceptionistStudentDetail error: " + ex);

                lblMsg.CssClass = "msg err";
                lblMsg.Text = "Error loading student details.";
                lblMsg.Visible = true;
            }
        }

        // NEW: Profile picture is view-only.
        private void LoadProfilePhoto(
            SqlConnection conn,
            int studentNumber)
        {
            imgStudentPhoto.Visible = false;
            pnlPhotoPlaceholder.Visible = true;

            if (studentNumber <= 0)
                return;

            try
            {
                // Look for the latest approved profile photo.
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT TOP 1 FilePath
                      FROM StudentDocuments
                      WHERE StudNumber = @StudNumber
                        AND DocumentType = 'Photo'
                        AND Status = 'Approved'
                      ORDER BY UploadedOn DESC, DocumentID DESC",
                    conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@StudNumber",
                        studentNumber);

                    object result = cmd.ExecuteScalar();

                    if (result == null ||
                        result == DBNull.Value)
                        return;

                    string filePath =
                        Convert.ToString(result);

                    if (string.IsNullOrWhiteSpace(filePath))
                        return;

                    // Use the protected photo handler so the
                    // image is not loaded from an unrestricted
                    // public upload URL.
                    imgStudentPhoto.ImageUrl =
                        ResolveUrl(
                            "~/StudentProfilePhoto.ashx?stud=" +
                            studentNumber);

                    imgStudentPhoto.Visible = true;
                    pnlPhotoPlaceholder.Visible = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "Student profile photo error: " + ex);

                imgStudentPhoto.Visible = false;
                pnlPhotoPlaceholder.Visible = true;
            }
        }
    }
}
