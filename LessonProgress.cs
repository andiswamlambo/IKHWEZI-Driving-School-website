using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace WebApplicationM3
{
    public static class LessonProgress
    {
        private const string SaTimeZoneId = "South Africa Standard Time";
        private static readonly TimeSpan DayStart = new TimeSpan(7, 0, 0);
        private static readonly TimeSpan DayEnd = new TimeSpan(17, 0, 0);
        private static readonly TimeSpan SatStart = new TimeSpan(8, 0, 0);
        private static readonly TimeSpan SatEnd = new TimeSpan(13, 0, 0);

        public const int DefaultMaxUpcomingBookings = 5;
        public const int InstalmentDays = 60;
        public const int OverdueGraceDays = 14;
        public const int LessonCancellationNoticeHours = 24;

        private const int LearnersCancelNoticeWorkingDays = 3;

        private static readonly TimeSpan CompletionLeadWindow = TimeSpan.FromMinutes(15);
        private const int LateCompletionWindowHours = 48;

        private static bool? _pkgTableReady;
        private static bool? _auditTableReady;
        private static bool? _bookingHasVehicle;
        private static readonly object _schemaLock = new object();

        private static string Cs()
        {
            return ConfigurationManager
                .ConnectionStrings["KhweziConnection"].ConnectionString;
        }

        // ================= TIME =================
        public static DateTime NowSa()
        {
            try
            {
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                    TimeZoneInfo.FindSystemTimeZoneById(SaTimeZoneId));
            }
            catch { return DateTime.Now; }
        }
        public static DateTime TodaySa() { return NowSa().Date; }

        // ================= TRADING HOURS =================
        public static string ValidateSlot(DateTime when, int durationMinutes = 60)
        {
            if (when.Date.DayOfWeek == DayOfWeek.Sunday)
                return "We are closed on Sundays. Please choose Monday to Saturday.";

            if (when <= NowSa())
                return "That time has already passed. Please choose a later time or a future date.";

            var dur = TimeSpan.FromMinutes(Math.Max(1, durationMinutes));
            TimeSpan s = when.TimeOfDay;
            TimeSpan e = s.Add(dur);

            if (when.Date.DayOfWeek == DayOfWeek.Saturday)
            {
                if (s < SatStart || e > SatEnd)
                    return "Saturday lessons run 08:00 to 13:00. Please pick an earlier slot.";
                return null;
            }

            if (s < DayStart || e > DayEnd)
                return "Lessons run 07:00 to 17:00 (Saturdays 08:00 to 13:00). Please pick a slot in that range.";
            return null;
        }

        // ================= SCHEMA HELPERS =================
        private static void EnsurePackageTable(SqlConnection conn)
        {
            lock (_schemaLock)
            {
                if (_pkgTableReady == true) return;
                using (SqlCommand cmd = new SqlCommand(
                    @"IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
                                     WHERE TABLE_NAME = 'StudentPackage')
                      CREATE TABLE StudentPackage
                      (
                          StudentPackageID INT IDENTITY(1,1) PRIMARY KEY,
                          StudNumber       INT NOT NULL,
                          PackageID        INT NOT NULL,
                          TotalLessons     INT NOT NULL,
                          UsedLessons      INT NOT NULL DEFAULT 0,
                          PurchasedOn      DATETIME NOT NULL DEFAULT GETDATE(),
                          IsActive         BIT NOT NULL DEFAULT 1
                      )", conn))
                    cmd.ExecuteNonQuery();
                _pkgTableReady = true;
            }
        }

        private static void EnsureAuditTable(SqlConnection conn)
        {
            lock (_schemaLock)
            {
                if (_auditTableReady == true) return;
                using (SqlCommand cmd = new SqlCommand(
                    @"IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
                                     WHERE TABLE_NAME = 'BookingAudit')
                      CREATE TABLE BookingAudit
                      (
                          AuditID    INT IDENTITY(1,1) PRIMARY KEY,
                          BookingID  INT NOT NULL,
                          Action     VARCHAR(30) NOT NULL,
                          ActorEmail VARCHAR(200) NULL,
                          ActorRole  VARCHAR(20) NULL,
                          ActionTime DATETIME NOT NULL,
                          Notes      NVARCHAR(500) NULL
                      )", conn))
                    cmd.ExecuteNonQuery();
                _auditTableReady = true;
            }
        }

        private static bool BookingHasVehicle()
        {
            lock (_schemaLock)
            {
                if (_bookingHasVehicle.HasValue) return _bookingHasVehicle.Value;
                try
                {
                    using (SqlConnection conn = new SqlConnection(Cs()))
                    using (SqlCommand cmd = new SqlCommand(
                        @"SELECT COUNT(1) FROM INFORMATION_SCHEMA.COLUMNS
                          WHERE TABLE_NAME = 'Booking' AND COLUMN_NAME = 'VehicleID'", conn))
                    {
                        conn.Open();
                        _bookingHasVehicle = ((int)cmd.ExecuteScalar()) > 0;
                    }
                }
                catch { _bookingHasVehicle = false; }
                return _bookingHasVehicle.Value;
            }
        }

        // ================= MODELS =================
        public class PackageProgress
        {
            public int StudentPackageID { get; set; }
            public int PackageID { get; set; }
            public string PackageName { get; set; }
            public int TotalLessons { get; set; }
            public int UsedLessons { get; set; }
            public int RemainingLessons { get { return TotalLessons - UsedLessons; } }
        }

        public class PackageStats
        {
            public string PackageName { get; set; }
            public int TotalLessons { get; set; }
            public int CompletedLessons { get; set; }
            public int UpcomingLessons { get; set; }
            public int RemainingLessons
            {
                get
                {
                    int r = TotalLessons - CompletedLessons - UpcomingLessons;
                    return r < 0 ? 0 : r;
                }
            }
        }

        public class PaymentSummary
        {
            public decimal PackagePrice { get; set; }
            public decimal TotalPaid { get; set; }
            public decimal BalanceDue { get { return PackagePrice - TotalPaid; } }
            public int PercentPaid
            {
                get
                {
                    if (PackagePrice <= 0) return 0;
                    return (int)Math.Round(100.0m * TotalPaid / PackagePrice);
                }
            }
            public DateTime? DueDate { get; set; }
            public int DaysRemaining
            {
                get
                {
                    if (DueDate == null) return 0;
                    return (int)Math.Round((DueDate.Value - NowSa()).TotalDays);
                }
            }
            public bool IsFullyPaid { get { return BalanceDue <= 0.01m; } }
            public bool IsOverdue
            {
                get { return DueDate != null && NowSa() > DueDate.Value && !IsFullyPaid; }
            }
        }

        // ================= BASIC LOOKUPS =================
        public static string GetStudentEmail(int studNumber)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Cs()))
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT TOP 1 StudEmailAddress FROM Students WHERE StudNumber = @N", conn))
                {
                    cmd.Parameters.AddWithValue("@N", studNumber);
                    conn.Open();
                    object r = cmd.ExecuteScalar();
                    return r == null ? null : r.ToString().Trim();
                }
            }
            catch { return null; }
        }

        public static string GetStudentDisplayName(string studEmail)
        {
            if (string.IsNullOrEmpty(studEmail)) return "Student";
            try
            {
                using (SqlConnection conn = new SqlConnection(Cs()))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT TOP 1 LTRIM(RTRIM(Name)) + ' ' + LTRIM(RTRIM(Surname))
                      FROM Students WHERE StudEmailAddress = @E", conn))
                {
                    cmd.Parameters.AddWithValue("@E", studEmail);
                    conn.Open();
                    object r = cmd.ExecuteScalar();
                    return r == null || r == DBNull.Value ? "Student" : r.ToString();
                }
            }
            catch { return "Student"; }
        }

        private static int GetStudentNumberTx(SqlConnection conn, SqlTransaction tx, string studEmail)
        {
            using (SqlCommand cmd = new SqlCommand(
                "SELECT TOP 1 StudNumber FROM Students WHERE StudEmailAddress = @E", conn, tx))
            {
                cmd.Parameters.AddWithValue("@E", studEmail);
                object r = cmd.ExecuteScalar();
                return r == null ? -1 : Convert.ToInt32(r);
            }
        }

        private static string GetPackageTypeTx(SqlConnection conn, SqlTransaction tx, int packageId)
        {
            using (SqlCommand cmd = new SqlCommand(
                "SELECT PackageType FROM Packages WHERE PackageID = @P", conn, tx))
            {
                cmd.Parameters.AddWithValue("@P", packageId);
                object r = cmd.ExecuteScalar();
                return r == null ? "" : r.ToString();
            }
        }

        private static PackageProgress FindWithRemaining(
            SqlConnection conn, SqlTransaction tx, int studNumber, int? preferPackageId)
        {
            string sql =
                @"SELECT TOP 1 sp.StudentPackageID, sp.PackageID,
                         p.PackageName, sp.TotalLessons, sp.UsedLessons
                  FROM StudentPackage sp
                  JOIN Packages p ON p.PackageID = sp.PackageID
                  WHERE sp.StudNumber = @S AND sp.IsActive = 1
                    AND sp.UsedLessons < sp.TotalLessons
                    AND ISNULL(p.PackageType, '') <> 'Learners' " +
                (preferPackageId.HasValue
                    ? "ORDER BY CASE WHEN sp.PackageID = @P THEN 0 ELSE 1 END, sp.PurchasedOn"
                    : "ORDER BY sp.PurchasedOn");

            using (SqlCommand cmd = tx == null ? new SqlCommand(sql, conn) : new SqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@S", studNumber);
                if (preferPackageId.HasValue) cmd.Parameters.AddWithValue("@P", preferPackageId.Value);

                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        return new PackageProgress
                        {
                            StudentPackageID = (int)r["StudentPackageID"],
                            PackageID = (int)r["PackageID"],
                            PackageName = r["PackageName"].ToString(),
                            TotalLessons = Convert.ToInt32(r["TotalLessons"]),
                            UsedLessons = Convert.ToInt32(r["UsedLessons"])
                        };
                    }
                }
            }
            return null;
        }

        private static PackageProgress EnsurePackageRowTx(
            SqlConnection conn, SqlTransaction tx, int studNumber, int packageId)
        {
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT TOP 1 sp.StudentPackageID, sp.PackageID,
                         p.PackageName, sp.TotalLessons, sp.UsedLessons
                  FROM StudentPackage sp
                  JOIN Packages p ON p.PackageID = sp.PackageID
                  WHERE sp.StudNumber = @S AND sp.PackageID = @P AND sp.IsActive = 1
                  ORDER BY sp.PurchasedOn DESC", conn, tx))
            {
                cmd.Parameters.AddWithValue("@S", studNumber);
                cmd.Parameters.AddWithValue("@P", packageId);
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (r.Read())
                        return new PackageProgress
                        {
                            StudentPackageID = (int)r["StudentPackageID"],
                            PackageID = (int)r["PackageID"],
                            PackageName = r["PackageName"].ToString(),
                            TotalLessons = Convert.ToInt32(r["TotalLessons"]),
                            UsedLessons = Convert.ToInt32(r["UsedLessons"])
                        };
                }
            }

            using (SqlCommand cmd = new SqlCommand(
                @"INSERT INTO StudentPackage (StudNumber, PackageID, TotalLessons, UsedLessons)
                  SELECT @S, PackageID, TotalLessons, 0
                  FROM Packages WHERE PackageID = @P", conn, tx))
            {
                cmd.Parameters.AddWithValue("@S", studNumber);
                cmd.Parameters.AddWithValue("@P", packageId);
                cmd.ExecuteNonQuery();
            }

            return FindWithRemainingIncludingLearners(conn, tx, studNumber, packageId);
        }

        private static PackageProgress FindWithRemainingIncludingLearners(
            SqlConnection conn, SqlTransaction tx, int studNumber, int packageId)
        {
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT TOP 1 sp.StudentPackageID, sp.PackageID,
                         p.PackageName, sp.TotalLessons, sp.UsedLessons
                  FROM StudentPackage sp
                  JOIN Packages p ON p.PackageID = sp.PackageID
                  WHERE sp.StudNumber = @S AND sp.PackageID = @P AND sp.IsActive = 1
                  ORDER BY sp.PurchasedOn DESC", conn, tx))
            {
                cmd.Parameters.AddWithValue("@S", studNumber);
                cmd.Parameters.AddWithValue("@P", packageId);
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (r.Read())
                        return new PackageProgress
                        {
                            StudentPackageID = (int)r["StudentPackageID"],
                            PackageID = (int)r["PackageID"],
                            PackageName = r["PackageName"].ToString(),
                            TotalLessons = Convert.ToInt32(r["TotalLessons"]),
                            UsedLessons = Convert.ToInt32(r["UsedLessons"])
                        };
                }
            }
            return null;
        }

        public static PackageProgress EnsureStudentPackage(int studNumber, int packageId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Cs()))
                {
                    conn.Open();
                    EnsurePackageTable(conn);
                    return EnsurePackageRowTx(conn, null, studNumber, packageId);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "EnsureStudentPackage failed for student " + studNumber +
                    ", package " + packageId + ": " + ex);
                return null;
            }
        }

        public static int ActivatePaidPackageTx(
            SqlConnection conn, SqlTransaction tx, int studNumber, int packageId)
        {
            if (conn == null || tx == null)
                throw new ArgumentException("An active SQL transaction is required.");
            if (studNumber <= 0 || packageId <= 0)
                throw new ArgumentException("Invalid student or package.");

            using (SqlCommand cmd = new SqlCommand(
                @"DECLARE @LockResult INT;
                  EXEC @LockResult = sp_getapplock
                      @Resource = @Resource,
                      @LockMode = 'Exclusive',
                      @LockOwner = 'Transaction',
                      @LockTimeout = 10000;
                  SELECT @LockResult;", conn, tx))
            {
                cmd.Parameters.AddWithValue("@Resource",
                    "IKHWEZI-PACKAGE-" + studNumber + "-" + packageId);
                int result = Convert.ToInt32(cmd.ExecuteScalar());
                if (result < 0)
                    throw new InvalidOperationException(
                        "Could not acquire package activation lock.");
            }

            using (SqlCommand cmd = new SqlCommand(
                @"SELECT TOP 1 StudentPackageID
                  FROM dbo.StudentPackage WITH (UPDLOCK, HOLDLOCK)
                  WHERE StudNumber = @Student
                    AND PackageID = @Package
                    AND IsActive = 1
                  ORDER BY PurchasedOn DESC, StudentPackageID DESC", conn, tx))
            {
                cmd.Parameters.AddWithValue("@Student", studNumber);
                cmd.Parameters.AddWithValue("@Package", packageId);
                object existing = cmd.ExecuteScalar();
                if (existing != null && existing != DBNull.Value)
                    return Convert.ToInt32(existing);
            }

            using (SqlCommand cmd = new SqlCommand(
                @"INSERT INTO dbo.StudentPackage
                    (StudNumber, PackageID, TotalLessons, UsedLessons,
                     PurchasedOn, IsActive)
                  OUTPUT INSERTED.StudentPackageID
                  SELECT @Student, PackageID, TotalLessons, 0, GETDATE(), 1
                  FROM dbo.Packages
                  WHERE PackageID = @Package AND TotalLessons > 0;", conn, tx))
            {
                cmd.Parameters.AddWithValue("@Student", studNumber);
                cmd.Parameters.AddWithValue("@Package", packageId);
                object inserted = cmd.ExecuteScalar();
                if (inserted == null || inserted == DBNull.Value)
                    throw new InvalidOperationException("Package not found or has no lessons.");
                return Convert.ToInt32(inserted);
            }
        }

        public static PackageProgress GetActiveStudentPackage(int studNumber)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Cs()))
                {
                    conn.Open();
                    EnsurePackageTable(conn);
                    return FindWithRemaining(conn, null, studNumber, null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "GetActiveStudentPackage failed for student " + studNumber + ": " + ex);
                return null;
            }
        }

        // =========================================================
        // PACKAGE STATS
        // =========================================================
        public static PackageStats GetPackageStats(int studNumber, int packageId)
        {
            var stats = new PackageStats();
            try
            {
                using (SqlConnection conn = new SqlConnection(Cs()))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT PackageName, TotalLessons FROM Packages WHERE PackageID = @P", conn))
                    {
                        cmd.Parameters.AddWithValue("@P", packageId);
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                stats.PackageName = r["PackageName"].ToString();
                                stats.TotalLessons = Convert.ToInt32(r["TotalLessons"]);
                            }
                        }
                    }

                    using (SqlCommand cmd = new SqlCommand(
                        @"SELECT COUNT(*) FROM Lesson L
                          INNER JOIN Booking B ON L.BookingID = B.BookingID
                          INNER JOIN Students S ON B.StudEmailAddress = S.StudEmailAddress
                          WHERE S.StudNumber = @S AND L.PackageID = @P AND L.Status = 'Completed'", conn))
                    {
                        cmd.Parameters.AddWithValue("@S", studNumber);
                        cmd.Parameters.AddWithValue("@P", packageId);
                        stats.CompletedLessons = (int)cmd.ExecuteScalar();
                    }

                    using (SqlCommand cmd = new SqlCommand(
                        @"SELECT COUNT(*) FROM Lesson L
                          INNER JOIN Booking B ON L.BookingID = B.BookingID
                          INNER JOIN Students S ON B.StudEmailAddress = S.StudEmailAddress
                          WHERE S.StudNumber = @S AND L.PackageID = @P
                            AND L.Status IN ('Pending', 'Rescheduled')", conn))
                    {
                        cmd.Parameters.AddWithValue("@S", studNumber);
                        cmd.Parameters.AddWithValue("@P", packageId);
                        stats.UpcomingLessons = (int)cmd.ExecuteScalar();
                    }
                }
            }
            catch { }
            return stats;
        }

        public static int GetRemainingLessons(int studNumber, int packageId)
        {
            return GetPackageStats(studNumber, packageId).RemainingLessons;
        }

        // =========================================================
        // PAYMENT SUMMARY
        // =========================================================
        public static PaymentSummary GetPaymentSummary(int studNumber, int? packageId = null)
        {
            var s = new PaymentSummary();
            try
            {
                using (SqlConnection conn = new SqlConnection(Cs()))
                {
                    conn.Open();

                    string email = GetStudentEmail(studNumber);
                    if (string.IsNullOrEmpty(email)) return s;

                    string sql =
                        @"SELECT TOP 1
                                 ISNULL(PackagePrice, 0) AS PackagePrice,
                                 ISNULL(TotalPaid, 0)    AS TotalPaid,
                                 PaymentDueDate
                          FROM Booking
                          WHERE StudEmailAddress = @E
                            AND BookingStatus IN ('Unsettled','Confirmed','Overdue','Suspended') " +
                        (packageId.HasValue ? " AND PackageID = @P " : "") +
                        @"ORDER BY ScheduledDateTime DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@E", email);
                        if (packageId.HasValue)
                            cmd.Parameters.AddWithValue("@P", packageId.Value);

                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                s.PackagePrice = Convert.ToDecimal(r["PackagePrice"]);
                                s.TotalPaid = Convert.ToDecimal(r["TotalPaid"]);
                                if (r["PaymentDueDate"] != DBNull.Value)
                                    s.DueDate = Convert.ToDateTime(r["PaymentDueDate"]);
                            }
                        }
                    }
                }
            }
            catch { }
            return s;
        }

        // =========================================================
        // RECHECK OVERDUE STATUS
        // =========================================================
        public static void RecheckBookingPaymentStatus(int studNumber)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Cs()))
                {
                    conn.Open();

                    string email = GetStudentEmail(studNumber);
                    if (string.IsNullOrEmpty(email)) return;

                    DateTime now = NowSa();
                    DateTime suspensionDate = now.AddDays(-OverdueGraceDays);

                    using (SqlCommand cmd = new SqlCommand(
                        @"UPDATE Booking
                          SET BookingStatus = 'Overdue'
                          WHERE StudEmailAddress = @E
                            AND BookingStatus = 'Confirmed'
                            AND PaymentDueDate IS NOT NULL
                            AND PaymentDueDate < @Now
                            AND ISNULL(TotalPaid, 0) < ISNULL(PackagePrice, 0) * 0.999", conn))
                    {
                        cmd.Parameters.AddWithValue("@E", email);
                        cmd.Parameters.AddWithValue("@Now", now);
                        cmd.ExecuteNonQuery();
                    }

                    using (SqlCommand cmd = new SqlCommand(
                        @"UPDATE Booking
                          SET BookingStatus = 'Suspended'
                          WHERE StudEmailAddress = @E
                            AND BookingStatus = 'Overdue'
                            AND PaymentDueDate < @SuspensionDate
                            AND ISNULL(TotalPaid, 0) < ISNULL(PackagePrice, 0) * 0.999", conn))
                    {
                        cmd.Parameters.AddWithValue("@E", email);
                        cmd.Parameters.AddWithValue("@SuspensionDate", suspensionDate);
                        cmd.ExecuteNonQuery();
                    }

                    using (SqlCommand cmd = new SqlCommand(
                        @"UPDATE Booking
                          SET BookingStatus = 'Confirmed'
                          WHERE StudEmailAddress = @E
                            AND BookingStatus IN ('Overdue','Suspended')
                            AND ISNULL(TotalPaid, 0) >= ISNULL(PackagePrice, 0) * 0.999", conn))
                    {
                        cmd.Parameters.AddWithValue("@E", email);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { }
        }

        // =========================================================
        // BOOKING GATE
        // =========================================================
        public static string GetBookingBlockReason(int studNumber, string studEmail, int packageId)
        {
            return GetBookingBlockReasonMulti(studNumber, studEmail, packageId, DefaultMaxUpcomingBookings);
        }

        public static string GetBookingBlockReasonMulti(
            int studNumber, string studEmail, int packageId, int maxUpcoming = 5)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Cs()))
                {
                    conn.Open();
                    EnsurePackageTable(conn);

                    var pay = GetPaymentSummary(studNumber, packageId);
                    if (pay.IsOverdue)
                    {
                        return "Your account has an outstanding balance of R " +
                               pay.BalanceDue.ToString("N2") +
                               " that was due on " +
                               pay.DueDate.Value.ToString("d MMM yyyy") +
                               ". Please settle your balance before booking new lessons.";
                    }

                    using (SqlCommand cmd = new SqlCommand(
                        @"SELECT COUNT(*) FROM Booking
                          WHERE StudEmailAddress = @E
                            AND BookingStatus = 'Suspended'", conn))
                    {
                        cmd.Parameters.AddWithValue("@E", studEmail);
                        int suspendedCount = Convert.ToInt32(cmd.ExecuteScalar());
                        if (suspendedCount > 0)
                            return "Your account is currently suspended due to unpaid balance. " +
                                   "Please contact reception to resolve this.";
                    }

                    int upcomingCount;
                    using (SqlCommand cmd = new SqlCommand(
                        @"SELECT COUNT(1) FROM Booking
                          WHERE StudEmailAddress = @E
                            AND BookingStatus IN ('Unsettled','Pending','Confirmed')
                            AND ScheduledDateTime > @Now", conn))
                    {
                        cmd.Parameters.AddWithValue("@E", studEmail);
                        cmd.Parameters.AddWithValue("@Now", NowSa());
                        upcomingCount = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    if (upcomingCount >= maxUpcoming)
                        return "You already have " + upcomingCount + " upcoming lesson(s) booked. " +
                               "Please complete some of your current lessons before booking more.";

                    PackageProgress existing = FindWithRemaining(conn, null, studNumber, null);
                    if (existing != null)
                    {
                        if (existing.PackageID == packageId)
                        {
                            if (existing.RemainingLessons <= 0)
                                return "All lessons in your '" + existing.PackageName +
                                       "' package have been used. Please purchase a new package.";
                            return null;
                        }

                        string switchReason;
                        if (CanStudentSwitchPackage(studNumber, out switchReason)) return null;

                        return switchReason ?? ("You already have an active package: '" +
                               existing.PackageName + "' with " +
                               existing.RemainingLessons +
                               " lesson(s) remaining. Please finish it first.");
                    }

                    return null;
                }
            }
            catch { return null; }
        }

        public static bool CanStudentSwitchPackage(int studNumber, out string reason)
        {
            reason = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(Cs()))
                {
                    conn.Open();
                    EnsurePackageTable(conn);

                    PackageProgress active = FindWithRemaining(conn, null, studNumber, null);
                    if (active == null) return true;

                    if (active.UsedLessons > 0)
                    {
                        reason = "You have already completed " + active.UsedLessons +
                                 " lesson(s) on your '" + active.PackageName +
                                 "' package, so it cannot be changed.";
                        return false;
                    }

                    string email = GetStudentEmail(studNumber);
                    if (!string.IsNullOrEmpty(email))
                    {
                        int activeBookings;
                        using (SqlCommand cmd = new SqlCommand(
                            @"SELECT COUNT(1) FROM Booking
                              WHERE StudEmailAddress = @E AND PackageID = @P
                                AND BookingStatus IN ('Unsettled','Pending','Confirmed')", conn))
                        {
                            cmd.Parameters.AddWithValue("@E", email);
                            cmd.Parameters.AddWithValue("@P", active.PackageID);
                            activeBookings = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        if (activeBookings > 0)
                        {
                            reason = "You still have " + activeBookings +
                                     " active booking(s) on your '" + active.PackageName +
                                     "' package.";
                            return false;
                        }
                    }

                    try
                    {
                        int paidCount;
                        using (SqlCommand cmd = new SqlCommand(
                            @"SELECT COUNT(1) FROM Payment p
                              JOIN Booking b ON p.BookingID = b.BookingID
                              WHERE b.StudEmailAddress =
                                    (SELECT StudEmailAddress FROM Students WHERE StudNumber = @S)
                                AND b.PackageID = @P AND p.PaymentStatus = 'Paid'", conn))
                        {
                            cmd.Parameters.AddWithValue("@S", studNumber);
                            cmd.Parameters.AddWithValue("@P", active.PackageID);
                            paidCount = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        if (paidCount > 0)
                        {
                            reason = "You have already paid a deposit on your '" +
                                     active.PackageName + "' package, so it cannot be changed.";
                            return false;
                        }
                    }
                    catch { }

                    return true;
                }
            }
            catch { return true; }
        }

        public static bool DeactivateStudentPackage(int studNumber, int packageId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Cs()))
                {
                    conn.Open();
                    EnsurePackageTable(conn);

                    using (SqlCommand cmd = new SqlCommand(
                        @"UPDATE StudentPackage SET IsActive = 0
                          WHERE StudNumber = @S AND PackageID = @P AND UsedLessons = 0", conn))
                    {
                        cmd.Parameters.AddWithValue("@S", studNumber);
                        cmd.Parameters.AddWithValue("@P", packageId);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch { return false; }
        }

        // =========================================================
        // CREATE BOOKING
        // =========================================================
        public static int CreateBooking(
            string studEmail, string staffEmail, int packageId,
            DateTime scheduled, int durationMinutes,
            string vehicleId, out string error,
            bool paidInFull = false,
            bool sendEmail = true)
        {
            error = null;
            studEmail = (studEmail ?? "").Trim();
            staffEmail = (staffEmail ?? "").Trim();

            if (durationMinutes <= 0) durationMinutes = 60;
            if (durationMinutes > 480) durationMinutes = 480;

            if (scheduled < TodaySa())
            { error = "Please choose today or a future date."; return -1; }

            string slotErr = ValidateSlot(scheduled, durationMinutes);
            if (slotErr != null) { error = slotErr; return -1; }

            try
            {
                using (SqlConnection conn = new SqlConnection(Cs()))
                {
                    conn.Open();
                    EnsurePackageTable(conn);
                    EnsureAuditTable(conn);

                    using (SqlTransaction tx = conn.BeginTransaction())
                    {
                        try
                        {
                            int studNumber = GetStudentNumberTx(conn, tx, studEmail);
                            if (studNumber <= 0)
                            { tx.Rollback(); error = "Student record not found."; return -1; }

                            // Same-day rule: one active booking per student per day.
                            using (SqlCommand cmd = new SqlCommand(
                                @"SELECT TOP 1 BookingID FROM Booking
                                  WHERE StudEmailAddress = @E
                                    AND CAST(ScheduledDateTime AS DATE) = @D
                                    AND BookingStatus IN ('Unsettled','Pending','Confirmed')
                                  ORDER BY BookingID", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@E", studEmail);
                                cmd.Parameters.AddWithValue("@D", scheduled.Date);
                                object r = cmd.ExecuteScalar();
                                if (r != null && r != DBNull.Value)
                                {
                                    tx.Rollback();
                                    error = "You already have a booking (#" + r + ") on " +
                                            scheduled.ToString("dddd, d MMMM yyyy") +
                                            ". Only one lesson per day is allowed.";
                                    return -1;
                                }
                            }

                            decimal packagePrice = 0;
                            using (SqlCommand cmd = new SqlCommand(
                                "SELECT ISNULL(Amount, 0) FROM Packages WHERE PackageID = @P", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@P", packageId);
                                object r = cmd.ExecuteScalar();
                                if (r != null && r != DBNull.Value)
                                    packagePrice = Convert.ToDecimal(r);
                            }

                            string pkgType = GetPackageTypeTx(conn, tx, packageId);
                            bool isLearners = pkgType.Equals("Learners", StringComparison.OrdinalIgnoreCase);

                            PackageProgress use = FindWithRemaining(conn, tx, studNumber, packageId);
                            if (use == null)
                            {
                                tx.Rollback();
                                error = isLearners
                                    ? "Learners package not found."
                                    : "You don't have an active lesson package yet. " +
                                      "Please purchase a package at reception to start booking.";
                                return -1;
                            }

                            int upcomingCount;
                            using (SqlCommand cmd = new SqlCommand(
                                @"SELECT COUNT(1) FROM Booking
                                  WHERE StudEmailAddress = @E
                                    AND BookingStatus IN ('Unsettled','Pending','Confirmed')
                                    AND ScheduledDateTime > @Now", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@E", studEmail);
                                cmd.Parameters.AddWithValue("@Now", NowSa());
                                upcomingCount = Convert.ToInt32(cmd.ExecuteScalar());
                            }
                            if (upcomingCount >= DefaultMaxUpcomingBookings)
                            {
                                tx.Rollback();
                                error = "You already have " + upcomingCount +
                                    " upcoming lessons booked.";
                                return -1;
                            }

                            EnsurePackageRowTx(conn, tx, studNumber, packageId);

                            TimeSpan sStart = scheduled.TimeOfDay;
                            TimeSpan sEnd = sStart.Add(TimeSpan.FromMinutes(durationMinutes));

                            // Instructor conflict
                            using (SqlCommand cmd = new SqlCommand(
                                @"SELECT
                                    (SELECT COUNT(1) FROM Booking
                                     WHERE StafEmailAddress = @O
                                       AND BookingStatus <> 'Cancelled'
                                       AND CAST(ScheduledDateTime AS DATE) = @D
                                       AND CAST(ScheduledDateTime AS TIME) < @End
                                       AND DATEADD(MINUTE, ISNULL(DurationMinutes, 60),
                                           CAST(ScheduledDateTime AS TIME)) > @Start)
                                  +
                                    (SELECT COUNT(1) FROM Lesson
                                     WHERE InstruEmailAddress = @O
                                       AND Status <> 'Cancelled'
                                       AND CAST(LessonDateTime AS DATE) = @D
                                       AND CAST(LessonDateTime AS TIME) < @End
                                       AND DATEADD(MINUTE, ISNULL(Duration, 60),
                                           CAST(LessonDateTime AS TIME)) > @Start)", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@O", staffEmail);
                                cmd.Parameters.AddWithValue("@D", scheduled.Date);
                                cmd.Parameters.AddWithValue("@Start", sStart);
                                cmd.Parameters.AddWithValue("@End", sEnd);
                                if ((int)cmd.ExecuteScalar() > 0)
                                {
                                    tx.Rollback();
                                    error = "That instructor was just booked for this time.";
                                    return -1;
                                }
                            }

                            // Vehicle conflict
                            if (!string.IsNullOrEmpty(vehicleId) && vehicleId != "TBD")
                            {
                                using (SqlCommand cmd = new SqlCommand(
                                    @"SELECT
                                        (SELECT COUNT(1) FROM Booking
                                         WHERE VehicleID = @V
                                           AND BookingStatus <> 'Cancelled'
                                           AND CAST(ScheduledDateTime AS DATE) = @D
                                           AND CAST(ScheduledDateTime AS TIME) < @End
                                           AND DATEADD(MINUTE, ISNULL(DurationMinutes, 60),
                                               CAST(ScheduledDateTime AS TIME)) > @Start)
                                      +
                                        (SELECT COUNT(1) FROM Lesson
                                         WHERE VehicleID = @V
                                           AND Status <> 'Cancelled'
                                           AND CAST(LessonDateTime AS DATE) = @D
                                           AND CAST(LessonDateTime AS TIME) < @End
                                           AND DATEADD(MINUTE, ISNULL(Duration, 60),
                                               CAST(LessonDateTime AS TIME)) > @Start)", conn, tx))
                                {
                                    cmd.Parameters.AddWithValue("@V", vehicleId);
                                    cmd.Parameters.AddWithValue("@D", scheduled.Date);
                                    cmd.Parameters.AddWithValue("@Start", sStart);
                                    cmd.Parameters.AddWithValue("@End", sEnd);
                                    if ((int)cmd.ExecuteScalar() > 0)
                                    {
                                        tx.Rollback();
                                        error = "That vehicle was just booked for this time.";
                                        return -1;
                                    }
                                }
                            }

                            bool hasVehicle = BookingHasVehicle();
                            string initialStatus = paidInFull ? "Confirmed" : "Unsettled";
                            decimal totalPaid = paidInFull ? packagePrice : 0m;

                            string insertSql = hasVehicle
                                ? @"INSERT INTO Booking
                                    (BookingDate, StudEmailAddress, StafEmailAddress,
                                     BookingStatus, PackageID, ScheduledDateTime, VehicleID,
                                     PackagePrice, TotalPaid, DurationMinutes)
                                    OUTPUT INSERTED.BookingID
                                    VALUES (@BDate, @Stud, @Staff, @Status,
                                            @Pkg, @Sched, @Veh, @Price, @Paid, @Dur)"
                                : @"INSERT INTO Booking
                                    (BookingDate, StudEmailAddress, StafEmailAddress,
                                     BookingStatus, PackageID, ScheduledDateTime,
                                     PackagePrice, TotalPaid, DurationMinutes)
                                    OUTPUT INSERTED.BookingID
                                    VALUES (@BDate, @Stud, @Staff, @Status,
                                            @Pkg, @Sched, @Price, @Paid, @Dur)";

                            int bookingId;
                            using (SqlCommand cmd = new SqlCommand(insertSql, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@BDate", TodaySa());
                                cmd.Parameters.AddWithValue("@Stud", studEmail);
                                cmd.Parameters.AddWithValue("@Staff", staffEmail);
                                cmd.Parameters.AddWithValue("@Status", initialStatus);
                                cmd.Parameters.AddWithValue("@Pkg", packageId);
                                cmd.Parameters.AddWithValue("@Sched", scheduled);
                                cmd.Parameters.AddWithValue("@Price", packagePrice);
                                cmd.Parameters.AddWithValue("@Paid", totalPaid);
                                cmd.Parameters.AddWithValue("@Dur", durationMinutes);
                                if (hasVehicle)
                                    cmd.Parameters.AddWithValue("@Veh",
                                        (object)vehicleId ?? DBNull.Value);

                                bookingId = Convert.ToInt32(cmd.ExecuteScalar());
                            }

                            LogAuditTx(conn, tx, bookingId, "Created",
                                studEmail, "Student",
                                paidInFull
                                    ? "Booked via website; package already paid"
                                    : "Booked via website; awaiting 50% deposit");

                            tx.Commit();

                            if (sendEmail)
                            {
                                try
                                {
                                    string studentName = GetStudentDisplayName(studEmail);
                                    EmailHelper.SendBookingReceivedEmail(
                                        studEmail, studentName, bookingId,
                                        "", scheduled, sStart, sEnd, staffEmail, vehicleId);
                                }
                                catch { }
                            }

                            return bookingId;
                        }
                        catch { tx.Rollback(); throw; }
                    }
                }
            }
            catch (Exception ex)
            {
                error = "Could not save booking: " + ex.Message;
                return -1;
            }
        }

        // =========================================================
        // RECORD DEPOSIT PAYMENT
        // =========================================================
        public static bool RecordDepositPayment(
            int bookingId, decimal amountPaid, string paymentMethod,
            string paymentReference, out string message)
        {
            message = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(Cs()))
                {
                    conn.Open();
                    EnsureAuditTable(conn);

                    string studentEmail = null;
                    DateTime scheduled = DateTime.MinValue;
                    string currentStatus = null;
                    int packageId = 0;
                    decimal packagePrice = 0;
                    decimal currentPaid = 0;
                    DateTime? currentDue = null;

                    using (SqlTransaction tx = conn.BeginTransaction())
                    {
                        try
                        {
                            using (SqlCommand cmd = new SqlCommand(
                                @"SELECT BookingStatus, StudEmailAddress,
                                         ScheduledDateTime, PackageID,
                                         ISNULL(PackagePrice, 0) AS PackagePrice,
                                         ISNULL(TotalPaid, 0) AS TotalPaid,
                                         PaymentDueDate
                                  FROM Booking WITH (UPDLOCK, ROWLOCK)
                                  WHERE BookingID = @B", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@B", bookingId);
                                using (SqlDataReader r = cmd.ExecuteReader())
                                {
                                    if (!r.Read())
                                    { tx.Rollback(); message = "Booking not found."; return false; }

                                    currentStatus = r["BookingStatus"].ToString();
                                    studentEmail = r["StudEmailAddress"] == DBNull.Value
                                        ? null : r["StudEmailAddress"].ToString().Trim();
                                    scheduled = Convert.ToDateTime(r["ScheduledDateTime"]);
                                    packageId = (int)r["PackageID"];
                                    packagePrice = Convert.ToDecimal(r["PackagePrice"]);
                                    currentPaid = Convert.ToDecimal(r["TotalPaid"]);
                                    if (r["PaymentDueDate"] != DBNull.Value)
                                        currentDue = Convert.ToDateTime(r["PaymentDueDate"]);
                                }
                            }

                            if (currentStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                            { tx.Rollback(); message = "This booking has been cancelled."; return false; }

                            if (currentStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                            { tx.Rollback(); message = "This booking is already completed."; return false; }

                            if (packagePrice <= 0)
                            {
                                using (SqlCommand cmd = new SqlCommand(
                                    "SELECT ISNULL(Amount,0) FROM Packages WHERE PackageID = @P", conn, tx))
                                {
                                    cmd.Parameters.AddWithValue("@P", packageId);
                                    object r = cmd.ExecuteScalar();
                                    if (r != null && r != DBNull.Value)
                                        packagePrice = Convert.ToDecimal(r);
                                }
                            }

                            using (SqlCommand cmd = new SqlCommand(
                                @"INSERT INTO Payment
                                  (BookingID, AmountPaid, PaymentMethod,
                                   PaymentStatus, PaymentDate)
                                  VALUES (@B, @Amt, @Method, 'Paid', @Date)", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@B", bookingId);
                                cmd.Parameters.AddWithValue("@Amt", amountPaid);
                                cmd.Parameters.AddWithValue("@Method",
                                    (object)paymentMethod ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@Date", NowSa());
                                cmd.ExecuteNonQuery();
                            }

                            decimal newPaid = currentPaid + amountPaid;
                            DateTime? newDue = currentDue;
                            if (newDue == null && newPaid > 0)
                                newDue = NowSa().AddDays(InstalmentDays);

                            string newStatus;
                            if (newPaid >= packagePrice * 0.999m) newStatus = "Confirmed";
                            else if (newPaid >= packagePrice * 0.5m) newStatus = "Confirmed";
                            else newStatus = "Unsettled";

                            using (SqlCommand cmd = new SqlCommand(
                                @"UPDATE Booking
                                  SET TotalPaid = @TP, BookingStatus = @S,
                                      PaymentDueDate = @Due
                                  WHERE BookingID = @B", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@TP", newPaid);
                                cmd.Parameters.AddWithValue("@S", newStatus);
                                cmd.Parameters.AddWithValue("@Due", (object)newDue ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@B", bookingId);
                                cmd.ExecuteNonQuery();
                            }

                            LogAuditTx(conn, tx, bookingId, "DepositPaid",
                                studentEmail, "Student",
                                "R" + amountPaid.ToString("N2") +
                                " via " + (paymentMethod ?? "unknown") +
                                " | TotalPaid=R" + newPaid.ToString("N2") +
                                " | Status=" + newStatus);

                            tx.Commit();

                            decimal balance = packagePrice - newPaid;
                            if (balance <= 0.01m)
                                message = "Payment received! Your booking is fully paid and confirmed.";
                            else
                                message = "Payment of R" + amountPaid.ToString("N2") +
                                          " received. Remaining balance: R" + balance.ToString("N2") +
                                          " due by " + newDue.Value.ToString("d MMM yyyy") + ".";
                        }
                        catch { tx.Rollback(); throw; }
                    }

                    if (!string.IsNullOrEmpty(studentEmail))
                    {
                        try
                        {
                            string studentName = GetStudentDisplayName(studentEmail);
                            EmailHelper.SendPaymentReceivedEmail(
                                studentEmail, studentName, bookingId,
                                amountPaid, scheduled);
                        }
                        catch { }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                message = "Could not record payment: " + ex.Message;
                return false;
            }
        }

        // =========================================================
        // MARK COMPLETED
        // =========================================================
        public static bool MarkBookingCompleted(
            int bookingId, string actorEmail, out string message)
        {
            return MarkBookingCompleted(bookingId, actorEmail, false, out message);
        }

        public static bool MarkBookingCompleted(
            int bookingId, string actorEmail, bool force, out string message)
        {
            actorEmail = (actorEmail ?? "").Trim();
            message = null;

            bool enforceOwnership = actorEmail.Length > 0 &&
                !actorEmail.Equals("system", StringComparison.OrdinalIgnoreCase);

            string studentEmail = null;
            DateTime scheduled = DateTime.MinValue;

            try
            {
                using (SqlConnection conn = new SqlConnection(Cs()))
                {
                    conn.Open();
                    EnsureAuditTable(conn);

                    using (SqlTransaction tx = conn.BeginTransaction())
                    {
                        try
                        {
                            int packageId, studNumber;
                            string status, owner;

                            using (SqlCommand cmd = new SqlCommand(
                                @"SELECT b.PackageID, b.BookingStatus,
                                         b.StafEmailAddress, s.StudNumber,
                                         b.StudEmailAddress, b.ScheduledDateTime
                                  FROM Booking b WITH (UPDLOCK, ROWLOCK)
                                  JOIN Students s ON s.StudEmailAddress = b.StudEmailAddress
                                  WHERE b.BookingID = @B", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@B", bookingId);
                                using (SqlDataReader r = cmd.ExecuteReader())
                                {
                                    if (!r.Read())
                                    { tx.Rollback(); message = "Booking not found."; return false; }

                                    packageId = (int)r["PackageID"];
                                    status = r["BookingStatus"].ToString();
                                    owner = r["StafEmailAddress"] == DBNull.Value
                                        ? "" : r["StafEmailAddress"].ToString().Trim();
                                    studNumber = Convert.ToInt32(r["StudNumber"]);
                                    studentEmail = r["StudEmailAddress"] == DBNull.Value
                                        ? null : r["StudEmailAddress"].ToString().Trim();
                                    scheduled = Convert.ToDateTime(r["ScheduledDateTime"]);
                                }
                            }

                            if (enforceOwnership && !owner.Equals(actorEmail, StringComparison.OrdinalIgnoreCase))
                            { tx.Rollback(); message = "This booking is not assigned to you."; return false; }

                            if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                            { tx.Rollback(); message = "This lesson is already marked as Completed."; return false; }

                            if (status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                            { tx.Rollback(); message = "Cancelled lessons cannot be marked as Completed."; return false; }

                            if (status.Equals("Unsettled", StringComparison.OrdinalIgnoreCase))
                            { tx.Rollback(); message = "This booking is Unsettled — the student hasn't paid the deposit yet. Please ask reception to settle it first."; return false; }

                            if (status.Equals("Suspended", StringComparison.OrdinalIgnoreCase))
                            { tx.Rollback(); message = "This booking is Suspended due to an unpaid balance. Contact reception to resolve it."; return false; }

                            DateTime now = NowSa();
                            TimeSpan untilStart = scheduled - now;

                            if (untilStart > CompletionLeadWindow)
                            {
                                tx.Rollback();
                                message = "This lesson is scheduled for " +
                                    scheduled.ToString("dddd, d MMM yyyy 'at' HH:mm") +
                                    ", which is in the future. You can only mark a lesson Completed " +
                                    "once it has started (or is about to start).";
                                return false;
                            }

                            bool isLate = (now - scheduled).TotalHours > LateCompletionWindowHours;
                            bool isVeryLate = (now - scheduled).TotalDays > 7;

                            int remaining;
                            using (SqlCommand cmd = new SqlCommand(
                                @"SELECT ISNULL(TotalLessons,0) - ISNULL(UsedLessons,0)
                                  FROM StudentPackage
                                  WHERE StudNumber = @S AND PackageID = @P
                                    AND IsActive = 1 AND UsedLessons < TotalLessons", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@S", studNumber);
                                cmd.Parameters.AddWithValue("@P", packageId);
                                object rr = cmd.ExecuteScalar();
                                remaining = rr == null || rr == DBNull.Value
                                    ? 0 : Convert.ToInt32(rr);
                            }

                            if (remaining <= 0)
                            {
                                tx.Rollback();
                                message = "The student has used all lessons in this package. " +
                                          "Please ask reception to add more lessons before marking this one as Completed.";
                                return false;
                            }

                            using (SqlCommand cmd = new SqlCommand(
                                @"UPDATE Booking SET BookingStatus = 'Completed'
                                  WHERE BookingID = @B", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@B", bookingId);
                                cmd.ExecuteNonQuery();
                            }

                            using (SqlCommand cmd = new SqlCommand(
                                @"UPDATE StudentPackage SET UsedLessons = UsedLessons + 1
                                   WHERE StudentPackageID = (
                                       SELECT TOP 1 StudentPackageID FROM StudentPackage
                                       WHERE StudNumber = @S AND PackageID = @P
                                         AND IsActive = 1 AND UsedLessons < TotalLessons
                                       ORDER BY PurchasedOn)", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@S", studNumber);
                                cmd.Parameters.AddWithValue("@P", packageId);
                                cmd.ExecuteNonQuery();
                            }

                            string note;
                            if (force) note = "Lesson completed (forced by staff); 1 lesson deducted";
                            else if (isVeryLate) note = "Lesson completed LATE (more than 7 days after scheduled time); 1 lesson deducted";
                            else if (isLate) note = "Lesson completed late (" +
                                Math.Round((now - scheduled).TotalHours) +
                                " hours after start); 1 lesson deducted";
                            else note = "Lesson completed; 1 lesson deducted";

                            LogAuditTx(conn, tx, bookingId, "Completed",
                                enforceOwnership ? actorEmail : "system",
                                "Instructor", note);

                            tx.Commit();
                            message = "Lesson #" + bookingId + " marked Completed — 1 lesson deducted.";
                        }
                        catch { tx.Rollback(); throw; }
                    }

                    if (!string.IsNullOrEmpty(studentEmail))
                    {
                        try
                        {
                            string studentName = GetStudentDisplayName(studentEmail);
                            EmailHelper.SendLessonCompletedNotice(
                                studentEmail, studentName, bookingId, scheduled);
                        }
                        catch { }
                    }
                    return true;
                }
            }
            catch (Exception ex) { message = "Error: " + ex.Message; return false; }
        }

        public static bool MarkBookingCompleted(int bookingId, out string message)
        { return MarkBookingCompleted(bookingId, "system", out message); }

        // =========================================================
        // CANCEL BY INSTRUCTOR / RECEPTION
        // =========================================================
        public static bool CancelBooking(int bookingId, string actorEmail, out string message)
        {
            actorEmail = (actorEmail ?? "").Trim();
            message = null;
            bool enforceOwnership = actorEmail.Length > 0 &&
                !actorEmail.Equals("system", StringComparison.OrdinalIgnoreCase);

            string studentEmail = null;
            DateTime scheduled = DateTime.MinValue;

            try
            {
                using (SqlConnection conn = new SqlConnection(Cs()))
                {
                    conn.Open();
                    EnsureAuditTable(conn);

                    using (SqlTransaction tx = conn.BeginTransaction())
                    {
                        try
                        {
                            string status, owner;
                            using (SqlCommand cmd = new SqlCommand(
                                @"SELECT BookingStatus, StafEmailAddress,
                                         StudEmailAddress, ScheduledDateTime
                                  FROM Booking WITH (UPDLOCK, ROWLOCK)
                                  WHERE BookingID = @B", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@B", bookingId);
                                using (SqlDataReader r = cmd.ExecuteReader())
                                {
                                    if (!r.Read())
                                    { tx.Rollback(); message = "Booking not found."; return false; }

                                    status = r["BookingStatus"].ToString();
                                    owner = r["StafEmailAddress"] == DBNull.Value
                                        ? "" : r["StafEmailAddress"].ToString().Trim();
                                    studentEmail = r["StudEmailAddress"] == DBNull.Value
                                        ? null : r["StudEmailAddress"].ToString().Trim();
                                    scheduled = Convert.ToDateTime(r["ScheduledDateTime"]);
                                }
                            }

                            if (enforceOwnership && !owner.Equals(actorEmail, StringComparison.OrdinalIgnoreCase))
                            { tx.Rollback(); message = "This booking is not assigned to you."; return false; }

                            if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                            { tx.Rollback(); message = "Completed lessons cannot be cancelled."; return false; }

                            if (status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                            { tx.Rollback(); message = "Already cancelled."; return false; }

                            using (SqlCommand cmd = new SqlCommand(
                                @"UPDATE Booking SET BookingStatus = 'Cancelled' WHERE BookingID = @B",
                                conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@B", bookingId);
                                cmd.ExecuteNonQuery();
                            }

                            using (SqlCommand cmd = new SqlCommand(
                                @"UPDATE Lesson SET Status = 'Cancelled'
                                  WHERE BookingID = @B AND Status <> 'Completed'", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@B", bookingId);
                                cmd.ExecuteNonQuery();
                            }

                            LogAuditTx(conn, tx, bookingId, "Cancelled",
                                enforceOwnership ? actorEmail : "system",
                                "Instructor", "Cancelled by staff");

                            tx.Commit();
                            message = "Booking #" + bookingId + " cancelled.";
                        }
                        catch { tx.Rollback(); throw; }
                    }

                    if (!string.IsNullOrEmpty(studentEmail))
                    {
                        try
                        {
                            string studentName = GetStudentDisplayName(studentEmail);
                            EmailHelper.SendCancellationNotice(studentEmail, studentName,
                                bookingId, scheduled, "Reception/Instructor");
                        }
                        catch { }
                    }
                    return true;
                }
            }
            catch (Exception ex) { message = "Error: " + ex.Message; return false; }
        }

        // =========================================================
        // CANCEL BY STUDENT (24-hour rule)
        // =========================================================
        public static bool CancelBookingByStudent(
            int bookingId, string studentEmail, out string message)
        {
            return CancelBookingByStudent(bookingId, studentEmail, out message, out _);
        }

        public static bool CancelBookingByStudent(
            int bookingId, string studentEmail,
            out string message, out bool tooLate)
        {
            studentEmail = (studentEmail ?? "").Trim();
            message = null;
            tooLate = false;

            if (string.IsNullOrEmpty(studentEmail))
            { message = "You must be signed in to cancel."; return false; }

            DateTime scheduled = DateTime.MinValue;

            try
            {
                using (SqlConnection conn = new SqlConnection(Cs()))
                {
                    conn.Open();
                    EnsureAuditTable(conn);

                    using (SqlTransaction tx = conn.BeginTransaction())
                    {
                        try
                        {
                            string status, owner;

                            using (SqlCommand cmd = new SqlCommand(
                                @"SELECT BookingStatus, StudEmailAddress, ScheduledDateTime
                                  FROM Booking WITH (UPDLOCK, ROWLOCK)
                                  WHERE BookingID = @B", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@B", bookingId);
                                using (SqlDataReader r = cmd.ExecuteReader())
                                {
                                    if (!r.Read())
                                    { tx.Rollback(); message = "Booking not found."; return false; }

                                    status = r["BookingStatus"].ToString();
                                    owner = r["StudEmailAddress"] == DBNull.Value
                                        ? "" : r["StudEmailAddress"].ToString().Trim();
                                    scheduled = Convert.ToDateTime(r["ScheduledDateTime"]);
                                }
                            }

                            if (!owner.Equals(studentEmail, StringComparison.OrdinalIgnoreCase))
                            { tx.Rollback(); message = "You can only cancel your own bookings."; return false; }

                            if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                            { tx.Rollback(); message = "Completed lessons cannot be cancelled."; return false; }

                            if (status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) ||
                                status.Equals("Canceled", StringComparison.OrdinalIgnoreCase))
                            { tx.Rollback(); message = "This booking is already cancelled."; return false; }

                            if (!status.Equals("Unsettled", StringComparison.OrdinalIgnoreCase) &&
                                !status.Equals("Pending", StringComparison.OrdinalIgnoreCase) &&
                                !status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase) &&
                                !status.Equals("Overdue", StringComparison.OrdinalIgnoreCase))
                            { tx.Rollback(); message = "This booking cannot be cancelled."; return false; }

                            TimeSpan timeUntil = scheduled - NowSa();
                            if (timeUntil.TotalHours < LessonCancellationNoticeHours)
                            {
                                tx.Rollback();
                                tooLate = true;

                                string hrs = timeUntil.TotalHours <= 0
                                    ? "already started or passed"
                                    : Math.Floor(timeUntil.TotalHours) + " hour(s) away";

                                message = "Your lesson is only " + hrs + ". " +
                                    "Cancellations need at least " +
                                    LessonCancellationNoticeHours + " hours' notice. " +
                                    "If you have an emergency, you can request an exception — " +
                                    "reception will review it and contact you.";
                                return false;
                            }

                            using (SqlCommand cmd = new SqlCommand(
                                @"UPDATE Booking SET BookingStatus = 'Cancelled' WHERE BookingID = @B",
                                conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@B", bookingId);
                                cmd.ExecuteNonQuery();
                            }

                            using (SqlCommand cmd = new SqlCommand(
                                @"UPDATE Lesson SET Status = 'Cancelled'
                                  WHERE BookingID = @B AND Status <> 'Completed'", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@B", bookingId);
                                cmd.ExecuteNonQuery();
                            }

                            LogAuditTx(conn, tx, bookingId, "Cancelled",
                                studentEmail, "Student",
                                "Student self-cancelled (>24h notice)");

                            tx.Commit();
                            message = "Booking #" + bookingId + " has been cancelled. " +
                                      "The lesson has been returned to your package balance.";
                        }
                        catch { tx.Rollback(); throw; }
                    }

                    try
                    {
                        string studentName = GetStudentDisplayName(studentEmail);
                        EmailHelper.SendCancellationNotice(
                            studentEmail, studentName, bookingId, scheduled, "Student");
                    }
                    catch { }

                    return true;
                }
            }
            catch (Exception ex)
            {
                message = "Error: " + ex.Message;
                System.Diagnostics.Trace.TraceError("CancelBookingByStudent: " + ex);
                return false;
            }
        }

        // =========================================================
        // CANCEL LEARNERS CLASS + REFUND
        // =========================================================
        public static bool CancelLearnersBookingByStudent(
            int bookingId, string studentEmail, out string message)
        {
            message = null;
            studentEmail = (studentEmail ?? "").Trim();

            if (string.IsNullOrEmpty(studentEmail))
            { message = "You must be signed in to cancel."; return false; }

            int studNumber = 0;
            DateTime scheduled = DateTime.MinValue;
            decimal amountPaid = 0;

            try
            {
                using (SqlConnection conn = new SqlConnection(Cs()))
                {
                    conn.Open();
                    EnsureAuditTable(conn);

                    using (SqlCommand cmd = new SqlCommand(
                        @"SELECT b.BookingID, b.StudEmailAddress, b.BookingStatus,
                                 b.ScheduledDateTime,
                                 (SELECT TOP 1 StudNumber FROM Students
                                  WHERE StudEmailAddress = b.StudEmailAddress) AS StudNumber,
                                 ISNULL(b.TotalPaid, 0) AS AmountPaid,
                                 (SELECT TOP 1 p.PackageType FROM Packages p
                                  WHERE p.PackageID = b.PackageID) AS PackageType
                          FROM Booking b WITH (UPDLOCK, ROWLOCK)
                          WHERE b.BookingID = @B", conn))
                    {
                        cmd.Parameters.AddWithValue("@B", bookingId);
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (!r.Read())
                            { message = "Booking not found."; return false; }

                            string ownerEmail = r["StudEmailAddress"] == DBNull.Value
                                ? "" : r["StudEmailAddress"].ToString().Trim();
                            if (!ownerEmail.Equals(studentEmail, StringComparison.OrdinalIgnoreCase))
                            { message = "You can only cancel your own bookings."; return false; }

                            string status = r["BookingStatus"].ToString();
                            if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                            { message = "Completed classes cannot be cancelled or refunded."; return false; }

                            if (status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                            { message = "This booking is already cancelled."; return false; }

                            string pkgType = r["PackageType"] == DBNull.Value
                                ? "" : r["PackageType"].ToString();
                            if (!pkgType.Equals("Learners", StringComparison.OrdinalIgnoreCase))
                            { message = "This is not a learners-class booking."; return false; }

                            scheduled = Convert.ToDateTime(r["ScheduledDateTime"]);
                            studNumber = r["StudNumber"] == DBNull.Value
                                ? 0 : Convert.ToInt32(r["StudNumber"]);
                            amountPaid = r["AmountPaid"] == DBNull.Value
                                ? 0 : Convert.ToDecimal(r["AmountPaid"]);
                        }
                    }

                    int workingDaysNotice = WorkingDaysBetween(TodaySa(), scheduled);
                    if (workingDaysNotice < LearnersCancelNoticeWorkingDays)
                    {
                        message = "Cancellations require at least " + LearnersCancelNoticeWorkingDays +
                                  " working days' notice. Your class is on " +
                                  scheduled.ToString("dddd, d MMMM yyyy") +
                                  ", which is only " + workingDaysNotice + " working day(s) away.";
                        return false;
                    }

                    using (SqlCommand cmd = new SqlCommand(
                        @"UPDATE Booking SET BookingStatus = 'Cancelled'
                          WHERE BookingID = @B
                            AND BookingStatus NOT IN ('Cancelled','Completed')", conn))
                    {
                        cmd.Parameters.AddWithValue("@B", bookingId);
                        if (cmd.ExecuteNonQuery() != 1)
                        { message = "Could not cancel — booking state changed."; return false; }
                    }

                    LogAuditTx(conn, null, bookingId, "Cancelled",
                        studentEmail, "Student", "Learners class cancelled by student");

                    string refundMsg;
                    bool refundOk = TryPayFastRefund(bookingId, amountPaid, out refundMsg);

                    using (SqlCommand cmd = new SqlCommand(
                        @"UPDATE Booking
                          SET RefundProcessed = @Processed,
                              RefundAmount = @Amount,
                              RefundRequestedOn = @Now
                          WHERE BookingID = @B", conn))
                    {
                        cmd.Parameters.AddWithValue("@Processed", refundOk ? 1 : 0);
                        cmd.Parameters.AddWithValue("@Amount", amountPaid);
                        cmd.Parameters.AddWithValue("@Now", NowSa());
                        cmd.Parameters.AddWithValue("@B", bookingId);
                        try { cmd.ExecuteNonQuery(); } catch { }
                    }

                    if (amountPaid > 0)
                    {
                        message = refundOk
                            ? "Your learners class on " + scheduled.ToString("dddd, d MMMM yyyy") +
                              " has been cancelled. A full refund of R " + amountPaid.ToString("N2") +
                              " has been submitted to PayFast. It will reflect in your account within 3 working days."
                            : "Your learners class on " + scheduled.ToString("dddd, d MMMM yyyy") +
                              " has been cancelled. A refund of R " + amountPaid.ToString("N2") +
                              " will be processed manually by reception within 3 working days.";
                    }
                    else
                    {
                        message = "Your learners class on " + scheduled.ToString("dddd, d MMMM yyyy") +
                                  " has been cancelled. No payment was received, so no refund is due.";
                    }

                    try
                    {
                        string email = GetStudentEmail(studNumber);
                        if (!string.IsNullOrEmpty(email))
                        {
                            string name = GetStudentDisplayName(email);
                            EmailHelper.SendLearnersCancellationEmail(
                                email, name, bookingId, scheduled, amountPaid);
                        }
                    }
                    catch { }

                    return true;
                }
            }
            catch (Exception ex)
            {
                message = "Error: " + ex.Message;
                System.Diagnostics.Trace.TraceError("CancelLearnersBookingByStudent: " + ex);
                return false;
            }
        }

        // =========================================================
        // SUBMIT CANCELLATION EXCEPTION REQUEST
        // =========================================================
        public static bool SubmitCancellationRequest(
            int bookingId, int studNumber, string studentEmail,
            string reason, out string message)
        {
            message = null;
            studentEmail = (studentEmail ?? "").Trim();

            if (string.IsNullOrEmpty(studentEmail))
            { message = "You must be signed in to submit a request."; return false; }

            if (string.IsNullOrWhiteSpace(reason))
            { message = "Please tell us why you need to cancel at short notice."; return false; }

            reason = reason.Trim();
            if (reason.Length > 500) reason = reason.Substring(0, 500);

            try
            {
                DateTime scheduled;
                string studentName;

                using (SqlConnection conn = new SqlConnection(Cs()))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(
                        @"SELECT b.BookingID, b.ScheduledDateTime,
                                 s.Name + ' ' + s.Surname AS StudentName
                          FROM Booking b
                          INNER JOIN Students s ON s.StudEmailAddress = b.StudEmailAddress
                          WHERE b.BookingID = @B
                            AND b.StudEmailAddress = @E
                            AND b.BookingStatus NOT IN ('Cancelled','Canceled','Completed')", conn))
                    {
                        cmd.Parameters.AddWithValue("@B", bookingId);
                        cmd.Parameters.AddWithValue("@E", studentEmail);
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (!r.Read())
                            { message = "This booking can no longer be cancelled."; return false; }

                            scheduled = Convert.ToDateTime(r["ScheduledDateTime"]);
                            studentName = r["StudentName"].ToString();
                        }
                    }

                    using (SqlCommand cmd = new SqlCommand(
                        @"SELECT COUNT(1) FROM CancellationRequest
                          WHERE BookingID = @B AND Status = 'Pending'", conn))
                    {
                        cmd.Parameters.AddWithValue("@B", bookingId);
                        if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                        {
                            message = "You already have a pending cancellation request for this booking. " +
                                      "Reception will contact you shortly.";
                            return false;
                        }
                    }

                    using (SqlCommand cmd = new SqlCommand(
                        @"INSERT INTO CancellationRequest
                              (BookingID, StudNumber, StudentEmail, ScheduledFor, Reason)
                          VALUES (@B, @S, @E, @Sched, @R)", conn))
                    {
                        cmd.Parameters.AddWithValue("@B", bookingId);
                        cmd.Parameters.AddWithValue("@S", studNumber);
                        cmd.Parameters.AddWithValue("@E", studentEmail);
                        cmd.Parameters.AddWithValue("@Sched", scheduled);
                        cmd.Parameters.AddWithValue("@R", reason);
                        cmd.ExecuteNonQuery();
                    }

                    EnsureAuditTable(conn);
                    using (SqlCommand cmd = new SqlCommand(
                        @"INSERT INTO BookingAudit
                              (BookingID, Action, ActorEmail, ActorRole, ActionTime, Notes)
                          VALUES (@B, 'CancelRequested', @E, 'Student', @Now, @N)", conn))
                    {
                        cmd.Parameters.AddWithValue("@B", bookingId);
                        cmd.Parameters.AddWithValue("@E", studentEmail);
                        cmd.Parameters.AddWithValue("@Now", NowSa());
                        cmd.Parameters.AddWithValue("@N", reason);
                        cmd.ExecuteNonQuery();
                    }

                    try
                    {
                        EmailHelper.SendCancellationRequestToReception(
                            studentName, studentEmail, bookingId, scheduled, reason);
                    }
                    catch (Exception mailEx)
                    {
                        System.Diagnostics.Trace.TraceError(
                            "Cancellation request email failed: " + mailEx);
                    }

                    message =
                        "Your request has been sent to reception. " +
                        "They will review it and get back to you by email or phone within 24 hours.";
                    return true;
                }
            }
            catch (Exception ex)
            {
                message = "Error: " + ex.Message;
                System.Diagnostics.Trace.TraceError("SubmitCancellationRequest: " + ex);
                return false;
            }
        }

        // =========================================================
        // RESCHEDULE
        // =========================================================
        public static bool RescheduleBooking(
            int bookingId, DateTime newWhen, string actorEmail, out string message)
        {
            actorEmail = (actorEmail ?? "").Trim();
            message = null;

            string slotErr = ValidateSlot(newWhen);
            if (slotErr != null) { message = slotErr; return false; }
            if (newWhen < NowSa()) { message = "Please choose a future time."; return false; }

            string studentEmail = null;
            DateTime oldWhen = DateTime.MinValue;

            try
            {
                using (SqlConnection conn = new SqlConnection(Cs()))
                {
                    conn.Open();
                    EnsureAuditTable(conn);

                    using (SqlTransaction tx = conn.BeginTransaction())
                    {
                        try
                        {
                            string status, owner;
                            using (SqlCommand cmd = new SqlCommand(
                                @"SELECT BookingStatus, StafEmailAddress,
                                         StudEmailAddress, ScheduledDateTime
                                  FROM Booking WITH (UPDLOCK, ROWLOCK)
                                  WHERE BookingID = @B", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@B", bookingId);
                                using (SqlDataReader r = cmd.ExecuteReader())
                                {
                                    if (!r.Read())
                                    { tx.Rollback(); message = "Booking not found."; return false; }

                                    status = r["BookingStatus"].ToString();
                                    owner = r["StafEmailAddress"] == DBNull.Value
                                        ? "" : r["StafEmailAddress"].ToString().Trim();
                                    studentEmail = r["StudEmailAddress"] == DBNull.Value
                                        ? null : r["StudEmailAddress"].ToString().Trim();
                                    oldWhen = Convert.ToDateTime(r["ScheduledDateTime"]);
                                }
                            }

                            bool enforceOwnership = actorEmail.Length > 0 &&
                                !actorEmail.Equals("system", StringComparison.OrdinalIgnoreCase);

                            if (enforceOwnership && !owner.Equals(actorEmail, StringComparison.OrdinalIgnoreCase))
                            { tx.Rollback(); message = "Not assigned to you."; return false; }

                            if (!status.Equals("Unsettled", StringComparison.OrdinalIgnoreCase) &&
                                !status.Equals("Pending", StringComparison.OrdinalIgnoreCase) &&
                                !status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase) &&
                                !status.Equals("Overdue", StringComparison.OrdinalIgnoreCase))
                            { tx.Rollback(); message = "Only active lessons can be rescheduled."; return false; }

                            TimeSpan start = newWhen.TimeOfDay;
                            TimeSpan end = start.Add(TimeSpan.FromHours(1));

                            using (SqlCommand cmd = new SqlCommand(
                                @"SELECT
                                    (SELECT COUNT(1) FROM Booking
                                     WHERE StafEmailAddress = @O
                                       AND BookingID <> @B
                                       AND BookingStatus IN ('Unsettled','Pending','Confirmed')
                                       AND CAST(ScheduledDateTime AS DATE) = @D
                                       AND CAST(ScheduledDateTime AS TIME) < @End
                                       AND DATEADD(MINUTE, ISNULL(DurationMinutes, 60),
                                           CAST(ScheduledDateTime AS TIME)) > @Start)
                                  +
                                    (SELECT COUNT(1) FROM Lesson
                                     WHERE InstruEmailAddress = @O
                                       AND Status <> 'Cancelled'
                                       AND CAST(LessonDateTime AS DATE) = @D
                                       AND CAST(LessonDateTime AS TIME) < @End
                                       AND DATEADD(MINUTE, ISNULL(Duration, 60),
                                           CAST(LessonDateTime AS TIME)) > @Start)", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@O", owner);
                                cmd.Parameters.AddWithValue("@B", bookingId);
                                cmd.Parameters.AddWithValue("@D", newWhen.Date);
                                cmd.Parameters.AddWithValue("@Start", start);
                                cmd.Parameters.AddWithValue("@End", end);
                                if ((int)cmd.ExecuteScalar() > 0)
                                { tx.Rollback(); message = "You already have another session at that time."; return false; }
                            }

                            using (SqlCommand cmd = new SqlCommand(
                                @"UPDATE Booking SET ScheduledDateTime = @New WHERE BookingID = @B",
                                conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@New", newWhen);
                                cmd.Parameters.AddWithValue("@B", bookingId);
                                cmd.ExecuteNonQuery();
                            }

                            using (SqlCommand cmd = new SqlCommand(
                                @"UPDATE Lesson SET LessonDateTime = @New, Status = 'Rescheduled'
                                  WHERE BookingID = @B
                                    AND Status NOT IN ('Completed','Cancelled')", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@New", newWhen);
                                cmd.Parameters.AddWithValue("@B", bookingId);
                                cmd.ExecuteNonQuery();
                            }

                            LogAuditTx(conn, tx, bookingId, "Rescheduled",
                                enforceOwnership ? actorEmail : "system",
                                "Instructor",
                                "Moved from " + oldWhen.ToString("yyyy-MM-dd HH:mm") +
                                " to " + newWhen.ToString("yyyy-MM-dd HH:mm"));

                            tx.Commit();
                            message = "Booking #" + bookingId + " rescheduled.";
                        }
                        catch { tx.Rollback(); throw; }
                    }

                    if (!string.IsNullOrEmpty(studentEmail))
                    {
                        try
                        {
                            string studentName = GetStudentDisplayName(studentEmail);
                            EmailHelper.SendRescheduleNotice(
                                studentEmail, studentName, bookingId, oldWhen, newWhen);
                        }
                        catch { }
                    }
                    return true;
                }
            }
            catch (Exception ex) { message = "Error: " + ex.Message; return false; }
        }

        // ================= PAYFAST REFUND (best-effort) =================
        private static bool TryPayFastRefund(int bookingId, decimal amount, out string errorMessage)
        {
            errorMessage = null;

            if (amount <= 0)
            { errorMessage = "No amount to refund."; return false; }

            try
            {
                string merchantId = ConfigurationManager.AppSettings["PayFastMerchantId"];
                string passphrase = ConfigurationManager.AppSettings["PayFastPassphrase"] ?? "";

                if (string.IsNullOrEmpty(merchantId))
                { errorMessage = "PayFast merchant ID not configured."; return false; }

                errorMessage = "PayFast refund API requires original payment id; please process manually.";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = "Refund error: " + ex.Message;
                return false;
            }
        }

        // ================= AUDIT =================
        private static void LogAuditTx(SqlConnection conn, SqlTransaction tx,
            int bookingId, string action, string actorEmail, string actorRole, string notes)
        {
            try
            {
                SqlCommand cmd = tx != null
                    ? new SqlCommand(
                        @"INSERT INTO BookingAudit
                          (BookingID, Action, ActorEmail, ActorRole, ActionTime, Notes)
                          VALUES (@B, @A, @E, @R, @T, @N)", conn, tx)
                    : new SqlCommand(
                        @"INSERT INTO BookingAudit
                          (BookingID, Action, ActorEmail, ActorRole, ActionTime, Notes)
                          VALUES (@B, @A, @E, @R, @T, @N)", conn);

                using (cmd)
                {
                    cmd.Parameters.AddWithValue("@B", bookingId);
                    cmd.Parameters.AddWithValue("@A",
                        (action ?? "").Substring(0, Math.Min(30, (action ?? "").Length)));
                    cmd.Parameters.AddWithValue("@E", (object)actorEmail ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@R", (object)actorRole ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@T", NowSa());
                    cmd.Parameters.AddWithValue("@N", (object)notes ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
            catch { }
        }

        // ================= WORKING DAYS =================
        private static int WorkingDaysBetween(DateTime from, DateTime to)
        {
            if (to <= from) return 0;

            int count = 0;
            for (DateTime d = from.Date.AddDays(1); d <= to.Date; d = d.AddDays(1))
            {
                if (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
                    continue;
                count++;
            }
            return count;
        }

        // ================= STALE PAYMENT CLEANUP =================
        public static int CleanupStalePayments(int olderThanHours = 24)
        {
            try
            {
                if (olderThanHours < 1) olderThanHours = 1;

                using (SqlConnection conn = new SqlConnection(Cs()))
                using (SqlCommand cmd = new SqlCommand(
                    "EXEC dbo.CleanupStalePayments @OlderThanHours = @H, @DryRun = 0;", conn))
                {
                    cmd.Parameters.AddWithValue("@H", olderThanHours);
                    cmd.CommandTimeout = 30;
                    conn.Open();

                    int affected = cmd.ExecuteNonQuery();
                    return affected >= 0 ? affected : 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("CleanupStalePayments: " + ex);
                return -1;
            }
        }

        // ================= DASHBOARD CARD =================
        public static string BuildProgressCardHtml(int studNumber)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Cs()))
                {
                    conn.Open();
                    EnsurePackageTable(conn);

                    PackageProgress pp = FindWithRemaining(conn, null, studNumber, null);
                    PaymentSummary pay = pp == null
                        ? new PaymentSummary()
                        : GetPaymentSummary(studNumber, pp.PackageID);

                    StringBuilder sb = new StringBuilder();
                    sb.Append("<div style='background:#fff;border-radius:12px;");
                    sb.Append("padding:20px 22px;box-shadow:0 4px 15px rgba(13,43,110,.08);");
                    sb.Append("border-top:3px solid #0d2b6e;max-width:520px;'>");

                    sb.Append("<div style='font-size:11px;font-weight:800;");
                    sb.Append("letter-spacing:2px;color:#e8283f;text-transform:uppercase;");
                    sb.Append("margin-bottom:6px;'>My Lesson Package</div>");

                    if (pp == null)
                    {
                        sb.Append("<div style='color:#0d2b6e;font-weight:700;");
                        sb.Append("font-size:1.05em;'>No active lesson balance</div>");
                        sb.Append("<div style='color:#7787a5;font-size:13px;margin-top:6px;'>");
                        sb.Append("Book a package to start your lessons.</div>");
                    }
                    else
                    {
                        int usedPct = pp.TotalLessons == 0 ? 0 :
                            (int)Math.Round(100.0 * pp.UsedLessons / pp.TotalLessons);

                        sb.Append("<div style='color:#0d2b6e;font-weight:800;");
                        sb.Append("font-size:1.1em;margin-bottom:10px;'>");
                        sb.Append(HttpUtility.HtmlEncode(pp.PackageName));
                        sb.Append("</div>");

                        sb.Append("<table style='width:100%;font-size:13px;margin-bottom:12px;'>");
                        sb.Append("<tr>");
                        sb.Append("<td style='color:#7787a5;'>Purchased</td>");
                        sb.Append("<td style='color:#7787a5;'>Used</td>");
                        sb.Append("<td style='color:#0d2b6e;font-weight:800;text-align:right;'>Remaining</td>");
                        sb.Append("</tr><tr>");
                        sb.Append("<td style='color:#333;font-weight:700;'>" + pp.TotalLessons + "</td>");
                        sb.Append("<td style='color:#333;font-weight:700;'>" + pp.UsedLessons + "</td>");
                        sb.Append("<td style='color:#22b573;font-weight:800;font-size:1.3em;text-align:right;'>" +
                                  pp.RemainingLessons + "</td>");
                        sb.Append("</tr></table>");

                        sb.Append("<div style='background:#e8effb;border-radius:6px;height:10px;overflow:hidden;margin-bottom:14px;'>");
                        sb.Append("<div style='height:100%;width:" + usedPct + "%;");
                        sb.Append("background:" + (pp.RemainingLessons == 0 ? "#e8283f" : "#22b573") + ";");
                        sb.Append("border-radius:6px;'></div></div>");

                        if (pay.PackagePrice > 0)
                        {
                            sb.Append("<div style='border-top:1px dashed #dbe4f6;padding-top:12px;margin-top:6px;'>");
                            sb.Append("<div style='font-size:11px;font-weight:800;letter-spacing:2px;");
                            sb.Append("color:#0d2b6e;text-transform:uppercase;margin-bottom:8px;'>");
                            sb.Append("Payment Progress</div>");

                            sb.Append("<table style='width:100%;font-size:13px;margin-bottom:10px;'>");
                            sb.Append("<tr>");
                            sb.Append("<td style='color:#7787a5;'>Package</td>");
                            sb.Append("<td style='color:#7787a5;'>Paid</td>");
                            sb.Append("<td style='color:#0d2b6e;font-weight:800;text-align:right;'>Balance</td>");
                            sb.Append("</tr><tr>");
                            sb.Append("<td style='color:#333;font-weight:700;'>R " + pay.PackagePrice.ToString("N2") + "</td>");
                            sb.Append("<td style='color:#333;font-weight:700;'>R " + pay.TotalPaid.ToString("N2") + "</td>");
                            sb.Append("<td style='font-weight:800;text-align:right;font-size:1.2em;color:" +
                                      (pay.IsFullyPaid ? "#22b573" : "#e8890c") + ";'>");
                            sb.Append("R " + pay.BalanceDue.ToString("N2") + "</td>");
                            sb.Append("</tr></table>");

                            sb.Append("<div style='background:#e8effb;border-radius:6px;height:10px;overflow:hidden;margin-bottom:10px;'>");
                            sb.Append("<div style='height:100%;width:" + pay.PercentPaid + "%;");
                            sb.Append("background:" + (pay.IsFullyPaid ? "#22b573" : "#e8890c") + ";");
                            sb.Append("border-radius:6px;'></div></div>");

                            if (pay.IsFullyPaid)
                            {
                                sb.Append("<div style='color:#22b573;font-size:12.5px;font-weight:700;'>");
                                sb.Append("&#10004; Fully paid</div>");
                            }
                            else if (pay.IsOverdue)
                            {
                                sb.Append("<div style='color:#e8283f;font-size:12.5px;font-weight:700;'>");
                                sb.Append("&#9888; Overdue since " + pay.DueDate.Value.ToString("d MMM yyyy"));
                                sb.Append("</div>");
                            }
                            else if (pay.DueDate.HasValue)
                            {
                                int days = pay.DaysRemaining;
                                sb.Append("<div style='color:" + (days < 14 ? "#e8890c" : "#7787a5") +
                                          ";font-size:12.5px;font-weight:700;'>");
                                sb.Append("Balance due in " + days + " day(s) — by " +
                                          pay.DueDate.Value.ToString("d MMM yyyy"));
                                sb.Append("</div>");
                            }
                            sb.Append("</div>");
                        }
                    }

                    sb.Append("</div>");
                    return sb.ToString();
                }
            }
            catch { return ""; }
        }
    }
}