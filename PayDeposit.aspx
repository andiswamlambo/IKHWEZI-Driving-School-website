 
<%@ Page Title="Pay Deposit" Language="C#"
    AutoEventWireup="true"
    CodeBehind="PayDeposit.aspx.cs"
    Inherits="WebApplicationM3.PayDeposit" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport"
          content="width=device-width, initial-scale=1.0" />

    <title>IKHWEZI - Secure Payment</title>

    <style>
        * {
            box-sizing: border-box;
            margin: 0;
            padding: 0;
        }

        body {
            font-family: 'Segoe UI', Arial, sans-serif;
            background: linear-gradient(180deg, #e9f1fc, #d9e5f5);
            min-height: 100vh;
            padding: 40px 20px;
        }

        .card {
            max-width: 580px;
            margin: 0 auto;
            background: white;
            border-radius: 14px;
            box-shadow: 0 5px 20px rgba(13,43,110,.10);
            overflow: hidden;
        }

        .head {
            background: #0d2b6e;
            color: white;
            padding: 25px;
        }

        .head h1 {
            font-size: 22px;
            margin-bottom: 6px;
        }

        .head p {
            color: #c3d2ee;
            font-size: 13px;
        }

        .body {
            padding: 26px;
        }

        .timer {
            background: #fff7e0;
            border: 1px solid #f0d999;
            color: #7a5a00;
            padding: 15px;
            border-radius: 9px;
            text-align: center;
            margin-bottom: 20px;
            font-weight: 700;
            font-size: 14px;
        }

        .timer-small {
            display: block;
            font-size: 12px;
            font-weight: 400;
            margin-top: 6px;
        }

        .summary-row {
            display: flex;
            justify-content: space-between;
            gap: 15px;
            padding: 12px 0;
            border-bottom: 1px dashed #dbe4f6;
            font-size: 14px;
        }

        .lbl {
            color: #7787a5;
            font-weight: 600;
        }

        .val {
            color: #0d2b6e;
            font-weight: 700;
            text-align: right;
        }

        .amount {
            background: #effaf4;
            border-radius: 10px;
            padding: 20px;
            text-align: center;
            margin: 22px 0;
        }

        .amount-title {
            font-size: 13px;
            color: #52647c;
            margin-bottom: 8px;
        }

        .amount-value {
            color: #22b573;
            font-size: 28px;
            font-weight: 800;
        }

        .payment-options {
            padding: 22px;
            margin: 20px 0;
            border: 1px solid #dbe3ef;
            border-radius: 12px;
            background: #f7faff;
        }

        .payment-options h3 {
            color: #0d2b6e;
            font-size: 18px;
            margin-bottom: 12px;
        }

        .payment-options p {
            color: #52647c;
            font-size: 13px;
            line-height: 1.7;
            margin-bottom: 15px;
        }

        .input-label {
            display: block;
            color: #0d2b6e;
            font-size: 14px;
            font-weight: 700;
            margin-bottom: 8px;
        }

        .percentage-input {
            width: 180px;
            max-width: 100%;
            padding: 12px;
            border: 1px solid #ccd5e0;
            border-radius: 8px;
            font-size: 16px;
        }

        .payment-preview {
            background: white;
            border: 1px solid #dbe3ef;
            border-radius: 9px;
            padding: 16px;
            margin-top: 18px;
        }

        .preview-row {
            display: flex;
            justify-content: space-between;
            gap: 12px;
            padding: 8px 0;
            font-size: 14px;
        }

        .preview-row strong {
            color: #0d2b6e;
        }

        .preview-total {
            border-top: 1px solid #e2e8f0;
            margin-top: 8px;
            padding-top: 14px;
            font-weight: 800;
        }

        .notice {
            background: #fff7e0;
            border: 1px solid #f0d999;
            border-radius: 8px;
            padding: 14px;
            margin-top: 16px;
            color: #7a5a00;
            font-size: 13px;
            line-height: 1.6;
        }

        .btn {
            display: block;
            width: 100%;
            padding: 15px;
            background: #22b573;
            color: white;
            border: none;
            border-radius: 25px;
            font-weight: 800;
            font-size: 15px;
            cursor: pointer;
            margin-top: 14px;
            text-align: center;
            text-decoration: none;
        }

        .btn:hover {
            background: #1d9e63;
        }

        .btn-secondary {
            background: #a9b4c4;
        }

        .btn-secondary:hover {
            background: #8d99ac;
        }

        .msg {
            padding: 14px;
            border-radius: 8px;
            font-size: 13px;
            font-weight: 600;
            margin-bottom: 16px;
        }

        .msg.err {
            background: #fdf0f1;
            color: #a3141f;
            border: 1px solid #f3c2c9;
        }

        .msg.warn {
            background: #fff7e0;
            color: #7a5a00;
            border: 1px solid #f0d999;
        }

        .msg.ok {
            background: #effaf4;
            color: #14663f;
            border: 1px solid #bfe8d4;
        }

        .confirm-inline {
            background: #fff7e0;
            border: 1px solid #f0d999;
            border-radius: 9px;
            padding: 18px;
            margin-top: 15px;
            font-size: 13px;
            line-height: 1.7;
        }

        .confirm-inline h4 {
            color: #7a5a00;
            margin-bottom: 8px;
        }

        .btn-yes {
            display: inline-block;
            background: #e8283f;
            color: white;
            padding: 10px 20px;
            border-radius: 20px;
            border: none;
            cursor: pointer;
            margin-top: 12px;
            text-decoration: none;
        }

        .btn-no {
            display: inline-block;
            background: #a9b4c4;
            color: white;
            padding: 10px 20px;
            border-radius: 20px;
            border: none;
            cursor: pointer;
            margin-top: 12px;
            text-decoration: none;
        }

        .back {
            display: inline-block;
            margin-top: 20px;
            color: #0d2b6e;
            font-size: 13px;
            font-weight: 700;
            text-decoration: none;
        }

        .input-error {
            color: #a3141f;
            font-size: 12px;
            margin-top: 8px;
            display: none;
            line-height: 1.5;
        }

        @media (max-width: 480px) {
            body { padding: 18px 10px; }
            .body { padding: 18px; }
            .payment-options { padding: 16px; }
        }
    </style>
</head>

<body>
<form id="form1" runat="server">

    <div class="card">

        <div class="head">
            <h1>IKHWEZI Driving School</h1>
            <p>Secure Package Payment</p>
        </div>

        <div class="body">

            <asp:Label
                ID="lblMsg"
                runat="server"
                Visible="false" />

            <asp:Panel
                ID="pnlPay"
                runat="server"
                Visible="false">

                <!-- RESERVATION TIMER -->
                <asp:Panel
                    ID="pnlTimer"
                    runat="server"
                    CssClass="timer"
                    Visible="false">

                    Reservation expires in
                    <asp:Literal
                        ID="litDaysLeft"
                        runat="server" />
                    day(s)

                    <span class="timer-small">
                        Deadline:
                        <asp:Literal
                            ID="litExpires"
                            runat="server" />
                    </span>

                </asp:Panel>

                <!-- PAYMENT SUMMARY -->
                <div class="summary-row">
                    <span class="lbl">Reference</span>
                    <span class="val">
                        #<asp:Literal
                            ID="litRef"
                            runat="server" />
                    </span>
                </div>

                <div class="summary-row">
                    <span class="lbl">Package</span>
                    <span class="val">
                        <asp:Literal
                            ID="litPackage"
                            runat="server" />
                    </span>
                </div>

                <div class="summary-row">
                    <span class="lbl">Package Total</span>
                    <span class="val">
                        R <asp:Literal
                            ID="litTotal"
                            runat="server" />
                    </span>
                </div>

                <!-- MINIMUM PAYMENT -->
                <div class="amount">
                    <div class="amount-title">
                        <asp:Literal
                            ID="litAmountLabel"
                            runat="server"
                            Text="Minimum Payment (50%)" />
                    </div>

                    <div class="amount-value">
                        R <asp:Literal
                            ID="litDeposit"
                            runat="server" />
                    </div>
                </div>

                <!-- FLEXIBLE PAYMENT -->
                <div class="payment-options">

                    <h3>Choose Your Payment Amount</h3>

                    <p>
                        Enter the amount you would like to pay today.
                        Your initial payment must be at least 50%
                        of the package price and cannot exceed
                        the full package price.
                    </p>

                    <asp:Label
                        ID="lblPaymentAmount"
                        runat="server"
                        AssociatedControlID="txtPaymentAmount"
                        CssClass="input-label"
                        Text="Amount to pay (R):" />

                    <asp:TextBox
                        ID="txtPaymentAmount"
                        runat="server"
                        TextMode="Number"
                        CssClass="percentage-input"
                        step="0.01"
                        min="0"
                        oninput="updatePaymentPreview()" />

                    <div id="paymentInputError"
                         class="input-error"
                         role="alert"></div>

                    <!-- SERVER VALUES FOR JAVASCRIPT -->
                    <asp:HiddenField
                        ID="hfPackagePrice"
                        runat="server" />

                    <asp:HiddenField
                        ID="hfMinimumPayment"
                        runat="server" />

                    <!-- PAYMENT PREVIEW -->
                    <div class="payment-preview">

                        <div class="preview-row">
                            <span>Package Price</span>
                            <strong id="previewPackage">
                                R 0.00
                            </strong>
                        </div>

                        <div class="preview-row">
                            <span>Payment Percentage</span>
                            <strong id="previewPercent">
                                50%
                            </strong>
                        </div>

                        <div class="preview-row preview-total">
                            <span>Pay Now</span>
                            <strong id="previewPayNow">
                                R 0.00
                            </strong>
                        </div>

                        <div class="preview-row">
                            <span>Remaining Balance</span>
                            <strong id="previewBalance">
                                R 0.00
                            </strong>
                        </div>

                    </div>

                    <div class="notice">
                        Any outstanding balance must be
                        settled within 60 days after
                        your first confirmed payment.
                        Your package will only activate
                        after payment verification.
                    </div>

                </div>

                <!-- PAYFAST BUTTON -->
                <asp:Button
                    ID="btnPay"
                    runat="server"
                    Text="Pay Securely with PayFast"
                    CssClass="btn"
                    OnClick="btnPay_Click"
                    OnClientClick="return validatePaymentAmount();" />

                <!-- CANCEL RESERVATION -->
                <asp:Button
                    ID="btnCancel"
                    runat="server"
                    Text="Cancel Reservation"
                    CssClass="btn btn-secondary"
                    OnClick="btnCancel_Click"
                    CausesValidation="false" />

                <asp:Panel
                    ID="pnlConfirmCancel"
                    runat="server"
                    CssClass="confirm-inline"
                    Visible="false">

                    <h4>Cancel this reservation?</h4>

                    <p>
                        Cancelling will release your
                        reservation. If a payment has
                        already been processed, contact
                        reception for payment review.
                    </p>

                    <asp:LinkButton
                        ID="btnCancelConfirmYes"
                        runat="server"
                        CssClass="btn-yes"
                        OnClick="btnCancelConfirmYes_Click"
                        CausesValidation="false">
                        Yes, Cancel
                    </asp:LinkButton>

                    <asp:LinkButton
                        ID="btnCancelConfirmNo"
                        runat="server"
                        CssClass="btn-no"
                        OnClick="btnCancelConfirmNo_Click"
                        CausesValidation="false">
                        Keep Reservation
                    </asp:LinkButton>

                </asp:Panel>

            </asp:Panel>

            <!-- VERIFIED PAYMENT SUCCESS -->
            <asp:Panel
                ID="pnlPaid"
                runat="server"
                Visible="false">

                <div class="msg ok">
                    Payment confirmed!
                    Your driving package payment
                    has been recorded.
                </div>

                <asp:Button
                    ID="btnContinue"
                    runat="server"
                    Text="Continue to Dashboard"
                    CssClass="btn"
                    OnClick="btnContinue_Click" />

            </asp:Panel>

            <a href="StudentDashboard.aspx"
               class="back">
                &larr; Back to Dashboard
            </a>

        </div>
    </div>

</form>

<script type="text/javascript">

    function paymentNumber(value) {
        return Number(String(value || "")
            .replace(/,/g, "")
            .replace(/\s/g, ""));
    }

    function formatMoney(value) {
        return "R " + value.toLocaleString("en-ZA", {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }

    function getPaymentValues() {
        var priceField = document.getElementById(
            '<%= hfPackagePrice.ClientID %>');

        var minimumField = document.getElementById(
            '<%= hfMinimumPayment.ClientID %>');

        var amountField = document.getElementById(
            '<%= txtPaymentAmount.ClientID %>');

        if (!priceField || !minimumField || !amountField)
            return null;

        return {
            price: paymentNumber(priceField.value),
            minimum: paymentNumber(minimumField.value),
            amount: paymentNumber(amountField.value),
            rawAmount: amountField.value.trim()
        };
    }

    function updatePaymentPreview() {
        var values = getPaymentValues();
        if (!values) return;

        var packageElement =
            document.getElementById("previewPackage");

        var percentElement =
            document.getElementById("previewPercent");

        var payNowElement =
            document.getElementById("previewPayNow");

        var balanceElement =
            document.getElementById("previewBalance");

        var errorElement =
            document.getElementById("paymentInputError");

        if (!Number.isFinite(values.price) ||
            values.price <= 0) {
            packageElement.textContent = "—";
            percentElement.textContent = "—";
            payNowElement.textContent = "—";
            balanceElement.textContent = "—";
            return;
        }

        packageElement.textContent =
            formatMoney(values.price);

        var validAmount =
            values.rawAmount !== "" &&
            Number.isFinite(values.amount) &&
            values.amount >= values.minimum &&
            values.amount <= values.price &&
            Math.round(values.amount * 100) / 100 ===
                values.amount;

        if (!validAmount) {
            percentElement.textContent = "—";
            payNowElement.textContent = "—";
            balanceElement.textContent = "—";

            errorElement.style.display = "block";

            if (values.rawAmount === "") {
                errorElement.textContent =
                    "Please enter the amount you want to pay.";
            }
            else if (values.amount < values.minimum) {
                errorElement.textContent =
                    "The minimum initial payment is " +
                    formatMoney(values.minimum) + ".";
            }
            else if (values.amount > values.price) {
                errorElement.textContent =
                    "The payment cannot exceed " +
                    formatMoney(values.price) + ".";
            }
            else {
                errorElement.textContent =
                    "Please enter a valid amount with no more than two decimal places.";
            }

            return;
        }

        errorElement.style.display = "none";
        errorElement.textContent = "";

        var percentage =
            values.amount / values.price * 100;

        var balance =
            Math.round((values.price - values.amount) * 100) / 100;

        percentElement.textContent =
            percentage.toFixed(2).replace(/\.?0+$/, "") + "%";

        payNowElement.textContent =
            formatMoney(values.amount);

        balanceElement.textContent =
            formatMoney(balance);
    }

    function validatePaymentAmount() {
        updatePaymentPreview();

        var values = getPaymentValues();
        if (!values) return false;

        var valid =
            values.rawAmount !== "" &&
            Number.isFinite(values.price) &&
            Number.isFinite(values.amount) &&
            values.price > 0 &&
            values.amount >= values.minimum &&
            values.amount <= values.price &&
            Math.round(values.amount * 100) / 100 ===
                values.amount;

        if (!valid) {
            document.getElementById(
                '<%= txtPaymentAmount.ClientID %>').focus();
        }

        return valid;
    }

    document.addEventListener(
        "DOMContentLoaded",
        updatePaymentPreview
    );

</script>

</body>
</html>
