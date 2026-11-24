 
<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="PaymentSuccess.aspx.cs"
    Inherits="WebApplicationM3.PaymentSuccess" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport"
          content="width=device-width, initial-scale=1.0" />
    <title>Payment Confirmation - IKHWEZI</title>

    <style>
        * {
            box-sizing: border-box;
            margin: 0;
            padding: 0;
        }

        body {
            font-family: 'Segoe UI', Arial, sans-serif;
            background: linear-gradient(180deg,#e9f1fc,#d9e5f5);
            min-height: 100vh;
            display: flex;
            justify-content: center;
            align-items: center;
            padding: 25px 15px;
        }

        .card {
            background: white;
            width: 100%;
            max-width: 510px;
            border-radius: 14px;
            overflow: hidden;
            box-shadow: 0 5px 22px rgba(13,43,110,.12);
        }

        .header {
            background: #0d2b6e;
            padding: 24px 28px;
            color: white;
        }

        .header h1 {
            font-size: 22px;
            font-weight: 800;
        }

        .header p {
            color: #c3d2ee;
            font-size: 12px;
            margin-top: 5px;
        }

        .content {
            padding: 28px;
            text-align: center;
        }

        .icon-circle {
            width: 76px;
            height: 76px;
            margin: 0 auto 16px;
            background: #effaf4;
            color: #22b573;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .icon-circle svg {
            width: 40px;
            height: 40px;
            fill: none;
            stroke: currentColor;
            stroke-width: 2.5;
            stroke-linecap: round;
            stroke-linejoin: round;
        }

        h2 {
            color: #0d2b6e;
            font-size: 23px;
            margin-bottom: 8px;
        }

        .subtitle {
            color: #52647c;
            font-size: 13px;
            line-height: 1.6;
        }

        .test-label {
            display: inline-block;
            background: #fff3d6;
            color: #926300;
            padding: 6px 13px;
            border-radius: 20px;
            font-size: 11px;
            font-weight: 800;
            margin-top: 15px;
            letter-spacing: .5px;
        }

        .paid-amount {
            margin: 22px 0;
            background: #effaf4;
            padding: 20px;
            border-radius: 10px;
        }

        .paid-amount span {
            display: block;
            color: #52647c;
            font-size: 12px;
            margin-bottom: 7px;
        }

        .paid-amount strong {
            font-size: 30px;
            color: #22b573;
        }

        .receipt {
            border: 1px solid #dbe4f6;
            border-radius: 10px;
            padding: 15px 18px;
            text-align: left;
        }

        .receipt-title {
            color: #0d2b6e;
            font-size: 15px;
            font-weight: 800;
            margin-bottom: 8px;
        }

        .receipt-row {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            gap: 15px;
            padding: 11px 0;
            border-bottom: 1px dashed #e2e8f0;
            font-size: 13px;
        }

        .receipt-row:last-child {
            border-bottom: none;
        }

        .receipt-row span {
            color: #7787a5;
        }

        .receipt-row strong {
            color: #0d2b6e;
            text-align: right;
            overflow-wrap: anywhere;
        }

        .balance {
            color: #e8890c !important;
        }

        .notice {
            background: #f2f6fd;
            border-radius: 8px;
            padding: 13px;
            margin-top: 18px;
            font-size: 12px;
            color: #52647c;
            line-height: 1.6;
            text-align: left;
        }

        .btn {
            display: block;
            width: 100%;
            background: #22b573;
            color: white;
            border-radius: 25px;
            padding: 15px;
            text-decoration: none;
            font-weight: 800;
            font-size: 14px;
            margin-top: 20px;
            text-align: center;
        }

        .btn:hover {
            background: #1d9e63;
        }

        .btn-secondary {
            background: #0d2b6e;
        }

        .btn-secondary:hover {
            background: #173c8c;
        }

        .spinner {
            border: 4px solid #e5edf7;
            border-top: 4px solid #22b573;
            border-radius: 50%;
            width: 42px;
            height: 42px;
            animation: spin 1s linear infinite;
            margin: 0 auto 18px;
        }

        @keyframes spin {
            to { transform: rotate(360deg); }
        }

        .error-icon {
            width: 65px;
            height: 65px;
            background: #fdf0f1;
            color: #e8283f;
            border-radius: 50%;
            margin: 0 auto 18px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 32px;
            font-weight: 800;
        }

        @media(max-width:480px) {
            .content { padding: 20px; }
            .header { padding: 22px; }
        }
    </style>
</head>

<body>
<form id="form1" runat="server">

    <div class="card">

        <div class="header">
            <h1>IKHWEZI Driving School</h1>
            <p>Secure Payment Confirmation</p>
        </div>

        <div class="content">

            <!-- SUCCESS RECEIPT -->
            <asp:Panel ID="pnlSuccess"
                runat="server" Visible="false">

                <div class="icon-circle">
                    <svg viewBox="0 0 24 24">
                        <polyline points="20 6 9 17 4 12" />
                    </svg>
                </div>

                <h2>Payment Successful!</h2>

                <p class="subtitle">
                    Your payment has been verified
                    and recorded successfully.
                </p>

                <span class="test-label">
                    TEST PAYMENT · DEMONSTRATION
                </span>

                <div class="paid-amount">
                    <span>Amount Paid</span>
                    <strong>
                        R <asp:Literal ID="litAmountPaid"
                            runat="server" />
                    </strong>
                </div>

                <div class="receipt">

                    <div class="receipt-title">
                        Payment Receipt
                    </div>

                    <div class="receipt-row">
                        <span>Reservation</span>
                        <strong>
                            #<asp:Literal ID="litReservation"
                                runat="server" />
                        </strong>
                    </div>

                    <div class="receipt-row">
                        <span>Driving Package</span>
                        <strong>
                            <asp:Literal ID="litPackage"
                                runat="server" />
                        </strong>
                    </div>

                    <div class="receipt-row">
                        <span>Package Price</span>
                        <strong>
                            R <asp:Literal ID="litPackagePrice"
                                runat="server" />
                        </strong>
                    </div>

                    <div class="receipt-row">
                        <span>Total Paid to Date</span>
                        <strong>
                            R <asp:Literal ID="litTotalPaid"
                                runat="server" />
                        </strong>
                    </div>

                    <div class="receipt-row">
                        <span>Remaining Balance</span>
                        <strong class="balance">
                            R <asp:Literal ID="litBalance"
                                runat="server" />
                        </strong>
                    </div>

                    <div class="receipt-row">
                        <span>Payment Method</span>
                        <strong>Paystack (Test)</strong>
                    </div>

                    <div class="receipt-row">
                        <span>Payment Reference</span>
                        <strong>
                            <asp:Literal ID="litReference"
                                runat="server" />
                        </strong>
                    </div>

                    <div class="receipt-row">
                        <span>Payment Date</span>
                        <strong>
                            <asp:Literal ID="litPaymentDate"
                                runat="server" />
                        </strong>
                    </div>

                    <div class="receipt-row">
                        <span>Payment Status</span>
                        <strong style="color:#22b573;">
                            Confirmed
                        </strong>
                    </div>

                </div>

                <div class="notice">
                    Your driving package has been processed.
                    You may now continue to book your lessons.
                    Any outstanding balance remains subject
                    to IKHWEZI's existing payment terms.
                    <br /><br />
                    This is a demonstration transaction.
                    No real money has been charged.
                </div>

                <a href="BookSession.aspx?newpayment=1"
                   class="btn">
                    Continue to Book Lessons
                </a>

            </asp:Panel>

            <!-- PROCESSING / PENDING -->
            <asp:Panel ID="pnlProcessing"
                runat="server" Visible="false">

                <div class="spinner"></div>

                <h2>Payment Processing</h2>

                <p class="subtitle">
                    Your payment is being confirmed.
                    Please do not attempt another payment
                    until its status has been checked.
                </p>

                <asp:Literal ID="litProcessingInfo"
                    runat="server" />

                <a href="StudentDashboard.aspx"
                   class="btn btn-secondary">
                    Return to Dashboard
                </a>

            </asp:Panel>

            <!-- ERROR / UNSUCCESSFUL -->
            <asp:Panel ID="pnlError"
                runat="server" Visible="false">

                <div class="error-icon">!</div>

                <h2>Payment Not Confirmed</h2>

                <p class="subtitle">
                    <asp:Literal ID="litError"
                        runat="server" />
                </p>

                <a href="StudentDashboard.aspx"
                   class="btn btn-secondary">
                    Return to Dashboard
                </a>

            </asp:Panel>

        </div>
    </div>

</form>
</body>
</html>
