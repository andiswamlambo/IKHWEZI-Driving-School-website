 
<%@ Page Title="Notify Staff"
    Language="C#"
    MasterPageFile="~/Site.master"
    AutoEventWireup="true"
    CodeBehind="StaffNotify.aspx.cs"
    Inherits="WebApplicationM3.Account.StaffNotify" %>

<asp:Content ID="Main"
    ContentPlaceHolderID="MainContent"
    runat="server">

<link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap"
      rel="stylesheet" />

<style>
    /* ==========================================
       1. MASTER PAGE OVERRIDES
       ========================================== */

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

    /* ==========================================
       2. MAIN LAYOUT
       ========================================== */

    .ik {
        display: flex;
        min-height: 100vh;
        width: 100%;
        background: #0B0F19;
        color: #F3F4F6;
        font-family: 'Inter', 'Segoe UI', Arial, sans-serif;
    }

    /* ==========================================
       3. SIDEBAR - MANAGER DASHBOARD STYLE
       ========================================== */

    .ik aside {
        width: 260px;
        background: #151A28;
        border-right: 1px solid #2A324B;
        padding: 24px 16px;
        display: flex;
        flex-direction: column;
        flex-shrink: 0;
        position: sticky;
        top: 0;
        height: 100vh;
        z-index: 10;
    }

    .ik .logo {
        font-size: 24px;
        font-weight: 800;
        font-style: italic;
        color: #FFFFFF;
        letter-spacing: 1px;
        margin: 0 0 30px;
        padding-left: 10px;
    }

    .ik .logo span {
        color: #E8283F;
    }

    .ik .staff-nav {
        display: flex;
        flex-direction: column;
        gap: 6px;
        flex: 1;
    }

    .ik .staff-nav a {
        display: flex;
        align-items: center;
        gap: 12px;
        padding: 12px 16px;
        border-radius: 8px;
        color: #9CA3AF;
        font-size: 14px;
        font-weight: 500;
        text-decoration: none;
        transition: background 0.2s, color 0.2s;
    }

    .ik .staff-nav a:hover {
        background: #1E2538;
        color: #FFFFFF;
        text-decoration: none;
    }

    .ik .staff-nav a.active {
        background: linear-gradient(
            90deg,
            rgba(26, 65, 153, 0.2),
            transparent
        );
        color: #FFFFFF;
        border-left: 3px solid #1A4199;
    }

    .ik .staff-nav svg {
        width: 18px;
        height: 18px;
        fill: none;
        stroke: currentColor;
        stroke-width: 2;
        stroke-linecap: round;
        stroke-linejoin: round;
        flex-shrink: 0;
    }

    /* ==========================================
       4. MAIN CONTENT
       ========================================== */

    .ik main {
        flex: 1;
        padding: 30px 40px;
        min-width: 0;
    }

    .ik h1 {
        font-size: 27px;
        font-weight: 700;
        margin: 0 0 8px;
        color: #FFFFFF;
    }

    .ik .muted {
        color: #9CA3AF;
        font-size: 14px;
        margin: 0;
    }

    /* ==========================================
       5. FORM CONTAINER
       ========================================== */

    .ik section {
        background: #151A28;
        border: 1px solid #30384D;
        border-radius: 14px;
        padding: 28px;
        margin-top: 25px;
        max-width: 950px;
    }

    .ik .field {
        margin-bottom: 22px;
    }

    .ik .field-label {
        display: block;
        margin-bottom: 9px;
        color: #CBD5E1;
        font-weight: 600;
        font-size: 14px;
    }

    /* ==========================================
       6. INPUTS
       ========================================== */

    .ik input[type="text"],
    .ik textarea,
    .ik select {
        width: 100%;
        max-width: 100%;
        background: #0B0F19;
        color: #FFFFFF;
        border: 1px solid #475569;
        padding: 12px 14px;
        border-radius: 8px;
        font-family: inherit;
        font-size: 14px;
        outline: none;
    }

    .ik input[type="text"]:focus,
    .ik textarea:focus,
    .ik select:focus {
        border-color: #8B5CF6;
        box-shadow: 0 0 0 2px rgba(139, 92, 246, 0.15);
    }

    .ik textarea {
        min-height: 190px;
        resize: vertical;
    }

    /* ==========================================
       7. BUTTONS
       ========================================== */

    .ik .btn {
        background: #8B5CF6;
        color: #FFFFFF;
        padding: 12px 20px;
        border: none;
        border-radius: 8px;
        font-size: 14px;
        font-weight: 600;
        cursor: pointer;
        transition: background 0.2s;
    }

    .ik .btn:hover {
        background: #7C3AED;
        color: #FFFFFF;
    }

    .ik .btn-secondary {
        background: #252D40;
        border: 1px solid #475569;
    }

    .ik .btn-secondary:hover {
        background: #374151;
    }

    .ik .actions {
        display: flex;
        gap: 12px;
        flex-wrap: wrap;
        margin-top: 12px;
    }

    /* ==========================================
       8. MESSAGES AND PREVIEW
       ========================================== */

    .ik .msg {
        display: block;
        color: #A7F3D0;
        margin: 12px 0;
        font-size: 14px;
    }

    .ik .err {
        color: #FCA5A5;
    }

    .ik .preview {
        margin-top: 15px;
        color: #CBD5E1;
        font-size: 14px;
        overflow-wrap: anywhere;
    }

    .ik .info {
        background: #1B2435;
        border: 1px solid #30384D;
        border-radius: 9px;
        padding: 14px;
        margin-bottom: 24px;
        color: #AEBBD0;
        font-size: 13px;
        line-height: 1.6;
    }

    .ik .required {
        color: #F87171;
    }

    /* ==========================================
       9. RESPONSIVE DESIGN
       ========================================== */

    @media (max-width: 950px) {

        .ik {
            flex-direction: column;
        }

        .ik aside {
            width: 100%;
            height: auto;
            position: relative;
            border-right: none;
            border-bottom: 1px solid #2A324B;
            padding: 15px;
        }

        .ik .logo {
            text-align: center;
            padding-left: 0;
            margin-bottom: 15px;
        }

        .ik .staff-nav {
            flex-direction: row;
            flex-wrap: wrap;
            justify-content: center;
            gap: 8px;
        }

        .ik .staff-nav a {
            padding: 8px 12px;
            font-size: 13px;
        }

        .ik .staff-nav a.active {
            border-left: none;
            border-bottom: 2px solid #1A4199;
        }

        .ik main {
            padding: 20px 15px;
        }
    }

    @media (max-width: 750px) {

        .ik section {
            padding: 20px;
        }

        .ik h1 {
            font-size: 23px;
        }
    }
