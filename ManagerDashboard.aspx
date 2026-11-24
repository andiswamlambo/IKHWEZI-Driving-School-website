 
<%@ Page Title="Manager Dashboard" Language="C#"
    MasterPageFile="~/Site.master"
    AutoEventWireup="true"
    CodeBehind="ManagerDashboard.aspx.cs"
    Inherits="WebApplicationM3.ManagerDashboard" %>

<asp:Content ID="Content1"
    ContentPlaceHolderID="MainContent"
    runat="server">

<style>
    body {
        background: #0B0F19 !important;
        padding-top: 0 !important;
    }

    .navbar-khwezi, .site-footer,
    #waFloat, #kwChatLauncher, #kwChatPanel {
        display: none !important;
    }

    .body-content {
        padding: 0 !important;
        max-width: 100% !important;
        width: 100% !important;
    }

    .manager-layout {
        display: flex;
        min-height: 100vh;
        background: #0B0F19;
        color: #F3F4F6;
        font-family: 'Segoe UI', Arial, sans-serif;
    }

    /* SIDEBAR */
    .manager-sidebar {
        width: 235px;
        min-width: 235px;
        height: 100vh;
        position: sticky;
        top: 0;
        background: #151A28;
        border-right: 1px solid #2A324B;
        padding: 20px 12px;
        display: flex;
        flex-direction: column;
        overflow-y: auto;
    }

    .manager-logo {
        font-size: 24px;
        font-weight: 800;
        font-style: italic;
        color: white;
        padding: 6px 12px 22px;
    }

    .manager-logo span {
        color: #E8283F;
    }

    .manager-nav {
        display: flex;
        flex-direction: column;
        gap: 4px;
        flex: none;
    }

    .manager-link {
        display: flex;
        align-items: center;
        gap: 12px;
        padding: 11px 13px;
        color: #9CA3AF;
        text-decoration: none;
        border-radius: 9px;
        font-size: 13.5px;
        font-weight: 500;
        transition: background .15s, color .15s;
    }

    .manager-link:hover {
        background: #222A40;
        color: white;
        text-decoration: none;
    }

    .manager-link.active {
        background: #1A4199;
        color: white;
        font-weight: 700;
    }

    .manager-link svg {
        width: 19px;
        height: 19px;
        flex-shrink: 0;
        stroke: currentColor;
        stroke-width: 2;
        fill: none;
        stroke-linecap: round;
        stroke-linejoin: round;
    }

    .manager-logout {
        margin-top: 8px;
        border-top: 1px solid #2A324B;
        padding-top: 14px;
        color: #FCA5A5;
    }

    .manager-logout:hover {
        background: #3B1820;
        color: white;
    }

    /* MAIN CONTENT */
    .manager-main {
        flex: 1;
        min-width: 0;
        padding: 22px 26px;
        max-width: 100%;
    }

    .manager-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 15px;
        margin-bottom: 20px;
    }

    .manager-header h1 {
        color: white;
        font-size: 24px;
        font-weight: 750;
        margin: 0 0 5px;
    }

    .muted {
        color: #9CA3AF;
        font-size: 12.5px;
    }

    .manager-avatar {
        width: 40px;
        height: 40px;
        border-radius: 50%;
        background: #1A4199;
        display: flex;
        align-items: center;
        justify-content: center;
        color: white;
        font-weight: 800;
    }

    /* DASHBOARD CARDS */
    .manager-cards {
        display: grid;
        grid-template-columns: repeat(4, minmax(0,1fr));
        gap: 13px;
        margin-bottom: 20px;
    }

    .manager-card, .payment-card {
        background: #151A28;
        border: 1px solid #2A324B;
        border-radius: 12px;
        padding: 17px;
        text-decoration: none;
        color: white;
        min-width: 0;
    }

    a.manager-card:hover {
        border-color: #1A4199;
        background: #192238;
        color: white;
        text-decoration: none;
    }

    .card-label {
        display: block;
        color: #9CA3AF;
        font-size: 12px;
        margin-bottom: 9px;
    }

    .card-number {
        display: block;
        font-size: 25px;
        font-weight: 800;
        color: white;
        overflow-wrap: anywhere;
    }

    .section-title {
        font-size: 18px;
        color: white;
        font-weight: 700;
        margin: 20px 0 13px;
    }

    .payment-grid {
        display: grid;
        grid-template-columns: repeat(3, minmax(0,1fr));
        gap: 13px;
        margin-bottom: 20px;
    }

    .payment-card .card-number.green {
        color: #6EE7B7;
    }

    .payment-card .card-number.orange {
        color: #FDBA74;
    }

    .payment-card .card-number.red {
        color: #FCA5A5;
    }

    /* PANELS */
    .manager-panel {
        background: #151A28;
        border: 1px solid #2A324B;
        border-radius: 12px;
        margin-bottom: 16px;
        overflow: hidden;
    }

    .panel-heading {
        padding: 14px 18px;
        border-bottom: 1px solid #2A324B;
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 12px;
        flex-wrap: wrap;
    }

    .panel-heading h3 {
        color: white;
        font-size: 15px;
        font-weight: 700;
        margin: 0;
    }

    .panel-actions {
        display: flex;
        align-items: center;
        gap: 12px;
        flex-wrap: wrap;
    }

    .view-all-btn {
        background: #222A40;
        border: 1px solid #35415E;
        border-radius: 7px;
        color: #93C5FD;
        padding: 7px 12px;
        font-size: 12px;
        font-weight: 700;
        cursor: pointer;
    }

    .view-all-btn:hover {
        background: #1A4199;
        color: white;
    }

    .payment-row {
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 15px;
        padding: 12px 18px;
        border-bottom: 1px solid #2A324B;
    }

    .payment-row:last-child {
        border-bottom: none;
    }

    .payment-row strong {
        display: block;
        color: white;
        font-size: 13px;
        margin-bottom: 5px;
    }

    .payment-row small {
        display: block;
        color: #9CA3AF;
        font-size: 11.5px;
        line-height: 1.6;
    }

    .payment-row a {
        color: #93C5FD;
        font-size: 12px;
        font-weight: 700;
        white-space: nowrap;
    }

    .empty-message {
        display: block;
        color: #9CA3AF;
        padding: 16px 18px;
        font-size: 13px;
    }

    .error-message {
        display: block;
        background: #3B1820;
        color: #FCA5A5;
        padding: 13px;
        border-radius: 9px;
        margin-bottom: 18px;
    }

    .manager-select {
        background: #0B0F19;
        color: white;
        border: 1px solid #2A324B;
        padding: 8px 10px;
        border-radius: 8px;
        font-size: 12px;
    }

    .table-scroll {
        overflow-x: auto;
    }

    .manager-table {
        width: 100%;
        border-collapse: collapse;
        min-width: 650px;
        font-size: 12.5px;
    }

    .manager-table th {
        color: #9CA3AF;
        text-align: left;
        padding: 12px 16px;
        border-bottom: 1px solid #2A324B;
        font-size: 11px;
    }

    .manager-table td {
        color: #F3F4F6;
        padding: 12px 16px;
        border-bottom: 1px solid #2A324B;
    }

    .action-link {
        display: inline-block;
        padding: 7px 12px;
        background: #222A40;
        border: 1px solid #2A324B;
        border-radius: 7px;
        color: white;
        text-decoration: none;
    }

    .action-link:hover {
        color: white;
        background: #1A4199;
    }

    .status-chip {
        display: inline-block;
        padding: 5px 10px;
        background: #222A40;
        border-radius: 20px;
        font-size: 12px;
    }

    /* PREVIEW: SHOW FIRST THREE RECORDS */
    .preview-list:not(.expanded) .payment-row:nth-child(n+4) {
        display: none;
    }

    .preview-table:not(.expanded) tbody tr:nth-child(n+4) {
        display: none;
    }

    @media (max-width: 1100px) {
        .manager-cards {
            grid-template-columns: repeat(2, minmax(0,1fr));
        }

        .payment-grid {
            grid-template-columns: repeat(3, minmax(0,1fr));
        }

        .manager-main {
            padding: 20px;
        }
    }

    @media (max-width: 850px) {
        .manager-layout {
            flex-direction: column;
        }

        .manager-sidebar {
            width: 100%;
            min-width: 0;
            height: auto;
            position: relative;
            overflow: visible;
        }

        .manager-nav {
            display: grid;
            grid-template-columns: repeat(2, minmax(0,1fr));
        }

        .payment-grid {
            grid-template-columns: 1fr;
        }
    }

    @media (max-width: 560px) {
        .manager-main {
            padding: 16px 12px;
        }

        .manager-cards {
            grid-template-columns: repeat(2, minmax(0,1fr));
            gap: 9px;
        }

        .manager-card, .payment-card {
            padding: 13px;
        }

        .card-number {
            font-size: 21px;
        }

        .manager-header {
            flex-wrap: wrap;
        }

        .payment-row {
            align-items: flex-start;
            flex-wrap: wrap;
        }
    }
