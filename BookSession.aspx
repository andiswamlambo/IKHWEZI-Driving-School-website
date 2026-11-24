 <%@ Page Title="New Booking" Language="C#" AutoEventWireup="true" CodeBehind="BookSession.aspx.cs" Inherits="WebApplicationM3.BookSession" %>

<!DOCTYPE html>
<html lang="en">
<head id="head1" runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title><%: Page.Title %></title>
    <style>
        * { box-sizing: border-box; margin: 0; padding: 0; }
        body { font-family: 'Segoe UI', Arial, sans-serif; background: linear-gradient(180deg, #e9f1fc, #d9e5f5); min-height: 100vh; }
        .topbar { background: #0d2b6e; display: flex; align-items: center; justify-content: space-between; padding: 10px 24px; }
        .logo { text-decoration: none; display: flex; flex-direction: column; line-height: 1.1; }
        .logo-name { font-size: 22px; font-weight: 800; font-style: italic; color: #fff; letter-spacing: 1px; }
        .logo-name span { color: #e8283f; }
        .logo-sub { font-size: 9px; color: #c3d2ee; letter-spacing: 3px; }
        .topbar-right { display: flex; align-items: center; gap: 16px; }
        .user-name { color: #fff; font-size: 14px; font-weight: 600; }
        .avatar { width: 34px; height: 34px; border-radius: 50%; background: #e8283f; color: #fff; display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: 13px; position: relative; overflow: hidden; }
        .avatar .avatar-img { position: absolute; inset: 0; width: 100%; height: 100%; object-fit: cover; border-radius: 50%; }
        .layout { display: flex; gap: 20px; padding: 20px 24px; align-items: flex-start; }
        .sidebar { width: 235px; flex-shrink: 0; background: #fff; border-radius: 12px; padding: 14px; box-shadow: 0 4px 15px rgba(13,43,110,.08); }
        .side-item { display: flex; align-items: center; gap: 10px; padding: 10px 12px; margin-bottom: 6px; border-radius: 8px; color: #0d2b6e; font-size: 13.5px; font-weight: 600; text-decoration: none; transition: 0.2s; }
        .side-item svg { width: 18px; height: 18px; stroke: currentColor; stroke-width: 2; fill: none; stroke-linecap: round; stroke-linejoin: round; }
        .side-item:hover { background: #eef4ff; }
        .side-item.selected, .side-item.selected:hover { background: #0d2b6e; color: #fff; }
        .subscription { margin-top: 18px; background: #0d2b6e; color: #fff; border-radius: 10px; padding: 16px; text-align: center; }
        .sub-title { font-weight: 700; font-size: 15px; margin-bottom: 6px; }
        .subscription p { font-size: 11.5px; color: #c3d2ee; }
        .btn-upgrade { width: 100%; margin-top: 10px; padding: 9px; background: #e8283f; color: #fff; border: none; border-radius: 16px; font-weight: 700; font-size: 13px; cursor: pointer; }
        .main { flex: 1; }
        .page-header { display: flex; align-items: center; gap: 12px; margin-bottom: 14px; }
        .back-btn { width: 30px; height: 30px; border-radius: 50%; background: #fff; color: #0d2b6e; display: flex; align-items: center; justify-content: center; text-decoration: none; font-weight: 700; font-size: 16px; box-shadow: 0 2px 8px rgba(13,43,110,.08); }
        .page-title { color: #0d2b6e; font-size: 20px; font-weight: 700; }
        .wizard-card { background: #fff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 15px rgba(13,43,110,.08); }
        .wizard-header { background: #0d2b6e; color: #fff; padding: 20px 24px; }
        .wizard-header h2 { font-size: 22px; margin-bottom: 4px; }
        .wizard-header p { color: #c3d2ee; font-size: 13px; }
        .wizard-body { padding: 22px 24px; }
        .section-title { color: #0d2b6e; font-weight: 700; font-size: 14.5px; margin: 18px 0 12px; }
        .section-title.first { margin-top: 0; }
        .form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 14px 18px; }
        .form-grid.three { grid-template-columns: 1fr 1fr 1fr; }
        .f-label { display: block; font-size: 12.5px; color: #444; font-weight: 600; margin-bottom: 5px; }
        .f-input { width: 100%; padding: 10px 12px; border: 1px solid #ccd7ec; border-radius: 6px; font-size: 14px; }
        .f-input:focus { outline: none; border-color: #2f80ed; }
        .hint { font-size: 11.5px; color: #888; margin-top: 4px; }
        .ddl { width: 100%; padding: 10px; border: 1px solid #ccd7ec; border-radius: 6px; font-size: 14px; background: #fff; }
        .rbl label { margin-right: 22px; font-size: 13.5px; color: #333; }
        .rbl input { margin-right: 6px; }
        .gate-note { background: #eef4ff; border: 1px solid #d9e2f5; border-radius: 8px; padding: 12px 14px; font-size: 12.5px; color: #445; margin-bottom: 16px; }
        .prefill-note { background: #eef4ff; border: 1px solid #d9e2f5; border-radius: 8px; padding: 10px 14px; font-size: 12.5px; color: #445; margin-bottom: 14px; }
        .prefill-note a { color: #0d2b6e; font-weight: 700; text-decoration: underline; }
        .readonly-field { background: #f4f7fd; color: #333; cursor: not-allowed; }
        .error-label { color: #e8283f; font-weight: 600; font-size: 13px; display: block; margin-top: 12px; }
        .wizard-footer { display: flex; justify-content: space-between; margin-top: 24px; }
        .btn-back { background: #a9b4c4; color: #fff; border: none; border-radius: 20px; padding: 10px 34px; font-weight: 700; font-size: 14px; cursor: pointer; }
        .btn-back:hover { background: #8d99ac; }
        .btn-continue { background: #22b573; color: #fff; border: none; border-radius: 20px; padding: 10px 42px; font-weight: 700; font-size: 14px; cursor: pointer; }
        .btn-continue:hover { background: #1d9e63; }
        .slots-grid { display: grid; grid-template-columns: repeat(5, 1fr); gap: 12px; max-width: 580px; }
        .slot { display: block; text-align: center; padding: 10px 0; background: #fff; border: 1px solid #ccd7ec; border-radius: 6px; color: #0d2b6e; font-weight: 700; font-size: 14px; text-decoration: none; }
        .slot:hover { border-color: #2f80ed; }
        .slot.selected { background: #22b573; border-color: #22b573; color: #fff; }
        .slot.taken { background: #e8283f; border-color: #e8283f; color: #fff; pointer-events: none; opacity: .85; }
        .legend { display: flex; align-items: center; gap: 18px; border: 1px dashed #c9d6ee; border-radius: 8px; padding: 10px 14px; margin-top: 18px; font-size: 12.5px; color: #444; flex-wrap: wrap; max-width: 580px; }
        .lg { display: inline-block; width: 14px; height: 14px; border-radius: 3px; margin-right: 6px; vertical-align: -2px; }
        .lg-free { background: #fff; border: 1px solid #ccd7ec; }
        .lg-taken { background: #e8283f; }
        .lg-selected { background: #22b573; }
        .legend-counts { margin-left: auto; color: #667; }
        .list-box { border: 1px solid #ccd7ec; border-radius: 6px; font-size: 13.5px; padding: 8px; }
        .note-line { font-size: 13px; color: #555; margin-bottom: 14px; }
        .confirm-box { border: 1px solid #ccd7ec; background: #f7faff; border-radius: 8px; padding: 16px 20px; }
        .c-row { display: flex; justify-content: space-between; gap: 20px; padding: 9px 0; border-bottom: 1px dashed #dbe4f6; font-size: 13.5px; }
        .c-row:last-child { border-bottom: none; }
        .c-label { font-weight: 700; color: #0d2b6e; }
        .c-value { color: #333; font-weight: 600; text-align: right; }
        .success-wrap { text-align: center; padding: 30px 10px; }
        .success-icon { width: 64px; height: 64px; margin: 0 auto 16px; background: #22b573; color: #fff; border-radius: 50%; display: flex; align-items: center; justify-content: center; }
        .success-icon svg { width: 32px; height: 32px; stroke: currentColor; stroke-width: 2; fill: none; stroke-linecap: round; stroke-linejoin: round; }
        .success-wrap h3 { color: #0d2b6e; margin-bottom: 8px; }
        .success-wrap p { color: #444; font-size: 14px; }
        .success-sub { color: #667; font-size: 13px; margin-top: 8px; }
        .btn-dash { background: #0d2b6e; color: #fff; border: none; border-radius: 20px; padding: 10px 36px; font-weight: 700; margin-top: 18px; cursor: pointer; }
        @media (max-width: 950px) {
            .layout { flex-direction: column; }
            .sidebar { width: 100%; }
            .form-grid, .form-grid.three { grid-template-columns: 1fr; }
            .slots-grid { grid-template-columns: repeat(3, 1fr); }
        }
        .suggest-box { background: #fffbe8; border: 1px solid #f0e0a0; border-radius: 10px; padding: 16px; margin-bottom: 16px; }
        .suggest-title { font-size: 13.5px; font-weight: 700; color: #7a5c00; margin-bottom: 12px; }
        .suggest-label { font-size: 12.5px; color: #555; font-weight: 600; margin: 10px 0 8px; }
        .day-chips { display: flex; flex-wrap: wrap; gap: 8px; }
        .day-chip { background: #0d2b6e; color: #fff; border-radius: 16px; padding: 7px 14px; font-size: 12.5px; font-weight: 700; text-decoration: none; }
        .day-chip:hover { background: #1a4199; color: #fff; text-decoration: none; }
        .day-chip.empty { background: #e3e8f0; color: #99a3b5; pointer-events: none; }
        .package-summary { background: #f0f7ff; border: 1px solid #b8d4f0; border-radius: 8px; padding: 14px 18px; margin-top: 14px; display: flex; justify-content: space-between; align-items: center; }
        .pkg-info { display: flex; flex-direction: column; gap: 4px; }
        .pkg-name { font-size: 14px; font-weight: 700; color: #0d2b6e; }
        .pkg-stats { font-size: 13px; color: #444; display: flex; gap: 16px; }
        .pkg-stat strong { color: #22b573; }
        .pkg-stat.warn strong { color: #e8283f; }
        .existing-package-banner { display:block; background:#fff8e1; border:1px solid #f0e0a0; border-radius:8px; padding:12px 16px; font-size:13px; color:#7a5c00; margin-bottom:14px; line-height:1.5; }
        .reservation-banner { display:block; background:#e8f4ff; border:1px solid #b8d4f0; border-radius:8px; padding:14px 18px; font-size:13px; color:#0d2b6e; margin-bottom:14px; line-height:1.6; }
        .reservation-banner strong { display: flex; align-items: center; gap: 6px; }
        .reservation-banner strong svg { width: 16px; height: 16px; stroke: currentColor; stroke-width: 2; fill: none; }
        .reservation-banner .pay-btn { display:inline-block; margin-top:10px; padding:10px 26px; background:#22b573; color:#fff; border-radius:20px; font-weight:800; text-decoration:none; }
        .reservation-banner .pay-btn:hover { background:#1d9e63; color:#fff; text-decoration: none; }
        .expired-banner { display:block; background:#fff0f2; border:1px solid #f3c2c9; border-radius:8px; padding:14px 18px; font-size:13px; color:#a3141f; margin-bottom:14px; line-height:1.6; }
        .expired-banner strong { display: flex; align-items: center; gap: 6px; }
        .expired-banner strong svg { width: 16px; height: 16px; stroke: currentColor; stroke-width: 2; fill: none; }
        
        /* NEW SUCCESS BANNER CSS */
        .success-banner {
            background: #effaf4;
            border: 1px solid #bfe8d4;
            color: #14663f;
            padding: 16px 20px;
            border-radius: 8px;
            margin-bottom: 20px;
            font-size: 14px;
            font-weight: 600;
            display: flex;
            align-items: center;
            gap: 12px;
            box-shadow: 0 4px 10px rgba(34, 181, 115, 0.1);
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">

        <div class="topbar">
            <a href="~/StudentDashboard" runat="server" class="logo">
                <span class="logo-name">IKHWEZI<span>.</span></span>
                <span class="logo-sub">DRIVING SCHOOL</span>
            </a>
            <div class="topbar-right">
                <span class="user-name"><asp:Literal ID="litStudentName" runat="server" /></span>
                <span class="avatar">
                    <asp:Literal ID="litInitials" runat="server" />
                    <asp:Image ID="imgTopAvatar" runat="server" Visible="false" CssClass="avatar-img" AlternateText="Profile photo" />
                </span>
            </div>
        </div>

        <div class="layout">
            <aside class="sidebar">
                <a href="~/StudentDashboard" runat="server" class="side-item">
                    <svg viewBox="0 0 24 24"><rect x="3" y="3" width="7" height="7"></rect><rect x="14" y="3" width="7" height="7"></rect><rect x="14" y="14" width="7" height="7"></rect><rect x="3" y="14" width="7" height="7"></rect></svg>
                    Dashboard
                </a>
                <a href="~/PersonalInfo" runat="server" class="side-item">
                    <svg viewBox="0 0 24 24"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path><circle cx="12" cy="7" r="4"></circle></svg>
                    Personal Info
                </a>
                <a href="~/Offers" runat="server" class="side-item">
                    <svg viewBox="0 0 24 24"><polyline points="20 12 20 22 4 22 4 12"></polyline><rect x="2" y="7" width="20" height="5"></rect><line x1="12" y1="22" x2="12" y2="7"></line><path d="M12 7H7.5a2.5 2.5 0 0 1 0-5C11 2 12 7 12 7z"></path><path d="M12 7h4.5a2.5 2.5 0 0 0 0-5C13 2 12 7 12 7z"></path></svg>
                    Offers
                </a>
                <a href="~/BookSession" runat="server" class="side-item selected">
                    <svg viewBox="0 0 24 24"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect><line x1="16" y1="2" x2="16" y2="6"></line><line x1="8" y1="2" x2="8" y2="6"></line><line x1="3" y1="10" x2="21" y2="10"></line></svg>
                    Make Booking
                </a>
                <a href="#" class="side-item">
                    <svg viewBox="0 0 24 24"><path d="M3 18v-6a9 9 0 0 1 18 0v6"></path><path d="M21 19a2 2 0 0 1-2 2h-1a2 2 0 0 1-2-2v-3a2 2 0 0 1 2-2h3zM3 19a2 2 0 0 0 2 2h1a2 2 0 0 0 2-2v-3a2 2 0 0 0-2-2H3z"></path></svg>
                    Support
                </a>
                <asp:LinkButton ID="btnLogoutSide" runat="server" CssClass="side-item" OnClick="btnLogoutSide_Click">
                    <svg viewBox="0 0 24 24"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"></path><polyline points="16 17 21 12 16 7"></polyline><line x1="21" y1="12" x2="9" y2="12"></line></svg>
                    Logout
                </asp:LinkButton>
                <div class="subscription">
                    <div class="sub-title">Subscription</div>
                    <p>Explore 20+ features with Premium Membership</p>
                    <asp:Button ID="btnUpgrade" runat="server" Text="Upgrade Now" CssClass="btn-upgrade" OnClientClick="alert('Premium coming soon!'); return false;" />
                </div>
            </aside>

            <main class="main">
                <div class="page-header">
                    <a href="javascript:history.back()" class="back-btn">&lsaquo;</a>
                    <h2 class="page-title">Make a Booking</h2>
                </div>

                <div class="wizard-card">
                    <div class="wizard-header">
                        <h2><asp:Literal ID="litStepTitle" runat="server" /></h2>
                        <p><asp:Literal ID="litStepSubtitle" runat="server" /></p>
                    </div>

                    <div class="wizard-body">
                        
                        <!-- NEW SUCCESS BANNER -->
                        <asp:Panel ID="pnlSuccessBanner" runat="server" CssClass="success-banner" Visible="false">
                            <svg viewBox="0 0 24 24" style="width:24px; height:24px; stroke:currentColor; stroke-width:2.5; fill:none; stroke-linecap:round; stroke-linejoin:round; flex-shrink:0;">
                                <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path>
                                <polyline points="22 4 12 14.01 9 11.01"></polyline>
                            </svg>
                            <span>Payment successful! Your package is now active. Please select a date and time below to book your first lesson.</span>
                        </asp:Panel>

                        <asp:Literal ID="litStaffBlocked" runat="server" Visible="false" />
                        
                        <asp:MultiView ID="mvWizard" runat="server" ActiveViewIndex="0">

                            <!-- ===== STEP 1: BEFORE YOU BEGIN ===== -->
                            <asp:View ID="vGate" runat="server">
                                <div class="gate-note">
                                    Welcome! Before your first booking we need your full contact details for our records.
                                    You also need a <strong>valid learner's licence</strong> for practical driving lessons.
                                </div>
                                <div class="section-title first">Complete Your Personal Details</div>
                                <div class="form-grid">
                                    <div>
                                        <label class="f-label" for="txtDob">Date of Birth</label>
                                        <asp:TextBox ID="txtDob" runat="server" CssClass="f-input" TextMode="Date" />
                                    </div>
                                    <div>
                                        <label class="f-label" for="txtStreet">Street Address</label>
                                        <asp:TextBox ID="txtStreet" runat="server" CssClass="f-input" placeholder="e.g. 162 Varsity Drive" MaxLength="150" />
                                    </div>
                                    <div>
                                        <label class="f-label" for="txtSuburb">Suburb</label>
                                        <asp:TextBox ID="txtSuburb" runat="server" CssClass="f-input" placeholder="e.g. Reservoir Hills" MaxLength="80" />
                                    </div>
                                    <div>
                                        <label class="f-label" for="txtCity">City</label>
                                        <asp:TextBox ID="txtCity" runat="server" CssClass="f-input" placeholder="e.g. Durban" MaxLength="80" />
                                    </div>
                                </div>
                                <div class="section-title">Learner's Licence</div>
                                <asp:RadioButtonList ID="rblLearners" runat="server" CssClass="rbl" RepeatDirection="Horizontal">
                                    <asp:ListItem Value="Yes">Yes, I have a valid learner's licence</asp:ListItem>
                                    <asp:ListItem Value="No">No, I don't have one yet</asp:ListItem>
                                </asp:RadioButtonList>
                                <div class="hint">No learner's licence? We'll take you to book a K53 theory class instead.</div>
                                <asp:Label ID="lblGateError" runat="server" CssClass="error-label" Visible="false" />
                                <div class="wizard-footer">
                                    <asp:Button ID="btnGateBack" runat="server" Text="Back" CssClass="btn-back" OnClick="btnGateBack_Click" />
                                    <asp:Button ID="btnGateNext" runat="server" Text="Continue" CssClass="btn-continue" OnClick="btnGateNext_Click" />
                                </div>
                            </asp:View>

                            <!-- ===== STEP 2: STUDENT DETAILS + PACKAGE ===== -->
                            <asp:View ID="vDetails" runat="server">
                                <div class="section-title first">Student Details</div>
                                <div class="prefill-note">
                                    These details are from your account. To change them, go to
                                    <a href="~/PersonalInfo" runat="server">Personal Info</a>.
                                </div>
                                <div class="form-grid">
                                    <div>
                                        <label class="f-label" for="txtFirstName">First Name</label>
                                        <asp:TextBox ID="txtFirstName" runat="server" CssClass="f-input readonly-field" placeholder="First Name" MaxLength="50" ReadOnly="true" />
                                    </div>
                                    <div>
                                        <label class="f-label" for="txtSurname">Surname</label>
                                        <asp:TextBox ID="txtSurname" runat="server" CssClass="f-input readonly-field" placeholder="Surname" MaxLength="50" ReadOnly="true" />
                                    </div>
                                    <div>
                                        <label class="f-label" for="txtPhone">Phone (10 digits)</label>
                                        <asp:TextBox ID="txtPhone" runat="server" CssClass="f-input readonly-field" placeholder="e.g. 0723456789" MaxLength="12" ReadOnly="true" />
                                        <div class="hint">To update your phone number, visit Personal Info.</div>
                                    </div>
                                    <div>
                                        <label class="f-label" for="txtEmail">Email Address</label>
                                        <asp:TextBox ID="txtEmail" runat="server" CssClass="f-input readonly-field" placeholder="student@email.com" TextMode="Email" ReadOnly="true" />
                                    </div>
                                </div>

                                <asp:Label ID="lblExistingPackageInfo" runat="server" Visible="false" CssClass="existing-package-banner" />

                                <asp:Panel ID="pnlReservationActive" runat="server" Visible="false" CssClass="reservation-banner">
                                    <strong>
                                        <svg viewBox="0 0 24 24"><circle cx="12" cy="12" r="10"></circle><polyline points="12 6 12 12 16 14"></polyline></svg>
                                        Package reserved
                                    </strong>
                                    <p style="margin:6px 0;"><asp:Literal ID="litReservationMsg" runat="server" /></p>
                                </asp:Panel>

                                <asp:Panel ID="pnlReservationExpired" runat="server" Visible="false" CssClass="expired-banner">
                                    <strong>
                                        <svg viewBox="0 0 24 24"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"></path><line x1="12" y1="9" x2="12" y2="13"></line><line x1="12" y1="17" x2="12.01" y2="17"></line></svg>
                                        Previous reservation expired
                                    </strong>
                                    <p style="margin:6px 0;">
                                        Your previous package reservation expired because payment was not received in time.
                                        Please choose a package again below to start a new 3-day reservation.
                                    </p>
                                </asp:Panel>

                                <div class="section-title">Select Package</div>
                                <asp:DropDownList ID="ddlPackage" runat="server" CssClass="ddl" AutoPostBack="true" OnSelectedIndexChanged="ddlPackage_SelectedIndexChanged" />

                                <asp:Panel ID="pnlPackageSummary" runat="server" CssClass="package-summary" Visible="false">
                                    <div class="pkg-info">
                                        <div class="pkg-name"><asp:Literal ID="litPkgName" runat="server" /></div>
                                        <div class="pkg-stats">
                                            <span class="pkg-stat">Completed: <strong><asp:Literal ID="litPkgCompleted" runat="server" /></strong></span>
                                            <span class="pkg-stat">Upcoming: <strong><asp:Literal ID="litPkgUpcoming" runat="server" /></strong></span>
                                            <span class="pkg-stat warn">Remaining: <strong><asp:Literal ID="litPkgRemaining" runat="server" /></strong></span>
                                        </div>
                                    </div>
                                </asp:Panel>

                                <asp:Label ID="lblDetailsError" runat="server" CssClass="error-label" Visible="false" />
                                <div class="wizard-footer">
                                    <asp:Button ID="btnDetailsBack" runat="server" Text="Back" CssClass="btn-back" OnClick="btnDetailsBack_Click" />
                                    <asp:Button ID="btnDetailsNext" runat="server" Text="Continue" CssClass="btn-continue" OnClick="btnDetailsNext_Click" />
                                </div>
                            </asp:View>

                            <!-- ===== STEP 3: SCHEDULE ===== -->
                            <asp:View ID="vSchedule" runat="server">
                                <div class="section-title first">Date, Duration, Lesson Type</div>
                                <div class="form-grid three">
                                    <div>
                                        <label class="f-label" for="txtDate">Select Date</label>
                                        <asp:TextBox ID="txtDate" runat="server" CssClass="f-input" TextMode="Date" AutoPostBack="true" OnTextChanged="txtDate_TextChanged" />
                                    </div>
                                    <div>
                                        <label class="f-label" for="ddlDuration">Duration</label>
                                        <asp:DropDownList ID="ddlDuration" runat="server" CssClass="ddl" AutoPostBack="true" OnSelectedIndexChanged="ddlDuration_SelectedIndexChanged">
                                            <asp:ListItem Value="1" Selected="True">1 hour</asp:ListItem>
                                            <asp:ListItem Value="2">2 hours</asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                    <div>
                                        <label class="f-label" for="ddlLessonType">Lesson Type</label>
                                        <asp:DropDownList ID="ddlLessonType" runat="server" CssClass="ddl">
                                            <asp:ListItem Selected="True">Manual Driving</asp:ListItem>
                                            <asp:ListItem>Automatic Driving</asp:ListItem>
                                            <asp:ListItem>Defensive Driving</asp:ListItem>
                                            <asp:ListItem>Advanced Driving</asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                </div>

                                <div class="section-title">Select a Time Slot</div>
                                <div class="slots-grid">
                                    <asp:LinkButton ID="slot0700" runat="server" CssClass="slot" CommandArgument="07:00" OnCommand="Slot_Command">07:00</asp:LinkButton>
                                    <asp:LinkButton ID="slot0800" runat="server" CssClass="slot" CommandArgument="08:00" OnCommand="Slot_Command">08:00</asp:LinkButton>
                                    <asp:LinkButton ID="slot0900" runat="server" CssClass="slot" CommandArgument="09:00" OnCommand="Slot_Command">09:00</asp:LinkButton>
                                    <asp:LinkButton ID="slot1000" runat="server" CssClass="slot" CommandArgument="10:00" OnCommand="Slot_Command">10:00</asp:LinkButton>
                                    <asp:LinkButton ID="slot1100" runat="server" CssClass="slot" CommandArgument="11:00" OnCommand="Slot_Command">11:00</asp:LinkButton>
                                    <asp:LinkButton ID="slot1200" runat="server" CssClass="slot" CommandArgument="12:00" OnCommand="Slot_Command">12:00</asp:LinkButton>
                                    <asp:LinkButton ID="slot1300" runat="server" CssClass="slot" CommandArgument="13:00" OnCommand="Slot_Command">13:00</asp:LinkButton>
                                    <asp:LinkButton ID="slot1400" runat="server" CssClass="slot" CommandArgument="14:00" OnCommand="Slot_Command">14:00</asp:LinkButton>
                                    <asp:LinkButton ID="slot1500" runat="server" CssClass="slot" CommandArgument="15:00" OnCommand="Slot_Command">15:00</asp:LinkButton>
                                    <asp:LinkButton ID="slot1600" runat="server" CssClass="slot" CommandArgument="16:00" OnCommand="Slot_Command">16:00</asp:LinkButton>
                                </div>

                                <div class="legend">
                                    <span><span class="lg lg-free"></span>Available</span>
                                    <span><span class="lg lg-taken"></span>Taken</span>
                                    <span><span class="lg lg-selected"></span>Selected</span>
                                    <span class="legend-counts"><asp:Literal ID="litSlotCounts" runat="server" /></span>
                                </div>

                                <asp:Label ID="lblScheduleError" runat="server" CssClass="error-label" Visible="false" />

                                <div class="wizard-footer">
                                    <asp:Button ID="btnScheduleBack" runat="server" Text="Back" CssClass="btn-back" OnClick="btnScheduleBack_Click" />
                                    <asp:Button ID="btnScheduleNext" runat="server" Text="Continue" CssClass="btn-continue" OnClick="btnScheduleNext_Click" />
                                </div>
                            </asp:View>

                            <!-- ===== STEP 4: INSTRUCTOR & VEHICLE ===== -->
                            <asp:View ID="vAssign" runat="server">
                                <p class="note-line"><asp:Literal ID="litAvailNote" runat="server" /></p>
                                <asp:Panel ID="pnlNoInstructors" runat="server" CssClass="suggest-box" Visible="false">
                                    <div class="suggest-title">
                                        No instructors free at that exact time — but here's when they <u>are</u> available.
                                        Click a green time to continue instantly:
                                    </div>
                                    <div class="suggest-label">Available times on <strong><asp:Literal ID="litSuggestDate" runat="server" /></strong>:</div>
                                    <div class="slots-grid">
                                        <asp:Repeater ID="repSlotSuggestions" runat="server">
                                            <ItemTemplate>
                                                <asp:LinkButton runat="server"
                                                    CssClass='<%# Eval("CssClass") %>'
                                                    CommandArgument='<%# Eval("TimeArg") %>'
                                                    Enabled='<%# Eval("IsAvailable") %>'
                                                    OnCommand="SuggestionSlot_Command"
                                                    Text='<%# Eval("TimeText") %>' />
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </div>
                                    <div class="suggest-label">Or check the coming days:</div>
                                    <div class="day-chips">
                                        <asp:Repeater ID="repDaySuggestions" runat="server">
                                            <ItemTemplate>
                                                <asp:LinkButton runat="server"
                                                    CssClass='<%# Eval("CssClass") %>'
                                                    CommandArgument='<%# Eval("DateArg") %>'
                                                    Enabled='<%# Eval("HasAvailability") %>'
                                                    OnCommand="SuggestionDay_Command"
                                                    Text='<%# Eval("DateText") %>' />
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </div>
                                </asp:Panel>
                                <div class="section-title first">Select Instructor:</div>
                                <asp:ListBox ID="lstInstructors" runat="server" CssClass="list-box" Rows="7" Width="100%" SelectionMode="Single" />

                                <div class="section-title">Select Vehicle:</div>
                                <asp:ListBox ID="lstVehicles" runat="server" CssClass="list-box" Rows="5" Width="80%" SelectionMode="Single" />

                                <asp:Label ID="lblAssignError" runat="server" CssClass="error-label" Visible="false" />

                                <div class="wizard-footer">
                                    <asp:Button ID="btnAssignBack" runat="server" Text="Back" CssClass="btn-back" OnClick="btnAssignBack_Click" />
                                    <asp:Button ID="btnAssignNext" runat="server" Text="Continue" CssClass="btn-continue" OnClick="btnAssignNext_Click" />
                                </div>
                            </asp:View>

                            <!-- ===== STEP 5: CONFIRM ===== -->
                            <asp:View ID="vConfirm" runat="server">
                                <div class="confirm-box">
                                    <asp:Literal ID="litConfirmDetails" runat="server" />
                                </div>

                                <asp:Label ID="lblConfirmError" runat="server" CssClass="error-label" Visible="false" />

                                <div class="wizard-footer">
                                    <asp:Button ID="btnConfirmBack" runat="server" Text="Back" CssClass="btn-back" OnClick="btnConfirmBack_Click" />
                                    <asp:Button ID="btnConfirm" runat="server" Text="Confirm Booking" CssClass="btn-continue" OnClick="btnConfirm_Click" />
                                </div>
                            </asp:View>

                            <!-- ===== STEP 6: SUCCESS ===== -->
                            <asp:View ID="vSuccess" runat="server">
                                <div class="success-wrap">
                                    <div class="success-icon">
                                        <svg viewBox="0 0 24 24"><polyline points="20 6 9 17 4 12"></polyline></svg>
                                    </div>
                                    <asp:Literal ID="litSuccess" runat="server" />
                                    <br />
                                    <asp:Button ID="btnGoDashboard" runat="server" Text="Back to My Dashboard" CssClass="btn-dash" OnClick="btnGoDashboard_Click" />
                                </div>
                            </asp:View>

                        </asp:MultiView>
                    </div>
                </div>
            </main>
        </div>

    </form>
</body>
</html>