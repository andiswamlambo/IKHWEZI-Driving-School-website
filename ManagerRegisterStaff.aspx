 
<%@ Page Title="Invite Staff"
    Language="C#"
    MasterPageFile="~/Site.master"
    AutoEventWireup="true"
    CodeBehind="ManagerRegisterStaff.aspx.cs"
    Inherits="WebApplicationM3.ManagerRegisterStaff" %>

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

    /* REGISTRATION FORM */
    .form-card {
        background: #151A28;
        border-radius: 16px;
        border: 1px solid #2A324B;
        padding: 30px;
        max-width: 600px;
    }

    .form-group {
        margin-bottom: 20px;
    }

    .form-group label {
        display: block;
        font-size: 13px;
        font-weight: 600;
        color: #9CA3AF;
        margin-bottom: 8px;
        text-transform: uppercase;
        letter-spacing: 0.5px;
    }

    .form-control {
        width: 100%;
        padding: 12px 16px;
        border: 1px solid #2A324B;
        border-radius: 8px;
        background: #0B0F19;
        color: #FFFFFF;
        font-size: 14px;
        outline: none;
        transition: border-color 0.2s;
        box-sizing: border-box;
    }

    .form-control:focus {
        border-color: #1A4199;
    }

    select.form-control {
        color: #FFFFFF;
        background-color: #0B0F19;
        appearance: none;
        -webkit-appearance: none;
        background-image: url("data:image/svg+xml;charset=UTF-8,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='white' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3e%3cpolyline points='6 9 12 15 18 9'%3e%3c/polyline%3e%3c/svg%3e");
        background-repeat: no-repeat;
        background-position: right 1rem center;
        background-size: 1em;
    }

    select.form-control option {
        background-color: #151A28;
        color: #FFFFFF;
        padding: 10px;
    }

    /* BUTTONS */
    .btn-primary {
        background: #22B573;
        color: #FFFFFF;
        border: none;
        padding: 14px 24px;
        border-radius: 8px;
        font-weight: 700;
        font-size: 15px;
        cursor: pointer;
        width: 100%;
        transition: background 0.2s;
    }

    .btn-primary:hover {
        background: #1D9E63;
    }

    /* MESSAGES */
    .msg {
        padding: 14px 20px;
        border-radius: 8px;
        font-size: 14px;
        font-weight: 600;
        margin-bottom: 24px;
    }

    .msg.ok {
        background: rgba(34,181,115,0.1);
        border: 1px solid rgba(34,181,115,0.3);
        color: #22B573;
    }

    .msg.err {
        background: rgba(232,40,63,0.1);
        border: 1px solid rgba(232,40,63,0.3);
        color: #E8283F;
    }

    /* MANAGER WARNING */
    .warning-box {
        background: rgba(232,137,12,0.1);
        border: 1px solid rgba(232,137,12,0.3);
        color: #E8890C;
        padding: 12px;
        border-radius: 8px;
        font-size: 13px;
        margin-bottom: 20px;
        display: none;
    }

    /* INVITATION SUCCESS */
    .success-box {
        text-align: center;
        padding: 20px;
    }

    .success-box h3 {
        color: #22B573;
        margin-bottom: 10px;
    }

    .success-box p {
        color: #9CA3AF;
        margin-bottom: 20px;
    }

    .invite-link {
        background: #0B0F19;
        padding: 12px;
        border: 1px dashed #1A4199;
        border-radius: 8px;
        color: #FFFFFF;
        font-family: monospace;
        word-break: break-all;
        margin-bottom: 20px;
    }

    .btn-whatsapp {
        background: #25D366;
        color: #FFFFFF;
        border: none;
        padding: 14px 24px;
        border-radius: 8px;
        font-weight: 700;
        font-size: 15px;
        cursor: pointer;
        width: 100%;
        text-decoration: none;
        display: block;
        box-sizing: border-box;
        margin-bottom: 10px;
    }

    .btn-whatsapp:hover {
        background: #128C7E;
        color: #FFFFFF;
        text-decoration: none;
    }

    /* RESPONSIVE DESIGN */
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
        .form-card {
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
               class="dash-link active">
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
               class="dash-link">
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

        <h2 class="page-title">Invite New Staff</h2>

        <p class="page-sub">
            Send a secure, single-use link to a new employee
            so they can set their own password.
        </p>

        <asp:Label
            ID="lblMsg"
            runat="server"
            Visible="false" />

        <!-- INVITATION FORM -->
        <asp:Panel ID="pnlForm" runat="server">

            <div class="form-card">

                <div class="form-group">
                    <label>First Name</label>
                    <asp:TextBox
                        ID="txtName"
                        runat="server"
                        CssClass="form-control"
                        placeholder="e.g. John" />
                </div>

                <div class="form-group">
                    <label>Surname</label>
                    <asp:TextBox
                        ID="txtSurname"
                        runat="server"
                        CssClass="form-control"
                        placeholder="e.g. Doe" />
                </div>

                <div class="form-group">
                    <label>Email Address</label>
                    <asp:TextBox
                        ID="txtEmail"
                        runat="server"
                        TextMode="Email"
                        CssClass="form-control"
                        placeholder="name@ikhwezi.com" />
                </div>

                <div class="form-group">
                    <label>Phone Number (For WhatsApp)</label>
                    <asp:TextBox
                        ID="txtPhone"
                        runat="server"
                        CssClass="form-control"
                        placeholder="e.g. 0723456789"
                        MaxLength="10" />
                </div>

                <div class="form-group">
                    <label>Role</label>

                    <asp:DropDownList
                        ID="ddlRole"
                        runat="server"
                        CssClass="form-control"
                        onchange="checkManagerRole()">

                        <asp:ListItem
                            Value="Instructor"
                            Text="Instructor (Teacher)" />

                        <asp:ListItem
                            Value="Admin"
                            Text="Receptionist (Admin)" />

                        <asp:ListItem
                            Value="Manager"
                            Text="Manager (Full Access)" />

                    </asp:DropDownList>
                </div>

                <!-- MANAGER WARNING -->
                <div id="managerWarning" class="warning-box">
                    ⚠ You are creating a Manager account.
                    This user will have full access to financials,
                    student data and system controls.
                    Proceed with caution.
                </div>

                <asp:Button
                    ID="btnGenerateInvite"
                    runat="server"
                    Text="Generate &amp; Send Invitation"
                    CssClass="btn-primary"
                    OnClick="btnGenerateInvite_Click" />

            </div>

        </asp:Panel>

        <!-- SUCCESS PANEL -->
        <asp:Panel
            ID="pnlSuccess"
            runat="server"
            CssClass="form-card success-box"
            Visible="false">

            <h3>Invitation Generated!</h3>

            <p>
                An email has been sent to the staff member.
                You can also send the link via WhatsApp.
            </p>

            <div class="invite-link">
                <asp:Literal
                    ID="litInviteLink"
                    runat="server" />
            </div>

            <asp:HyperLink
                ID="hlWhatsApp"
                runat="server"
                CssClass="btn-whatsapp"
                Target="_blank">

                <svg style="width:20px;height:20px;fill:currentColor;vertical-align:middle;margin-right:8px;"
                     viewBox="0 0 24 24">
                    <path d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347m-5.421 7.403h-.004a9.87 9.87 0 01-5.031-1.378l-.361-.214-3.741.982.998-3.648-.235-.374a9.86 9.86 0 01-1.51-5.26c.001-5.45 4.436-9.884 9.888-9.884 2.64 0 5.122 1.03 6.988 2.898a9.825 9.825 0 012.893 6.994c-.003 5.45-4.437 9.884-9.885 9.884m8.413-18.297A11.815 11.815 0 0012.05 0C5.495 0 .16 5.335.157 11.892c0 2.096.547 4.142 1.588 5.945L.057 24l6.305-1.654a11.882 11.882 0 005.683 1.448h.005c6.554 0 11.89-5.335 11.893-11.893a11.821 11.821 0 00-3.48-8.413z" />
                </svg>

                Send via WhatsApp

            </asp:HyperLink>

            <asp:Button
                ID="btnInviteAnother"
                runat="server"
                Text="Invite Another Staff Member"
                CssClass="btn-primary"
                OnClick="btnInviteAnother_Click"
                style="background:#1A4199;" />

        </asp:Panel>

    </main>

</div>

<script>
    function checkManagerRole() {
        var role = document.getElementById(
            '<%= ddlRole.ClientID %>'
        ).value;

        document.getElementById('managerWarning').style.display =
            (role === 'Manager') ? 'block' : 'none';
    }

    document.addEventListener(
        'DOMContentLoaded',
        checkManagerRole
    );
</script>

</asp:Content>
