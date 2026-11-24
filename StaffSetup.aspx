 <%@ Page Title="Set Up Account" Language="C#" AutoEventWireup="true" CodeBehind="StaffSetup.aspx.cs" Inherits="WebApplicationM3.StaffSetup" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>IKHWEZI - Setup Account</title>
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap" rel="stylesheet">
    <style>
        body { font-family: 'Inter', sans-serif; background: #0B0F19; display: flex; align-items: center; justify-content: center; min-height: 100vh; color: #F3F4F6; margin: 0; }
        .card { background: #151A28; border-radius: 16px; padding: 40px; width: 100%; max-width: 420px; box-shadow: 0 10px 30px rgba(0,0,0,0.5); border-top: 4px solid #22b573; }
        h2 { color: #fff; margin-bottom: 10px; font-size: 24px; text-align: center; }
        p { color: #9CA3AF; font-size: 14px; text-align: center; margin-bottom: 24px; }
        .form-group { margin-bottom: 20px; }
        label { display: block; font-size: 12px; font-weight: 700; color: #9CA3AF; text-transform: uppercase; margin-bottom: 8px; }
        input { width: 100%; padding: 12px 16px; border: 1px solid #2A324B; border-radius: 8px; background: #0B0F19; color: #fff; font-size: 14px; outline: none; }
        input:focus { border-color: #22b573; }
        .btn { width: 100%; padding: 14px; background: #22b573; color: #fff; border: none; border-radius: 8px; font-weight: 700; font-size: 15px; cursor: pointer; margin-top: 10px; }
        .btn:hover { background: #1d9e63; }
        .msg { padding: 14px; border-radius: 8px; margin-bottom: 20px; font-size: 14px; text-align: center; font-weight: 600; }
        .msg.ok { background: rgba(34, 181, 115, 0.1); color: #22b573; border: 1px solid #22b573; }
        .msg.err { background: rgba(232, 40, 63, 0.1); color: #e8283f; border: 1px solid #e8283f; }
        .welcome-name { color: #22b573; font-weight: 700; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="card">
            
            <asp:Panel ID="pnlError" runat="server" Visible="false">
                <h2 style="color:#e8283f;">Invalid Link</h2>
                <p>This invitation link is invalid or has expired. Please contact your manager to generate a new one.</p>
            </asp:Panel>

            <asp:Panel ID="pnlSetup" runat="server" Visible="false">
                <h2>Welcome, <span class="welcome-name"><asp:Literal ID="litName" runat="server" /></span></h2>
                <p>You've been invited to join IKHWEZI as an <strong><asp:Literal ID="litRole" runat="server" /></strong>. Create your password to activate your account.</p>

                <asp:Label ID="lblMsg" runat="server" Visible="false" CssClass="msg" />

                <div class="form-group">
                    <label>Email Address (Locked)</label>
                    <input type="text" value="<%= HiddenEmail.Value %>" disabled style="opacity:0.5;" />
                    <asp:HiddenField ID="HiddenEmail" runat="server" />
                </div>

                <div class="form-group">
                    <label>New Password (Min 8 chars)</label>
                    <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" />
                </div>

                <div class="form-group">
                    <label>Confirm Password</label>
                    <asp:TextBox ID="txtConfirm" runat="server" TextMode="Password" />
                </div>

                <asp:Button ID="btnSetPassword" runat="server" Text="Activate My Account" CssClass="btn" OnClick="btnSetPassword_Click" />
            </asp:Panel>

        </div>
    </form>
</body>
</html>