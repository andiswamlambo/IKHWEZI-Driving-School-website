using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace WebApplicationM3
{
    public partial class ManagerStudents : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!StaffAuth.CheckAccess("Manager")) return;
            if (!IsPostBack) LoadStudents();
        }

        private void LoadStudents()
        {
            try
            {
                string search = txtSearch.Text.Trim();
                string cs = ConfigurationManager.ConnectionStrings["KhweziConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(cs))
                {
                    string sql = @"SELECT StudNumber, LTRIM(RTRIM(ISNULL(Name,'') + ' ' + ISNULL(Surname,''))) AS FullName,
                                          StudEmailAddress, ISNULL(PhoneNum, 'N/A') AS PhoneNum, ISNULL(Status, 'Active') AS Status
                                   FROM Students WHERE ISNULL(Status, 'Active') <> 'Archived'";

                    if (search.Length > 0)
                        sql += @" AND (Name LIKE @Search OR Surname LIKE @Search OR StudEmailAddress LIKE @Search OR CAST(StudNumber AS VARCHAR(20)) LIKE @Search)";

                    sql += " ORDER BY Name ASC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        if (search.Length > 0) cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            repStudents.DataSource = dt;
                            repStudents.DataBind();
                        }
                    }
                }
            }
            catch (Exception ex) { System.Diagnostics.Trace.TraceError("ManagerStudents Load Error: " + ex.Message); }
        }

        protected void btnSearch_Click(object sender, EventArgs e) { LoadStudents(); }
    }
}