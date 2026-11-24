 <%@ Page Title="Personal Info" Language="C#" AutoEventWireup="true" CodeBehind="PersonalInfo.aspx.cs" Inherits="WebApplicationM3.PersonalInfo" %>

<!DOCTYPE html>
<html lang="en">
<head id="head1" runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title><%: Page.Title %> - Khwezi Driving School</title>
    <style>
        * { box-sizing: border-box; margin: 0; padding: 0; }
        body { font-family: 'Segoe UI', Arial, sans-serif; background: linear-gradient(180deg, #e9f1fc, #d9e5f5); min-height: 100vh; }

        .topbar { background: #0d2b6e; display: flex; align-items: center; justify-content: space-between; padding: 10px 24px; }
        .logo { text-decoration: none; display: flex; flex-direction: column; line-height: 1.1; }
        .logo-name { font-size: 22px; font-weight: 800; font-style: italic; color: #fff; letter-spacing: 1px; }
        .logo-name span { color: #e8283f; }
        .logo-sub { font-size: 9px; color: #c3d2ee; letter-spacing: 3px; }
        .topbar-right { display: flex; align-items: center; gap: 16px; }
        
        /* Top Icon (Notification) */
        .top-icon { color: #fff; text-decoration: none; display: flex; align-items: center; opacity: .85; transition: opacity 0.2s; }
        .top-icon svg { width: 20px; height: 20px; stroke: currentColor; stroke-width: 2; fill: none; stroke-linecap: round; stroke-linejoin: round; }
        .top-icon:hover { opacity: 1; color: #fff; text-decoration: none; }
        
        .user-name { color: #fff; font-size: 14px; font-weight: 600; }
        
        .avatar { width: 34px; height: 34px; border-radius: 50%; background: #e8283f; color: #fff; display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: 13px; position: relative; overflow: hidden; }
        .avatar .avatar-img { position: absolute; inset: 0; width: 100%; height: 100%; object-fit: cover; border-radius: 50%; }

        .layout { display: flex; gap: 20px; padding: 20px 24px; align-items: flex-start; }

        .sidebar { width: 235px; flex-shrink: 0; background: #fff; border-radius: 12px; padding: 14px; box-shadow: 0 4px 15px rgba(13,43,110,.08); }
        
        /* Side Items with SVG */
        .side-item { display: flex; align-items: center; gap: 10px; padding: 10px 12px; margin-bottom: 6px; border-radius: 8px; border: 1px solid transparent; color: #0d2b6e; font-size: 13.5px; font-weight: 600; text-decoration: none; transition: 0.2s; }
        .side-item svg { width: 18px; height: 18px; stroke: currentColor; stroke-width: 2; fill: none; stroke-linecap: round; stroke-linejoin: round; }
        .side-item:hover { background: #eef4ff; }
        .side-item.selected, .side-item.selected:hover { background: #0d2b6e; color: #fff; }
        
        .subscription { margin-top: 18px; background: #0d2b6e; color: #fff; border-radius: 10px; padding: 16px; text-align: center; }
        .sub-title { font-weight: 700; font-size: 15px; margin-bottom: 6px; }
        .subscription p { font-size: 11.5px; color: #c3d2ee; }
        .btn-upgrade { width: 100%; margin-top: 10px; padding: 9px; background: #e8283f; color: #fff; border: none; border-radius: 16px; font-weight: 700; font-size: 13px; cursor: pointer; }
        .btn-upgrade:hover { background: #c81e33; }

        .main { flex: 1; background: #fff; border-radius: 12px; padding: 26px 30px; box-shadow: 0 4px 15px rgba(13,43,110,.08); }
        .page-header { display: flex; align-items: center; gap: 12px; margin-bottom: 22px; }
        .back-btn { width: 30px; height: 30px; border-radius: 50%; background: #f1f4fb; color: #0d2b6e; display: flex; align-items: center; justify-content: center; text-decoration: none; font-weight: 700; font-size: 16px; }
        .back-btn:hover { background: #e2e8f5; color: #0d2b6e; text-decoration: none; }
        .page-title { color: #0d2b6e; font-size: 20px; font-weight: 700; }

        .msg { display: block; border-radius: 8px; padding: 12px 16px; font-size: 13.5px; font-weight: 600; margin-bottom: 18px; }
        .msg.ok  { background: #effaf4; border: 1px solid #bfe8d4; color: #14663f; }
        .msg.err { background: #fdf0f1; border: 1px solid #f3c2c9; color: #a3141f; }

        .form-section { margin-bottom: 26px; }
        .section-title { color: #0d2b6e; font-weight: 700; font-size: 15px; margin-bottom: 12px; padding-bottom: 6px; border-bottom: 2px solid #eef2fa; }

        .form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 16px 20px; }
        .form-grid.three { grid-template-columns: 1fr 1fr 1fr; }

        .f-label { display: block; font-size: 12.5px; color: #444; font-weight: 600; margin-bottom: 6px; }
        .f-input { width: 100%; padding: 11px 13px; border: 1px solid #ccd7ec; border-radius: 6px; font-size: 14px; background: #fff; }
        .f-input:focus { outline: none; border-color: #2f80ed; }
        .f-input.readonly-field { background: #f4f7fd; color: #667; cursor: not-allowed; }

        .lock-note { background: #f4f7fd; border: 1px solid #dbe4f6; border-radius: 8px; padding: 14px 16px; font-size: 12.5px; color: #445; line-height: 1.6; margin-bottom: 16px; display: flex; gap: 12px; align-items: flex-start; }
        .lock-note svg { width: 18px; height: 18px; stroke: #0d2b6e; stroke-width: 2; fill: none; flex-shrink: 0; margin-top: 2px; }
        .lock-note strong { color: #0d2b6e; }
        .lock-note a { color: #0d2b6e; font-weight: 700; text-decoration: none; }
        .lock-note a:hover { text-decoration: underline; }

        .lock-note.warn { background: #fff7e0; border-color: #f0d999; color: #7a5a00; }
        .lock-note.warn svg { stroke: #8a5a00; }
        .lock-note.warn strong { color: #8a5a00; }

        .rbl { display: flex; gap: 20px; margin-top: 4px; }
        .rbl label { font-size: 13.5px; color: #333; }
        .rbl input { margin-right: 6px; }
        .rbl input:disabled + label { color: #999; }

        .wizard-footer { display: flex; justify-content: flex-end; gap: 12px; margin-top: 24px; padding-top: 22px; border-top: 1px solid #eef2fa; }
        .btn-save { background: #22b573; color: #fff; border: none; border-radius: 20px; padding: 12px 42px; font-weight: 700; font-size: 14px; cursor: pointer; }
        .btn-save:hover { background: #1d9e63; }
        .btn-cancel { background: #a9b4c4; color: #fff; border: none; border-radius: 20px; padding: 12px 34px; font-weight: 700; font-size: 14px; cursor: pointer; text-decoration: none; display: inline-flex; align-items: center; }
        .btn-cancel:hover { background: #8d99ac; color: #fff; text-decoration: none; }

        @media (max-width: 950px) {
            .layout { flex-direction: column; }
            .sidebar { width: 100%; }
            .form-grid, .form-grid.three { grid-template-columns: 1fr; }
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
                <!-- Notification Bell Icon -->
                <a href="#" class="top-icon" title="Notifications">
                    <svg viewBox="0 0 24 24">
                        <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"></path>
                        <path d="M13.73 21a2 2 0 0 1-3.46 0"></path>
                    </svg>
                </a>
                <span class="user-name"><asp:Literal ID="litStudentName" runat="server" /></span>
                <span class="avatar">
                    <asp:Literal ID="litInitials" runat="server" />
                    <asp:Image ID="imgTopAvatar" runat="server" Visible="false" CssClass="avatar-img" AlternateText="Profile photo" />
                </span>
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
                <a href="~/PersonalInfo" runat="server" class="side-item selected">
                    <svg viewBox="0 0 24 24"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path><circle cx="12" cy="7" r="4"></circle></svg>
                    Personal Info
                </a>
                
                <!-- Gift Icon -->
                <a href="~/Offers" runat="server" class="side-item">
                    <svg viewBox="0 0 24 24"><polyline points="20 12 20 22 4 22 4 12"></polyline><rect x="2" y="7" width="20" height="5"></rect><line x1="12" y1="22" x2="12" y2="7"></line><path d="M12 7H7.5a2.5 2.5 0 0 1 0-5C11 2 12 7 12 7z"></path><path d="M12 7h4.5a2.5 2.5 0 0 0 0-5C13 2 12 7 12 7z"></path></svg>
                    Offers
                </a>
                
                <!-- Document Icon -->
                <a href="~/MyDocuments" runat="server" class="side-item">
                    <svg viewBox="0 0 24 24"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path><polyline points="14 2 14 8 20 8"></polyline><line x1="16" y1="13" x2="8" y2="13"></line><line x1="16" y1="17" x2="8" y2="17"></line><polyline points="10 9 9 9 8 9"></polyline></svg>
                    My Documents
                </a>
                
                <!-- Book Icon -->
                <a href="~/LearnersGuide" runat="server" class="side-item">
                    <svg viewBox="0 0 24 24"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20"></path><path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z"></path></svg>
                    Theory (K53)
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

                <div class="subscription">
                    <div class="sub-title">Subscription</div>
                    <p>Explore 20+ features with Premium Membership</p>
                    <asp:Button ID="btnUpgrade" runat="server" Text="Upgrade Now" CssClass="btn-upgrade"
                        OnClientClick="return false;" />
                </div>
            </aside>

            <main class="main">
                <div class="page-header">
                    <a href="~/StudentDashboard" runat="server" class="back-btn">&lsaquo;</a>
                    <h2 class="page-title">Personal Information</h2>
                </div>

                <asp:Label ID="lblMsg" runat="server" Visible="false" />

                <!-- NAME / EMAIL LOCK NOTICE -->
                <asp:Panel ID="pnlNameLocked" runat="server" Visible="false" CssClass="lock-note">
                    <svg viewBox="0 0 24 24"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect><path d="M7 11V7a5 5 0 0 1 10 0v4"></path></svg>
                    <div>
                        <strong>Your name and email are locked</strong><br/>
                        To correct your name or email address, please contact reception on
                        <a href="tel:+27835448544">+27 83 544 8544</a>
                        or email
                        <a href="mailto:info@khwezidrivingschool.co.za">info@khwezidrivingschool.co.za</a>.
                    </div>
                </asp:Panel>

                <!-- SECTION 1 — IDENTITY (READ-ONLY) -->
                <div class="form-section">
                    <div class="section-title">Identity</div>
                    <div class="form-grid">
                        <div>
                            <label class="f-label" for="txtName">First Name</label>
                            <asp:TextBox ID="txtName" runat="server" CssClass="f-input readonly-field"
                                MaxLength="50" ReadOnly="true" />
                        </div>
                        <div>
                            <label class="f-label" for="txtSurname">Surname</label>
                            <asp:TextBox ID="txtSurname" runat="server" CssClass="f-input readonly-field"
                                MaxLength="50" ReadOnly="true" />
                        </div>
                        <div style="grid-column: 1 / -1;">
                            <label class="f-label" for="txtEmail">Email Address</label>
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="f-input readonly-field"
                                TextMode="Email" ReadOnly="true" />
                        </div>
                    </div>
                </div>

                <!-- SECTION 2 — CONTACT DETAILS (EDITABLE) -->
                <div class="form-section">
                    <div class="section-title">Contact Details</div>
                    <div class="form-grid">
                        <div>
                            <label class="f-label" for="txtPhone">Phone Number (10 digits)</label>
                            <asp:TextBox ID="txtPhone" runat="server" CssClass="f-input"
                                placeholder="e.g. 0723456789" MaxLength="12" />
                        </div>
                        <div>
                            <label class="f-label" for="txtStreet">Street Address</label>
                            <asp:TextBox ID="txtStreet" runat="server" CssClass="f-input"
                                placeholder="e.g. 162 Varsity Drive" MaxLength="150" />
                        </div>
                        <div>
                            <label class="f-label" for="txtSuburb">Suburb</label>
                            <asp:TextBox ID="txtSuburb" runat="server" CssClass="f-input"
                                placeholder="e.g. Reservoir Hills" MaxLength="80" />
                        </div>
                        <div>
                            <label class="f-label" for="txtCity">City</label>
                            <asp:TextBox ID="txtCity" runat="server" CssClass="f-input"
                                placeholder="e.g. Durban" MaxLength="80" />
                        </div>
                    </div>
                </div>

                <!-- SECTION 3 — LEARNER'S LICENCE -->
                <div class="form-section">
                    <div class="section-title">Learner's Licence</div>

                    <asp:RadioButtonList ID="rblLicence" runat="server" CssClass="rbl"
                        RepeatDirection="Horizontal">
                        <asp:ListItem Value="Yes">Yes, I have a learner's licence</asp:ListItem>
                        <asp:ListItem Value="No">No, I don't have one yet</asp:ListItem>
                    </asp:RadioButtonList>

                    <asp:Panel ID="pnlLicenceLocked" runat="server" Visible="false" CssClass="lock-note warn" style="margin-top:14px;">
                        <svg viewBox="0 0 24 24"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect><path d="M7 11V7a5 5 0 0 1 10 0v4"></path></svg>
                        <div>
                            <strong>Your learner's licence status is locked</strong><br/>
                            You have already booked a learners-licence class on your account, so this
                            setting can no longer be changed. If you believe this is incorrect, please
                            contact reception on
                            <a href="tel:+27835448544"> 083 544 8544</a>.
                        </div>
                    </asp:Panel>
                </div>

                <!-- SAVE / CANCEL -->
                <div class="wizard-footer">
                    <a href="~/StudentDashboard" runat="server" class="btn-cancel">Cancel</a>
                    <asp:Button ID="btnSave" runat="server" Text="Save Changes"
                        CssClass="btn-save" OnClick="btnSave_Click" />
                </div>
            </main>
        </div>

    </form>
</body>
</html>