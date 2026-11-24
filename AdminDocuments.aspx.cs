
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApplicationM3
{
    public partial class AdminDocuments : System.Web.UI.Page
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            if (Session != null)
                ViewStateUserKey = Session.SessionID ?? "anon";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!StaffAuth.CheckAccess("Admin")) return;

            phManagerOnly.Visible = StaffAuth.IsManagerOrHigher();

            if (!IsPostBack)
            {
                LoadTopBar();
                LoadDocs();
            }
        }

        private void LoadTopBar()
        {
            string fullName = StaffAuth.Name;
            if (string.IsNullOrEmpty(fullName))
                fullName = "Reception";

            litStaffName.Text = fullName;
            litInitials.Text = GetInitials(fullName);
            litRoleBadge.Text = StaffAuth.Role;
        }

        private string GetInitials(string name)
        {
            var parts = name.Split(
                new[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries);

            string ini = "";

            foreach (var p in parts)
                ini += char.ToUpper(p[0]);

            return ini.Length > 0
                ? ini.Substring(0, Math.Min(2, ini.Length))
                : "?";
        }

        // ORIGINAL: LOAD DOCUMENTS
        private void LoadDocs()
        {
            litEmpty.Visible = false;

            try
            {
                string cs = ConfigurationManager
                    .ConnectionStrings["KhweziConnection"]
                    .ConnectionString;

                string filter = ddlFilter.SelectedValue;
                string where;

                switch (filter)
                {
                    case "photo":
                        where = "WHERE d.DocumentType = 'Photo' AND d.Status = 'Pending'";
                        break;

                    case "all":
                        where = "WHERE d.UploadedOn >= DATEADD(DAY,-90,GETDATE())";
                        break;

                    default:
                        where = "WHERE d.Status = 'Pending'";
                        break;
                }

                string sql =
                    @"SELECT d.DocumentID, d.StudNumber, d.DocumentType,
                             d.FileName, d.FilePath, d.FileSizeKB, d.Status,
                             CONVERT(VARCHAR(20), d.UploadedOn, 106) AS UploadedOnText,
                             LTRIM(RTRIM(ISNULL(s.Name,'') + ' ' + ISNULL(s.Surname,''))) AS StudentName,
                             ISNULL(s.StudEmailAddress,'') AS StudEmail
                      FROM StudentDocuments d
                      LEFT JOIN Students s ON s.StudNumber = d.StudNumber
                      " + where + @"
                      ORDER BY CASE WHEN d.Status = 'Pending' THEN 0 ELSE 1 END,
                               d.UploadedOn DESC";

                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    repDocs.DataSource = dt;
                    repDocs.DataBind();
                    litEmpty.Visible = dt.Rows.Count == 0;

                    int pending = 0;

                    foreach (DataRow row in dt.Rows)
                    {
                        if (string.Equals(
                            row["Status"].ToString(),
                            "Pending",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            pending++;
                        }
                    }

                    lblPendingCount.Text = pending.ToString();
                }
            }
            catch (Exception ex)
            {
                ShowMsg(
                    DbFriendly.Message(ex, "loading documents"),
                    false);
            }
        }

        protected void ddlFilter_SelectedIndexChanged(
            object sender, EventArgs e)
        {
            LoadDocs();
        }

        // ORIGINAL COMMANDS + NEW DOWNLOAD COMMANDS
        protected void repDocs_ItemCommand(
            object source,
            RepeaterCommandEventArgs e)
        {
            if (!StaffAuth.CheckAccess("Admin")) return;

            // NEW: DOWNLOAD ONE DOCUMENT
            if (e.CommandName == "Download")
            {
                int documentId;

                if (int.TryParse(
                    Convert.ToString(e.CommandArgument),
                    out documentId))
                {
                    DownloadSingleDocument(documentId);
                }

                return;
            }

            // NEW: DOWNLOAD ALL DOCUMENTS FOR ONE STUDENT
            if (e.CommandName == "DownloadAll")
            {
                int studentNumber;

                if (int.TryParse(
                    Convert.ToString(e.CommandArgument),
                    out studentNumber))
                {
                    DownloadStudentDocuments(studentNumber);
                }

                return;
            }

            // ORIGINAL: PROFILE
            if (e.CommandName == "Profile")
            {
                int studNumber;

                if (int.TryParse(
                    Convert.ToString(e.CommandArgument),
                    out studNumber))
                {
                    Response.Redirect(
                        "~/AdminStudentDetail.aspx?stud=" + studNumber,
                        false);

                    Context.ApplicationInstance.CompleteRequest();
                }

                return;
            }

            // ORIGINAL: APPROVE / REJECT
            if (e.CommandName != "Approve" &&
                e.CommandName != "Reject")
                return;

            int docId;

            if (!int.TryParse(
                Convert.ToString(e.CommandArgument),
                out docId))
                return;

            string newStatus = e.CommandName == "Approve"
                ? "Approved"
                : "Rejected";

            try
            {
                string cs = ConfigurationManager
                    .ConnectionStrings["KhweziConnection"]
                    .ConnectionString;

                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"UPDATE StudentDocuments
                      SET Status = @St,
                          ReviewedBy = @R,
                          ReviewedOn = GETDATE()
                      WHERE DocumentID = @D", conn))
                {
                    cmd.Parameters.AddWithValue("@St", newStatus);
                    cmd.Parameters.AddWithValue("@R", StaffAuth.Email);
                    cmd.Parameters.AddWithValue("@D", docId);

                    conn.Open();
                    int n = cmd.ExecuteNonQuery();

                    ShowMsg(
                        n > 0
                            ? "Document " +
                              newStatus.ToLowerInvariant() + "."
                            : "Document not found.",
                        n > 0);
                }
            }
            catch (Exception ex)
            {
                ShowMsg(
                    DbFriendly.Message(ex, "updating document"),
                    false);
            }

            LoadDocs();
        }

        // NEW: DOWNLOAD ONE DOCUMENT
        private void DownloadSingleDocument(int documentId)
        {
            try
            {
                string cs = ConfigurationManager
                    .ConnectionStrings["KhweziConnection"]
                    .ConnectionString;

                string fileName;
                string filePath;

                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT FileName, FilePath
                      FROM StudentDocuments
                      WHERE DocumentID = @DocumentID", conn))
                {
                    cmd.Parameters.Add(
                        "@DocumentID",
                        SqlDbType.Int).Value = documentId;

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            ShowMsg("Document not found.", false);
                            return;
                        }

                        fileName = Convert.ToString(reader["FileName"]);
                        filePath = Convert.ToString(reader["FilePath"]);
                    }
                }

                string physicalPath = GetDocumentPath(filePath);

                if (!File.Exists(physicalPath))
                {
                    ShowMsg(
                        "The uploaded document could not be found.",
                        false);
                    return;
                }

                SendFileDownload(
                    physicalPath,
                    GetSafeFileName(fileName, physicalPath));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "Single document download error: " + ex);

                ShowMsg(
                    "Unable to download the selected document.",
                    false);
            }
        }

        // NEW: DOWNLOAD ALL DOCUMENTS FOR ONE STUDENT
        private void DownloadStudentDocuments(int studentNumber)
        {
            try
            {
                string cs = ConfigurationManager
                    .ConnectionStrings["KhweziConnection"]
                    .ConnectionString;

                DataTable documents = new DataTable();

                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT DocumentID, FileName, FilePath
                      FROM StudentDocuments
                      WHERE StudNumber = @StudNumber
                      ORDER BY DocumentID", conn))
                {
                    cmd.Parameters.Add(
                        "@StudNumber",
                        SqlDbType.Int).Value = studentNumber;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(documents);
                    }
                }

                if (documents.Rows.Count == 0)
                {
                    ShowMsg(
                        "No uploaded documents found for this student.",
                        false);
                    return;
                }

                using (MemoryStream memory = new MemoryStream())
                {
                    int added = 0;

                    using (ZipArchive zip = new ZipArchive(
                        memory,
                        ZipArchiveMode.Create,
                        true))
                    {
                        foreach (DataRow row in documents.Rows)
                        {
                            string filePath =
                                Convert.ToString(row["FilePath"]);

                            string physicalPath;

                            try
                            {
                                physicalPath = GetDocumentPath(filePath);
                            }
                            catch (InvalidOperationException)
                            {
                                System.Diagnostics.Trace.TraceWarning(
                                    "Skipped unsafe document path for ID " +
                                    row["DocumentID"]);
                                continue;
                            }

                            if (!File.Exists(physicalPath))
                                continue;

                            string fileName = GetSafeFileName(
                                Convert.ToString(row["FileName"]),
                                physicalPath);

                            // Prevent duplicate names inside ZIP.
                            string entryName =
                                Convert.ToString(row["DocumentID"]) +
                                "_" + fileName;

                            ZipArchiveEntry entry = zip.CreateEntry(
                                entryName,
                                CompressionLevel.Optimal);

                            using (Stream entryStream = entry.Open())
                            using (FileStream input = new FileStream(
                                physicalPath,
                                FileMode.Open,
                                FileAccess.Read,
                                FileShare.Read))
                            {
                                input.CopyTo(entryStream);
                            }

                            added++;
                        }
                    }

                    if (added == 0)
                    {
                        ShowMsg(
                            "No downloadable document files were found.",
                            false);
                        return;
                    }

                    string zipFileName =
                        "Student_" + studentNumber + "_Documents.zip";

                    SendBytesDownload(
                        memory.ToArray(),
                        zipFileName,
                        "application/zip");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "Download all documents error: " + ex);

                ShowMsg(
                    "Unable to download the student's documents.",
                    false);
            }
        }

        // NEW: RESOLVE FILES FROM THE DOCUMENT UPLOAD FOLDER
        private string GetDocumentPath(string storedPath)
        {
            if (string.IsNullOrWhiteSpace(storedPath))
                throw new InvalidOperationException(
                    "Document path is empty.");

            string path = storedPath.Trim();

            if (path.Contains("://") ||
                path.StartsWith("//") ||
                path.StartsWith(@"\\") ||
                Path.IsPathRooted(path) ||
                path.IndexOf('?') >= 0 ||
                path.IndexOf('#') >= 0)
            {
                throw new InvalidOperationException(
                    "Invalid document path.");
            }

            string decoded = HttpUtility.UrlDecode(path);

            if (decoded.Contains("..") ||
                decoded.Contains("\\") ||
                decoded.Contains(":") ||
                decoded.Contains("%"))
            {
                throw new InvalidOperationException(
                    "Invalid document path.");
            }

            if (decoded.StartsWith("~/"))
            {
                decoded = decoded.Substring(2);
            }
            else if (decoded.StartsWith("/"))
            {
                string appPath =
                    Request.ApplicationPath.TrimEnd('/');

                if (!string.IsNullOrEmpty(appPath))
                {
                    if (!decoded.StartsWith(
                        appPath + "/",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            "Path is outside this application.");
                    }

                    decoded = decoded.Substring(appPath.Length);
                }

                decoded = decoded.TrimStart('/');
            }

            // Only allow files inside the document upload folder.
            string allowedRoot = Path.GetFullPath(
                Server.MapPath("~/Uploads/Documents/"));

            string allowedPrefix =
                allowedRoot.TrimEnd(Path.DirectorySeparatorChar) +
                Path.DirectorySeparatorChar;

            string applicationRoot = Path.GetFullPath(
                Server.MapPath("~/"));

            string physicalPath = Path.GetFullPath(
                Path.Combine(
                    applicationRoot,
                    decoded.Replace(
                        '/',
                        Path.DirectorySeparatorChar)));

            if (!physicalPath.StartsWith(
                allowedPrefix,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "File is outside the document upload folder.");
            }

            return physicalPath;
        }

        // NEW: SAFE FILE NAME
        private string GetSafeFileName(
            string fileName,
            string physicalPath)
        {
            string name = Path.GetFileName(
                (fileName ?? "").Replace('\\', '/'));

            if (string.IsNullOrWhiteSpace(name))
                name = Path.GetFileName(physicalPath);

            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');

            name = System.Text.RegularExpressions.Regex.Replace(
                name,
                @"[^A-Za-z0-9._-]",
                "_");

            return string.IsNullOrWhiteSpace(name)
                ? "Document"
                : name;
        }

        // NEW: DOWNLOAD ONE FILE WITHOUT LOADING IT INTO MEMORY
        private void SendFileDownload(
            string physicalPath,
            string fileName)
        {
            Response.Clear();
            Response.BufferOutput = false;
            Response.ContentType = "application/octet-stream";

            Response.AddHeader(
                "Content-Disposition",
                "attachment; filename=\"" + fileName + "\"");

            Response.AddHeader(
                "Content-Length",
                new FileInfo(physicalPath).Length.ToString());

            Response.Cache.SetCacheability(
                HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.TransmitFile(physicalPath);

            Context.ApplicationInstance.CompleteRequest();
        }

        // NEW: DOWNLOAD ZIP
        private void SendBytesDownload(
            byte[] bytes,
            string fileName,
            string contentType)
        {
            Response.Clear();
            Response.BufferOutput = true;
            Response.ContentType = contentType;

            Response.AddHeader(
                "Content-Disposition",
                "attachment; filename=\"" + fileName + "\"");

            Response.AddHeader(
                "Content-Length",
                bytes.Length.ToString());

            Response.Cache.SetCacheability(
                HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            Response.BinaryWrite(bytes);

            Context.ApplicationInstance.CompleteRequest();
        }

        // ORIGINAL: MESSAGES
        private void ShowMsg(string html, bool ok)
        {
            lblMsg.CssClass = ok ? "msg ok" : "msg err";
            lblMsg.Text = Server.HtmlEncode(html);
            lblMsg.Visible = true;
        }

        // ORIGINAL: LOGOUT
        protected void btnLogoutSide_Click(
            object sender,
            EventArgs e)
        {
            StaffAuth.SignOutStaff();
            Session.Clear();
            Session.Abandon();

            Response.Redirect(
                "~/Account/Login.aspx",
                false);

            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
