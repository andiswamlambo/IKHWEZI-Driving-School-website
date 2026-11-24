
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace WebApplicationM3
{
    public static class PayFastPackageProcessor
    {
        private static string Cs
        {
            get
            {
                return ConfigurationManager
                    .ConnectionStrings["KhweziConnection"]
                    .ConnectionString;
            }
        }

        public static string Apply(
            string reference,
            string providerId,
            decimal amount)
        {
            if (string.IsNullOrWhiteSpace(reference) ||
                string.IsNullOrWhiteSpace(providerId) ||
                amount <= 0m)
            {
                throw new ArgumentException(
                    "Invalid verified payment details.");
            }

            using (SqlConnection c = new SqlConnection(Cs))
            {
                c.Open();

                using (SqlTransaction tx =
                    c.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        string result = ApplyInsideTransaction(
                            c, tx, reference, providerId, amount);

                        tx.Commit();
                        return result;
                    }
                    catch
                    {
                        try
                        {
                            tx.Rollback();
                        }
                        catch
                        {
                        }

                        throw;
                    }
                }
            }
        }

        private static string ApplyInsideTransaction(
            SqlConnection c,
            SqlTransaction tx,
            string reference,
            string providerId,
            decimal amount)
        {
            int attemptId;
            int reservationId;
            int student;
            decimal expectedAmount;
            string kind;
            string attemptStatus;
            string savedProvider;

            // STEP 1: Lock and read the payment attempt.
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT
                    AttemptID,
                    FlowKind,
                    FlowID,
                    StudNumber,
                    Amount,
                    Status,
                    ProviderPaymentID
                FROM dbo.PayFastAttempts
                    WITH (UPDLOCK, HOLDLOCK)
                WHERE Reference = @Reference",
                c, tx))
            {
                cmd.Parameters.Add(
                    "@Reference", SqlDbType.VarChar, 100)
                    .Value = reference;

                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    if (!rd.Read())
                    {
                        throw new InvalidOperationException(
                            "Unknown PayFast reference.");
                    }

                    attemptId = Convert.ToInt32(
                        rd["AttemptID"]);

                    reservationId = Convert.ToInt32(
                        rd["FlowID"]);

                    student = Convert.ToInt32(
                        rd["StudNumber"]);

                    expectedAmount = Convert.ToDecimal(
                        rd["Amount"]);

                    kind = Convert.ToString(
                        rd["FlowKind"]);

                    attemptStatus = Convert.ToString(
                        rd["Status"]);

                    savedProvider =
                        rd["ProviderPaymentID"] == DBNull.Value
                        ? null
                        : Convert.ToString(
                            rd["ProviderPaymentID"]);
                }
            }

            // STEP 2: Validate the payment details.
            if (kind != "Reservation" && kind != "Balance")
            {
                throw new InvalidOperationException(
                    "Unsupported payment flow.");
            }

            if (expectedAmount != amount)
            {
                throw new InvalidOperationException(
                    "PayFast payment amount mismatch.");
            }

            if (!string.IsNullOrEmpty(savedProvider) &&
                savedProvider != providerId)
            {
                throw new InvalidOperationException(
                    "PayFast transaction ID mismatch.");
            }

            // Prevent a provider transaction being reused.
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT COUNT(*)
                FROM dbo.PayFastAttempts
                    WITH (UPDLOCK, HOLDLOCK)
                WHERE ProviderPaymentID = @Provider
                  AND AttemptID <> @Attempt",
                c, tx))
            {
                cmd.Parameters.Add(
                    "@Provider", SqlDbType.VarChar, 100)
                    .Value = providerId;

                cmd.Parameters.Add(
                    "@Attempt", SqlDbType.Int)
                    .Value = attemptId;

                if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                {
                    throw new InvalidOperationException(
                        "PayFast transaction already belongs " +
                        "to another payment attempt.");
                }
            }

            // STEP 3: Handle duplicate notifications.
            if (attemptStatus == "Applied")
            {
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT COUNT(*)
                    FROM dbo.PackagePaymentLedger
                    WHERE AttemptID = @Attempt
                      AND ProviderPaymentID = @Provider
                      AND Amount = @Amount",
                    c, tx))
                {
                    cmd.Parameters.Add(
                        "@Attempt", SqlDbType.Int)
                        .Value = attemptId;

                    cmd.Parameters.Add(
                        "@Provider", SqlDbType.VarChar, 100)
                        .Value = providerId;

                    SqlParameter p = cmd.Parameters.Add(
                        "@Amount", SqlDbType.Decimal);

                    p.Precision = 12;
                    p.Scale = 2;
                    p.Value = amount;

                    if (Convert.ToInt32(cmd.ExecuteScalar()) != 1)
                    {
                        throw new InvalidOperationException(
                            "Applied payment has an inconsistent ledger.");
                    }
                }

                return "Already processed";
            }

            if (attemptStatus == "Review")
            {
                return "Already awaiting manual review";
            }

            if (attemptStatus != "Pending" &&
                attemptStatus != "Verified")
            {
                throw new InvalidOperationException(
                    "Payment attempt is not eligible.");
            }

            // STEP 4: Lock the reservation.
            int packageId;
            decimal packagePrice;
            decimal minimumDeposit;
            string reservationStatus;
            DateTime expires;

            using (SqlCommand cmd = new SqlCommand(@"
                SELECT
                    PackageID,
                    PackagePrice,
                    DepositAmount,
                    Status,
                    ExpiresOn
                FROM dbo.PackageReservation
                    WITH (UPDLOCK, HOLDLOCK)
                WHERE ReservationID = @Reservation
                  AND StudNumber = @Student",
                c, tx))
            {
                cmd.Parameters.Add(
                    "@Reservation", SqlDbType.Int)
                    .Value = reservationId;

                cmd.Parameters.Add(
                    "@Student", SqlDbType.Int)
                    .Value = student;

                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    if (!rd.Read())
                    {
                        throw new InvalidOperationException(
                            "Reservation not found.");
                    }

                    packageId = Convert.ToInt32(
                        rd["PackageID"]);

                    packagePrice = Convert.ToDecimal(
                        rd["PackagePrice"]);

                    minimumDeposit = Convert.ToDecimal(
                        rd["DepositAmount"]);

                    reservationStatus = Convert.ToString(
                        rd["Status"]);

                    expires = Convert.ToDateTime(
                        rd["ExpiresOn"]);
                }
            }

            // STEP 5: Read existing balance information.
            decimal alreadyPaid = 0m;
            bool balanceExists = false;
            string balanceStatus = "";

            using (SqlCommand cmd = new SqlCommand(@"
                SELECT TotalPaid, PaymentStatus
                FROM dbo.PackagePaymentBalance
                    WITH (UPDLOCK, HOLDLOCK)
                WHERE ReservationID = @Reservation",
                c, tx))
            {
                cmd.Parameters.Add(
                    "@Reservation", SqlDbType.Int)
                    .Value = reservationId;

                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    if (rd.Read())
                    {
                        balanceExists = true;

                        alreadyPaid = Convert.ToDecimal(
                            rd["TotalPaid"]);

                        balanceStatus = Convert.ToString(
                            rd["PaymentStatus"]);
                    }
                }
            }

            bool firstPayment = alreadyPaid == 0m;
            bool eligible = false;

            if (firstPayment)
            {
                decimal half = decimal.Round(
                    packagePrice * 0.50m,
                    2,
                    MidpointRounding.AwayFromZero);

                decimal required = Math.Max(
                    half, minimumDeposit);

                eligible =
                    kind == "Reservation" &&
                    reservationStatus == "Reserved" &&
                    expires > DateTime.Now &&
                    amount >= required &&
                    amount <= packagePrice &&
                    balanceStatus != "ReviewRequired";
            }
            else
            {
                eligible =
                    kind == "Balance" &&
                    reservationStatus == "Paid" &&
                    balanceExists &&
                    balanceStatus == "PartiallyPaid" &&
                    alreadyPaid > 0m &&
                    amount <= packagePrice - alreadyPaid;

                if (eligible)
                {
                    // Existing package must be activated.
                    using (SqlCommand cmd = new SqlCommand(@"
                        SELECT COUNT(*)
                        FROM dbo.ReservationPackageActivation a
                        INNER JOIN dbo.StudentPackage sp
                            ON sp.StudentPackageID =
                               a.StudentPackageID
                        WHERE a.ReservationID = @Reservation
                          AND sp.StudNumber = @Student
                          AND sp.PackageID = @Package",
                        c, tx))
                    {
                        cmd.Parameters.Add(
                            "@Reservation", SqlDbType.Int)
                            .Value = reservationId;

                        cmd.Parameters.Add(
                            "@Student", SqlDbType.Int)
                            .Value = student;

                        cmd.Parameters.Add(
                            "@Package", SqlDbType.Int)
                            .Value = packageId;

                        eligible =
                            Convert.ToInt32(
                                cmd.ExecuteScalar()) == 1;
                    }
                }
            }

            // STEP 6: Hold invalid or late payments for review.
            if (!eligible)
            {
                using (SqlCommand cmd = new SqlCommand(@"
                    UPDATE dbo.PayFastAttempts
                    SET Status = 'Review',
                        ProviderPaymentID = @Provider,
                        VerifiedUtc = COALESCE(
                            VerifiedUtc, SYSUTCDATETIME()),
                        ErrorMessage =
                            'Verified payment requires reconciliation'
                    WHERE AttemptID = @Attempt",
                    c, tx))
                {
                    cmd.Parameters.Add(
                        "@Attempt", SqlDbType.Int)
                        .Value = attemptId;

                    cmd.Parameters.Add(
                        "@Provider", SqlDbType.VarChar, 100)
                        .Value = providerId;

                    cmd.ExecuteNonQuery();
                }

                return "Payment requires manual review";
            }

            // STEP 7: Record payment.
            int paymentId;

            using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO dbo.Payment
                (
                    BookingID,
                    AmountPaid,
                    PaymentMethod,
                    PaymentStatus,
                    PaymentDate,
                    ProviderPaymentID
                )
                OUTPUT INSERTED.PaymentID
                VALUES
                (
                    NULL,
                    @Amount,
                    'PayFast',
                    'Paid',
                    GETDATE(),
                    @Provider
                )",
                c, tx))
            {
                SqlParameter p = cmd.Parameters.Add(
                    "@Amount", SqlDbType.Decimal);

                p.Precision = 12;
                p.Scale = 2;
                p.Value = amount;

                cmd.Parameters.Add(
                    "@Provider", SqlDbType.VarChar, 100)
                    .Value = providerId;

                paymentId = Convert.ToInt32(
                    cmd.ExecuteScalar());
            }

            // STEP 8: Activate package for first payment.
            if (firstPayment)
            {
                int studentPackageId = CreatePackageAllocation(
                    c, tx, student, packageId);

                using (SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO dbo.ReservationPackageActivation
                    (
                        ReservationID,
                        StudentPackageID,
                        ActivatedUtc
                    )
                    VALUES
                    (
                        @Reservation,
                        @StudentPackage,
                        SYSUTCDATETIME()
                    )",
                    c, tx))
                {
                    cmd.Parameters.Add(
                        "@Reservation", SqlDbType.Int)
                        .Value = reservationId;

                    cmd.Parameters.Add(
                        "@StudentPackage", SqlDbType.Int)
                        .Value = studentPackageId;

                    cmd.ExecuteNonQuery();
                }

                string status = amount == packagePrice
                    ? "FullyPaid"
                    : "PartiallyPaid";

                if (!balanceExists)
                {
                    using (SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO dbo.PackagePaymentBalance
                        (
                            ReservationID,
                            StudNumber,
                            PackagePrice,
                            TotalPaid,
                            FirstPaidOn,
                            DueDate,
                            PaymentStatus
                        )
                        VALUES
                        (
                            @Reservation,
                            @Student,
                            @Price,
                            @Amount,
                            SYSUTCDATETIME(),
                            DATEADD(day, 60, SYSUTCDATETIME()),
                            @Status
                        )",
                        c, tx))
                    {
                        cmd.Parameters.Add(
                            "@Reservation", SqlDbType.Int)
                            .Value = reservationId;

                        cmd.Parameters.Add(
                            "@Student", SqlDbType.Int)
                            .Value = student;

                        SqlParameter priceParam =
                            cmd.Parameters.Add(
                                "@Price", SqlDbType.Decimal);

                        priceParam.Precision = 12;
                        priceParam.Scale = 2;
                        priceParam.Value = packagePrice;

                        SqlParameter amountParam =
                            cmd.Parameters.Add(
                                "@Amount", SqlDbType.Decimal);

                        amountParam.Precision = 12;
                        amountParam.Scale = 2;
                        amountParam.Value = amount;

                        cmd.Parameters.Add(
                            "@Status", SqlDbType.VarChar, 30)
                            .Value = status;

                        cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    using (SqlCommand cmd = new SqlCommand(@"
                        UPDATE dbo.PackagePaymentBalance
                        SET TotalPaid = @Amount,
                            FirstPaidOn = SYSUTCDATETIME(),
                            DueDate = DATEADD(
                                day, 60, SYSUTCDATETIME()),
                            PaymentStatus = @Status
                        WHERE ReservationID = @Reservation
                          AND TotalPaid = 0",
                        c, tx))
                    {
                        cmd.Parameters.Add(
                            "@Reservation", SqlDbType.Int)
                            .Value = reservationId;

                        SqlParameter amountParam =
                            cmd.Parameters.Add(
                                "@Amount", SqlDbType.Decimal);

                        amountParam.Precision = 12;
                        amountParam.Scale = 2;
                        amountParam.Value = amount;

                        cmd.Parameters.Add(
                            "@Status", SqlDbType.VarChar, 30)
                            .Value = status;

                        if (cmd.ExecuteNonQuery() != 1)
                        {
                            throw new InvalidOperationException(
                                "Initial balance update failed.");
                        }
                    }
                }

                using (SqlCommand cmd = new SqlCommand(@"
                    UPDATE dbo.PackageReservation
                    SET Status = 'Paid',
                        PaidOn = GETDATE(),
                        PaymentID = @Payment
                    WHERE ReservationID = @Reservation
                      AND Status = 'Reserved'
                      AND ExpiresOn > GETDATE()",
                    c, tx))
                {
                    cmd.Parameters.Add(
                        "@Payment", SqlDbType.Int)
                        .Value = paymentId;

                    cmd.Parameters.Add(
                        "@Reservation", SqlDbType.Int)
                        .Value = reservationId;

                    if (cmd.ExecuteNonQuery() != 1)
                    {
                        throw new InvalidOperationException(
                            "Reservation activation failed.");
                    }
                }
            }
            else
            {
                // STEP 9: Balance payment.
                // Never create another lesson allocation.
                decimal newTotal = alreadyPaid + amount;

                using (SqlCommand cmd = new SqlCommand(@"
                    UPDATE dbo.PackagePaymentBalance
                    SET TotalPaid = @Total,
                        PaymentStatus = @Status
                    WHERE ReservationID = @Reservation
                      AND TotalPaid = @Previous",
                    c, tx))
                {
                    cmd.Parameters.Add(
                        "@Reservation", SqlDbType.Int)
                        .Value = reservationId;

                    SqlParameter previous =
                        cmd.Parameters.Add(
                            "@Previous", SqlDbType.Decimal);

                    previous.Precision = 12;
                    previous.Scale = 2;
                    previous.Value = alreadyPaid;

                    SqlParameter total =
                        cmd.Parameters.Add(
                            "@Total", SqlDbType.Decimal);

                    total.Precision = 12;
                    total.Scale = 2;
                    total.Value = newTotal;

                    cmd.Parameters.Add(
                        "@Status", SqlDbType.VarChar, 30)
                        .Value = newTotal == packagePrice
                            ? "FullyPaid"
                            : "PartiallyPaid";

                    if (cmd.ExecuteNonQuery() != 1)
                    {
                        throw new InvalidOperationException(
                            "Balance payment update failed.");
                    }
                }
            }

            // STEP 10: Record payment in ledger.
            using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO dbo.PackagePaymentLedger
                (
                    ReservationID,
                    AttemptID,
                    ProviderPaymentID,
                    Amount,
                    PaidOn
                )
                VALUES
                (
                    @Reservation,
                    @Attempt,
                    @Provider,
                    @Amount,
                    SYSUTCDATETIME()
                )",
                c, tx))
            {
                cmd.Parameters.Add(
                    "@Reservation", SqlDbType.Int)
                    .Value = reservationId;

                cmd.Parameters.Add(
                    "@Attempt", SqlDbType.Int)
                    .Value = attemptId;

                cmd.Parameters.Add(
                    "@Provider", SqlDbType.VarChar, 100)
                    .Value = providerId;

                SqlParameter amountParam =
                    cmd.Parameters.Add(
                        "@Amount", SqlDbType.Decimal);

                amountParam.Precision = 12;
                amountParam.Scale = 2;
                amountParam.Value = amount;

                cmd.ExecuteNonQuery();
            }

            // STEP 11: Mark the attempt as applied.
            using (SqlCommand cmd = new SqlCommand(@"
                UPDATE dbo.PayFastAttempts
                SET Status = 'Applied',
                    ProviderPaymentID = @Provider,
                    VerifiedUtc = COALESCE(
                        VerifiedUtc, SYSUTCDATETIME()),
                    AppliedUtc = SYSUTCDATETIME(),
                    ErrorMessage = NULL
                WHERE AttemptID = @Attempt",
                c, tx))
            {
                cmd.Parameters.Add(
                    "@Attempt", SqlDbType.Int)
                    .Value = attemptId;

                cmd.Parameters.Add(
                    "@Provider", SqlDbType.VarChar, 100)
                    .Value = providerId;

                if (cmd.ExecuteNonQuery() != 1)
                {
                    throw new InvalidOperationException(
                        "Could not finalise payment attempt.");
                }
            }

            return "Payment successfully applied";
        }

        private static int CreatePackageAllocation(
            SqlConnection c,
            SqlTransaction tx,
            int student,
            int packageId)
        {
            using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO dbo.StudentPackage
                (
                    StudNumber,
                    PackageID,
                    TotalLessons,
                    UsedLessons,
                    PurchasedOn,
                    IsActive
                )
                OUTPUT INSERTED.StudentPackageID
                SELECT
                    @Student,
                    PackageID,
                    TotalLessons,
                    0,
                    GETDATE(),
                    1
                FROM dbo.Packages
                WHERE PackageID = @Package
                  AND TotalLessons > 0",
                c, tx))
            {
                cmd.Parameters.Add(
                    "@Student", SqlDbType.Int)
                    .Value = student;

                cmd.Parameters.Add(
                    "@Package", SqlDbType.Int)
                    .Value = packageId;

                object result = cmd.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                {
                    throw new InvalidOperationException(
                        "Package allocation failed.");
                }

                return Convert.ToInt32(result);
            }
        }
    }
}
