using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace WebApplicationM3
{
    public static class PackageReservationService
    {
        public const int ReservationDays = 3;

        private static string Cs()
        {
            return ConfigurationManager
                .ConnectionStrings["KhweziConnection"]
                .ConnectionString;
        }

        private static DateTime NowSa() { return LessonProgress.NowSa(); }

        public class Reservation
        {
            public int ReservationID { get; set; }
            public int StudNumber { get; set; }
            public int PackageID { get; set; }
            public string PackageName { get; set; }
            public decimal PackagePrice { get; set; }
            public decimal DepositAmount { get; set; }
            public DateTime ReservedOn { get; set; }
            public DateTime ExpiresOn { get; set; }
            public string Status { get; set; }
            public int? PaymentID { get; set; }
            public DateTime? PaidOn { get; set; }

            public int DaysRemaining
            {
                get
                {
                    int days = (int)Math.Ceiling(
                        (ExpiresOn - NowSa()).TotalDays);
                    return Math.Max(0, days);
                }
            }

            public bool IsActive
            {
                get { return Status == "Reserved" && ExpiresOn > NowSa(); }
            }

            public bool IsPaid
            {
                get { return Status == "Paid"; }
            }
        }

        public static Reservation GetCurrent(int student)
        {
            try
            {
                ExpireOld(student);

                using (var c = new SqlConnection(Cs()))
                using (var cmd = new SqlCommand(
                    @"SELECT TOP 1
                          r.ReservationID, r.StudNumber,
                          r.PackageID, p.PackageName,
                          r.PackagePrice, r.DepositAmount,
                          r.ReservedOn, r.ExpiresOn,
                          r.Status, r.PaymentID, r.PaidOn
                      FROM dbo.PackageReservation r
                      INNER JOIN dbo.Packages p
                          ON p.PackageID=r.PackageID
                      WHERE r.StudNumber=@S
                        AND r.Status IN ('Reserved','Paid')
                      ORDER BY
                          CASE WHEN r.Status='Reserved'
                               THEN 0 ELSE 1 END,
                          r.ReservedOn DESC", c))
                {
                    cmd.Parameters.AddWithValue("@S", student);
                    c.Open();

                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read()) return null;

                        return new Reservation
                        {
                            ReservationID = Convert.ToInt32(rd["ReservationID"]),
                            StudNumber = Convert.ToInt32(rd["StudNumber"]),
                            PackageID = Convert.ToInt32(rd["PackageID"]),
                            PackageName = Convert.ToString(rd["PackageName"]),
                            PackagePrice = Convert.ToDecimal(rd["PackagePrice"]),
                            DepositAmount = Convert.ToDecimal(rd["DepositAmount"]),
                            ReservedOn = Convert.ToDateTime(rd["ReservedOn"]),
                            ExpiresOn = Convert.ToDateTime(rd["ExpiresOn"]),
                            Status = Convert.ToString(rd["Status"]),
                            PaymentID = rd["PaymentID"] == DBNull.Value
                                ? (int?)null
                                : Convert.ToInt32(rd["PaymentID"]),
                            PaidOn = rd["PaidOn"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(rd["PaidOn"])
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("GetCurrent: " + ex);
                return null;
            }
        }

        public static int Create(
            int student,
            int packageId,
            decimal packagePrice,
            decimal deposit,
            out string message)
        {
            message = null;

            if (student <= 0 || packageId <= 0 || packagePrice <= 0m ||
                deposit <= 0m || deposit > packagePrice)
            {
                message = "Invalid reservation details.";
                return -1;
            }

            try
            {
                using (var c = new SqlConnection(Cs()))
                {
                    c.Open();

                    using (var tx = c.BeginTransaction(IsolationLevel.Serializable))
                    {
                        try
                        {
                            // Per-student lock — serialises concurrent reservation
                            // creation without blocking other students.
                            using (var lockCmd = new SqlCommand(
                                @"DECLARE @LockResult INT;
                                  EXEC @LockResult = sp_getapplock
                                      @Resource = @Resource,
                                      @LockMode = 'Exclusive',
                                      @LockOwner = 'Transaction',
                                      @LockTimeout = 10000;
                                  SELECT @LockResult;", c, tx))
                            {
                                lockCmd.Parameters.AddWithValue(
                                    "@Resource",
                                    "IKHWEZI-RESERVATION-" + student);

                                int r = Convert.ToInt32(lockCmd.ExecuteScalar());
                                if (r < 0)
                                    throw new InvalidOperationException(
                                        "Could not acquire reservation lock.");
                            }

                            using (var cmd = new SqlCommand(
                                @"UPDATE dbo.PackageReservation
                                  SET Status='Cancelled',
                                      CancelledOn=@Now,
                                      Notes='Replaced by new reservation'
                                  WHERE StudNumber=@S
                                    AND Status='Reserved'", c, tx))
                            {
                                cmd.Parameters.AddWithValue("@S", student);
                                cmd.Parameters.AddWithValue("@Now", NowSa());
                                cmd.ExecuteNonQuery();
                            }

                            DateTime now = NowSa();
                            DateTime expiry = now.AddDays(ReservationDays);

                            int id;

                            using (var cmd = new SqlCommand(
                                @"INSERT INTO dbo.PackageReservation
                                    (StudNumber, PackageID, PackagePrice,
                                     DepositAmount, ReservedOn, ExpiresOn, Status)
                                  OUTPUT INSERTED.ReservationID
                                  VALUES
                                    (@S, @P, @Price, @Deposit, @Now, @Expiry, 'Reserved')",
                                c, tx))
                            {
                                cmd.Parameters.AddWithValue("@S", student);
                                cmd.Parameters.AddWithValue("@P", packageId);
                                cmd.Parameters.AddWithValue("@Price", packagePrice);
                                cmd.Parameters.AddWithValue("@Deposit", deposit);
                                cmd.Parameters.AddWithValue("@Now", now);
                                cmd.Parameters.AddWithValue("@Expiry", expiry);

                                id = Convert.ToInt32(cmd.ExecuteScalar());
                            }

                            tx.Commit();

                            message = "Reservation created successfully.";
                            return id;
                        }
                        catch
                        {
                            tx.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Create reservation: " + ex);
                message = "Could not create reservation.";
                return -1;
            }
        }

        public static void ExpireOld(int student)
        {
            try
            {
                using (var c = new SqlConnection(Cs()))
                using (var cmd = new SqlCommand(
                    @"UPDATE dbo.PackageReservation
                      SET Status='Expired',
                          Notes='Expired without payment'
                      WHERE StudNumber=@S
                        AND Status='Reserved'
                        AND ExpiresOn<=@Now", c))
                {
                    cmd.Parameters.AddWithValue("@S", student);
                    cmd.Parameters.AddWithValue("@Now", NowSa());
                    c.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("ExpireOld: " + ex);
            }
        }

        public static int ExpireAll()
        {
            try
            {
                using (var c = new SqlConnection(Cs()))
                using (var cmd = new SqlCommand(
                    @"UPDATE dbo.PackageReservation
                      SET Status='Expired',
                          Notes='Expired without payment'
                      WHERE Status='Reserved'
                        AND ExpiresOn<=@Now", c))
                {
                    cmd.Parameters.AddWithValue("@Now", NowSa());
                    c.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("ExpireAll: " + ex);
                return 0;
            }
        }

        public static bool Cancel(int reservationId, string reason)
        {
            try
            {
                using (var c = new SqlConnection(Cs()))
                using (var cmd = new SqlCommand(
                    @"UPDATE dbo.PackageReservation
                      SET Status='Cancelled',
                          CancelledOn=@Now,
                          Notes=@Reason
                      WHERE ReservationID=@R
                        AND Status='Reserved'", c))
                {
                    cmd.Parameters.AddWithValue("@R", reservationId);
                    cmd.Parameters.AddWithValue("@Now", NowSa());
                    cmd.Parameters.AddWithValue("@Reason",
                        (object)reason ?? DBNull.Value);

                    c.Open();
                    return cmd.ExecuteNonQuery() == 1;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Cancel: " + ex);
                return false;
            }
        }

        // Legacy compatibility only. Payments must be activated from the
        // verified PayFast notification, never from a button click.
        public static bool MarkPaid(
            int reservationId, int paymentId, decimal amountPaid,
            out string message)
        {
            message = "Direct payment activation is disabled. " +
                      "Use the verified PayFast notification.";
            return false;
        }
    }
}