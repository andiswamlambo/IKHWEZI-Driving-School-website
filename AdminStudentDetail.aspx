 
<%@ Page Title="Student Management Centre" Language="C#"
    AutoEventWireup="true"
    CodeBehind="AdminStudentDetail.aspx.cs"
    Inherits="WebApplicationM3.AdminStudentDetail" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Student Management Centre | IKHWEZI</title>
    <style>
        * { box-sizing: border-box; }
        body {
            margin: 0;
            background: #0B0F19;
            color: #F3F4F6;
            font-family: 'Segoe UI', Arial, sans-serif;
        }
        .layout { display: flex; min-height: 100vh; }
        .sidebar {
            width: 255px;
            flex-shrink: 0;
            background: #151A28;
            border-right: 1px solid #2A324B;
            padding: 26px 16px;
            display: flex;
            flex-direction: column;
            gap: 5px;
        }
        .logo {
            color: white;
            font-size: 25px;
            font-weight: 800;
            font-style: italic;
            padding: 4px 12px 25px;
        }
        .logo span { color: #E8283F; }
        .nav-link {
            display: block;
            color: #A9B1C2;
            text-decoration: none;
            padding: 13px 15px;
            border-radius: 9px;
            font-size: 14px;
        }
        .nav-link:hover {
            background: #222A40;
            color: white;
        }
        .nav-link.active {
            background: #1A4199;
            color: white;
            font-weight: 700;
        }
        .main {
            flex: 1;
            min-width: 0;
            padding: 30px;
        }
        .page-title {
            font-size: 26px;
            margin: 0 0 8px;
            color: white;
        }
        .muted { color: #A9B1C2; font-size: 13px; }
        .back {
            color: #93C5FD;
            text-decoration: none;
            display: inline-block;
            margin-bottom: 20px;
            font-size: 14px;
        }
        .panel {
            background: #151A28;
            border: 1px solid #2A324B;
            border-radius: 14px;
            padding: 22px;
            margin-bottom: 20px;
        }
        .panel h2 {
            font-size: 18px;
            margin: 0 0 18px;
            color: white;
        }
        .profile {
            display: flex;
            gap: 20px;
            align-items: center;
            flex-wrap: wrap;
        }
        .photo, .initials {
            width: 94px;
            height: 94px;
            border-radius: 50%;
            object-fit: cover;
        }
        .initials {
            background: #1A4199;
            color: white;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 30px;
            font-weight: 800;
        }
        .profile h2 {
            margin: 0 0 8px;
            font-size: 23px;
        }
        .status {
            display: inline-block;
            background: #223D32;
            color: #86EFAC;
            border-radius: 20px;
            padding: 5px 11px;
            font-size: 12px;
        }
        .info-grid {
            display: grid;
            grid-template-columns: repeat(3, minmax(0,1fr));
            gap: 18px;
        }
        .info-label {
            display: block;
            color: #9CA3AF;
            font-size: 12px;
            margin-bottom: 7px;
        }
        .info-value {
            color: #F3F4F6;
            font-size: 14px;
            overflow-wrap: anywhere;
        }
        .cards {
            display: grid;
            grid-template-columns: repeat(5, minmax(0,1fr));
            gap: 14px;
            margin-bottom: 20px;
        }
        .card {
            background: #151A28;
            border: 1px solid #2A324B;
            border-radius: 12px;
            padding: 19px;
            min-width: 0;
        }
        .card-label {
            display: block;
            color: #A9B1C2;
            font-size: 12px;
            margin-bottom: 13px;
        }
        .card-value {
            display: block;
            font-size: 23px;
            font-weight: 800;
            overflow-wrap: anywhere;
        }
        .green { color: #6EE7B7; }
        .orange { color: #FDBA74; }
        .warning {
            background: #352617;
            border: 1px solid #875B24;
            color: #FCD9A4;
            padding: 17px 20px;
            border-radius: 11px;
            margin-bottom: 20px;
            line-height: 1.6;
        }
        .warning strong {
            display: block;
            margin-bottom: 6px;
        }
        .notice {
            background: #182B43;
            border: 1px solid #315782;
            color: #BFDBFE;
            padding: 15px 18px;
            border-radius: 10px;
            margin-bottom: 20px;
        }
        .progress {
            width: 100%;
            height: 11px;
            background: #2A324B;
            border-radius: 20px;
            overflow: hidden;
            margin: 15px 0 10px;
        }
        .progress-fill {
            width: 0;
            height: 100%;
            background: #22C55E;
            border-radius: 20px;
        }
        .table-wrap {
            overflow-x: auto;
            width: 100%;
        }
        .data-grid {
            width: 100%;
            border-collapse: collapse;
            color: #E5E7EB;
            font-size: 13px;
            min-width: 660px;
        }
        .data-grid th {
            background: #1E2538;
            color: #C4CAD6;
            text-align: left;
            font-weight: 600;
            padding: 13px;
            white-space: nowrap;
            border-bottom: 1px solid #34405A;
        }
        .data-grid td {
            padding: 13px;
            border-bottom: 1px solid #2A324B;
            vertical-align: middle;
        }
        .data-grid tr:hover td { background: #1B2233; }
        .data-grid .empty-row td {
            color: #A9B1C2;
            text-align: center;
        }
        .actions {
            display: flex;
            gap: 12px;
            flex-wrap: wrap;
        }
        .action {
            display: inline-block;
            background: #1A4199;
            color: white;
            padding: 11px 16px;
            border-radius: 8px;
            text-decoration: none;
            font-size: 13px;
            font-weight: 600;
        }
        .action.secondary {
            background: #252E45;
        }
        .error {
            display: block;
            background: #3B1820;
            color: #FCA5A5;
            padding: 15px;
            border-radius: 10px;
            margin-bottom: 20px;
        }
        @media (max-width: 1100px) {
            .cards { grid-template-columns: repeat(3, 1fr); }
            .info-grid { grid-template-columns: repeat(2, 1fr); }
        }
        @media (max-width: 760px) {
            .layout { flex-direction: column; }
            .sidebar { width: 100%; }
            .main { padding: 20px 14px; }
            .cards { grid-template-columns: repeat(2, 1fr); }
            .info-grid { grid-template-columns: 1fr; }
        }
        @media (max-width: 430px) {
            .cards { grid-template-columns: 1fr; }
        }
    </style>
</head>
<body>
<form id="form1" runat="server">
<div class="layout">

    <aside class="sidebar">
        <div class="logo">IKHWEZI<span>.</span></div>
        <a class="nav-link" href="ManagerDashboard.aspx">Overview</a>
        <a class="nav-link active" href="ManagerStudents.aspx">Students</a>
        <a class="nav-link" href="AdminDashboard.aspx">Bookings</a>
        <a class="nav-link" href="ManagerRegisterStaff.aspx">Register Staff</a>
        <a class="nav-link" href="ManagerOverrides.aspx">System Controls</a>
        <a class="nav-link" href="Account/StaffNotify.aspx">Notify Staff</a>
        <a class="nav-link" href="Reports.aspx">Reports</a>
        <a class="nav-link" href="ManagerPersonalInfo.aspx">My Profile</a>
    </aside>

    <main class="main">
        <a class="back" href="ManagerStudents.aspx">
            &#8592; Back to Students
        </a>

        <h1 class="page-title">Student Management Centre</h1>
        <p class="muted">
            Student information, lessons, reservations and payment history
        </p>

        <asp:Label ID="lblMessage" runat="server"
            Visible="false" CssClass="error" />

        <asp:Panel ID="pnlDetails" runat="server" Visible="false">

            <section class="panel">
                <div class="profile">
                    <asp:Image ID="imgStudentPhoto" runat="server"
                        CssClass="photo" Visible="false"
                        AlternateText="Student profile photo" />

                    <asp:Panel ID="pnlNoPhoto" runat="server"
                        CssClass="initials">
                        <asp:Literal ID="litInitials" runat="server" />
                    </asp:Panel>

                    <div>
                        <h2><asp:Literal ID="litName" runat="server" /></h2>
                        <p class="muted">
                            Student #<asp:Literal ID="litNumber" runat="server" />
                        </p>
                        <span class="status">
                            <asp:Literal ID="litStatus" runat="server" />
                        </span>
                    </div>
                </div>
            </section>

            <section class="panel">
                <h2>Personal Information</h2>
                <div class="info-grid">
                    <div>
                        <span class="info-label">Email Address</span>
                        <span class="info-value">
                            <asp:Literal ID="litEmail" runat="server" />
                        </span>
                    </div>
                    <div>
                        <span class="info-label">Phone Number</span>
                        <span class="info-value">
                            <asp:Literal ID="litPhone" runat="server" />
                        </span>
                    </div>
                    <div>
                        <span class="info-label">Date of Birth</span>
                        <span class="info-value">
                            <asp:Literal ID="litDOB" runat="server" />
                        </span>
                    </div>
                    <div>
                        <span class="info-label">Registration Date</span>
                        <span class="info-value">
                            <asp:Literal ID="litRegistered" runat="server" />
                        </span>
                    </div>
                    <div>
                        <span class="info-label">Address</span>
                        <span class="info-value">
                            <asp:Literal ID="litAddress" runat="server" />
                        </span>
                    </div>
                    <div>
                        <span class="info-label">Learner's Licence</span>
                        <span class="info-value">
                            <asp:Literal ID="litLearners" runat="server" />
                        </span>
                    </div>
                </div>
            </section>

            <asp:Panel ID="pnlPaymentReview" runat="server"
                Visible="false" CssClass="warning">
                <strong>Package Payment Attention</strong>
                <asp:Literal ID="litReviewMessage" runat="server" />
            </asp:Panel>

            <div class="cards">
                <div class="card">
                    <span class="card-label">Total Lessons</span>
                    <span class="card-value">
                        <asp:Literal ID="litTotalLessons" runat="server"
                            Text="0" />
                    </span>
                </div>
                <div class="card">
                    <span class="card-label">Lessons Used</span>
                    <span class="card-value">
                        <asp:Literal ID="litUsedLessons" runat="server"
                            Text="0" />
                    </span>
                </div>
                <div class="card">
                    <span class="card-label">Lessons Remaining</span>
                    <span class="card-value green">
                        <asp:Literal ID="litRemainingLessons" runat="server"
                            Text="0" />
                    </span>
                </div>
                <div class="card">
                    <span class="card-label">Package Payments Recorded</span>
                    <span class="card-value green">
                        <asp:Literal ID="litTotalPaid" runat="server"
                            Text="Not available" />
                    </span>
                </div>
                <div class="card">
                    <span class="card-label">Outstanding Package Balance</span>
                    <span class="card-value orange">
                        <asp:Literal ID="litOutstanding" runat="server"
                            Text="Not available" />
                    </span>
                </div>
            </div>

            <section class="panel">
                <h2>Lesson Progress</h2>
                <strong>
                    <asp:Literal ID="litProgressPercent"
                        runat="server" Text="0" />% completed
                </strong>
                <div class="progress">
                    <div id="progressFill" runat="server"
                        class="progress-fill"></div>
                </div>
                <span class="muted">
                    <asp:Literal ID="litProgressText" runat="server" />
                </span>
            </section>

            <section class="panel">
                <h2>Manager Quick Actions</h2>
                <div class="actions">
                    <asp:HyperLink ID="hlEmailStudent" runat="server"
                        CssClass="action"
                        Text="Email Student" />
                    <a href="ManagerStudents.aspx" class="action secondary">
                        All Students
                    </a>
                    <a href="ManagerOverrides.aspx" class="action secondary">
                        System Controls
                    </a>
                </div>
            </section>

            <section class="panel">
                <h2>Student Lesson Packages</h2>
                <div class="table-wrap">
                    <asp:GridView ID="gvPackages" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="data-grid"
                        GridLines="None"
                        EmptyDataText="No lesson packages recorded.">
                        <Columns>
                            <asp:BoundField DataField="StudentPackageID"
                                HeaderText="Package ID" />
                            <asp:BoundField DataField="PackageName"
                                HeaderText="Package" />
                            <asp:BoundField DataField="TotalLessons"
                                HeaderText="Total" />
                            <asp:BoundField DataField="UsedLessons"
                                HeaderText="Used" />
                            <asp:BoundField DataField="Remaining"
                                HeaderText="Remaining" />
                            <asp:CheckBoxField DataField="IsActive"
                                HeaderText="Active" />
                            <asp:BoundField DataField="PurchasedOn"
                                HeaderText="Purchased"
                                DataFormatString="{0:dd MMM yyyy}" />
                        </Columns>
                    </asp:GridView>
                </div>
            </section>

            <section class="panel">
                <h2>Package Financial Overview</h2>
                <asp:Panel ID="pnlOutstanding" runat="server"
                    Visible="false" CssClass="notice">
                    This student has an outstanding package balance.
                </asp:Panel>
                <div class="table-wrap">
                    <asp:GridView ID="gvBalances" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="data-grid"
                        GridLines="None"
                        EmptyDataText="No package reservation financial records.">
                        <Columns>
                            <asp:BoundField DataField="ReservationID"
                                HeaderText="Reservation" />
                            <asp:BoundField DataField="PackageName"
                                HeaderText="Package" />
                            <asp:BoundField DataField="ReservationStatus"
                                HeaderText="Reservation Status" />
                            <asp:BoundField DataField="PackagePrice"
                                HeaderText="Price"
                                DataFormatString="R{0:N2}" />
                            <asp:BoundField DataField="TotalPaid"
                                HeaderText="Recorded Paid"
                                DataFormatString="R{0:N2}"
                                NullDisplayText="Not available" />
                            <asp:BoundField DataField="BalanceDue"
                                HeaderText="Balance"
                                DataFormatString="R{0:N2}"
                                NullDisplayText="Not available" />
                            <asp:BoundField DataField="PaymentStatus"
                                HeaderText="Payment Status" />
                            <asp:BoundField DataField="DueDate"
                                HeaderText="Due Date"
                                DataFormatString="{0:dd MMM yyyy}"
                                NullDisplayText="Not available" />
                        </Columns>
                    </asp:GridView>
                </div>
                <p class="muted">
                    <asp:Label ID="lblFinanceNote" runat="server" />
                </p>
            </section>

            <section class="panel">
                <h2>Package Payment Ledger</h2>
                <div class="table-wrap">
                    <asp:GridView ID="gvPackagePayments" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="data-grid"
                        GridLines="None"
                        EmptyDataText="No package payments recorded.">
                        <Columns>
                            <asp:BoundField DataField="LedgerID"
                                HeaderText="Ledger ID" />
                            <asp:BoundField DataField="ReservationID"
                                HeaderText="Reservation" />
                            <asp:BoundField DataField="PackageName"
                                HeaderText="Package" />
                            <asp:BoundField DataField="Amount"
                                HeaderText="Amount"
                                DataFormatString="R{0:N2}" />
                            <asp:BoundField DataField="PaidOn"
                                HeaderText="Paid On"
                                DataFormatString="{0:dd MMM yyyy HH:mm}" />
                            <asp:BoundField DataField="ProviderPaymentID"
                                HeaderText="Payment Reference" />
                            <asp:BoundField DataField="ReservationStatus"
                                HeaderText="Reservation Status" />
                        </Columns>
                    </asp:GridView>
                </div>
            </section>

            <section class="panel">
                <h2>Booking History</h2>
                <div class="table-wrap">
                    <asp:GridView ID="gvBookings" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="data-grid"
                        GridLines="None"
                        EmptyDataText="No bookings found.">
                        <Columns>
                            <asp:BoundField DataField="BookingID"
                                HeaderText="Booking ID" />
                            <asp:BoundField DataField="ScheduledDateTime"
                                HeaderText="Scheduled"
                                DataFormatString="{0:dd MMM yyyy HH:mm}" />
                            <asp:BoundField DataField="BookingStatus"
                                HeaderText="Status" />
                        </Columns>
                    </asp:GridView>
                </div>
            </section>

            <section class="panel">
                <h2>Booking-Linked Payments</h2>
                <div class="table-wrap">
                    <asp:GridView ID="gvPayments" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="data-grid"
                        GridLines="None"
                        EmptyDataText="No booking-linked payments found.">
                        <Columns>
                            <asp:BoundField DataField="PaymentID"
                                HeaderText="Payment ID" />
                            <asp:BoundField DataField="BookingID"
                                HeaderText="Booking ID" />
                            <asp:BoundField DataField="AmountPaid"
                                HeaderText="Amount"
                                DataFormatString="R{0:N2}" />
                            <asp:BoundField DataField="PaymentStatus"
                                HeaderText="Status" />
                            <asp:BoundField DataField="PaymentDate"
                                HeaderText="Date"
                                DataFormatString="{0:dd MMM yyyy}" />
                        </Columns>
                    </asp:GridView>
                </div>
            </section>

        </asp:Panel>
    </main>
</div>
</form>
</body>
</html>
