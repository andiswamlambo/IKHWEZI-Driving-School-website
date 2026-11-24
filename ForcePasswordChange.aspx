 <%@ Page Title="Update Password" Language="C#" AutoEventWireup="true" CodeBehind="ForcePasswordChange.aspx.cs" Inherits="WebApplicationM3.ForcePasswordChange" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>IKHWEZI - Update Password</title>
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap" rel="stylesheet">
    <style>
        body { font-family: 'Inter', sans-serif; background: #0B0F19; display: flex; align-items: center; justify-content: center; min-height: 100vh; color: #F3F4F6; margin: 0; padding: 20px; box-sizing: border-box; }
        .card { background: #151A28; border-radius: 16px; padding: 40px; width: 100%; max-width: 420px; box-shadow: 0 10px 30px rgba(0,0,0,0.5); border-top: 4px solid #e8890c; }
        h2 { color: #fff; margin-bottom: 10px; font-size: 24px; text-align: center; }
        p { color: #9CA3AF; font-size: 14px; text-align: center; margin-bottom: 24px; line-height: 1.5; }
        .form-group { margin-bottom: 20px; }
        label { display: block; font-size: 12px; font-weight: 700; color: #9CA3AF; text-transform: uppercase; margin-bottom: 8px; }
        input { width: 100%; padding: 12px 16px; border: 1px solid #2A324B; border-radius: 8px; background: #0B0F19; color: #fff; font-size: 14px; outline: none; box-sizing: border-box; }
        input:focus { border-color: #e8890c; }
        .btn { width: 100%; padding: 14px; background: #22b573; color: #fff; border: none; border-radius: 8px; font-weight: 700; font-size: 15px; cursor: pointer; margin-top: 10px; transition: 0.2s; }
        .btn:hover { background: #1d9e63; }
        .msg { padding: 14px; border-radius: 8px; margin-bottom: 20px; font-size: 14px; text-align: center; font-weight: 600; }
        .msg.ok { background: rgba(34, 181, 115, 0.1); color: #22b573; border: 1px solid #22b573; }
        .msg.err { background: rgba(232, 40, 63, 0.1); color: #e8283f; border: 1px solid #e8283f; }
        .hint { font-size: 12px; color: #9CA3AF; margin-top: -10px; margin-bottom: 15px; display: block; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="card">
            <h2>Action Required</h2>
            <p>Your password was recently reset by management. Please create a new, secure password to access your dashboard.</p>

            <asp:Label ID="lblMsg" runat="server" Visible="false" CssClass="msg" />

            <div class="form-group">
                <label>New Password (Min 8 chars)</label>
                <asp:TextBox ID="txtNewPwd" runat="server" TextMode="Password" />
                <span class="hint">Use a mix of letters, numbers, and symbols for security.</span>
            </div>
            <div class="form-group">
                <label>Confirm New Password</label>
                <asp:TextBox ID="txtConfirmPwd" runat="server" TextMode="Password" />
            </div>

            <asp:Button ID="btnSave" runat="server" Text="Save Password & Continue" CssClass="btn" OnClick="btnSave_Click" />
        </div>
    </form>
</body>
</html>