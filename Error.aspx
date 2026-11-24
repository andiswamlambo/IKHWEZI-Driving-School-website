 <%@ Page Title="Oops! Something went wrong" Language="C#" MasterPageFile="" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="WebApplicationM3.Error" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>IKHWEZI - Error</title>
    <style>
        * { box-sizing: border-box; margin: 0; padding: 0; }
        body { font-family: 'Segoe UI', Arial, sans-serif; background: linear-gradient(180deg, #e9f1fc, #d9e5f5); min-height: 100vh; display: flex; align-items: center; justify-content: center; padding: 20px; }
        
        .error-card { 
            background: #fff; 
            border-radius: 12px; 
            box-shadow: 0 10px 30px rgba(13,43,110,.15); 
            max-width: 520px; 
            width: 100%; 
            overflow: hidden; 
            text-align: center;
        }
        
        .error-header { 
            background: #0d2b6e; 
            padding: 30px 20px; 
            color: #fff; 
        }
        
        .error-header h1 { 
            font-size: 24px; 
            font-weight: 800; 
            letter-spacing: 1px;
        }
        
        .error-body { 
            padding: 40px 30px; 
        }
        
        .icon-circle { 
            width: 80px; 
            height: 80px; 
            background: #fdf0f1; 
            border-radius: 50%; 
            display: flex; 
            align-items: center; 
            justify-content: center; 
            margin: 0 auto 20px; 
            color: #e8283f;
        }
        
        .icon-circle svg { 
            width: 40px; 
            height: 40px; 
            stroke: currentColor; 
            /* FIXED: Changed from 2.5 to 2 to remove VS warning */
            stroke-width: 2; 
            fill: none; 
            stroke-linecap: round; 
            stroke-linejoin: round; 
        }
        
        .error-title { 
            color: #0d2b6e; 
            font-size: 20px; 
            font-weight: 700; 
            margin-bottom: 12px; 
        }
        
        .error-text { 
            color: #52647c; 
            font-size: 14px; 
            line-height: 1.6; 
            margin-bottom: 25px; 
        }
        
        .ref-box { 
            background: #f7faff; 
            border: 1px dashed #b8d4f0; 
            border-radius: 8px; 
            padding: 15px; 
            margin-bottom: 25px; 
            font-size: 13px; 
            color: #0d2b6e; 
        }
        
        .ref-box strong { 
            font-family: monospace; 
            font-size: 14px; 
            color: #e8283f; 
            display: block; 
            margin-top: 5px; 
        }
        
        .btn-group { 
            display: flex; 
            gap: 12px; 
            justify-content: center; 
            flex-wrap: wrap; 
        }
        
        .btn { 
            padding: 12px 30px; 
            border-radius: 24px; 
            font-weight: 700; 
            font-size: 14px; 
            text-decoration: none; 
            cursor: pointer; 
            border: none; 
            transition: 0.2s;
        }
        
        .btn-primary { 
            background: #22b573; 
            color: #fff; 
        }
        
        .btn-primary:hover { 
            background: #1d9e63; 
        }
        
        .btn-secondary { 
            background: #a9b4c4; 
            color: #fff; 
        }
        
        .btn-secondary:hover { 
            background: #8d99ac; 
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="error-card">
            <div class="error-header">
                <h1>IKHWEZI DRIVING SCHOOL</h1>
            </div>
            <div class="error-body">
                <div class="icon-circle">
                    <svg viewBox="0 0 24 24">
                        <circle cx="12" cy="12" r="10"></circle>
                        <line x1="12" y1="8" x2="12" y2="12"></line>
                        <line x1="12" y1="16" x2="12.01" y2="16"></line>
                    </svg>
                </div>
                
                <h2 class="error-title">Something went wrong</h2>
                <p class="error-text">
                    We apologize, but an unexpected error occurred while processing your request. 
                    Our technical team has been notified and is looking into it.
                </p>

                <div class="ref-box" runat="server" id="pnlRef" visible="false">
                    Please provide this reference number to reception if you need help:
                    <strong><asp:Literal ID="litErrorId" runat="server" /></strong>
                </div>

                <div class="btn-group">
                    <asp:Button ID="btnGoHome" runat="server" Text="Go to Home" CssClass="btn btn-secondary" OnClick="btnGoHome_Click" />
                    <asp:Button ID="btnGoDashboard" runat="server" Text="My Dashboard" CssClass="btn btn-primary" OnClick="btnGoDashboard_Click" />
                </div>
            </div>
        </div>
    </form>
</body>
</html>