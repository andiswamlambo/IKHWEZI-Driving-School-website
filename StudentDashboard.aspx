 <%@ Page Title="Student Dashboard" Language="C#" AutoEventWireup="true" CodeBehind="StudentDashboard.aspx.cs" Inherits="WebApplicationM3.StudentDashboard" %>

<!DOCTYPE html>
<html lang="en">
<head id="head1" runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title><%: Page.Title %> - Khwezi Driving School</title>
    <style>
        * { box-sizing: border-box; margin: 0; padding: 0; }
        body { font-family: 'Segoe UI', Arial, sans-serif; background: linear-gradient(180deg, #e9f1fc, #d9e5f5); min-height: 100vh; }

        /* ===== TOP BAR ===== */
        .topbar { background: #0d2b6e; display: flex; align-items: center; justify-content: space-between; padding: 10px 24px; position: relative; z-index: 100; }
        .logo { text-decoration: none; display: flex; flex-direction: column; line-height: 1.1; }
        .logo-name { font-size: 22px; font-weight: 800; font-style: italic; color: #fff; letter-spacing: 1px; }
        .logo-name span { color: #e8283f; }
        .logo-sub { font-size: 9px; color: #c3d2ee; letter-spacing: 3px; }
        
        .topbar-right { display: flex; align-items: center; gap: 16px; position: relative; }
        
        /* Profile Link */
        .profile-link { display: flex; align-items: center; gap: 10px; text-decoration: none; color: inherit; transition: opacity .2s; }
        .profile-link:hover { opacity: 0.8; text-decoration: none; color: inherit; }
        .user-name { color: #fff; font-size: 14px; font-weight: 600; white-space: nowrap; }
        
        /* Updated Avatar to support Images */
        .avatar { width: 34px; height: 34px; border-radius: 50%; background: #e8283f; color: #fff; display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: 13px; flex-shrink: 0; position: relative; overflow: hidden; }
        .avatar .avatar-img { position: absolute; inset: 0; width: 100%; height: 100%; object-fit: cover; border-radius: 50%; }

        /* ===== NOTIFICATION BELL ===== */
        .notif-bell { position: relative; background: transparent; border: none; cursor: pointer; padding: 4px; display: flex; align-items: center; justify-content: center; transition: transform .2s; }
        .notif-bell:hover { transform: scale(1.1); }
        .notif-bell svg { width: 24px; height: 24px; stroke: #fff; fill: none; stroke-width: 2; stroke-linecap: round; stroke-linejoin: round; }
        .notif-badge { position: absolute; top: 0; right: 0; width: 10px; height: 10px; background: #e8283f; border-radius: 50%; border: 2px solid #0d2b6e; display: none; }
        .notif-badge.show { display: block; animation: pulse 2s infinite; }
        @keyframes pulse {
            0% { box-shadow: 0 0 0 0 rgba(232,40,63, 0.7); }
            70% { box-shadow: 0 0 0 6px rgba(232,40,63, 0); }
            100% { box-shadow: 0 0 0 0 rgba(232,40,63, 0); }
        }

        /* ===== NOTIFICATION DROPDOWN ===== */
        .notif-dropdown {
            position: absolute;
            top: 55px;
            right: 220px;
            width: 360px;
            background: #fff;
            border-radius: 12px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.15);
            border: 1px solid #dbe4f6;
            opacity: 0;
            visibility: hidden;
            transform: translateY(-10px);
            transition: all 0.25s cubic-bezier(0.2, 0.8, 0.2, 1);
            z-index: 1000;
        }
        .notif-dropdown.show { opacity: 1; visibility: visible; transform: translateY(0); }
        
        .notif-header { background: #f7faff; padding: 14px 18px; border-bottom: 1px solid #dbe4f6; border-radius: 12px 12px 0 0; display: flex; justify-content: space-between; align-items: center; }
        .notif-header h3 { font-size: 14px; color: #0a1e4a; font-weight: 800; margin: 0; }
        .notif-header span { font-size: 11px; color: #e8283f; font-weight: 700; cursor: pointer; }
        
        .notif-list { max-height: 380px; overflow-y: auto; }
        .notif-item { display: flex; gap: 14px; padding: 14px 18px; border-bottom: 1px solid #eef2fa; text-decoration: none; color: inherit; transition: background 0.2s; }
        .notif-item:hover { background: #f8fbff; }
        .notif-item:last-child { border-bottom: none; border-radius: 0 0 12px 12px; }
        
        .notif-icon { width: 36px; height: 36px; border-radius: 50%; display: flex; align-items: center; justify-content: center; flex-shrink: 0; font-size: 16px; }
        .notif-icon.info { background: #eef4ff; color: #0d2b6e; }
        .notif-icon.success { background: #dcf5e8; color: #14663f; }
        .notif-icon.warn { background: #fff3d6; color: #8a6100; }
        .notif-icon.danger { background: #fde3e7; color: #c81e33; }
        
        .notif-content h4 { font-size: 13.5px; color: #0a1e4a; font-weight: 700; margin-bottom: 4px; }
        .notif-content p { font-size: 12.5px; color: #667; line-height: 1.5; margin: 0; }
        .notif-time { font-size: 11px; color: #99a3b5; margin-top: 6px; display: block; }

        .notif-empty { padding: 30px 20px; text-align: center; color: #7787a5; font-size: 13px; }

        /* ===== LAYOUT & SIDEBAR ===== */
        .layout { display: flex; gap: 20px; padding: 20px 24px; align-items: flex-start; }
        .sidebar { width: 235px; flex-shrink: 0; background: #fff; border-radius: 12px; padding: 14px; box-shadow: 0 4px 15px rgba(13,43,110,.08); }
        
        /* Updated Side Items to handle SVG */
        .side-item { display: flex; align-items: center; gap: 10px; padding: 10px 12px; margin-bottom: 6px; border-radius: 8px; border: 1px solid transparent; color: #0d2b6e; font-size: 13.5px; font-weight: 600; text-decoration: none; transition: 0.2s; }
        .side-item svg { width: 18px; height: 18px; stroke: currentColor; stroke-width: 2; fill: none; stroke-linecap: round; stroke-linejoin: round; }
        .side-item:hover { background: #eef4ff; }
        .side-item.selected, .side-item.selected:hover { background: #0d2b6e; color: #fff; }
        
        .subscription { margin-top: 18px; background: #0d2b6e; color: #fff; border-radius: 10px; padding: 16px; text-align: center; }
        .sub-title { font-weight: 700; font-size: 15px; margin-bottom: 6px; }
        .subscription p { font-size: 11.5px; color: #c3d2ee; }
        .btn-upgrade { width: 100%; margin-top: 10px; padding: 9px; background: #e8283f; color: #fff; border: none; border-radius: 16px; font-weight: 700; font-size: 13px; cursor: pointer; }
        .btn-upgrade:hover { background: #c81e33; }

        /* ===== MAIN CONTENT ===== */
        .main { flex: 1; background: #fff; border-radius: 12px; padding: 22px; box-shadow: 0 4px 15px rgba(13,43,110,.08); }
        .page-header { display: flex; align-items: center; gap: 12px; margin-bottom: 18px; }
        .back-btn { width: 30px; height: 30px; border-radius: 50%; background: #f1f4fb; color: #0d2b6e; display: flex; align-items: center; justify-content: center; text-decoration: none; font-weight: 700; font-size: 16px; }
        .page-title { color: #0d2b6e; font-size: 20px; font-weight: 700; }

        .package-card-wrapper { margin-bottom: 12px; }
        .btn-release-package { display: inline-block; padding: 8px 20px; background: #e8890c; color: #fff; border-radius: 16px; font-size: 12.5px; font-weight: 700; text-decoration: none; cursor: pointer; margin-bottom: 18px; }
        .btn-release-package:hover { background: #c4760a; color: #fff; text-decoration: none; }

        .content-grid { display: flex; gap: 18px; align-items: flex-start; }
        .filter-panel { width: 235px; flex-shrink: 0; border: 1px solid #e3eaf6; border-radius: 10px; padding: 14px; }
        .filter-title { font-weight: 700; color: #0d2b6e; font-size: 14px; margin-bottom: 10px; }
        .check-row { display: flex; align-items: center; gap: 8px; font-size: 12.5px; color: #333; margin-bottom: 8px; cursor: pointer; }
        .btn-new-lesson { width: 100%; margin-top: 14px; padding: 10px; background: #e8283f; color: #fff; border: none; border-radius: 20px; font-weight: 700; font-size: 13.5px; cursor: pointer; }
        .btn-new-lesson:hover { background: #c81e33; }

        .lessons-panel { flex: 1; }
        .total-hours { font-size: 13px; color: #555; margin-bottom: 12px; }

        .dashboard-msg { display: block; border-radius: 8px; padding: 14px 18px; font-size: 13.5px; font-weight: 600; margin-bottom: 16px; }
        .dashboard-msg.ok  { background: #effaf4; border: 1px solid #bfe8d4; color: #14663f; }
        .dashboard-msg.err { background: #fdf0f1; border: 1px solid #f3c2c9; color: #a3141f; }
        .dashboard-msg.warn{ background: #fff7e0; border: 1px solid #f0d999; color: #7a5a00; }

        .lesson-card { background: #eef2fa; border-radius: 10px; padding: 14px 16px; margin-bottom: 14px; }
        .lesson-top { display: flex; justify-content: space-between; margin-bottom: 8px; }
        .badge { padding: 4px 14px; border-radius: 6px; color: #fff; font-size: 12px; font-weight: 700; }
        .badge-green { background: #22b573; }
        .badge-blue  { background: #2f80ed; }
        .status { font-size: 13px; font-weight: 600; }
        .status-green  { color: #22b573; }
        .status-orange { color: #e8890c; }
        .status-red    { color: #e8283f; }
        .lesson-date { font-weight: 700; color: #1a1a2e; font-size: 14px; }
        .lesson-time { font-size: 13px; color: #555; margin: 2px 0 8px; }
        .lesson-meta { display: flex; justify-content: space-between; font-size: 12.5px; color: #444; align-items: center; }
        .lesson-meta span { display: flex; align-items: center; gap: 6px; }
        .lesson-meta svg { width: 14px; height: 14px; stroke: currentColor; stroke-width: 2; fill: none; stroke-linecap: round; stroke-linejoin: round; }
        .transmission { display: inline-block; margin-top: 10px; font-size: 12.5px; color: #2f80ed; text-decoration: underline; }

        .lesson-actions { margin-top: 12px; text-align: right; display: flex; gap: 8px; justify-content: flex-end; }
        .btn-pay-deposit { display: inline-flex; align-items: center; gap: 6px; padding: 8px 18px; background: #22b573; color: #fff; border-radius: 14px; font-size: 12.5px; font-weight: 700; text-decoration: none; cursor: pointer; border: none; }
        .btn-pay-deposit svg { width: 14px; height: 14px; stroke: currentColor; stroke-width: 2; fill: none; stroke-linecap: round; stroke-linejoin: round; }
        .btn-pay-deposit:hover { background: #1d9e63; color: #fff; text-decoration: none; }
        
        .btn-cancel-lesson { display: inline-flex; align-items: center; gap: 6px; padding: 8px 18px; background: #e8283f; color: #fff; border-radius: 14px; font-size: 12.5px; font-weight: 700; text-decoration: none; cursor: pointer; border: none; }
        .btn-cancel-lesson svg { width: 14px; height: 14px; stroke: currentColor; stroke-width: 2; fill: none; stroke-linecap: round; stroke-linejoin: round; }
        .btn-cancel-lesson:hover { background: #c81e33; color: #fff; text-decoration: none; }

        .confirm-inline { margin-top: 12px; background: #fff7e0; border: 1px solid #f0d999; border-left: 4px solid #e8890c; border-radius: 8px; padding: 14px 16px; text-align: left; font-size: 13px; color: #5a4400; }
        .confirm-inline .confirm-text { font-weight: 700; margin-bottom: 10px; color: #7a5a00; }
        .confirm-inline .confirm-buttons { display: flex; gap: 10px; justify-content: flex-end; }

        .exception-inline { margin-bottom: 16px; background: #fef3e7; border: 1px solid #f5cfa0; border-left: 4px solid #e8890c; border-radius: 8px; padding: 18px 20px; text-align: left; font-size: 13.5px; color: #6b4500; line-height: 1.6; }
        .exception-inline .title { font-weight: 800; color: #8a5a00; margin-bottom: 8px; font-size: 15px; display: flex; align-items: center; gap: 8px;}
        .exception-inline .title svg { width: 18px; height: 18px; stroke: currentColor; stroke-width: 2; fill: none; }
        .exception-inline .sub { font-size: 12.5px; color: #6b4500; margin-bottom: 12px; }
        .exception-inline textarea { width: 100%; min-height: 80px; padding: 10px 12px; border: 1px solid #e0c896; border-radius: 6px; font-family: inherit; font-size: 13px; resize: vertical; background: #fff; }
        .exception-inline .buttons { display: flex; gap: 10px; justify-content: flex-end; margin-top: 12px; }

        .btn-yes { display: inline-block; padding: 8px 20px; background: #e8283f; color: #fff; border-radius: 14px; font-size: 12.5px; font-weight: 700; text-decoration: none; border: none; cursor: pointer; }
        .btn-yes:hover { background: #c81e33; color: #fff; text-decoration: none; }
        .btn-no { display: inline-block; padding: 8px 20px; background: #a9b4c4; color: #fff; border-radius: 14px; font-size: 12.5px; font-weight: 700; text-decoration: none; border: none; cursor: pointer; }
        .btn-no:hover { background: #8d99ac; color: #fff; text-decoration: none; }
        .btn-request { display: inline-block; padding: 10px 24px; background: #e8890c; color: #fff; border-radius: 20px; font-size: 13px; font-weight: 800; text-decoration: none; border: none; cursor: pointer; }
        .btn-request:hover { background: #c4760a; color: #fff; text-decoration: none; }

        .empty-state { border: 2px dashed #c9d6ee; border-radius: 10px; padding: 30px; text-align: center; color: #667; }

        /* =========================================================
           LOGOUT MODAL
           ========================================================= */
        .modal-overlay { position: fixed; inset: 0; background: rgba(10,30,74,.7); backdrop-filter: blur(4px); display: none; align-items: center; justify-content: center; padding: 20px; z-index: 99999; }
        .modal-overlay.show { display: flex; }
        .modal-card { background: #fff; border-radius: 20px; max-width: 420px; width: 100%; padding: 36px 30px; box-shadow: 0 30px 80px rgba(0,0,0,.4); position: relative; text-align: center; border-top: 4px solid #e8283f; }
        .modal-icon { width: 64px; height: 64px; border-radius: 50%; background: #fdf0f1; color: #e8283f; display: flex; align-items: center; justify-content: center; margin: 0 auto 20px; }
        .modal-icon svg { width: 32px; height: 32px; }
        .modal-title { color: #0a1e4a; font-size: 1.4em; font-weight: 800; margin-bottom: 10px; }
        .modal-text { color: #667; font-size: 14px; line-height: 1.6; margin-bottom: 24px; }
        .modal-actions { display: flex; gap: 12px; justify-content: center; }
        .modal-btn { padding: 12px 24px; border-radius: 24px; font-weight: 800; font-size: 14px; text-decoration: none; cursor: pointer; border: none; min-width: 120px; transition: all .2s; }
        .modal-btn-logout { background: #e8283f; color: #fff; box-shadow: 0 6px 18px rgba(232,40,63,.3); }
        .modal-btn-logout:hover { background: #c81e33; color: #fff; transform: translateY(-1px); }
        .modal-btn-cancel { background: #f1f4fb; color: #0d2b6e; }
        .modal-btn-cancel:hover { background: #e2e8f5; color: #0d2b6e; }

        @media (max-width: 950px) {
            .layout { flex-direction: column; }
            .sidebar, .filter-panel { width: 100%; }
            .content-grid { flex-direction: column; }
            .notif-dropdown { right: 10px; width: calc(100vw - 20px); max-width: 380px; }
            .user-name { display: none; }
            .modal-actions { flex-direction: column; }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">

        <div class="topbar">
            <a href="~/StudentDashboard" runat="server" class="logo">
                <span class="logo-name">KHWEZI<span>.</span></span>
                <span class="logo-sub">DRIVING SCHOOL</span>
            </a>
            
            <div class="topbar-right">
                
                <!-- NOTIFICATION BELL -->
                <button type="button" id="notifBtn" class="notif-bell" title="Notifications" onclick="toggleNotifications(event)">
                    <svg viewBox="0 0 24 24">
                        <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"></path>
                        <path d="M13.73 21a2 2 0 0 1-3.46 0"></path>
                    </svg>
                    <span id="notifBadge" class="notif-badge"></span>
                </button>

                <!-- PROFILE LINK -->
                <a href="~/PersonalInfo" runat="server" class="profile-link" title="Go to Personal Info">
                    <span class="user-name"><asp:Literal ID="litStudentName" runat="server" /></span>
                    <span class="avatar">
                        <asp:Literal ID="litInitials" runat="server" />
                        <asp:Image ID="imgTopAvatar" runat="server" Visible="false" CssClass="avatar-img" AlternateText="Profile photo" />
                    </span>
                </a>
            </div>

            <!-- NOTIFICATION DROPDOWN -->
            <div id="notifDropdown" class="notif-dropdown" onclick="event.stopPropagation()">
                <div class="notif-header">
                    <h3>Notifications</h3>
                    <span onclick="clearNotifications()">Mark all as read</span>
                </div>
                <div class="notif-list">
                    <asp:Literal ID="litNotifications" runat="server" />
                </div>
            </div>

        </div>

        <div class="layout">

            <aside class="sidebar">
                <!-- Dashboard Icon -->
                <a href="~/StudentDashboard" runat="server" class="side-item selected">
                    <svg viewBox="0 0 24 24"><rect x="3" y="3" width="7" height="7"></rect><rect x="14" y="3" width="7" height="7"></rect><rect x="14" y="14" width="7" height="7"></rect><rect x="3" y="14" width="7" height="7"></rect></svg>
                    Dashboard
                </a>
                
                <!-- User Icon -->
                <a href="~/PersonalInfo" runat="server" class="side-item">
                    <svg viewBox="0 0 24 24"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path><circle cx="12" cy="7" r="4"></circle></svg>
                    Personal Info
                </a>
                
                <!-- Gift/Offers Icon -->
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
                
                <!-- Trophy Icon -->
                <a href="~/MyProgress" runat="server" class="side-item">
                    <svg viewBox="0 0 24 24"><path d="M6 9H4.5a2.5 2.5 0 0 1 0-5H6"></path><path d="M18 9h1.5a2.5 2.5 0 0 0 0-5H18"></path><path d="M4 22h16"></path><path d="M10 14.66V17c0 .55-.47.98-.97 1.21C7.85 18.75 7 20.24 7 22"></path><path d="M14 14.66V17c0 .55.47.98.97 1.21C16.15 18.75 17 20.24 17 22"></path><path d="M18 2H6v7a6 6 0 0 0 12 0V2Z"></path></svg>
                    My Progress
                </a>
                
                <!-- Headphones Icon -->
                <a href="~/Support" runat="server" class="side-item">
                    <svg viewBox="0 0 24 24"><path d="M3 18v-6a9 9 0 0 1 18 0v6"></path><path d="M21 19a2 2 0 0 1-2 2h-1a2 2 0 0 1-2-2v-3a2 2 0 0 1 2-2h3zM3 19a2 2 0 0 0 2 2h1a2 2 0 0 0 2-2v-3a2 2 0 0 0-2-2H3z"></path></svg>
                    Support
                </a>

                <!-- Logout Button triggers the modal -->
                <a href="javascript:void(0);" onclick="showLogoutModal(); return false;" class="side-item">
                    <svg viewBox="0 0 24 24"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"></path><polyline points="16 17 21 12 16 7"></polyline><line x1="21" y1="12" x2="9" y2="12"></line></svg>
                    Logout
                </a>

                <!-- HIDDEN backend button to actually log the user out -->
                <asp:LinkButton ID="btnLogoutSide" runat="server" style="display:none;" OnClick="btnLogoutSide_Click">Logout</asp:LinkButton>

                <div class="subscription">
                    <div class="sub-title">Subscription</div>
                    <p>Explore 20+ features with Premium Membership</p>
                    <asp:Button ID="btnUpgrade" runat="server" Text="Upgrade Now" CssClass="btn-upgrade" OnClick="btnUpgrade_Click" />
                </div>
            </aside>

            <main class="main">
                <div class="page-header">
                    <a href="javascript:history.back()" class="back-btn">&lsaquo;</a>
                    <h2 class="page-title">Driving Lessons</h2>
                </div>

                <div class="package-card-wrapper">
                    <asp:Literal ID="litPackageCard" runat="server" />
                </div>

                <asp:LinkButton ID="btnReleasePackage" runat="server"
                    CssClass="btn-release-package"
                    Visible="false"
                    OnClick="btnReleasePackage_Click">
                    <svg style="width:12px; height:12px; vertical-align:middle; margin-right:4px;" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg> Release Current Package
                </asp:LinkButton>

                <div class="content-grid">

                    <div class="filter-panel">
                        <div class="filter-title">Lessons</div>

                        <label class="check-row">
                            <asp:CheckBox ID="chkAwaiting" runat="server" AutoPostBack="true" OnCheckedChanged="Filter_Changed" />
                            Awaiting deposit / confirmation
                        </label>
                        <label class="check-row">
                            <asp:CheckBox ID="chkCompleted" runat="server" AutoPostBack="true" OnCheckedChanged="Filter_Changed" />
                            Completed
                        </label>
                        <label class="check-row">
                            <asp:CheckBox ID="chkConfirmed" runat="server" AutoPostBack="true" OnCheckedChanged="Filter_Changed" />
                            Confirmed
                        </label>
                        <label class="check-row">
                            <asp:CheckBox ID="chkCanceled" runat="server" AutoPostBack="true" OnCheckedChanged="Filter_Changed" />
                            Cancelled
                        </label>

                        <asp:Button ID="btnNewLesson" runat="server" Text="Make Booking"
                            CssClass="btn-new-lesson" OnClick="btnNewLesson_Click" />
                    </div>

                    <div class="lessons-panel">

                        <asp:Panel ID="pnlMessage" runat="server" Visible="false">
                            <asp:Literal ID="litMessage" runat="server" />
                        </asp:Panel>

                        <asp:Panel ID="pnlException" runat="server" CssClass="exception-inline" Visible="false">
                            <div class="title">
                                <svg viewBox="0 0 24 24"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"></path><line x1="12" y1="9" x2="12" y2="13"></line><line x1="12" y1="17" x2="12.01" y2="17"></line></svg>
                                This lesson is inside the 24-hour window
                            </div>
                            <div class="sub">
                                Cancellations need at least <strong>24 hours' notice</strong>.
                                Because your lesson <asp:Literal ID="litExceptionLesson" runat="server" />
                                is sooner than that, we can't cancel it automatically.
                                You can ask reception for an exception &mdash; please tell them why below.
                            </div>
                            <asp:TextBox ID="txtExceptionReason" runat="server"
                                TextMode="MultiLine"
                                placeholder="e.g. A family emergency has come up. Please consider cancelling and refunding the lesson to my balance."
                                MaxLength="500" />
                            <div class="buttons">
                                <asp:LinkButton ID="btnSubmitException" runat="server"
                                    CssClass="btn-request"
                                    OnClick="btnSubmitException_Click">
                                    Send request to reception
                                </asp:LinkButton>
                                <asp:LinkButton ID="btnDismissException" runat="server"
                                    CssClass="btn-no"
                                    OnClick="btnDismissException_Click">
                                    Never mind
                                </asp:LinkButton>
                            </div>
                        </asp:Panel>

                        <asp:Literal ID="litHoursTotal" runat="server" />

                        <asp:Repeater ID="repLessons" runat="server">
                            <ItemTemplate>
                                <div class="lesson-card">
                                    <div class="lesson-top">
                                        <span class='badge <%# Eval("BadgeClass") %>'><%# Eval("Type") %></span>
                                        <span class='status <%# Eval("StatusClass") %>'><%# Eval("Status") %></span>
                                    </div>
                                    <div class="lesson-date"><%# Eval("DateText") %></div>
                                    <div class="lesson-time"><%# Eval("TimeText") %></div>
                                    
                                    <div class="lesson-meta">
                                        <span>
                                            <svg viewBox="0 0 24 24"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"></path><circle cx="12" cy="10" r="3"></circle></svg>
                                            <%# Eval("Location") %>
                                        </span>
                                        <span><%# Eval("Instructor") %></span>
                                    </div>
                                    <span class="transmission"><%# Eval("Transmission") %></span>

                                    <div class="lesson-actions">
                                        <asp:LinkButton runat="server"
                                            CssClass="btn-pay-deposit"
                                            CommandArgument='<%# Eval("BookingID") %>'
                                            OnCommand="btnPayDeposit_Command"
                                            Visible='<%# IsUnsettled(Eval("Status").ToString()) %>'>
                                            <svg viewBox="0 0 24 24"><rect x="1" y="4" width="22" height="16" rx="2" ry="2"></rect><line x1="1" y1="10" x2="23" y2="10"></line></svg>
                                            Pay Deposit
                                        </asp:LinkButton>

                                        <asp:LinkButton runat="server"
                                            CssClass="btn-cancel-lesson"
                                            CommandArgument='<%# Eval("BookingID") %>'
                                            OnCommand="btnCancelLesson_ShowConfirm"
                                            Visible='<%# IsCancellable(Eval("Status").ToString()) %>'>
                                            <svg viewBox="0 0 24 24"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
                                            Cancel Booking
                                        </asp:LinkButton>
                                    </div>

                                    <asp:Panel runat="server" CssClass="confirm-inline"
                                        Visible='<%# IsPendingCancel(Container.DataItem) %>'>
                                        <div class="confirm-text">
                                            Are you sure you want to cancel this booking? This cannot be undone.
                                        </div>
                                        <div class="confirm-buttons">
                                            <asp:LinkButton runat="server"
                                                CssClass="btn-yes"
                                                CommandArgument='<%# Eval("BookingID") %>'
                                                OnCommand="btnCancelLesson_Confirm">
                                                Yes, cancel it
                                            </asp:LinkButton>
                                            <asp:LinkButton runat="server"
                                                CssClass="btn-no"
                                                OnCommand="btnCancelLesson_Dismiss">
                                                No, keep it
                                            </asp:LinkButton>
                                        </div>
                                    </asp:Panel>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>

                        <asp:Panel ID="pnlEmpty" runat="server" CssClass="empty-state" Visible="false">
                            <p><strong>No lessons to show.</strong></p>
                            <p>Tick a filter above, or use the <em>Make Booking</em> button to book a session.</p>
                        </asp:Panel>
                    </div>

                </div>
            </main>
        </div>

        <!-- LOGOUT MODAL -->
        <div id="logoutModal" class="modal-overlay">
            <div class="modal-card">
                <div class="modal-icon">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                        <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"></path>
                        <polyline points="16 17 21 12 16 7"></polyline>
                        <line x1="21" y1="12" x2="9" y2="12"></line>
                    </svg>
                </div>
                <h2 class="modal-title">Ready to leave?</h2>
                <p class="modal-text">Are you sure you want to log out? You will need to enter your email and password to sign back in.</p>
                <div class="modal-actions">
                    <a href="javascript:void(0);" onclick="document.getElementById('<%= btnLogoutSide.ClientID %>').click();" class="modal-btn modal-btn-logout">Yes, Log Out</a>
                    <button type="button" onclick="hideLogoutModal();" class="modal-btn modal-btn-cancel">Cancel</button>
                </div>
            </div>
        </div>

    </form>

    <!-- Scripts -->
    <script>
        function toggleNotifications(e) {
            e.preventDefault();
            var dropdown = document.getElementById('notifDropdown');
            dropdown.classList.toggle('show');
        }

        document.addEventListener('click', function (event) {
            var dropdown = document.getElementById('notifDropdown');
            var btn = document.getElementById('notifBtn');

            if (dropdown.classList.contains('show') && !dropdown.contains(event.target) && !btn.contains(event.target)) {
                dropdown.classList.remove('show');
            }
        });

        function clearNotifications() {
            var badge = document.getElementById('notifBadge');
            if (badge) badge.classList.remove('show');
            document.getElementById('notifDropdown').classList.remove('show');
        }

        function showLogoutModal() {
            document.getElementById('logoutModal').classList.add('show');
        }

        function hideLogoutModal() {
            document.getElementById('logoutModal').classList.remove('show');
        }
    </script>
</body>
</html>