</style>

<script type="text/javascript">
    function toggleManagerSection(sectionId, button) {
        var section = document.getElementById(sectionId);

        if (!section) return false;

        var expanded = section.classList.toggle("expanded");

        button.textContent = expanded ? "Show Less" : "View All";
        button.setAttribute("aria-expanded", expanded ? "true" : "false");

        return false;
    }
</script>

<div class="manager-layout">

    <!-- SIDEBAR -->
    <aside class="manager-sidebar">

        <div class="manager-logo">
            IKHWEZI<span>.</span>
        </div>

        <nav class="manager-nav">

            <a href="ManagerDashboard.aspx" class="manager-link active">
                <svg viewBox="0 0 24 24">
                    <rect x="3" y="3" width="7" height="7" rx="1"></rect>
                    <rect x="14" y="3" width="7" height="7" rx="1"></rect>
                    <rect x="3" y="14" width="7" height="7" rx="1"></rect>
                    <rect x="14" y="14" width="7" height="7" rx="1"></rect>
                </svg>
                Overview
            </a>

            <a href="ManagerStudents.aspx" class="manager-link">
                <svg viewBox="0 0 24 24">
                    <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
                    <circle cx="9" cy="7" r="4"></circle>
                    <path d="M23 21v-2a4 4 0 0 0-3-3.87"></path>
                    <path d="M16 3.13a4 4 0 0 1 0 7.75"></path>
                </svg>
                Students
            </a>

            <a href="AdminDashboard.aspx" class="manager-link">
                <svg viewBox="0 0 24 24">
                    <rect x="3" y="4" width="18" height="18" rx="2"></rect>
                    <line x1="16" y1="2" x2="16" y2="6"></line>
                    <line x1="8" y1="2" x2="8" y2="6"></line>
                    <line x1="3" y1="10" x2="21" y2="10"></line>
                </svg>
                Bookings
            </a>

            <a href="ManagerRegisterStaff.aspx" class="manager-link">
                <svg viewBox="0 0 24 24">
                    <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"></path>
                    <circle cx="9" cy="7" r="4"></circle>
                    <line x1="19" y1="8" x2="19" y2="16"></line>
                    <line x1="15" y1="12" x2="23" y2="12"></line>
                </svg>
                Register Staff
            </a>

            <a href="ManagerOverrides.aspx" class="manager-link">
                <svg viewBox="0 0 24 24">
                    <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"></path>
                    <path d="M9 12l2 2 4-4"></path>
                </svg>
                System Controls
            </a>

            <a href="Account/StaffNotify.aspx" class="manager-link">
                <svg viewBox="0 0 24 24">
                    <rect x="2" y="4" width="20" height="16" rx="2"></rect>
                    <polyline points="22,6 12,13 2,6"></polyline>
                </svg>
                Notify Staff
            </a>

            <a href="Reports.aspx" class="manager-link">
                <svg viewBox="0 0 24 24">
                    <line x1="18" y1="20" x2="18" y2="10"></line>
                    <line x1="12" y1="20" x2="12" y2="4"></line>
                    <line x1="6" y1="20" x2="6" y2="14"></line>
                </svg>
                Reports
            </a>

            <a href="ManagerPersonalInfo.aspx" class="manager-link">
                <svg viewBox="0 0 24 24">
                    <circle cx="12" cy="8" r="4"></circle>
                    <path d="M4 21v-2a8 8 0 0 1 16 0v2"></path>
                </svg>
                My Profile
            </a>

        </nav>

        <!-- LOGOUT DIRECTLY BELOW NAVIGATION -->
        <asp:LinkButton
            ID="btnLogout"
            runat="server"
            CssClass="manager-link manager-logout"
            OnClick="btnLogout_Click">

            <svg viewBox="0 0 24 24">
                <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"></path>
                <polyline points="16 17 21 12 16 7"></polyline>
                <line x1="21" y1="12" x2="9" y2="12"></line>
            </svg>
            Logout
        </asp:LinkButton>

    </aside>

    <!-- MAIN DASHBOARD -->
    <main class="manager-main">

        <asp:Label
            ID="lblError"
            runat="server"
            Visible="false"
            CssClass="error-message" />

        <header class="manager-header">
            <div>
                <h1>Manager Overview</h1>
                <p class="muted">
                    Welcome back. Monitor your driving school.
                </p>
            </div>

            <div style="display:flex;align-items:center;gap:12px">
                <span class="muted">
                    <asp:Literal ID="litRole" runat="server" Text="Manager" />
                </span>
                <div class="manager-avatar">
                    <asp:Literal ID="litInitials" runat="server" Text="M" />
                </div>
            </div>
        </header>

        <!-- OVERVIEW STATISTICS -->
        <div class="manager-cards">

            <a href="ManagerDashboard.aspx?filter=revenue" class="manager-card">
                <span class="card-label">Revenue This Month</span>
                <span class="card-number">
                    R<asp:Literal ID="lblRevenueMonth" runat="server" Text="0.00" />
                </span>
            </a>

            <a href="ManagerDashboard.aspx?filter=active" class="manager-card">
                <span class="card-label">Total Active Users</span>
                <span class="card-number">
                    <asp:Literal ID="lblActiveUsers" runat="server" Text="0" />
                </span>
            </a>

            <a href="ManagerDashboard.aspx?filter=pending" class="manager-card">
                <span class="card-label">Pending Confirmations</span>
                <span class="card-number">
                    <asp:Literal ID="lblPending" runat="server" Text="0" />
                </span>
            </a>

            <a href="ManagerDashboard.aspx?filter=today" class="manager-card">
                <span class="card-label">Today's Lessons</span>
                <span class="card-number">
                    <asp:Literal ID="lblToday" runat="server" Text="0" />
                </span>
            </a>

        </div>

        <!-- PACKAGE PAYMENT MONITORING -->
        <h2 class="section-title">Package Payment Monitoring</h2>

        <div class="payment-grid">

            <div class="payment-card">
                <span class="card-label">Package Payments (Last 7 Days)</span>
                <span class="card-number green">
                    <asp:Literal ID="litNewPaymentCount" runat="server" Text="0" />
                </span>
                <span class="muted">Recorded package payments</span>
            </div>

            <div class="payment-card">
                <span class="card-label">Packages Awaiting Payment</span>
                <span class="card-number orange">
                    <asp:Literal ID="litAwaitingCount" runat="server" Text="0" />
                </span>
                <span class="muted">Active unpaid reservations</span>
            </div>

            <div class="payment-card">
                <span class="card-label">Payments Requiring Review</span>
                <span class="card-number red">
                    <asp:Literal ID="litReviewCount" runat="server" Text="0" />
                </span>
                <span class="muted">Financial records needing attention</span>
            </div>

        </div>

        <asp:Label
            ID="lblPaymentAlertsError"
            runat="server"
            Visible="false"
            CssClass="error-message"
            Text="Payment updates are temporarily unavailable." />

        <!-- RECENT PACKAGE PAYMENTS -->
        <section class="manager-panel">

            <div class="panel-heading">
                <h3>Recent Package Payments</h3>

                <div class="panel-actions">
                    <span class="muted">Last 7 days</span>
                    <button type="button" class="view-all-btn"
                        aria-expanded="false"
                        aria-controls="recentPayments"
                        onclick="return toggleManagerSection('recentPayments', this);">
                        View All
                    </button>
                </div>
            </div>

            <div id="recentPayments" class="preview-list">

                <asp:Repeater ID="repPackagePayments" runat="server">
                    <ItemTemplate>
                        <div class="payment-row">
                            <div>
                                <strong>
                                    <%#: Eval("StudentName") %>
                                    —
                                    R<%# Convert.ToDecimal(Eval("Amount")).ToString("N2") %>
                                </strong>
                                <small>
                                    <%#: Eval("PackageName") %>
                                    |
                                    <%# Convert.ToDateTime(Eval("PaidOn")).ToString("dd MMM yyyy HH:mm") %>
                                    |
                                    Payment recorded
                                </small>
                            </div>

                            <asp:LinkButton
                                ID="btnPaymentStudent"
                                runat="server"
                                Text="View Student"
                                CommandArgument='<%# Eval("StudEmailAddress") %>'
                                OnCommand="btnViewStudent_Command" />
                        </div>
                    </ItemTemplate>
                </asp:Repeater>

                <asp:Label
                    ID="lblNoPackagePayments"
                    runat="server"
                    Visible="false"
                    CssClass="empty-message"
                    Text="No recent package payments." />

            </div>
        </section>

        <!-- AWAITING PAYMENT -->
        <section class="manager-panel">

            <div class="panel-heading">
                <h3>Packages Awaiting Payment</h3>

                <div class="panel-actions">
                    <span class="muted">Unpaid reservations</span>
                    <button type="button" class="view-all-btn"
                        aria-expanded="false"
                        aria-controls="awaitingPayments"
                        onclick="return toggleManagerSection('awaitingPayments', this);">
                        View All
                    </button>
                </div>
            </div>

            <div id="awaitingPayments" class="preview-list">

                <asp:Repeater ID="repAwaitingPayments" runat="server">
                    <ItemTemplate>
                        <div class="payment-row">
                            <div>
                                <strong>
                                    <%#: Eval("StudentName") %>
                                    —
                                    <%#: Eval("PackageName") %>
                                </strong>

                                <small>
                                    Package Price:
                                    R<%# Convert.ToDecimal(Eval("PackagePrice")).ToString("N2") %>
                                    |
                                    Selected:
                                    <%# Convert.ToDateTime(Eval("ReservedOn")).ToString("dd MMM yyyy") %>
                                </small>

                                <small style="color:#FDBA74">
                                    Awaiting payment
                                </small>
                            </div>

                            <asp:LinkButton
                                ID="btnAwaitingStudent"
                                runat="server"
                                Text="View Student"
                                CommandArgument='<%# Eval("StudEmailAddress") %>'
                                OnCommand="btnViewStudent_Command" />
                        </div>
                    </ItemTemplate>
                </asp:Repeater>

                <asp:Label
                    ID="lblNoAwaiting"
                    runat="server"
                    Visible="false"
                    CssClass="empty-message"
                    Text="No active reservations awaiting payment." />

            </div>
        </section>

        <!-- PAYMENT REVIEW -->
        <section class="manager-panel">

            <div class="panel-heading">
                <h3>Payment Review Required</h3>

                <div class="panel-actions">
                    <span class="muted">Manager attention</span>
                    <button type="button" class="view-all-btn"
                        aria-expanded="false"
                        aria-controls="paymentReviews"
                        onclick="return toggleManagerSection('paymentReviews', this);">
                        View All
                    </button>
                </div>
            </div>

            <div id="paymentReviews" class="preview-list">

                <asp:Repeater ID="repPaymentReviews" runat="server">
                    <ItemTemplate>
                        <div class="payment-row">
                            <div>
                                <strong>
                                    <%#: Eval("StudentName") %>
                                    —
                                    <%#: Eval("PackageName") %>
                                </strong>

                                <small>
                                    Recorded Paid:
                                    R<%# Convert.ToDecimal(Eval("TotalPaid")).ToString("N2") %>
                                    |
                                    Balance:
                                    R<%# Convert.ToDecimal(Eval("BalanceDue")).ToString("N2") %>
                                </small>

                                <small style="color:#FCA5A5">
                                    Review financial record
                                </small>
                            </div>

                            <asp:LinkButton
                                ID="btnReviewStudent"
                                runat="server"
                                Text="Review Student"
                                CommandArgument='<%# Eval("StudEmailAddress") %>'
                                OnCommand="btnViewStudent_Command" />
                        </div>
                    </ItemTemplate>
                </asp:Repeater>

                <asp:Label
                    ID="lblNoReviews"
                    runat="server"
                    Visible="false"
                    CssClass="empty-message"
                    Text="No payments currently flagged for review." />

            </div>
        </section>

        <!-- BOOKING ACTIVITY -->
        <section class="manager-panel">

            <div class="panel-heading">
                <h3>Recent Activity</h3>

                <div class="panel-actions">

                    <asp:DropDownList
                        ID="ddlFilter"
                        runat="server"
                        CssClass="manager-select"
                        AutoPostBack="true"
                        OnSelectedIndexChanged="ddlFilter_SelectedIndexChanged">

                        <asp:ListItem Value="active">Active Bookings</asp:ListItem>
                        <asp:ListItem Value="unsettled">Awaiting Deposit</asp:ListItem>
                        <asp:ListItem Value="pending">Pending</asp:ListItem>
                        <asp:ListItem Value="confirmed">Confirmed</asp:ListItem>
                        <asp:ListItem Value="today">Today's Lessons</asp:ListItem>
                        <asp:ListItem Value="revenue">Paid Bookings</asp:ListItem>

                    </asp:DropDownList>

                    <button type="button" class="view-all-btn"
                        aria-expanded="false"
                        aria-controls="bookingActivity"
                        onclick="return toggleManagerSection('bookingActivity', this);">
                        View All
                    </button>

                </div>
            </div>

            <div id="bookingActivity" class="table-scroll preview-table">

                <asp:Repeater ID="repBookings" runat="server">

                    <HeaderTemplate>
                        <table class="manager-table">
                            <thead>
                                <tr>
                                    <th>Reference</th>
                                    <th>Student</th>
                                    <th>Scheduled</th>
                                    <th>Status</th>
                                    <th>Action</th>
                                </tr>
                            </thead>
                            <tbody>
                    </HeaderTemplate>

                    <ItemTemplate>
                        <tr>
                            <td>
                                #<%# Eval("BookingID") %>
                            </td>

                            <td>
                                <strong>
                                    <%#: Eval("StudentName") %>
                                </strong>

                                <div class="muted">
                                    <%#: Eval("StudEmail") %>
                                </div>
                            </td>

                            <td>
                                <%# Convert.ToDateTime(Eval("ScheduledDateTime")).ToString("dd MMM yyyy HH:mm") %>
                            </td>

                            <td>
                                <span class="status-chip">
                                    <%#: Eval("BookingStatus") %>
                                </span>
                            </td>

                            <td>
                                <asp:LinkButton
                                    ID="btnBookingStudent"
                                    runat="server"
                                    CssClass="action-link"
                                    Text="Profile"
                                    CommandArgument='<%# Eval("StudEmail") %>'
                                    OnCommand="btnViewStudent_Command" />
                            </td>
                        </tr>
                    </ItemTemplate>

                    <FooterTemplate>
                            </tbody>
                        </table>
                    </FooterTemplate>

                </asp:Repeater>

            </div>

        </section>

    </main>
</div>

</asp:Content>
