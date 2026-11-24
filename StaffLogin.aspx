 <%@ Page Title="Staff Login" Language="C#" AutoEventWireup="true" CodeBehind="StaffLogin.aspx.cs" Inherits="WebApplicationM3.StaffLogin" %>

<!DOCTYPE html>

<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Staff Login - IKHWEZI Driving School</title>
    <style>
        * { box-sizing: border-box; margin: 0; padding: 0; }
        body { font-family: 'Segoe UI', Arial, sans-serif; background: linear-gradient(180deg, #e9f1fc, #d9e5f5); min-height: 100vh; display: flex; align-items: center; justify-content: center; padding: 20px; }
        .card { background: #fff; border-radius: 14px; box-shadow: 0 12px 34px rgba(13,43,110,.14); overflow: hidden; width: 400px; max-width: 100%; }
        .head { background: #0d2b6e; color: #fff; padding: 26px 28px 22px; border-bottom: 3px solid #e8283f; }
        .brand { font-size: 26px; font-weight: 800; font-style: italic; letter-spacing: 1px; }
        .brand span { color: #ff5a6e; }
        .sub { font-size: 9px; letter-spacing: 4px; color: #c3d2ee; margin-top: 3px; }
        .head h1 { font-size: 17px; font-weight: 700; margin-top: 16px; }
        .head p { font-size: 12.5px; color: #c3d2ee; margin-top: 3px; }
        .body { padding: 26px 28px 30px; }
        .f-label { display: block; font-size: 12.5px; color: #444; font-weight: 600; margin-bottom: 5px; }
        .f-input { width: 100%; padding: 11px 13px; border: 1px solid #ccd7ec; border-radius: 6px; font-size: 14px; margin-bottom: 16px; }
        .f-input:focus { outline: none; border-color: #e8283f; }
        .btn { width: 100%; background: #e8283f; color: #fff; border: none; border-radius: 22px; padding: 12px; font-size: 14.5px; font-weight: 700; cursor: pointer; }
        .btn:hover { background: #c81e33; }
        .err { color: #c81e33; font-weight: 600; font-size: 13px; display: block; margin-bottom: 14px; }
        .ok { color: #14663f; font-weight: 600; font-size: 13px; display: block; margin-bottom: 14px; }
        .hint { font-size: 11.5px; color: #99a3b5; margin-top: 16px; text-align: center; }
        .hint a { color: #0d2b6e; font-weight: 700; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="card">
            <div class="head">
                <div class="brand">IKHWEZI<span>.</span></div>
                <div class="sub">DRIVING SCHOOL</div>
                <h1>Staff Login</h1>
                <p>Instructors &middot; Reception &middot; Management</p>
            </div>
            <div class="body">
                <asp:Label ID="lblMsg" runat="server" Visible="false" />

                <label class="f-label" for="txtEmail">Staff Email</label>
                <asp:TextBox ID="txtEmail" runat="server" CssClass="f-input" TextMode="Email"
                    placeholder="e.g. sarah@ikhwezi.com" />

                <label class="f-label" for="txtPassword">Password</label>
                <asp:TextBox ID="txtPassword" runat="server" CssClass="f-input" TextMode="Password" />

                <asp:Button ID="btnLogin" runat="server" Text="Sign In" CssClass="btn" OnClick="btnLogin_Click" />

                <div class="hint">
                    Student? <a runat="server" href="~/Account/Login.aspx">Use the student login</a>
                </div>
            </div>
        </div>
    </form>
</body>
</html>