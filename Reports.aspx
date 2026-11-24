 
<%@ Page Title="IKHWEZI Reports"
    Language="C#"
    MasterPageFile="~/Site.master"
    AutoEventWireup="true"
    CodeBehind="Reports.aspx.cs"
    Inherits="WebApplicationM3.Reports" %>

<asp:Content ID="Content1"
    ContentPlaceHolderID="MainContent"
    runat="server">

<style>
    body {
        background: #0B0F19 !important;
        padding-top: 0 !important;
    }

    .navbar-khwezi,
    .site-footer,
    #waFloat,
    #kwChatLauncher,
    #kwChatPanel {
        display: none !important;
    }

    .body-content {
        padding: 0 !important;
        max-width: 100% !important;
        width: 100% !important;
    }

    .reports-wrapper {
        min-height: 100vh;
        background: #0B0F19;
        color: #F3F4F6;
        padding: 30px;
        font-family: 'Segoe UI', Arial, sans-serif;
    }

    .reports-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        flex-wrap: wrap;
        gap: 15px;
        margin-bottom: 25px;
    }

    .reports-title {
        font-size: 27px;
        font-weight: 700;
        color: white;
        margin: 0;
    }

    .reports-subtitle {
        color: #9CA3AF;
        margin-top: 8px;
        font-size: 14px;
    }

    .reports-back {
        background: #1E2538;
        border: 1px solid #2A324B;
        color: white;
        padding: 12px 18px;
        border-radius: 8px;
        text-decoration: none;
        font-weight: 600;
    }

    .reports-back:hover {
        background: #29334C;
        color: white;
        text-decoration: none;
    }

    .reports-card {
        background: #151A28;
        border: 1px solid #2A324B;
        border-radius: 16px;
        padding: 20px;
    }

    .reports-card-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 12px;
        flex-wrap: wrap;
        margin-bottom: 18px;
    }

    .reports-card-title {
        font-size: 18px;
        font-weight: 600;
        margin: 0;
    }

    .reports-badge {
        color: #FACC15;
        font-size: 13px;
        font-weight: 600;
    }

    .reports-frame {
        width: 100%;
        height: 80vh;
        min-height: 550px;
        border: none;
        border-radius: 10px;
        background: white;
    }

    .reports-footer {
        margin-top: 20px;
        color: #9CA3AF;
        font-size: 13px;
        text-align: center;
    }

    @media (max-width: 768px) {
        .reports-wrapper {
            padding: 15px;
        }

        .reports-title {
            font-size: 22px;
        }

        .reports-card {
            padding: 10px;
        }

        .reports-frame {
            min-height: 450px;
        }
    }
</style>

<div class="reports-wrapper">

    <div class="reports-header">

        <div>
            <h1 class="reports-title">
                IKHWEZI Business Intelligence
            </h1>

            <div class="reports-subtitle">
                Microsoft Power BI |
                Management Analytics
            </div>
        </div>

        <a href="ManagerDashboard.aspx"
           class="reports-back">
            Back to Manager Dashboard
        </a>

    </div>

    <div class="reports-card">

        <div class="reports-card-header">

            <h2 class="reports-card-title">
                Management Reports
            </h2>

            <span class="reports-badge">
                Powered by Microsoft Power BI
            </span>

        </div>

        <iframe
            id="powerBIFrame"
            runat="server"
            class="reports-frame"
            title="IKHWEZI Power BI Reports"
            allowfullscreen="true">
        </iframe>

    </div>

    <div class="reports-footer">
        IKHWEZI Driving School |
        Business Intelligence &amp; Analytics
    </div>

</div>

</asp:Content>
