 
<%@ Page Title="System Controls"
    Language="C#"
    MasterPageFile="~/Site.master"
    AutoEventWireup="true"
    CodeBehind="ManagerOverrides.aspx.cs"
    Inherits="WebApplicationM3.ManagerOverrides" %>

<asp:Content ID="Content1"
    ContentPlaceHolderID="MainContent"
    runat="server">

<link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap"
      rel="stylesheet" />

<style>
    /* MASTER PAGE */
    body {
        background-color: #0B0F19 !important;
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

    /* MAIN LAYOUT */
    .dash-wrapper {
        display: flex;
        min-height: 100vh;
        width: 100%;
        background: #0B0F19;
        color: #F3F4F6;
        font-family: 'Inter', 'Segoe UI', Arial, sans-serif;
    }

    /* SIDEBAR */
    .dash-sidebar {
        width: 260px;
        background: #151A28;
        border-right: 1px solid #2A324B;
        padding: 24px 16px;
        display: flex;
        flex-direction: column;
        flex-shrink: 0;
        height: 100vh;
        position: sticky;
        top: 0;
        z-index: 10;
    }

    .dash-logo {
        font-size: 24px;
        font-weight: 800;
        font-style: italic;
        letter-spacing: 1px;
        color: #FFFFFF;
        margin: 0 0 30px;
        padding-left: 10px;
    }

    .dash-logo span {
        color: #E8283F;
    }

    .dash-nav {
        display: flex;
        flex-direction: column;
        gap: 6px;
        flex: 1;
    }

    .dash-link {
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

    .dash-link:hover {
        background: #1E2538;
        color: #FFFFFF;
        text-decoration: none;
    }

    .dash-link.active {
        background: linear-gradient(
            90deg,
            rgba(26, 65, 153, 0.2),
            transparent
        );
        color: #FFFFFF;
        border-left: 3px solid #1A4199;
    }

    .dash-link svg {
        width: 18px;
        height: 18px;
        stroke: currentColor;
        stroke-width: 2;
        fill: none;
        stroke-linecap: round;
        stroke-linejoin: round;
        flex-shrink: 0;
    }

    /* MAIN CONTENT */
    .dash-main {
        flex: 1;
        min-width: 0;
        padding: 30px 40px;
    }

    .page-title {
        font-size: 26px;
        font-weight: 700;
        color: #FFFFFF;
        margin: 0 0 6px;
    }

    .page-sub {
        font-size: 14px;
        color: #9CA3AF;
        margin: 0 0 30px;
    }

    /* SYSTEM CONTROL CARDS */
    .override-grid {
        display: grid;
        grid-template-columns: 1fr;
        gap: 24px;
        max-width: 800px;
    }

    .override-card {
        background: #151A28;
        border-radius: 16px;
        border: 1px solid #2A324B;
        padding: 24px;
    }

    .override-card h3 {
        font-size: 18px;
        font-weight: 600;
        color: #FFFFFF;
        margin: 0 0 8px;
        display: flex;
        align-items: center;
        gap: 10px;
    }

    .override-card h3 svg {
        width: 22px;
        height: 22px;
        stroke: #1A4199;
        fill: none;
        stroke-width: 2;
        stroke-linecap: round;
        stroke-linejoin: round;
    }

    .override-card p {
        font-size: 13.5px;
        color: #9CA3AF;
        margin-bottom: 20px;
        line-height: 1.6;
    }

    /* FORM INPUTS */
    .input-group {
        display: flex;
        gap: 12px;
        flex-wrap: wrap;
    }

    .f-input {
        flex: 1 1 100%;
        width: 100%;
        padding: 12px 16px;
        border: 1px solid #2A324B;
        border-radius: 8px;
        background: #0B0F19;
        color: #FFFFFF;
        font-size: 14px;
        outline: none;
        box-sizing: border-box;
        margin-bottom: 10px;
    }

    .f-input:focus {
        border-color: #1A4199;
    }

    /* ACTION BUTTONS */
    .btn-danger,
    .btn-warning,
    .btn-success,
    .btn-navy {
        color: #FFFFFF;
        border: none;
        padding: 12px 24px;
        border-radius: 8px;
        font-weight: 600;
        font-size: 14px;
        cursor: pointer;
        flex: 1 1 100%;
        margin-bottom: 10px;
        transition: opacity 0.2s;
    }

    .btn-danger {
        background: #E8283F;
    }

    .btn-warning {
        background: #E8890C;
    }

    .btn-success {
        background: #22B573;
    }

    .btn-navy {
        background: #1A4199;
    }

    .btn-danger:hover,
    .btn-warning:hover,
    .btn-success:hover,
    .btn-navy:hover {
        opacity: 0.88;
    }

    /* RESULT MESSAGES */
    .msg {
        display: block;
        padding: 14px 20px;
        border-radius: 8px;
        font-size: 14px;
        font-weight: 600;
        margin-bottom: 24px;
    }

    .msg.ok {
        background: rgba(16,185,129,0.1);
        border: 1px solid rgba(16,185,129,0.3);
        color: #10B981;
    }

    .msg.err {
        background: rgba(239,68,68,0.1);
        border: 1px solid rgba(239,68,68,0.3);
        color: #EF4444;
    }

    /* RESPONSIVE */
    @media (max-width: 950px) {
        .dash-wrapper {
            flex-direction: column;
        }

        .dash-sidebar {
            width: 100%;
            height: auto;
            position: relative;
            border-right: none;
            border-bottom: 1px solid #2A324B;
            padding: 15px;
        }

        .dash-logo {
            text-align: center;
            padding-left: 0;
            margin-bottom: 15px;
        }

        .dash-nav {
            flex-direction: row;
            flex-wrap: wrap;
            justify-content: center;
            gap: 8px;
        }

        .dash-link {
            padding: 8px 12px;
            font-size: 13px;
        }

        .dash-link.active {
            border-left: none;
            border-bottom: 2px solid #1A4199;
        }

        .dash-main {
            padding: 20px 16px;
        }
    }

    @media (max-width: 750px) {
        .override-card {
            padding: 20px;
        }

        .page-title {
            font-size: 23px;
        }
    }
</style>

<div class="dash-wrapper">

    <!-- SIDEBAR -->
    <aside class="dash-sidebar">

        <div class="dash-logo">
            IKHWEZI<span>.</span>
        </div>

        <nav class="dash-nav">

            <!-- OVERVIEW -->
            <a href="<%= ResolveUrl("~/ManagerDashboard.aspx") %>"
               class="dash-link">
                <svg viewBox="0 0 24 24">
                    <rect x="3" y="3" width="7" height="7"></rect>
                    <rect x="14" y="3" width="7" height="7"></rect>
                    <rect x="14" y="14" width="7" height="7"></rect>
                    <rect x="3" y="14" width="7" height="7"></rect>
                </svg>
                Overview
            </a>

            <!-- STUDENTS -->
            <a href="<%= ResolveUrl("~/ManagerStudents.aspx") %>"
               class="dash-link">
                <svg viewBox="0 0 24 24">
                    <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
                    <circle cx="9" cy="7" r="4"></circle>
                    <path d="M23 21v-2a4 4 0 0 0-3-3.87"></path>
                    <path d="M16 3.13a4 4 0 0 1 0 7.75"></path>
                </svg>
                Students
            </a>

            <!-- BOOKINGS -->
            <a href="<%= ResolveUrl("~/AdminDashboard.aspx") %>"
               class="dash-link">
                <svg viewBox="0 0 24 24">
                    <rect x="3" y="4" width="18" height="18" rx="2"></rect>
                    <line x1="16" y1="2" x2="16" y2="6"></line>
                    <line x1="8" y1="2" x2="8" y2="6"></line>
                    <line x1="3" y1="10" x2="21" y2="10"></line>
                </svg>
                Bookings
            </a>

            <!-- REGISTER STAFF -->
            <a href="<%= ResolveUrl("~/ManagerRegisterStaff.aspx") %>"
               class="dash-link">
                <svg viewBox="0 0 24 24">
                    <path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
                    <circle cx="8.5" cy="7" r="4"></circle>
                    <line x1="20" y1="8" x2="20" y2="14"></line>
                    <line x1="23" y1="11" x2="17" y2="11"></line>
                </svg>
                Register Staff
            </a>

            <!-- SYSTEM CONTROLS -->
            <a href="<%= ResolveUrl("~/ManagerOverrides.aspx") %>"
               class="dash-link active">
                <svg viewBox="0 0 24 24">
                    <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"></path>
                </svg>
                System Controls
            </a>

            <!-- NOTIFY STAFF -->
            <a href="<%= ResolveUrl("~/Account/StaffNotify.aspx") %>"
               class="dash-link">
                <svg viewBox="0 0 24 24">
                    <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"></path>
                    <polyline points="22,6 12,13 2,6"></polyline>
                </svg>
                Notify Staff
            </a>

            <!-- REPORTS -->
            <a href="<%= ResolveUrl("~/Reports.aspx") %>"
               class="dash-link">
                <svg viewBox="0 0 24 24">
                    <polyline points="23 6 13.5 15.5 8.5 10.5 1 18"></polyline>
                    <polyline points="17 6 23 6 23 12"></polyline>
                </svg>
                Reports
            </a>

            <!-- MY PROFILE -->
            <a href="<%= ResolveUrl("~/ManagerPersonalInfo.aspx") %>"
               class="dash-link">
                <svg viewBox="0 0 24 24">
                    <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path>
                    <circle cx="12" cy="7" r="4"></circle>
                </svg>
                My Profile
            </a>

        </nav>

    </aside>

    <!-- MAIN CONTENT -->
    <main class="dash-main">

        <h2 class="page-title">System Controls</h2>

        <p class="page-sub">
            Manage sensitive exceptions. Every action requires a
            reason and is permanently logged.
        </p>

        <asp:Label
            ID="lblMsg"
            runat="server"
            Visible="false" />

        <div class="override-grid">

            <!-- 1. MANUAL LESSON ADJUSTMENT -->
            <div class="override-card">

                <h3>
                    <svg viewBox="0 0 24 24">
                        <path d="M12 2v20M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"></path>
                    </svg>
                    Manual Lesson Adjustment
                </h3>

                <p>
                    If a student paid cash and reception forgot to
                    allocate the package, or if a lesson was wrongly deducted.
                </p>

                <div class="input-group">

                    <asp:TextBox
                        ID="txtLessonStudent"
                        runat="server"
                        CssClass="f-input"
                        placeholder="Student Email or Number" />

                    <asp:TextBox
                        ID="txtLessonCount"
                        runat="server"
                        CssClass="f-input"
                        placeholder="Number of Lessons (use -1 to deduct)" />

                    <asp:TextBox
                        ID="txtLessonReason"
                        runat="server"
                        CssClass="f-input"
                        placeholder="Reason (e.g. Cash paid R1500 at reception)"
                        MaxLength="200" />

                    <asp:Button
                        ID="btnInjectLesson"
                        runat="server"
                        Text="Apply Adjustment"
                        CssClass="btn-navy"
                        OnClick="btnInjectLesson_Click" />

                </div>
            </div>

            <!-- 2. STAFF PASSWORD RECOVERY -->
            <div class="override-card">

                <h3>
                    <svg viewBox="0 0 24 24">
                        <rect x="3" y="11" width="18" height="11"
                              rx="2" ry="2"></rect>
                        <path d="M7 11V7a5 5 0 0 1 10 0v4"></path>
                    </svg>
                    Force Staff Password Reset
                </h3>

                <p>
                    Generates a unique temporary password and sends it
                    directly to the staff member's email and WhatsApp.
                </p>

                <div class="input-group">

                    <asp:TextBox
                        ID="txtResetStaff"
                        runat="server"
                        CssClass="f-input"
                        placeholder="Enter Staff Email" />

                    <asp:TextBox
                        ID="txtResetReason"
                        runat="server"
                        CssClass="f-input"
                        placeholder="Reason (e.g. Instructor locked out)"
                        MaxLength="200" />

                    <asp:Button
                        ID="btnResetStaff"
                        runat="server"
                        Text="Generate &amp; Send Password"
                        CssClass="btn-warning"
                        OnClick="btnResetStaff_Click" />

                </div>
            </div>

            <!-- 3. ARCHIVE / RESTORE STUDENT -->
            <div class="override-card">

                <h3>
                    <svg viewBox="0 0 24 24">
                        <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path>
                        <circle cx="12" cy="7" r="4"></circle>
                    </svg>
                    Archive / Restore Student
                </h3>

                <p>
                    Archive a student who is no longer attending.
                    Restore them if they return.
                </p>

                <div class="input-group">

                    <asp:TextBox
                        ID="txtArchiveStudent"
                        runat="server"
                        CssClass="f-input"
                        placeholder="Enter Student Email or Number" />

                    <asp:TextBox
                        ID="txtArchiveReason"
                        runat="server"
                        CssClass="f-input"
                        placeholder="Reason for change"
                        MaxLength="200" />

                    <div style="display:flex;gap:12px;width:100%;">

                        <asp:Button
                            ID="btnArchiveStudent"
                            runat="server"
                            Text="Archive"
                            CssClass="btn-danger"
                            OnClick="btnArchiveStudent_Click" />

                        <asp:Button
                            ID="btnRestoreStudent"
                            runat="server"
                            Text="Restore"
                            CssClass="btn-success"
                            OnClick="btnRestoreStudent_Click" />

                    </div>

                </div>
            </div>

            <!-- 4. SUSPEND / REACTIVATE STAFF -->
            <div class="override-card">

                <h3>
                    <svg viewBox="0 0 24 24">
                        <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
                        <circle cx="9" cy="7" r="4"></circle>
                        <line x1="23" y1="11" x2="17" y2="11"></line>
                    </svg>
                    Suspend / Reactivate Staff
                </h3>

                <p>
                    Immediately revoke or restore access for an employee.
                </p>

                <div class="input-group">

                    <asp:TextBox
                        ID="txtSuspendStaff"
                        runat="server"
                        CssClass="f-input"
                        placeholder="Enter Staff Email Address" />

                    <asp:TextBox
                        ID="txtSuspendReason"
                        runat="server"
                        CssClass="f-input"
                        placeholder="Reason for change"
                        MaxLength="200" />

                    <div style="display:flex;gap:12px;width:100%;">

                        <asp:Button
                            ID="btnSuspendStaff"
                            runat="server"
                            Text="Suspend"
                            CssClass="btn-danger"
                            OnClick="btnSuspendStaff_Click" />

                        <asp:Button
                            ID="btnReactivateStaff"
                            runat="server"
                            Text="Reactivate"
                            CssClass="btn-success"
                            OnClick="btnReactivateStaff_Click" />

                    </div>

                </div>
            </div>

        </div>

    </main>

</div>

</asp:Content>