</style>

<div class="ik">

    <!-- ======================================
         SIDEBAR
         ====================================== -->

    <aside>

        <div class="logo">
            IKHWEZI<span>.</span>
        </div>

        <nav class="staff-nav">

            <!-- OVERVIEW -->
            <a href="<%= ResolveUrl("~/ManagerDashboard.aspx") %>">
                <svg viewBox="0 0 24 24">
                    <rect x="3" y="3" width="7" height="7"></rect>
                    <rect x="14" y="3" width="7" height="7"></rect>
                    <rect x="14" y="14" width="7" height="7"></rect>
                    <rect x="3" y="14" width="7" height="7"></rect>
                </svg>
                Overview
            </a>

            <!-- STUDENTS -->
            <a href="<%= ResolveUrl("~/ManagerStudents.aspx") %>">
                <svg viewBox="0 0 24 24">
                    <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
                    <circle cx="9" cy="7" r="4"></circle>
                    <path d="M23 21v-2a4 4 0 0 0-3-3.87"></path>
                    <path d="M16 3.13a4 4 0 0 1 0 7.75"></path>
                </svg>
                Students
            </a>

            <!-- BOOKINGS -->
            <a href="<%= ResolveUrl("~/AdminDashboard.aspx") %>">
                <svg viewBox="0 0 24 24">
                    <rect x="3" y="4" width="18" height="18" rx="2"></rect>
                    <line x1="16" y1="2" x2="16" y2="6"></line>
                    <line x1="8" y1="2" x2="8" y2="6"></line>
                    <line x1="3" y1="10" x2="21" y2="10"></line>
                </svg>
                Bookings
            </a>

            <!-- REGISTER STAFF -->
            <a href="<%= ResolveUrl("~/ManagerRegisterStaff.aspx") %>">
                <svg viewBox="0 0 24 24">
                    <path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
                    <circle cx="8.5" cy="7" r="4"></circle>
                    <line x1="20" y1="8" x2="20" y2="14"></line>
                    <line x1="23" y1="11" x2="17" y2="11"></line>
                </svg>
                Register Staff
            </a>

            <!-- SYSTEM CONTROLS -->
            <a href="<%= ResolveUrl("~/ManagerOverrides.aspx") %>">
                <svg viewBox="0 0 24 24">
                    <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"></path>
                </svg>
                System Controls
            </a>

            <!-- NOTIFY STAFF -->
            <a href="<%= ResolveUrl("~/Account/StaffNotify.aspx") %>"
               class="active">
                <svg viewBox="0 0 24 24">
                    <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"></path>
                    <polyline points="22,6 12,13 2,6"></polyline>
                </svg>
                Notify Staff
            </a>

            <!-- REPORTS -->
            <a href="<%= ResolveUrl("~/Reports.aspx") %>">
                <svg viewBox="0 0 24 24">
                    <polyline points="23 6 13.5 15.5 8.5 10.5 1 18"></polyline>
                    <polyline points="17 6 23 6 23 12"></polyline>
                </svg>
                Reports
            </a>

            <!-- MY PROFILE -->
            <a href="<%= ResolveUrl("~/ManagerPersonalInfo.aspx") %>">
                <svg viewBox="0 0 24 24">
                    <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path>
                    <circle cx="12" cy="7" r="4"></circle>
                </svg>
                My Profile
            </a>

        </nav>

    </aside>

    <!-- ======================================
         MAIN CONTENT - ORIGINAL FUNCTIONALITY
         ====================================== -->

    <main>

        <h1>Notify Staff</h1>

        <p class="muted">
            Send announcements and important updates
            to IKHWEZI Driving School employees.
        </p>

        <section>

            <!-- RESULT MESSAGE -->
            <asp:Label
                ID="lblResult"
                runat="server"
                CssClass="msg"
                EnableViewState="false" />

            <div class="info">
                Select your recipients, enter the email subject
                and message, then click Send announcement.
                You can preview the recipients before sending.
            </div>

            <!-- RECIPIENTS -->
            <div class="field">

                <label class="field-label">
                    Recipients
                    <span class="required">*</span>
                </label>

                <asp:DropDownList
                    ID="ddlGroup"
                    runat="server"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="GroupChanged">

                    <asp:ListItem Value="All">
                        All active staff
                    </asp:ListItem>

                    <asp:ListItem Value="Instructor">
                        Instructors only
                    </asp:ListItem>

                    <asp:ListItem Value="Receptionist">
                        Receptionists only
                    </asp:ListItem>

                    <asp:ListItem Value="Individual">
                        Individual staff member
                    </asp:ListItem>

                </asp:DropDownList>

            </div>

            <!-- INDIVIDUAL STAFF -->
            <asp:Panel
                ID="pnlIndividual"
                runat="server"
                Visible="false">

                <div class="field">

                    <label class="field-label">
                        Select Employee
                    </label>

                    <asp:DropDownList
                        ID="ddlStaff"
                        runat="server" />

                </div>

            </asp:Panel>

            <!-- PREVIEW RECIPIENTS -->
            <div class="field">

                <asp:Button
                    ID="btnPreview"
                    runat="server"
                    Text="Preview recipients"
                    CssClass="btn btn-secondary"
                    OnClick="Preview_Click"
                    CausesValidation="false" />

                <div class="preview">
                    <asp:Literal
                        ID="litPreview"
                        runat="server" />
                </div>

            </div>

            <!-- EMAIL SUBJECT -->
            <div class="field">

                <label class="field-label">
                    Email Subject
                    <span class="required">*</span>
                </label>

                <asp:TextBox
                    ID="txtSubject"
                    runat="server"
                    MaxLength="160"
                    placeholder="Enter announcement subject" />

            </div>

            <!-- EMAIL MESSAGE -->
            <div class="field">

                <label class="field-label">
                    Message
                    <span class="required">*</span>
                </label>

                <asp:TextBox
                    ID="txtMessage"
                    runat="server"
                    TextMode="MultiLine"
                    Rows="8"
                    MaxLength="5000"
                    placeholder="Write your announcement here..." />

            </div>

            <!-- SEND BUTTON -->
            <div class="actions">

                <asp:Button
                    ID="btnSend"
                    runat="server"
                    Text="Send announcement"
                    CssClass="btn"
                    OnClick="Send_Click"
                    UseSubmitBehavior="false" />

            </div>

        </section>

    </main>

</div>

</asp:Content>
