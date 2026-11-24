 <%@ Page Title="Forgot Password" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ForgotPassword.aspx.cs" Inherits="WebApplicationM3.Account.ForgotPassword" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .fp-container { background:#e6f2ff; min-height:80vh; display:flex; align-items:center;
                         justify-content:center; border-radius:8px; padding:20px; margin-top:20px; }
        .fp-box { background:#fff; padding:40px; border-radius:8px;
                  box-shadow:0 4px 15px rgba(0,0,0,.1); width:440px; }
        .fp-title { color:#003366; margin-bottom:10px; font-weight:700; text-align:center; }
        .fp-subtitle { color:#555; margin-bottom:26px; font-size:14px;
                       text-align:center; line-height:1.5; }
        .f-label { display:block; font-size:13px; color:#333; font-weight:600;
                   margin-bottom:6px; margin-top:14px; }
        .form-control { width:100%; height:45px; padding:10px 12px;
                        border:1px solid #ccd7ec; border-radius:6px; font-size:14px; }
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

    <div class="fp-container">
        <div class="fp-box">

            <asp:Panel ID="pnlRequest" runat="server">
                <h2 class="fp-title">Forgot Password?</h2>
                <p class="fp-subtitle">
                    Enter the email address linked to your account.
                    We'll send you a 6-digit code to reset your password.
                </p>

                <asp:Label ID="lblMsg" runat="server" CssClass="message-label" Visible="false" />

                <label class="f-label">Email Address</label>
                <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control"
                    TextMode="Email" placeholder="you@email.com" MaxLength="200" />

                <asp:Button ID="btnSendCode" runat="server" Text="Send Reset Code"
                    CssClass="btn-primary" OnClick="btnSendCode_Click" />

                <a href="Login.aspx" class="bottom-link">&larr; Back to Login</a>
            </asp:Panel>

            <asp:Panel ID="pnlSent" runat="server" Visible="false">
                <h2 class="fp-title">Check Your Inbox</h2>
                <p class="fp-subtitle">
                    If <strong><asp:Literal ID="litSentEmail" runat="server" /></strong>
                    is registered with us, you'll receive a 6-digit reset code
                    within a minute. It expires in 15 minutes.
                </p>
                <p class="fp-subtitle">
                    Didn't get it? Check your spam folder, or
                    <asp:LinkButton ID="btnRetry" runat="server" OnClick="btnRetry_Click">try another email</asp:LinkButton>.
                </p>
                <a href="VerifyResetCode.aspx?email=<%= Server.UrlEncode(litSentEmail.Text) %>"
                   class="btn-primary"
                   style="display:block; text-align:center; line-height:46px; text-decoration:none;">
                    I Have a Code &rarr;
                </a>
            </asp:Panel>

        </div>
    </div>

</asp:Content>