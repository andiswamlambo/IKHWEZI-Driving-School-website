 <%@ Page Title="Login" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="WebApplicationM3.Account.Login" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .login-container {
            background: #e6f2ff;
            min-height: 80vh;
            display: flex;
            align-items: center;
            justify-content: center;
            border-radius: 8px;
            padding: 20px;
            margin-top: 20px;
        }
        .login-box {
            background: white;
            padding: 40px;
            border-radius: 8px;
            box-shadow: 0 4px 15px rgba(0,0,0,0.1);
            width: 400px;
            text-align: center;
        }
        .login-title { 
            color: #003366; 
            margin-bottom: 10px; 
            font-weight: 700;
        }
        .login-subtitle {
            color: #555;
            margin-bottom: 30px;
            font-size: 1.1em;
        }
        .form-control { 
            margin-bottom: 15px; 
            height: 45px; 
            border-radius: 5px; 
            width: 100%;
        }
        .btn-login {
            background-color: #007bff; 
            color: white; 
            width: 100%; 
            height: 45px;
            font-size: 1.2em; 
            border: none; 
            border-radius: 5px; 
            font-weight: 600;
            margin-top: 10px;
        }
        .btn-login:hover { background-color: #0056b3; }
        .message-label { 
            margin-top: 15px; 
            font-weight: bold; 
            display: block; 
        }
        .signup-link {
            display: block;
            margin-top: 20px;
            color: #007bff;
            text-decoration: none;
        }
        .signup-link:hover {
            text-decoration: underline;
        }
        .forgot-link {
            display: block;
            margin-top: 16px;
            color: #e8283f;
            text-decoration: none;
            font-size: 14px;
            font-weight: 600;
        }
        .forgot-link:hover {
            text-decoration: underline;
        }
    </style>

    <div class="login-container">
        <div class="login-box">
            <h2 class="login-title">Welcome Back</h2>
            <p class="login-subtitle">Login to your Khwezi Driving School account.</p>
            
            <!-- Error Message Display -->
            <asp:Label ID="lblMessage" runat="server" CssClass="message-label" ForeColor="Red"></asp:Label>

            <!-- Email -->
            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" placeholder="Email Address" TextMode="Email"></asp:TextBox>
            
            <!-- Password -->
            <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" placeholder="Password" TextMode="Password"></asp:TextBox>
            
            <!-- Button -->
            <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="btn-login" OnClick="btnLogin_Click" />
            
            <!-- Forgot password -->
            <a href="ForgotPassword.aspx" class="forgot-link">Forgot your password?</a>
            
            <!-- Sign up -->
            <a href="SignUp.aspx" class="signup-link">Don't have an account? Sign up here.</a>
        </div>
    </div>

</asp:Content>