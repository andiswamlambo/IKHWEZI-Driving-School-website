using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
namespace WebApplicationM3.Account
{
    public partial class StaffNotify : Page
    {
        private string CS { get { return ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString; } }
        protected void Page_Init(object sender, EventArgs e) { if (Session != null) ViewStateUserKey = Session.SessionID; }
        protected void Page_Load(object sender, EventArgs e) { if (!StaffAuth.CheckAccess("Manager")) return; if (!IsPostBack) BindStaff(); }
        private void BindStaff() { using (var c = new SqlConnection(CS)) using (var cmd = new SqlCommand("SELECT StafEmailAddress, Name, Surname FROM dbo.Admin_Staff WHERE AdminStatus='Employed' ORDER BY Name,Surname", c)) { c.Open(); using (var r = cmd.ExecuteReader()) { ddlStaff.Items.Clear(); while (r.Read()) ddlStaff.Items.Add(new ListItem(Convert.ToString(r["Name"]) + " " + Convert.ToString(r["Surname"]) + " (" + Convert.ToString(r["StafEmailAddress"]) + ")", Convert.ToString(r["StafEmailAddress"]))); } } }
        protected void GroupChanged(object sender, EventArgs e) { pnlIndividual.Visible = ddlGroup.SelectedValue == "Individual"; litPreview.Text = ""; }
        private List<string> Recipients() { var result = new List<string>(); using (var c = new SqlConnection(CS)) using (var cmd = new SqlCommand(@"SELECT DISTINCT StafEmailAddress FROM dbo.Admin_Staff WHERE AdminStatus='Employed' AND StafEmailAddress IS NOT NULL AND ( @Group='All' OR (@Group='Individual' AND StafEmailAddress=@Email) OR Role=@Group)", c)) { cmd.Parameters.Add("@Group", SqlDbType.NVarChar, 30).Value = ddlGroup.SelectedValue; cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 256).Value = ddlStaff.SelectedValue ?? ""; c.Open(); using (var r = cmd.ExecuteReader()) while (r.Read()) { string email = Convert.ToString(r[0]); try { var address = new MailAddress(email); if (address.Address.Equals(email, StringComparison.OrdinalIgnoreCase)) result.Add(email); } catch (FormatException) { } } } return result; }
        protected void Preview_Click(object sender, EventArgs e) { try { var emails = Recipients(); litPreview.Text = "<p>" + emails.Count + " recipient(s): " + HttpUtility.HtmlEncode(string.Join(", ", emails)) + "</p>"; } catch (Exception ex) { System.Diagnostics.Trace.TraceError(ex.ToString()); lblResult.Text = "Unable to load recipients."; } }
        protected void Send_Click(object sender, EventArgs e) { if (!StaffAuth.CheckAccess("Manager")) return; string subject = txtSubject.Text.Trim(), body = txtMessage.Text.Trim(); if (subject.Length < 3 || subject.Length > 160 || body.Length < 5 || body.Length > 5000) { lblResult.Text = "Enter a subject (3–160 characters) and message (5–5000 characters)."; return; } try { var emails = Recipients(); if (emails.Count == 0) { lblResult.Text = "No active recipients found."; return; } int sent = 0; foreach (string email in emails) if (EmailHelper.SendStaffAnnouncement(email, subject, body)) sent++; System.Diagnostics.Trace.TraceInformation("Staff announcement by " + StaffAuth.Email + "; accepted=" + sent + "; failed=" + (emails.Count - sent)); lblResult.Text = "Email service accepted " + sent + " of " + emails.Count + " messages. This does not guarantee inbox delivery."; if (sent == emails.Count) { txtSubject.Text = ""; txtMessage.Text = ""; } } catch (Exception ex) { System.Diagnostics.Trace.TraceError(ex.ToString()); lblResult.Text = "Email sending failed. Check server logs."; } }
    }
}