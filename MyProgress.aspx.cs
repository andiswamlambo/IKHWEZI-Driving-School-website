using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;

namespace WebApplicationM3
{
    public partial class MyProgress : System.Web.UI.Page
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
                Response.Redirect("~/Account/Login.aspx?return=MyProgress", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                LoadProgress();
            }
        }

        private void LoadProgress()
        {
            try
            {
                string cs = ConfigurationManager
                    .ConnectionStrings["KhweziConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(cs))
                {
                    conn.Open();

                    // Get total lessons, completed lessons, upcoming lessons
                    int totalBooked = 0;
                    int completed = 0;
                    int upcoming = 0;

                    using (SqlCommand cmd = new SqlCommand(
                        @"SELECT 
                            (SELECT COUNT(*) FROM Booking WHERE StudEmailAddress = (SELECT StudEmailAddress FROM Students WHERE StudNumber = @S)) AS TotalBooked,
                            (SELECT COUNT(*) FROM Booking WHERE StudEmailAddress = (SELECT StudEmailAddress FROM Students WHERE StudNumber = @S) AND BookingStatus = 'Completed') AS Completed,
                            (SELECT COUNT(*) FROM Booking WHERE StudEmailAddress = (SELECT StudEmailAddress FROM Students WHERE StudNumber = @S) AND BookingStatus IN ('Confirmed', 'Pending', 'Unsettled')) AS Upcoming", conn))
                    {
                        cmd.Parameters.AddWithValue("@S", StudNumber);
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                totalBooked = Convert.ToInt32(r["TotalBooked"]);
                                completed = Convert.ToInt32(r["Completed"]);
                                upcoming = Convert.ToInt32(r["Upcoming"]);
                            }
                        }
                    }

                    // Setup basic stats
                    litTotalBooked.Text = totalBooked.ToString();
                    litCompleted.Text = completed.ToString();
                    litUpcoming.Text = upcoming.ToString();

                    // Hours driven (Assuming 1 hour per completed lesson)
                    litHours.Text = completed.ToString();

                    // ================= MILESTONES =================

                    // MS1: Account Created (always true if we got this far)
                    ms1.Attributes["class"] = "milestone achieved";

                    // MS2: ID Uploaded
                    bool hasID = false;
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(
                            @"SELECT COUNT(*) FROM StudentDocuments 
                              WHERE StudNumber = @S AND DocumentType = 'ID'", conn))
                        {
                            cmd.Parameters.AddWithValue("@S", StudNumber);
                            hasID = (int)cmd.ExecuteScalar() > 0;
                        }
                    }
                    catch { } // Ignore if table doesn't exist yet

                    if (hasID)
                    {
                        ms2.Attributes["class"] = "milestone achieved";
                        ms2Status.InnerText = "Unlocked";
                    }

                    // MS3: First Lesson Booked
                    if (totalBooked > 0)
                    {
                        ms3.Attributes["class"] = "milestone achieved";
                        ms3Status.InnerText = "Unlocked";
                    }

                    // MS4: First Lesson Completed
                    if (completed > 0)
                    {
                        ms4.Attributes["class"] = "milestone achieved";
                        ms4Status.InnerText = "Unlocked";
                    }

                    // MS5: 5 Lessons Completed
                    if (completed >= 5)
                    {
                        ms5.Attributes["class"] = "milestone achieved";
                        ms5Status.InnerText = "Unlocked";
                    }

                    // MS6: Test Ready (18+ Completed)
                    if (completed >= 18)
                    {
                        ms6.Attributes["class"] = "milestone achieved";
                        ms6Status.InnerText = "Unlocked";
                    }

                    // MS7: Fully Paid (Check if there's a balance due)
                    bool fullyPaid = false;
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(
                            @"SELECT TOP 1 
                                CASE WHEN ISNULL(TotalPaid, 0) >= ISNULL(PackagePrice, 0) * 0.999 THEN 1 ELSE 0 END
                              FROM Booking 
                              WHERE StudEmailAddress = (SELECT StudEmailAddress FROM Students WHERE StudNumber = @S)
                                AND PackagePrice > 0
                              ORDER BY BookingID DESC", conn))
                        {
                            cmd.Parameters.AddWithValue("@S", StudNumber);
                            object r = cmd.ExecuteScalar();
                            if (r != null && r != DBNull.Value && Convert.ToInt32(r) == 1)
                            {
                                fullyPaid = true;
                            }
                        }
                    }
                    catch { } // In case legacy columns aren't set up

                    if (fullyPaid)
                    {
                        ms7.Attributes["class"] = "milestone achieved";
                        ms7Status.InnerText = "Unlocked";
                    }
                }
            }
            catch 
            {
                // Optional: Log the error
            }
        }
    }
}