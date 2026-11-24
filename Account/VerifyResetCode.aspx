 <%@ Page Title="Verify Reset Code" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="VerifyResetCode.aspx.cs" Inherits="WebApplicationM3.Account.VerifyResetCode" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .vrc-container { background:#e6f2ff; min-height:80vh; display:flex;
                         align-items:center; justify-content:center;
                         border-radius:8px; padding:20px; margin-top:20px; }
        .vrc-box { background:#fff; padding:40px; border-radius:8px;
                   box-shadow:0 4px 15px rgba(0,0,0,.1); width:440px; }
        .vrc-title { color:#003366; margin-bottom:10px; font-weight:700; text-align:center; }
        .vrc-subtitle { color:#555; margin-bottom:26px; font-size:14px;
                        text-align:center; line-height:1.5; }
        .f-label { display:block; font-size:13px; color:#333; font-weight:600;
                   margin-bottom:6px; margin-top:14px; }
        .form-control { width:100%; height:48px; padding:10px 12px;
                        border:1px solid #ccd7ec; border-radius:6px;
                        font-size:22px; text-align:center;
                        letter-spacing:6px; font-family:Courier New, monospace; }
        .form-control:focus { outline:none; border-color:#007bff; }
        .btn-primary { background:#007bff; color:#fff; width:100%; height:46px;
                       font-size:15px; border:none; border-radius:6px;
                       font-weight:700; margin-top:22px; cursor:pointer; }
        .btn-primary:hover { background:#0056b3; }
        .message-label { display:block; margin-top:14px; font-weight:600;
                         font-size:13.5px; text-align:center; }
        .bottom-link { display:block; text-align:center; margin-top:18px;
                       color:#007bff; text-decoration:none; font-size:13px; }
        .bottom-link:hover { text-decoration:underline; }
    </style>

    <div class="vrc-container">
        <div class="vrc-box">
            <h2 class="vrc-title">Enter Your Code</h2>
            <p class="vrc-subtitle">
                We sent a 6-digit code to
                <strong><asp:Literal ID="litEmail" runat="server" /></strong>.
                Enter it below to reset your password.
            </p>

            <asp:Label ID="lblMsg" runat="server" CssClass="message-label" Visible="false" />

            <label class="f-label">6-Digit Code</label>
            <asp:TextBox ID="txtCode" runat="server" CssClass="form-control"
                MaxLength="6" placeholder="000000" autocomplete="off" />

            <asp:Button ID="btnVerify" runat="server" Text="Verify Code"
                CssClass="btn-primary" OnClick="btnVerify_Click" />

            <a href="ForgotPassword.aspx" class="bottom-link">&larr; Send a new code</a>
        </div>
    </div>

</asp:Content>