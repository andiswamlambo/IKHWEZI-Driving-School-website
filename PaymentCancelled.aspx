 <%@ Page Title="Payment Cancelled" Language="C#" AutoEventWireup="true"
    CodeBehind="PaymentCancelled.aspx.cs" Inherits="WebApplicationM3.PaymentCancelled" %>
<!DOCTYPE html>
<html lang="en"><head runat="server">
<meta charset="utf-8" /><meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>Payment Cancelled - IKHWEZI</title>
<style>
body{font-family:'Segoe UI',Arial,sans-serif;background:linear-gradient(180deg,#e9f1fc,#d9e5f5);
min-height:100vh;padding:40px 20px;display:flex;align-items:center;justify-content:center;}
.card{max-width:520px;width:100%;background:#fff;border-radius:12px;
box-shadow:0 4px 15px rgba(13,43,110,.08);overflow:hidden;text-align:center;}
.head{background:#e8890c;color:#fff;padding:32px 26px;}
.head .icon{font-size:42px;margin-bottom:8px;}
.head h1{font-size:22px;margin-bottom:4px;}
.head p{font-size:13px;opacity:.9;}
.body{padding:26px;}
.body p{color:#444;font-size:14px;line-height:1.6;margin-bottom:14px;}
.btn{display:inline-block;padding:12px 30px;background:#22b573;color:#fff;
border-radius:24px;font-weight:800;font-size:14px;text-decoration:none;margin-top:8px;margin-right:6px;}
.btn:hover{background:#1d9e63;color:#fff;text-decoration:none;}
.btn-secondary{background:#a9b4c4;}
.btn-secondary:hover{background:#8d99ac;}
</style></head>
<body><form id="form1" runat="server">
<div class="card">
<div class="head">
<div class="icon">&#9888;</div>
<h1>Payment Cancelled</h1>
<p>Your reservation is still held</p>
</div>
<div class="body">
<p>You cancelled the payment. Your package reservation is still active for now — you can pay later from your dashboard or by clicking below.</p>
<asp:Literal ID="litRef" runat="server" />
<a href="BookSession.aspx" class="btn">Try Again</a>
<a href="StudentDashboard.aspx" class="btn btn-secondary">Back to Dashboard</a>
</div>
</div>
</form></body></html>