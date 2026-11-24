using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;

namespace WebApplicationM3.Account
{
    public partial class VerifyResetCode : System.Web.UI.Page
    {
        private const int MaxAttempts = 3;

        private string Email
        {
            get { return Session["PwdResetEmail"] as string ?? ""; }
            set { Session["PwdResetEmail"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string q = Request.QueryString["email"];
                if (!string.IsNullOrEmpty(q))
                    Email = q.Trim().ToLowerInvariant();

                if (string.IsNullOrEmpty(Email))
                {
                    Response.Redirect("~/Account/ForgotPassword.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }

                litEmail.Text = Server.HtmlEncode(Email);
            }
        }

        protected void btnVerify_Click(object sender, EventArgs e)
        {
            lblMsg.Visible = false;

            string code = txtCode.Text.Trim();

            if (code.Length != 6 || !int.TryParse(code, out _))
            {
                ShowMsg("Please enter the 6-digit code.", false);
                return;
            }

            try
            {
                string cs = ConfigurationManager
                    .ConnectionStrings["KhweziConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(cs))
                {
                    conn.Open();

                    int resetId = 0;
                    int attempts = 0;
                    DateTime expiresOn = DateTime.MinValue;
                    string storedCode = null;

                    using (SqlCommand cmd = new SqlCommand(
                        @"SELECT TOP 1 ResetID, Code, ExpiresOn, Attempts
                          FROM PasswordReset
                          WHERE Email = @E AND UsedOn IS NULL
                          ORDER BY RequestedOn DESC", conn))
                    {
                        cmd.Parameters.AddWithValue("@E", Email);
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (!r.Read())
                            {
                                ShowMsg("No active reset request found. Please request a new code.", false);
                                return;
                            }
                            resetId = (int)r["ResetID"];
                            storedCode = r["Code"].ToString();
                            expiresOn = Convert.ToDateTime(r["ExpiresOn"]);
                            attempts = Convert.ToInt32(r["Attempts"]);
                        }
                    }

                    if (DateTime.Now > expiresOn)
                    {
                        MarkUsed(resetId);
                        ShowMsg("That code has expired. Please request a new one.", false);
                        return;
                    }

                    if (attempts >= MaxAttempts)
                    {
                        MarkUsed(resetId);
                        ShowMsg("Too many incorrect attempts. Please request a new code.", false);
                        return;
                    }

                    if (!code.Equals(storedCode, StringComparison.Ordinal))
                    {
                        IncrementAttempts(resetId);
                        int remaining = MaxAttempts - (attempts + 1);
                        ShowMsg("Incorrect code. " +
                                (remaining > 0
                                    ? remaining + " attempt(s) left."
                                    : "Please request a new code."), false);
                        return;
                    }

                    MarkUsed(resetId);

                    Session["PwdResetVerified"] = true;
                    Session["PwdResetResetId"] = resetId;

                    Response.Redirect("~/Account/ResetPassword.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
            }
            catch (Exception ex)
            {
                ShowMsg("Error: " + ex.Message, false);
            }
        }

        private void MarkUsed(int resetId)
        {
            try
            {
                string cs = ConfigurationManager
                    .ConnectionStrings["KhweziConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"UPDATE PasswordReset SET UsedOn = GETDATE()
                      WHERE ResetID = @ID", conn))
                {
                    cmd.Parameters.AddWithValue("@ID", resetId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch { }
        }

        private void IncrementAttempts(int resetId)
        {
            try
            {
                string cs = ConfigurationManager
                    .ConnectionStrings["KhweziConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(
                    @"UPDATE PasswordReset SET Attempts = Attempts + 1
                      WHERE ResetID = @ID", conn))
                {
                    cmd.Parameters.AddWithValue("@ID", resetId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch { }
        }

        private void ShowMsg(string html, bool ok)
        {
            lblMsg.ForeColor = ok
                ? System.Drawing.Color.FromArgb(20, 102, 63)
                : System.Drawing.Color.FromArgb(163, 20, 31);
            lblMsg.Text = Server.HtmlEncode(html);
            lblMsg.Visible = true;
        }
    }
}