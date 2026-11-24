 <%@ Page Title="Learners Classes" Language="C#" AutoEventWireup="true" CodeBehind="LearnersClass.aspx.cs" Inherits="WebApplicationM3.LearnersClass" %>

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
        .avatar { width: 34px; height: 34px; border-radius: 50%; background: #e8283f; color: #fff; display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: 13px; }
        
        .layout { display: flex; gap: 20px; padding: 20px 24px; align-items: flex-start; }
        .sidebar { width: 235px; flex-shrink: 0; background: #fff; border-radius: 12px; padding: 14px; box-shadow: 0 4px 15px rgba(13,43,110,.08); }
        
        /* Updated Side Items to handle SVG */
        .side-item { display: flex; align-items: center; gap: 10px; padding: 10px 12px; margin-bottom: 6px; border-radius: 8px; color: #0d2b6e; font-size: 13.5px; font-weight: 600; text-decoration: none; transition: 0.2s; }
        .side-item svg { width: 18px; height: 18px; stroke: currentColor; stroke-width: 2; fill: none; stroke-linecap: round; stroke-linejoin: round; }
        .side-item:hover { background: #eef4ff; }
        .side-item.selected, .side-item.selected:hover { background: #0d2b6e; color: #fff; }
        
        .main { flex: 1; background: #fff; border-radius: 12px; padding: 22px; box-shadow: 0 4px 15px rgba(13,43,110,.08); }
        .page-title { color: #0d2b6e; font-size: 20px; font-weight: 700; margin-bottom: 6px; }
        .tagline { font-size: 13px; color: #555; margin-bottom: 18px; }
        
        .codes-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 14px; margin-bottom: 22px; }
        .code-card { background: #eef2fa; border-radius: 10px; padding: 16px; border-top: 3px solid #0d2b6e; }
        .code-card h4 { color: #0d2b6e; font-size: 15px; margin-bottom: 6px; }
        .code-card p { font-size: 12.5px; color: #555; }
        
        .section-title { color: #0d2b6e; font-weight: 700; font-size: 14.5px; margin: 18px 0 12px; }
        .form-grid { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 14px 18px; }
        .f-label { display: block; font-size: 12.5px; color: #444; font-weight: 600; margin-bottom: 5px; }
        .f-input, .ddl { width: 100%; padding: 10px 12px; border: 1px solid #ccd7ec; border-radius: 6px; font-size: 14px; background: #fff; }
        
        .btn-book { background: #22b573; color: #fff; border: none; border-radius: 20px; padding: 10px 42px; font-weight: 700; font-size: 14px; cursor: pointer; margin-top: 18px; }
        .btn-book:hover { background: #1d9e63; }
        .btn-cancel { background: #e8283f; color: #fff; border: none; border-radius: 20px; padding: 10px 34px; font-weight: 700; font-size: 14px; cursor: pointer; margin-top: 18px; margin-left: 10px; }
        .btn-cancel:hover { background: #c81e33; }
        .btn-cancel-grey { background: #a9b4c4; color: #fff; border: none; border-radius: 20px; padding: 10px 34px; font-weight: 700; font-size: 14px; cursor: pointer; margin-top: 18px; margin-left: 10px; }
        .btn-cancel-grey:hover { background: #8d99ac; }
        
        .error-label { color: #e8283f; font-weight: 600; font-size: 13px; display: block; margin-top: 12px; }
        
        .notice-box { background: #eef4ff; border: 1px solid #c9d9f3; border-left: 4px solid #0d2b6e; border-radius: 8px; padding: 14px 18px; margin-bottom: 18px; font-size: 13.5px; color: #22355f; line-height: 1.6; }
        .success-box { border: 1px solid #bfe8d4; background: #effaf4; border-radius: 8px; padding: 16px 20px; margin-top: 16px; color: #14663f; font-size: 14px; font-weight: 600; }
        
        .blocked-box { background:#fdf7ec; border:1px solid #f0d999; border-left:4px solid #e8890c; border-radius:8px; padding:20px 22px; margin-bottom:20px; font-size:14px; color:#5a4400; line-height:1.7; }
        .blocked-box .label { font-size:11px; font-weight:800; letter-spacing:2px; color:#e8890c; text-transform:uppercase; margin-bottom:8px; }
        .blocked-box h3 { font-size:16px; font-weight:800; color:#7a5a00; margin-bottom:10px; }
        .blocked-box p { margin:0 0 12px 0; }
        
        .blocked-contact { background:#fff; border-radius:8px; padding:12px 16px; font-size:13px; color:#7a5a00; margin-top:14px; display: flex; flex-direction: column; gap: 8px; }
        .blocked-contact strong { color: #5a4400; }
        .blocked-contact-row { display: flex; align-items: center; gap: 8px; }
        .blocked-contact-row svg { width: 14px; height: 14px; stroke: #0d2b6e; stroke-width: 2; fill: none; }
        .blocked-contact a { color:#0d2b6e; font-weight:700; text-decoration:none; }
        
        .already-booked-box { background:#fef3e7; border:1px solid #f5cfa0; border-left:4px solid #e8890c; border-radius:8px; padding:20px 22px; margin-bottom:20px; font-size:14px; color:#6b4500; line-height:1.7; }
        .already-booked-box h3 { font-size:16px; font-weight:800; color:#8a5a00; margin-bottom:10px; }
        .already-booked-box .ref { background:#fff; border-radius:8px; padding:14px 18px; font-size:14px; font-weight:700; color:#0d2b6e; margin:14px 0; }
        
        .pay-online-box { background:#eef4ff; border:1px solid #c9d9f3; border-radius:8px; padding:16px 20px; margin-top:16px; }
        .pay-online-box h4 { color:#0d2b6e; font-size:14px; margin-bottom:8px; display: flex; align-items: center; gap: 6px; }
        .pay-online-box h4 svg { width: 16px; height: 16px; stroke: currentColor; stroke-width: 2; fill: none; }
        .pay-online-box p { font-size:13px; color:#445; line-height:1.6; margin-bottom:10px; }
        
        .btn-pay { display:inline-flex; align-items: center; gap: 8px; padding:12px 30px; background:#22b573; color:#fff; border-radius:24px; font-weight:800; font-size:14px; text-decoration:none; }
        .btn-pay svg { width: 16px; height: 16px; stroke: currentColor; stroke-width: 2; fill: none; }
        .btn-pay:hover { background:#1d9e63; color:#fff; text-decoration: none; }
        
        @media (max-width: 950px) { .layout { flex-direction: column; } .sidebar { width: 100%; } .form-grid { grid-template-columns: 1fr; } }
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
                <span class="avatar"><asp:Literal ID="litInitials" runat="server" /></span>
            </div>
        </div>

        <div class="layout">
            <aside class="sidebar">
                <!-- Dashboard Icon -->
                <a href="~/StudentDashboard" runat="server" class="side-item">
                    <svg viewBox="0 0 24 24"><rect x="3" y="3" width="7" height="7"></rect><rect x="14" y="3" width="7" height="7"></rect><rect x="14" y="14" width="7" height="7"></rect><rect x="3" y="14" width="7" height="7"></rect></svg>
                    Dashboard
                </a>
                
                <!-- User Icon -->
                <a href="~/PersonalInfo" runat="server" class="side-item">
                    <svg viewBox="0 0 24 24"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path><circle cx="12" cy="7" r="4"></circle></svg>
                    Personal Info
                </a>
                
                <!-- Gift Icon -->
                <a href="~/Offers" runat="server" class="side-item">
                    <svg viewBox="0 0 24 24"><polyline points="20 12 20 22 4 22 4 12"></polyline><rect x="2" y="7" width="20" height="5"></rect><line x1="12" y1="22" x2="12" y2="7"></line><path d="M12 7H7.5a2.5 2.5 0 0 1 0-5C11 2 12 7 12 7z"></path><path d="M12 7h4.5a2.5 2.5 0 0 0 0-5C13 2 12 7 12 7z"></path></svg>
                    Offers
                </a>
                
                <!-- Calendar Icon -->
                <a href="~/BookSession" runat="server" class="side-item">
                    <svg viewBox="0 0 24 24"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect><line x1="16" y1="2" x2="16" y2="6"></line><line x1="8" y1="2" x2="8" y2="6"></line><line x1="3" y1="10" x2="21" y2="10"></line></svg>
                    Make Booking
                </a>
                
                <!-- Book Icon -->
                <a href="~/LearnersClass" runat="server" class="side-item selected">
                    <svg viewBox="0 0 24 24"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20"></path><path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z"></path></svg>
                    Learners Classes
                </a>
                
                <!-- Headphones Icon -->
                <a href="~/Support" runat="server" class="side-item">
                    <svg viewBox="0 0 24 24"><path d="M3 18v-6a9 9 0 0 1 18 0v6"></path><path d="M21 19a2 2 0 0 1-2 2h-1a2 2 0 0 1-2-2v-3a2 2 0 0 1 2-2h3zM3 19a2 2 0 0 0 2 2h1a2 2 0 0 0 2-2v-3a2 2 0 0 0-2-2H3z"></path></svg>
                    Support
                </a>
                
                <!-- Logout Icon -->
                <asp:LinkButton ID="btnLogoutSide" runat="server" CssClass="side-item" OnClick="btnLogoutSide_Click">
                    <svg viewBox="0 0 24 24"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"></path><polyline points="16 17 21 12 16 7"></polyline><line x1="21" y1="12" x2="9" y2="12"></line></svg>
                    Logout
                </asp:LinkButton>
            </aside>

            <main class="main">
                <h2 class="page-title">Learner's Licence Classes</h2>
                <p class="tagline">
                    You need a learner's licence before practical driving lessons.
                    Book a K53 theory class below — we'll automatically assign an available instructor.
                </p>

                <!-- BLOCKED: student has an active practical package -->
                <asp:Panel ID="pnlBlocked" runat="server" Visible="false" CssClass="blocked-box">
                    <div class="label">Programme Notice</div>
                    <h3>Learners-licence classes are not available on your account</h3>
                    <p>
                        You are currently enrolled in a <strong>practical driving lesson package</strong>.
                        To keep your training programme aligned and avoid scheduling conflicts,
                        learners-licence theory bookings are not available while a lesson package
                        is active on your account.
                    </p>
                    <p>
                        If you need to switch to the learners-licence programme, or if you believe
                        this is shown in error, please contact reception.
                    </p>
                    <div class="blocked-contact">
                        <strong>Reception — IKHWEZI Driving School</strong>
                        <div class="blocked-contact-row">
                            <svg viewBox="0 0 24 24"><path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z"></path></svg>
                            <a href="tel:+27835448544">+27 83 544 8544</a>
                        </div>
                        <div class="blocked-contact-row">
                            <svg viewBox="0 0 24 24"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"></path><polyline points="22,6 12,13 2,6"></polyline></svg>
                            <a href="mailto:info@khwezidrivingschool.co.za">info@khwezidrivingschool.co.za</a>
                        </div>
                    </div>
                </asp:Panel>

                <!-- ALREADY BOOKED / COMPLETED -->
                <asp:Panel ID="pnlAlreadyBooked" runat="server" Visible="false" CssClass="already-booked-box">
                    <h3><asp:Literal ID="litAlreadyBookedTitle" runat="server" /></h3>
                    <p><asp:Literal ID="litAlreadyBookedMessage" runat="server" /></p>
                    <div class="ref">
                        Booking Reference: <strong>#<asp:Literal ID="litExistingBookingId" runat="server" /></strong><br/>
                        <asp:Literal ID="litExistingBookingInfo" runat="server" />
                    </div>
                    <p>
                        Only one learners-licence class is permitted per student. If you need to change
                        your date, or if you have any questions, please contact reception.
                    </p>
                    <div class="blocked-contact">
                        <strong>Reception — IKHWEZI Driving School</strong>
                        <div class="blocked-contact-row">
                            <svg viewBox="0 0 24 24"><path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z"></path></svg>
                            <a href="tel:+27835448544">+27 83 544 8544</a>
                        </div>
                        <div class="blocked-contact-row">
                            <svg viewBox="0 0 24 24"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"></path><polyline points="22,6 12,13 2,6"></polyline></svg>
                            <a href="mailto:info@khwezidrivingschool.co.za">info@khwezidrivingschool.co.za</a>
                        </div>
                    </div>

                    <!-- CANCEL LEARNERS BOOKING PANEL -->
                    <asp:Panel ID="pnlCancelLearners" runat="server" Visible="false"
                               style="margin-top:16px;padding:16px 18px;background:#fff7e0;
                                      border:1px solid #f0d999;border-radius:8px;">
                        <div style="font-weight:800;color:#7a5a00;margin-bottom:8px;display:flex;align-items:center;gap:6px;">
                            <svg style="width:16px;height:16px;stroke:currentColor;stroke-width:2;fill:none;" viewBox="0 0 24 24"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect><line x1="16" y1="2" x2="16" y2="6"></line><line x1="8" y1="2" x2="8" y2="6"></line><line x1="3" y1="10" x2="21" y2="10"></line></svg>
                            Need to cancel or change this booking?
                        </div>
                        <div style="font-size:13px;color:#5a4400;line-height:1.6;margin-bottom:12px;">
                            Because your class is still more than 3 working days away, you can cancel
                            it here and receive a full refund within 3 working days. If you'd rather
                            keep the booking, simply close this page.
                        </div>

                        <!-- Trigger button -->
                        <asp:LinkButton ID="btnCancelLearnersBooking" runat="server"
                            CssClass="btn-cancel"
                            OnClick="btnCancelLearnersBooking_Click">
                            Cancel my learners booking
                        </asp:LinkButton>

                        <!-- Inline confirmation (no browser pop-up) -->
                        <asp:Panel ID="pnlConfirmLearnersCancel" runat="server" Visible="false"
                                   style="margin-top:14px;padding:14px 16px;background:#fff;
                                          border:1px solid #f0d999;border-left:4px solid #e8890c;
                                          border-radius:8px;color:#5a4400;">
                            <div style="font-weight:700;margin-bottom:10px;">
                                Are you sure you want to cancel this learners class and request a refund?
                                This cannot be undone.
                            </div>
                            <asp:LinkButton ID="btnConfirmYes" runat="server"
                                CssClass="btn-cancel"
                                OnClick="btnConfirmYes_Click">
                                Yes, cancel it
                            </asp:LinkButton>
                            <asp:LinkButton ID="btnConfirmNo" runat="server"
                                CssClass="btn-cancel-grey"
                                OnClick="btnConfirmNo_Click">
                                No, keep it
                            </asp:LinkButton>
                        </asp:Panel>
                    </asp:Panel>
                </asp:Panel>

                <div class="codes-grid">
                    <div class="code-card">
                        <h4>Code 08 — Light Motor Vehicle</h4>
                        <p>Cars and light vehicles. Minimum age 17 for the learner's test.</p>
                    </div>
                    <div class="code-card">
                        <h4>Code 10 — Medium Truck</h4>
                        <p>Medium commercial vehicles up to 16 000 kg GVM.</p>
                    </div>
                    <div class="code-card">
                        <h4>The K53 Theory Class</h4>
                        <p>A 1-hour structured session covering road signs, rules and vehicle controls.</p>
                    </div>
                </div>

                <asp:Panel ID="pnlNotice" runat="server" CssClass="notice-box" Visible="false">
                    <strong>Practical bookings are locked for now.</strong><br />
                    You told us you don't have a learner's licence yet, so practical
                    lesson packages are unavailable. Book and pass your K53 theory
                    class first &mdash; then click the button below to unlock practical bookings.
                    <br />
                    <asp:LinkButton ID="btnDeclareLicence" runat="server" OnClick="btnDeclareLicence_Click"
                        style="display:inline-block;margin-top:12px;background:#22b573;color:#fff;border:none;border-radius:16px;padding:9px 24px;font-weight:700;font-size:13px;cursor:pointer;text-decoration:none;">
                        I have my learner's licence now &rsaquo;
                    </asp:LinkButton>
                </asp:Panel>

                <asp:Panel ID="pnlBook" runat="server">
                    <div class="section-title">Book Your Theory Class — <asp:Literal ID="litPrice" runat="server" /></div>
                    <div class="form-grid">
                        <div>
                            <label class="f-label" for="ddlCode">Licence Code</label>
                            <asp:DropDownList ID="ddlCode" runat="server" CssClass="ddl">
                                <asp:ListItem>Code 08</asp:ListItem>
                                <asp:ListItem>Code 10</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div>
                            <label class="f-label" for="txtDate">Select Date</label>
                            <asp:TextBox ID="txtDate" runat="server" CssClass="f-input" TextMode="Date" />
                        </div>
                        <div>
                            <label class="f-label" for="ddlTime">Select Time</label>
                            <asp:DropDownList ID="ddlTime" runat="server" CssClass="ddl" />
                        </div>
                    </div>

                    <asp:Button ID="btnBook" runat="server" Text="Book Learners Class"
                        CssClass="btn-book" OnClick="btnBook_Click" />
                    <asp:Label ID="lblError" runat="server" CssClass="error-label" Visible="false" />
                </asp:Panel>

                <asp:Panel ID="pnlSuccess" runat="server" CssClass="success-box" Visible="false">
                    <asp:Literal ID="litSuccess" runat="server" />

                    <div class="pay-online-box">
                        <h4>
                            <svg viewBox="0 0 24 24"><rect x="1" y="4" width="22" height="16" rx="2" ry="2"></rect><line x1="1" y1="10" x2="23" y2="10"></line></svg>
                            Payment Options
                        </h4>
                        <p>
                            You can pay the full amount now online, or pay at reception when convenient.
                            Your booking is held either way.
                        </p>

                        <asp:HyperLink ID="lnkPayOnline" runat="server"
                            CssClass="btn-pay"
                            Visible="false">
                            <svg viewBox="0 0 24 24"><rect x="1" y="4" width="22" height="16" rx="2" ry="2"></rect><line x1="1" y1="10" x2="23" y2="10"></line></svg>
                            Pay Online Now
                        </asp:HyperLink>

                        <div style="margin-top:14px;font-size:12.5px;color:#445; display:flex; flex-direction:column; gap:8px;">
                            <strong>Or pay at reception:</strong>
                            <div style="display:flex; align-items:center; gap:6px;">
                                <svg style="width:14px;height:14px;stroke:#0d2b6e;stroke-width:2;fill:none;" viewBox="0 0 24 24"><path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z"></path></svg>
                                <a href="tel:+27835448544" style="color:#0d2b6e;font-weight:700;">+27 83 544 8544</a>
                            </div>
                            <div style="display:flex; align-items:center; gap:6px;">
                                <svg style="width:14px;height:14px;stroke:#0d2b6e;stroke-width:2;fill:none;" viewBox="0 0 24 24"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"></path><polyline points="22,6 12,13 2,6"></polyline></svg>
                                <a href="mailto:info@khwezidrivingschool.co.za" style="color:#0d2b6e;font-weight:700;">info@khwezidrivingschool.co.za</a>
                            </div>
                        </div>
                    </div>

                    <div style="margin-top:18px;">
                        <a runat="server" href="~/StudentDashboard"
                           style="display:inline-block;padding:10px 26px;background:#0d2b6e;color:#fff;
                                  border-radius:20px;font-weight:800;text-decoration:none;font-size:13.5px;">
                            Return to my dashboard &rsaquo;
                        </a>
                    </div>
                </asp:Panel>
            </main>
        </div>

    </form>
</body>
</html>