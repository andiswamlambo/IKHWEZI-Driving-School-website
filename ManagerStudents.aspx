 
<%@ Page Title="Student Directory"
    Language="C#"
    MasterPageFile="~/Site.master"
    AutoEventWireup="true"
    CodeBehind="ManagerStudents.aspx.cs"
    Inherits="WebApplicationM3.ManagerStudents" %>

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

    /* STUDENT TABLE */
    .table-container {
        background: #151A28;
        border-radius: 16px;
        border: 1px solid #2A324B;
        overflow: hidden;
    }

    /* SEARCH */
    .search-bar {
        padding: 20px;
        border-bottom: 1px solid #2A324B;
        display: flex;
        gap: 10px;
    }

    .search-input {
        flex: 1;
        min-width: 0;
        padding: 12px 16px;
        border: 1px solid #2A324B;
        border-radius: 8px;
        background: #0B0F19;
        color: #FFFFFF;
        outline: none;
        font-size: 14px;
    }

    .search-input:focus {
        border-color: #1A4199;
    }

    .btn-search {
        background: #8B5CF6;
        color: #FFFFFF;
        border: none;
        padding: 0 24px;
        border-radius: 8px;
        font-weight: 600;
        cursor: pointer;
        min-height: 44px;
    }

    .btn-search:hover {
        background: #7C3AED;
    }

    /* TABLE */
    .table-scroll {
        width: 100%;
        overflow-x: auto;
    }

    .dash-grid {
        width: 100%;
        border-collapse: collapse;
        font-size: 14px;
        min-width: 720px;
    }

    .dash-grid th {
        text-align: left;
        padding: 16px 24px;
        color: #9CA3AF;
        font-size: 12px;
        text-transform: uppercase;
        border-bottom: 1px solid #2A324B;
        background: rgba(0,0,0,0.2);
        white-space: nowrap;
    }

    .dash-grid td {
        padding: 16px 24px;
        border-bottom: 1px solid #2A324B;
        color: #F3F4F6;
    }

    .dash-grid tr:hover td {
        background: rgba(255,255,255,0.02);
    }

    /* VIEW PROFILE BUTTON */
    .btn-view {
        display: inline-block;
        background: #1E2538;
        color: #FFFFFF;
        border: 1px solid #2A324B;
        padding: 6px 12px;
        border-radius: 6px;
        text-decoration: none;
        font-size: 12px;
        white-space: nowrap;
    }

    .btn-view:hover {
        background: #2A324B;
        color: #FFFFFF;
        text-decoration: none;
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

    @media (max-width: 600px) {
        .search-bar {
            flex-direction: column;
        }

        .btn-search {
            padding: 12px 24px;
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
               class="dash-link active">
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

        <h2 class="page-title">Student Directory</h2>

        <p class="page-sub">
            Manager View - Manage and view full student profiles.
        </p>

        <div class="table-container">

            <!-- SEARCH BAR -->
            <div class="search-bar">

                <asp:TextBox
                    ID="txtSearch"
                    runat="server"
                    CssClass="search-input"
                    placeholder="Search by name, email, or student number..." />

                <asp:Button
                    ID="btnSearch"
                    runat="server"
                    Text="Search"
                    CssClass="btn-search"
                    OnClick="btnSearch_Click" />

            </div>

            <!-- STUDENT DIRECTORY -->
            <div class="table-scroll">

                <asp:Repeater ID="repStudents" runat="server">

                    <HeaderTemplate>
                        <table class="dash-grid">
                            <thead>
                                <tr>
                                    <th>Student No</th>
                                    <th>Name</th>
                                    <th>Email</th>
                                    <th>Phone</th>
                                    <th>Status</th>
                                    <th>Action</th>
                                </tr>
                            </thead>
                            <tbody>
                    </HeaderTemplate>

                    <ItemTemplate>
                        <tr>
                            <td>
                                <b>#<%# Eval("StudNumber") %></b>
                            </td>

                            <td>
                                <%# Eval("FullName") %>
                            </td>

                            <td>
                                <%# Eval("StudEmailAddress") %>
                            </td>

                            <td>
                                <%# Eval("PhoneNum") %>
                            </td>

                            <td>
                                <%# Eval("Status") %>
                            </td>

                            <td>
                                <asp:HyperLink
                                    runat="server"
                                    CssClass="btn-view"
                                    NavigateUrl='<%# "AdminStudentDetail.aspx?email=" + Server.UrlEncode(Eval("StudEmailAddress").ToString()) %>'
                                    Text="View Profile" />
                            </td>
                        </tr>
                    </ItemTemplate>

                    <FooterTemplate>
                            </tbody>
                        </table>
                    </FooterTemplate>

                </asp:Repeater>

            </div>

        </div>

    </main>

</div>

</asp:Content>
