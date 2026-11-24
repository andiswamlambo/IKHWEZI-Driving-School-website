using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApplicationM3
{
    public partial class MyDocuments : System.Web.UI.Page
    {
        private int StudNumber
        {
            get
            {
                return Session["StudNumber"] != null
                    ? (int)Session["StudNumber"] : 0;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (StudNumber <= 0)
            {
                Response.Redirect("~/Account/Login.aspx?return=MyDocuments", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                LoadDocuments();
                LoadProfilePhoto();
            }
        }

        public static string GetProfilePhotoUrl(int studNumber)
        {
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT TOP 1 FilePath
                      FROM StudentDocuments
                      WHERE StudNumber = @S
                        AND DocumentType = 'Photo'
                        AND Status <> 'Rejected'
                      ORDER BY UploadedOn DESC", conn))
                {
                    cmd.Parameters.AddWithValue("@S", studNumber);
                    conn.Open();
                    object r = cmd.ExecuteScalar();
                    return r == null || r == DBNull.Value ? null : r.ToString();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("MyDocuments.GetProfilePhotoUrl: " + ex);
                return null;
            }
        }

        public static string GetProfilePhotoStatus(int studNumber)
        {
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT TOP 1 Status
                      FROM StudentDocuments
                      WHERE StudNumber = @S
                        AND DocumentType = 'Photo'
                        AND Status <> 'Rejected'
                      ORDER BY UploadedOn DESC", conn))
                {
                    cmd.Parameters.AddWithValue("@S", studNumber);
                    conn.Open();
                    object r = cmd.ExecuteScalar();
                    return r == null || r == DBNull.Value ? null : r.ToString();
                }
            }
            catch { return null; }
        }

        private void LoadProfilePhoto()
        {
            string url = GetProfilePhotoUrl(StudNumber);
            string status = GetProfilePhotoStatus(StudNumber);

            if (!string.IsNullOrEmpty(url))
            {
                imgProfile.ImageUrl = ResolveUrl(url);
                imgProfile.Visible = true;
                lblNoPhoto.Visible = false;

                if (!string.IsNullOrEmpty(status))
                {
                    string cls = status.Equals("Approved", StringComparison.OrdinalIgnoreCase) ? "approved" : "pending";
                    lblPhotoStatus.CssClass = "photo-status " + cls;
                    lblPhotoStatus.Text = status.Equals("Approved", StringComparison.OrdinalIgnoreCase) ? "\u2714 Approved" : "\u23F3 Awaiting approval";
                    lblPhotoStatus.Visible = true;
                }
                else
                {
                    lblPhotoStatus.Visible = false;
                }
            }
            else
            {
                imgProfile.Visible = false;
                lblNoPhoto.Visible = true;
                lblPhotoStatus.Visible = false;
            }
        }

        private void LoadDocuments()
        {
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT DocumentID,
                             DocumentType,
                             FileName,
                             FileSizeKB,
                             Status,
                             CONVERT(VARCHAR(20), UploadedOn, 106) AS UploadedOnText,
                             CASE
                                 WHEN Status = 'Approved' THEN 'approved'
                                 WHEN Status = 'Rejected' THEN 'rejected'
                                 ELSE 'pending'
                             END AS StatusClass
                      FROM StudentDocuments
                      WHERE StudNumber = @S
                      ORDER BY UploadedOn DESC", conn))
                {
                    cmd.Parameters.AddWithValue("@S", StudNumber);
                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader r = cmd.ExecuteReader()) dt.Load(r);

                    repDocuments.DataSource = dt;
                    repDocuments.DataBind();
                    litEmpty.Visible = dt.Rows.Count == 0;
                }
            }
            catch (Exception ex)
            {
                ShowMsg("Could not load documents: " + ex.Message, false);
            }
        }

        private void ShowMsg(string html, bool ok)
        {
            lblMsg.CssClass = "msg " + (ok ? "ok" : "err");
            lblMsg.Text = Server.HtmlEncode(html);
            lblMsg.Visible = true;
        }

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            lblMsg.Visible = false;

            if (string.IsNullOrEmpty(ddlDocType.SelectedValue))
            {
                ShowMsg("Please select a document type.", false);
                return;
            }

            if (!fuDocument.HasFile)
            {
                ShowMsg("Please choose a file to upload.", false);
                return;
            }

            string ext = Path.GetExtension(fuDocument.FileName).ToLowerInvariant();
            string[] allowed = { ".pdf", ".jpg", ".jpeg", ".png" };
            bool okExt = Array.IndexOf(allowed, ext) >= 0;

            if (!okExt)
            {
                ShowMsg("Only PDF, JPG, and PNG files are allowed.", false);
                return;
            }

            int fileSize = fuDocument.PostedFile.ContentLength;
            if (fileSize > 5 * 1024 * 1024)
            {
                ShowMsg("File size must be under 5 MB.", false);
                return;
            }

            try
            {
                string uploadsFolder = Server.MapPath("~/Uploads/Documents/");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string safeFileName = StudNumber + "_" +
                    DateTime.Now.ToString("yyyyMMddHHmmss") + "_" +
                    Path.GetFileName(fuDocument.FileName).Replace(" ", "_");

                string fullPath = Path.Combine(uploadsFolder, safeFileName);
                string relPath = "~/Uploads/Documents/" + safeFileName;

                fuDocument.SaveAs(fullPath);

                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;

                // DETERMINE STATUS: Profile photos are auto-approved, others are Pending
                string docType = ddlDocType.SelectedValue;
                string newStatus = docType.Equals("Photo", StringComparison.OrdinalIgnoreCase) ? "Approved" : "Pending";

                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"INSERT INTO StudentDocuments
                      (StudNumber, DocumentType, FileName, FilePath, FileSizeKB, MimeType, Status, UploadedOn)
                      VALUES (@S, @T, @F, @P, @Z, @M, @St, GETDATE())", conn))
                {
                    cmd.Parameters.AddWithValue("@S", StudNumber);
                    cmd.Parameters.AddWithValue("@T", docType);
                    cmd.Parameters.AddWithValue("@F", Path.GetFileName(fuDocument.FileName));
                    cmd.Parameters.AddWithValue("@P", relPath);
                    cmd.Parameters.AddWithValue("@Z", fileSize / 1024);
                    cmd.Parameters.AddWithValue("@M", fuDocument.PostedFile.ContentType);
                    cmd.Parameters.AddWithValue("@St", newStatus);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                ShowMsg("Document uploaded successfully. " +
                        (newStatus == "Approved" ? "Your profile photo is now visible!" : "Reception will review it shortly."), true);

                ddlDocType.SelectedIndex = 0;
                LoadProfilePhoto();
                LoadDocuments();
            }
            catch (Exception ex)
            {
                ShowMsg("Upload failed: " + ex.Message, false);
            }
        }

        protected void repDocuments_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "Delete") return;

            int docId;
            if (!int.TryParse(e.CommandArgument.ToString(), out docId)) return;

            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;

                string filePath = null;
                using (SqlConnection conn = new SqlConnection(cs))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(
                        @"SELECT FilePath FROM StudentDocuments
                          WHERE DocumentID = @D AND StudNumber = @S", conn))
                    {
                        cmd.Parameters.AddWithValue("@D", docId);
                        cmd.Parameters.AddWithValue("@S", StudNumber);
                        object r = cmd.ExecuteScalar();
                        if (r != null) filePath = r.ToString();
                    }

                    using (SqlCommand cmd = new SqlCommand(
                        @"DELETE FROM StudentDocuments
                          WHERE DocumentID = @D AND StudNumber = @S", conn))
                    {
                        cmd.Parameters.AddWithValue("@D", docId);
                        cmd.Parameters.AddWithValue("@S", StudNumber);
                        cmd.ExecuteNonQuery();
                    }
                }

                if (!string.IsNullOrEmpty(filePath))
                {
                    try
                    {
                        string full = Server.MapPath(filePath);
                        if (File.Exists(full)) File.Delete(full);
                    }
                    catch { }
                }

                ShowMsg("Document removed.", true);
                LoadProfilePhoto();
                LoadDocuments();
            }
            catch (Exception ex)
            {
                ShowMsg("Could not remove document: " + ex.Message, false);
            }
        }
    }
}