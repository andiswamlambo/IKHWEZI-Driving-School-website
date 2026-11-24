 <%@ Page Title="Reset Password" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ResetPassword.aspx.cs" Inherits="WebApplicationM3.Account.ResetPassword" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .rp-container { background:#e6f2ff; min-height:80vh; display:flex;
                        align-items:center; justify-content:center;
                        border-radius:8px; padding:20px; margin-top:20px; }
        .rp-box { background:#fff; padding:40px; border-radius:8px;
                  box-shadow:0 4px 15px rgba(0,0,0,.1); width:440px; }
        .rp-title { color:#003366; margin-bottom:10px; font-weight:700; text-align:center; }
        .rp-subtitle { color:#555; margin-bottom:26px; font-size:14px;
                       text-align:center; line-height:1.5; }
        .f-label { display:block; font-size:13px; color:#333; font-weight:600;
                   margin-bottom:6px; margin-top:14px; }
        .form-control { width:100%; height:46px; padding:10px 12px;
                        border:1px solid #ccd7ec; border-radius:6px; font-size:14px; }
        .form-control:focus { outline:none; border-color:#007bff; }
        .btn-primary { background:#007bff; color:#fff; width:100%; height:46px;
                       font-size:15px; border:none; border-radius:6px;
                       font-weight:700; margin-top:22px; cursor:pointer; }
        .btn-primary:hover { background:#0056b3; }
        .message-label { display:block; margin-top:14px; font-weight:600;
                         font-size:13.5px; text-align:center; }
        .hint { font-size:11.5px; color:#888; margin-top:4px; }
    </style>

    <div class="rp-container">
        <div class="rp-box">
            <h2 class="rp-title">Set a New Password</h2>
            <p class="rp-subtitle">
                Choose a strong password. It must be at least 8 characters long
                and contain letters and numbers.
            </p>

            <asp:Label ID="lblMsg" runat="server" CssClass="message-label" Visible="false" />

            <label class="f-label">New Password</label>
            <asp:TextBox ID="txtNew" runat="server" CssClass="form-control"
                TextMode="Password" MaxLength="100" />
            <div class="hint">Minimum 8 characters, letters and numbers.</div>

            <label class="f-label">Confirm New Password</label>
            <asp:TextBox ID="txtConfirm" runat="server" CssClass="form-control"
                TextMode="Password" MaxLength="100" />

            <asp:Button ID="btnReset" runat="server" Text="Reset Password"
                CssClass="btn-primary" OnClick="btnReset_Click" />
        </div>
    </div>

</asp:Content>