using System;
using System.Configuration;
using System.Data.SqlClient;

namespace WebApplicationM3
{
    public partial class ManagerOverrides : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!StaffAuth.CheckAccess("Manager")) return;
        }

        // ==========================================
        // 1. MANUAL LESSON ADJUSTMENT
        // ==========================================
        protected void btnInjectLesson_Click(object sender, EventArgs e)
        {
            string student = txtLessonStudent.Text.Trim();
            string reason = txtLessonReason.Text.Trim();
            int count;

            if (string.IsNullOrEmpty(student) || string.IsNullOrEmpty(reason) || !int.TryParse(txtLessonCount.Text.Trim(), out count) || count == 0)
            {
                ShowMsg("Please provide the student, a valid lesson count, and a reason.", false); return;
            }

            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                {
                    conn.Open();

                    int studNumber = 0;
                    using (SqlCommand cmd = new SqlCommand("SELECT StudNumber FROM Students WHERE StudEmailAddress = @E OR CAST(StudNumber AS VARCHAR(20)) = @E", conn))
                    {
                        cmd.Parameters.AddWithValue("@E", student);
                        object r = cmd.ExecuteScalar();
                        if (r == null) { ShowMsg("Student not found.", false); return; }
                        studNumber = Convert.ToInt32(r);
                    }

                    int pkgId = 0;
                    using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 PackageID FROM StudentPackage WHERE StudNumber = @S AND IsActive = 1 ORDER BY PurchasedOn DESC", conn))
                    {
                        cmd.Parameters.AddWithValue("@S", studNumber);
                        object r = cmd.ExecuteScalar();
                        if (r != null) pkgId = Convert.ToInt32(r);
                    }

                    if (pkgId > 0)
                    {
                        using (SqlCommand cmd = new SqlCommand("UPDATE StudentPackage SET TotalLessons = TotalLessons + @C WHERE StudNumber = @S AND PackageID = @P AND IsActive = 1", conn))
                        {
                            cmd.Parameters.AddWithValue("@C", count);
                            cmd.Parameters.AddWithValue("@S", studNumber);
                            cmd.Parameters.AddWithValue("@P", pkgId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        if (count < 0) { ShowMsg("Cannot deduct lessons. Student has no active package.", false); return; }
                        using (SqlCommand cmd = new SqlCommand("INSERT INTO StudentPackage (StudNumber, PackageID, TotalLessons, UsedLessons, IsActive) VALUES (@S, 2, @C, 0, 1)", conn))
                        {
                            cmd.Parameters.AddWithValue("@S", studNumber);
                            cmd.Parameters.AddWithValue("@C", count);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    LogAudit("LessonAdjustment", student, reason);
                    ShowMsg("Successfully adjusted lessons by " + count + ". Reason logged.", true);
                    txtLessonStudent.Text = ""; txtLessonCount.Text = ""; txtLessonReason.Text = "";
                }
            }
            catch (Exception ex) { ShowMsg("Error: " + ex.Message, false); }
        }

        // ==========================================
        // 2. STAFF PASSWORD RECOVERY (Silent Send)
        // ==========================================
        protected void btnResetStaff_Click(object sender, EventArgs e)
        {
            string email = txtResetStaff.Text.Trim().ToLowerInvariant();
            string reason = txtResetReason.Text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(reason)) { ShowMsg("Please provide the staff email and a reason.", false); return; }

            try
            {
                // 1. Generate a unique 8-character password
                string tempPassword = "IKH" + Guid.NewGuid().ToString("N").Substring(0, 5).ToUpper();
                string hashed = PasswordHelper.Hash(tempPassword);

                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                {
                    conn.Open();

                    // 2. Find the staff member to get their Name and Phone
                    string name = "", phone = "";
                    using (SqlCommand cmd = new SqlCommand("SELECT Name, ISNULL(PhoneNum, '') FROM Admin_Staff WHERE StafEmailAddress = @Email", conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (!r.Read()) { ShowMsg("No staff member found.", false); return; }
                            name = r.GetString(0);
                            phone = r.GetString(1);
                        }
                    }

                    // 3. Update DB with the new password and force change flag
                    using (SqlCommand cmd = new SqlCommand(
                        "UPDATE Admin_Staff SET PasswordHash = @Pwd, MustChangePassword = 1, AdminStatus = 'Employed' WHERE StafEmailAddress = @Email", conn))
                    {
                        cmd.Parameters.AddWithValue("@Pwd", hashed);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.ExecuteNonQuery();
                    }

                    LogAudit("PasswordReset", email, reason);

                    // 4. Silently send the password via Email
                    try
                    {
                        string emailBody = "Hi " + name + ",<br/><br/>Your IKHWEZI account password has been reset.<br/><br/>" +
                                           "<strong>Username:</strong> " + email + "<br/>" +
                                           "<strong>Temporary Password:</strong> " + tempPassword + "<br/><br/>" +
                                           "Please log in and change this password immediately.";
                        EmailHelper.SendStaffAnnouncement(email, "Your Temporary Password", emailBody);
                    }
                    catch { }

                    // 5. Generate the WhatsApp link (Manager can click it to send the message)
                    string waMessage = "Hi " + name + ", your IKHWEZI account password has been reset. Login with: " + tempPassword + " and change it immediately.";
                    string waPhone = phone.TrimStart('0');
                    waPhone = "27" + waPhone;

                    // Instead of showing a popup, we show a green success message with a WhatsApp button
                    string msg = "Password successfully reset! An email has been sent to " + email + ".";
                    msg += "<br/><br/><a href='https://wa.me/" + waPhone + "?text=" + Server.UrlEncode(waMessage) + "' target='_blank' style='display:inline-block; padding:10px 20px; background:#25D366; color:white; border-radius:8px; text-decoration:none; font-weight:bold;'>Send via WhatsApp</a>";

                    ShowMsg(msg, true);
                    txtResetStaff.Text = ""; txtResetReason.Text = "";
                }
            }
            catch (Exception ex) { ShowMsg("Error: " + ex.Message, false); }
        }

        // ==========================================
        // 3. ARCHIVE / RESTORE STUDENT
        // ==========================================
        protected void btnArchiveStudent_Click(object sender, EventArgs e)
        {
            string student = txtArchiveStudent.Text.Trim();
            string reason = txtArchiveReason.Text.Trim();
            if (string.IsNullOrEmpty(student) || string.IsNullOrEmpty(reason)) { ShowMsg("Please provide the student and a reason.", false); return; }

            UpdateStudentStatus(student, "Archived", "ArchiveStudent", reason);
        }

        protected void btnRestoreStudent_Click(object sender, EventArgs e)
        {
            string student = txtArchiveStudent.Text.Trim();
            string reason = txtArchiveReason.Text.Trim();
            if (string.IsNullOrEmpty(student) || string.IsNullOrEmpty(reason)) { ShowMsg("Please provide the student and a reason.", false); return; }

            UpdateStudentStatus(student, "Active", "RestoreStudent", reason);
        }

        private void UpdateStudentStatus(string student, string status, string action, string reason)
        {
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand("UPDATE Students SET Status = @S WHERE StudEmailAddress = @Input OR CAST(StudNumber AS VARCHAR(20)) = @Input", conn))
                {
                    cmd.Parameters.AddWithValue("@S", status);
                    cmd.Parameters.AddWithValue("@Input", student);
                    conn.Open();
                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        LogAudit(action, student, reason);
                        ShowMsg("Student status updated to " + status + ". Reason logged.", true);
                        txtArchiveStudent.Text = ""; txtArchiveReason.Text = "";
                    }
                    else ShowMsg("No student found.", false);
                }
            }
            catch (Exception ex) { ShowMsg("Error: " + ex.Message, false); }
        }

        // ==========================================
        // 4. SUSPEND / REACTIVATE STAFF
        // ==========================================
        protected void btnSuspendStaff_Click(object sender, EventArgs e)
        {
            string email = txtSuspendStaff.Text.Trim().ToLowerInvariant();
            string reason = txtSuspendReason.Text.Trim();
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(reason)) { ShowMsg("Please provide the staff email and a reason.", false); return; }

            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("UPDATE Admin_Staff SET Role = 'Suspended', AdminStatus = 'Suspended' WHERE StafEmailAddress = @Email", conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        if (cmd.ExecuteNonQuery() > 0)
                        {
                            using (SqlCommand cmdInstructor = new SqlCommand("UPDATE Instructor SET Specialization = 'Inactive' WHERE InstruEmailAddress = @Email", conn))
                            {
                                cmdInstructor.Parameters.AddWithValue("@Email", email);
                                cmdInstructor.ExecuteNonQuery();
                            }
                            LogAudit("SuspendStaff", email, reason);
                            ShowMsg("Staff member suspended. Reason logged.", true);
                            txtSuspendStaff.Text = ""; txtSuspendReason.Text = "";
                        }
                        else ShowMsg("No staff found with that email.", false);
                    }
                }
            }
            catch (Exception ex) { ShowMsg("Error: " + ex.Message, false); }
        }

        protected void btnReactivateStaff_Click(object sender, EventArgs e)
        {
            string email = txtSuspendStaff.Text.Trim().ToLowerInvariant();
            string reason = txtSuspendReason.Text.Trim();
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(reason)) { ShowMsg("Please provide the staff email and a reason.", false); return; }

            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(
                        @"UPDATE Admin_Staff 
                          SET AdminStatus = 'Employed', 
                              Role = CASE WHEN EXISTS(SELECT 1 FROM Instructor WHERE InstruEmailAddress = @Email) THEN 'Instructor' ELSE 'Admin' END
                          WHERE StafEmailAddress = @Email", conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        if (cmd.ExecuteNonQuery() > 0)
                        {
                            using (SqlCommand cmdInstructor = new SqlCommand("UPDATE Instructor SET Specialization = 'Manual' WHERE InstruEmailAddress = @Email AND Specialization = 'Inactive'", conn))
                            {
                                cmdInstructor.Parameters.AddWithValue("@Email", email);
                                cmdInstructor.ExecuteNonQuery();
                            }
                            LogAudit("ReactivateStaff", email, reason);
                            ShowMsg("Staff member reactivated. Reason logged.", true);
                            txtSuspendStaff.Text = ""; txtSuspendReason.Text = "";
                        }
                        else ShowMsg("No staff found with that email.", false);
                    }
                }
            }
            catch (Exception ex) { ShowMsg("Error: " + ex.Message, false); }
        }

        // ==========================================
        // AUDIT LOG HELPER
        // ==========================================
        private void LogAudit(string actionType, string targetUser, string reason)
        {
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"INSERT INTO ManagerAudit (ManagerEmail, ActionType, TargetUser, Reason, ActionDate) 
                      VALUES (@Manager, @Action, @Target, @Reason, GETDATE())", conn))
                {
                    cmd.Parameters.AddWithValue("@Manager", StaffAuth.Email);
                    cmd.Parameters.AddWithValue("@Action", actionType);
                    cmd.Parameters.AddWithValue("@Target", (object)targetUser ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Reason", reason);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch { /* Silent fail so it doesn't break the UI if audit fails */ }
        }

        private void ShowMsg(string html, bool ok)
        {
            lblMsg.CssClass = "msg " + (ok ? "ok" : "err");
            lblMsg.Text = html;
            lblMsg.Visible = true;
        }
    }
}