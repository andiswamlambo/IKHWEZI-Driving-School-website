  <%@ Page Title="Sign Up" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SignUp.aspx.cs" Inherits="WebApplicationM3.Account.Register" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    
    <style>
        .signup-container {
            background: #e6f2ff;
            min-height: 80vh;
            display: flex;
            align-items: center;
            justify-content: center;
            border-radius: 8px;
            padding: 20px;
            margin-top: 20px;
        }
        .signup-box {
            background: white;
            padding: 40px;
            border-radius: 8px;
            box-shadow: 0 4px 15px rgba(0,0,0,0.1);
            width: 450px;
            text-align: center;
        }
        .signup-title { 
            color: #003366; 
            margin-bottom: 10px; 
            font-weight: 700;
        }
        .signup-subtitle {
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
        .btn-signup {
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
        .btn-signup:hover { background-color: #0056b3; }
        .message-label { margin-top: 15px; font-weight: bold; display: block; }
        .login-link {
            display: block;
            margin-top: 20px;
            color: #007bff;
            text-decoration: none;
        }
        .login-link:hover {
            text-decoration: underline;
        }
        .text-danger {
            color: red;
            font-size: 0.9em;
            display: block;
            text-align: left;
            margin-left: 5px;
            margin-bottom: 5px;
        }
    </style>

    <div class="signup-container">
        <div class="signup-box">
            <h2 class="signup-title">Create Student Account</h2>
            <p class="signup-subtitle">Join Khwezi Driving School today.</p>
            
            <!-- Error Message Display -->
            <asp:Label ID="ErrorMessage" runat="server" CssClass="text-danger" Visible="false"></asp:Label>

            <!-- Full Name -->
            <asp:TextBox ID="txtFullName" runat="server" CssClass="form-control" placeholder="Full Name"></asp:TextBox>
            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtFullName" CssClass="text-danger" ErrorMessage="Full name is required." Display="Dynamic" />

            <!-- Email -->
            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" placeholder="Email Address" TextMode="Email"></asp:TextBox>
            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEmail" CssClass="text-danger" ErrorMessage="Email is required." Display="Dynamic" />

            <!-- Password -->
            <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" placeholder="Password" TextMode="Password"></asp:TextBox>
            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPassword" CssClass="text-danger" ErrorMessage="Password is required." Display="Dynamic" />

            <!-- Confirm Password -->
            <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="form-control" placeholder="Confirm Password" TextMode="Password"></asp:TextBox>
            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtConfirmPassword" CssClass="text-danger" Display="Dynamic" ErrorMessage="Confirm password is required." />
            <asp:CompareValidator runat="server" ControlToCompare="txtPassword" ControlToValidate="txtConfirmPassword" CssClass="text-danger" Display="Dynamic" ErrorMessage="Passwords do not match." />

            <!-- Button -->
            <asp:Button ID="btnRegister" runat="server" Text="Register" CssClass="btn-signup" OnClick="btnRegister_Click" />
            
            <a href="Login.aspx" class="login-link">Already have an account? Login here.</a>
        </div>
    </div>

</asp:Content>