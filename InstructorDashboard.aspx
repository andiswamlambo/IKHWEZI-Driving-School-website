 <%@ Page Title="Instructor Dashboard" Language="C#" AutoEventWireup="true" CodeBehind="InstructorDashboard.aspx.cs" Inherits="WebApplicationM3.InstructorDashboard" %>
<!DOCTYPE html>
<html lang="en">
<head id="head1" runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title><%: Page.Title %> - IKHWEZI</title>
    
    <link href="https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700;800&display=swap" rel="stylesheet">
    
    <style>
        :root {
            --navy: #0d2b6e;
            --red: #e8283f;
            --green: #10b981;
            --orange: #f59e0b;
            --bg-body: #f0f4f8;
            --card-bg: #ffffff;
            --text-main: #1e293b;
            --text-muted: #64748b;
            --border: #e2e8f0;
            --shadow-sm: 0 1px 3px 0 rgba(0, 0, 0, 0.05);
            --shadow-md: 0 4px 6px -1px rgba(0, 0, 0, 0.05);
            --shadow-lg: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
        }

        * { box-sizing: border-box; margin: 0; padding: 0; }
        
        /* Force the body to hide the master page background and use our clean color */
        body { 
            font-family: 'Plus Jakarta Sans', 'Segoe UI', sans-serif; 
            background-color: var(--bg-body) !important; 
            color: var(--text-main); 
            min-height: 100vh; 
        }

        /* TOP BAR */
        .topbar { background: #0d2b6e; display: flex; align-items: center; justify-content: space-between; padding: 12px 24px; position: sticky; top: 0; z-index: 100; box-shadow: 0 4px 10px rgba(0,0,0,0.1); }
        .logo { text-decoration: none; display: flex; flex-direction: column; line-height: 1.1; }
        .logo-name { font-size: 22px; font-weight: 800; font-style: italic; color: #fff; letter-spacing: 1px; }
        .logo-name span { color: #e8283f; }
        .logo-sub { font-size: 9px; color: #c3d2ee; letter-spacing: 3px; }
        .topbar-right { display: flex; align-items: center; gap: 16px; }
        .profile-wrap { display: flex; align-items: center; gap: 12px; text-decoration: none; color: inherit; transition: opacity 0.2s; }
        .profile-wrap:hover { opacity: 0.8; }
        .user-name { color: #fff; font-size: 14px; font-weight: 600; }
        .avatar { width: 36px; height: 36px; border-radius: 50%; background: #e8283f; color: #fff; display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: 14px; }

        /* LAYOUT & SIDEBAR */
        .layout { display: flex; gap: 24px; padding: 24px; align-items: flex-start; max-width: 1600px; margin: 0 auto; }
        .sidebar { width: 260px; flex-shrink: 0; background: var(--card-bg); border-radius: 16px; padding: 20px 16px; box-shadow: var(--shadow-sm); position: sticky; top: 90px; border: 1px solid var(--border); }
        .side-item { display: flex; align-items: center; gap: 12px; padding: 12px 16px; margin-bottom: 8px; border-radius: 10px; color: var(--text-muted); font-size: 14px; font-weight: 600; text-decoration: none; transition: all 0.2s; }
        .side-item svg { width: 20px; height: 20px; stroke: currentColor; stroke-width: 2; fill: none; stroke-linecap: round; stroke-linejoin: round; }
        .side-item:hover { background: #f8fafc; color: var(--navy); }
        .side-item.selected { background: var(--navy); color: #fff; box-shadow: 0 4px 12px rgba(13, 43, 110, 0.2); }
        .instructor-card { margin-top: 24px; background: #f8fafc; border: 1px solid var(--border); border-radius: 12px; padding: 16px; }
        .instructor-card .title { font-weight: 800; font-size: 13px; margin-bottom: 8px; color: var(--navy); text-transform: uppercase; }
        .instructor-card p { font-size: 12px; color: var(--text-muted); margin-bottom: 12px; line-height: 1.5; }
        .res-link { display: flex; align-items: center; gap: 8px; font-size: 13px; color: var(--navy); text-decoration: none; font-weight: 700; margin-bottom: 8px; }
        .res-link svg { width: 16px; height: 16px; stroke: currentColor; stroke-width: 2; fill: none; }
        .res-link:hover { color: var(--red); }

        /* MAIN CONTENT */
        .main { flex: 1; min-width: 0; }
        .page-title { color: var(--navy); font-size: 24px; font-weight: 800; margin-bottom: 6px; letter-spacing: -0.5px; }
        .page-sub { font-size: 14px; color: var(--text-muted); margin-bottom: 24px; font-weight: 500; }

        /* STAT CARDS */
        .cards-row { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 16px; margin-bottom: 24px; }
        .stat-card { background: var(--card-bg); border-radius: 16px; padding: 20px; box-shadow: var(--shadow-sm); text-decoration: none; color: inherit; display: block; cursor: pointer; transition: all 0.3s; border: 1px solid var(--border); position: relative; overflow: hidden; }
        .stat-card::before { content: ''; position: absolute; top: 0; left: 0; width: 100%; height: 4px; background: var(--navy); }
        .stat-card.hot::before { background: var(--red); }
        .stat-card.warn::before { background: var(--orange); }
        .stat-card.green::before { background: var(--green); }
        .stat-card:hover { transform: translateY(-5px); box-shadow: var(--shadow-lg); border-color: transparent; text-decoration: none; color: inherit; }
        .stat-card.active { border: 2px solid var(--navy); background: #f8fafc; }
        .stat-value { font-size: 32px; font-weight: 800; color: var(--navy); display: block; margin-bottom: 4px; letter-spacing: -1px; }
        .stat-card.hot .stat-value { color: var(--red); }
        .stat-card.warn .stat-value { color: var(--orange); }
        .stat-card.green .stat-value { color: var(--green); }
        .stat-label { font-size: 12px; color: var(--text-muted); font-weight: 700; letter-spacing: 0.5px; text-transform: uppercase; }

        /* BLOCK OUT DATE CARD */
        .block-card { background: #fff; border-radius: 16px; padding: 24px; box-shadow: var(--shadow-sm); margin-bottom: 24px; border-left: 5px solid var(--navy); }
        .block-card h3 { font-size: 16px; color: var(--navy); margin-bottom: 16px; font-weight: 800; display: flex; align-items: center; gap: 8px; }
        .block-card h3 svg { width: 20px; height: 20px; stroke: var(--navy); fill: none; stroke-width: 2; }
        .block-form { display: flex; gap: 16px; align-items: flex-end; flex-wrap: wrap; }
        .block-form .f-group { display: flex; flex-direction: column; gap: 6px; flex: 1; min-width: 200px; }
        .block-form label { font-size: 12px; font-weight: 700; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.5px; }
        .block-form input { padding: 12px 14px; border: 1px solid var(--border); border-radius: 10px; font-family: inherit; font-size: 14px; outline: none; transition: 0.2s; background: #f8fafc; }
        .block-form input:focus { border-color: var(--navy); background: #fff; box-shadow: 0 0 0 3px rgba(13, 43, 110, 0.1); }
        .btn-block { background: var(--navy); color: #fff; border: none; padding: 12px 24px; border-radius: 10px; font-weight: 700; font-size: 14px; cursor: pointer; transition: 0.2s; box-shadow: 0 4px 12px rgba(13, 43, 110, 0.3); height: 44px; }
        .btn-block:hover { background: #081c4a; transform: translateY(-2px); }

        /* SESSIONS TABLE */
        .session-card { background: var(--card-bg); border-radius: 16px; box-shadow: var(--shadow-sm); overflow: hidden; border: 1px solid var(--border); }
        .session-head { background: var(--navy); color: #fff; padding: 18px 24px; display: flex; align-items: center; justify-content: space-between; flex-wrap: wrap; gap: 16px; }
        .session-head h3 { font-size: 18px; font-weight: 700; }
        .session-head .f-label { color: #93c5fd; margin-bottom: 0; display: inline-block; margin-right: 12px; font-weight: 600; }
        .ddl { padding: 8px 14px; border: none; border-radius: 8px; font-size: 13px; background: #fff; color: var(--navy); font-weight: 700; outline: none; cursor: pointer; }
        .session-body { padding: 0; }

        .msg { display: block; border-radius: 8px; padding: 14px 20px; font-size: 14px; font-weight: 600; margin: 20px 24px 0; }
        .msg.ok { background: #ecfdf5; border: 1px solid #a7f3d0; color: #065f46; }
        .msg.err { background: #fef2f2; border: 1px solid #fecaca; color: #991b1b; }

        .table-responsive { overflow-x: auto; }
        .grid { width: 100%; border-collapse: collapse; font-size: 14px; min-width: 900px; }
        .grid th { text-align: left; background: #f8fafc; color: var(--text-muted); font-size: 11.5px; text-transform: uppercase; padding: 16px 24px; border-bottom: 1px solid var(--border); font-weight: 800; }
        .grid td { padding: 16px 24px; border-bottom: 1px solid var(--border); color: var(--text-main); vertical-align: middle; }
        .grid tr:hover td { background: #f8fafc; }

        .chip { display: inline-flex; align-items: center; padding: 6px 12px; border-radius: 20px; font-size: 12px; font-weight: 700; }
        .chip.unsettled { background: #fff7ed; color: #c2410c; }
        .chip.pending { background: #fefce8; color: #a16207; }
        .chip.confirmed { background: #eff6ff; color: #1d4ed8; }
        .chip.completed { background: #ecfdf5; color: #047857; }
        .chip.cancelled { background: #fef2f2; color: #b91c1c; }

        .action-group { display: flex; align-items: center; gap: 8px; justify-content: flex-end; }
        .btn-act { border: none; border-radius: 8px; padding: 8px 16px; font-weight: 700; font-size: 12.5px; cursor: pointer; text-decoration: none; display: inline-flex; align-items: center; gap: 6px; transition: 0.2s; }
        .btn-complete { background: #ecfdf5; color: #047857; border: 1px solid #a7f3d0; }
        .btn-resched { background: #eff6ff; color: #1d4ed8; border: 1px solid #bfdbfe; }
        
        .dropdown { position: relative; display: inline-block; }
        .dropdown summary { list-style: none; cursor: pointer; background: #f1f5f9; color: var(--text-muted); border: 1px solid var(--border); padding: 8px 12px; border-radius: 8px; font-weight: 800; display: flex; align-items: center; justify-content: center; }
        .dropdown summary::-webkit-details-marker { display: none; }
        .dropdown summary:hover { background: #e2e8f0; color: var(--text-main); }
        .dropdown-menu { position: absolute; right: 0; top: 100%; margin-top: 4px; background: #fff; min-width: 180px; border-radius: 8px; box-shadow: var(--shadow-lg); border: 1px solid var(--border); z-index: 50; display: flex; flex-direction: column; padding: 6px; }
        .dropdown-item { padding: 10px 12px; border-radius: 6px; font-size: 13px; font-weight: 600; color: var(--text-main); text-decoration: none; display: flex; align-items: center; gap: 8px; }
        .dropdown-item.danger { color: var(--red); }

        .pending-badge { display: inline-flex; align-items: center; gap: 6px; background: #fffbeb; color: #b45309; border: 1px solid #fde68a; padding: 6px 12px; border-radius: 8px; font-size: 12px; font-weight: 700; }
        .pending-badge svg { width: 14px; height: 14px; stroke: currentColor; stroke-width: 2.5; fill: none; }

        .student-cell-wrap { display: flex; gap: 12px; align-items: center; }
        .student-avatar { width: 38px; height: 38px; border-radius: 50%; background: #e2e8f0; color: var(--navy); display: flex; align-items: center; justify-content: center; font-weight: 800; font-size: 14px; flex-shrink: 0; overflow: hidden; position: relative; border: 1px solid var(--border); }
        .student-avatar img { position: absolute; inset: 0; width: 100%; height: 100%; object-fit: cover; }
        .student-info { display: flex; flex-direction: column; }
        .student-cell { font-weight: 800; color: var(--navy); margin-bottom: 2px; font-size: 13.5px; }
        .student-meta { font-size: 12px; color: var(--text-muted); line-height: 1.5; }
        .student-meta a { color: var(--navy); text-decoration: none; font-weight: 600; }

        .empty { text-align: center; color: var(--text-muted); padding: 40px 0; font-size: 14px; font-weight: 500; }
        
        .action-panel { margin: 20px 24px; background: #f8fafc; border: 1px solid var(--border); border-radius: 8px; padding: 20px; }
        .action-panel b { color: var(--text-main); font-size: 15px; display: block; margin-bottom: 8px; }
        .action-row { display: flex; gap: 12px; flex-wrap: wrap; align-items: flex-end; margin-top: 16px; }
        .btn-panel { border: none; border-radius: 6px; padding: 10px 20px; font-weight: 600; font-size: 13.5px; cursor: pointer; }
        .btn-navy { background: var(--navy); color: #fff; }
        .btn-red { background: var(--red); color: #fff; }
        .btn-grey { background: #e2e8f0; color: var(--text-main); }
        .action-textarea { width: 100%; min-height: 80px; padding: 12px; border: 1px solid var(--border); border-radius: 6px; font-family: inherit; font-size: 14px; resize: vertical; outline: none; }

        .complete-inline { margin-top: 12px; background: #ecfdf5; border: 1px solid #a7f3d0; border-left: 4px solid #10b981; border-radius: 8px; padding: 12px 16px; text-align: left; }
        .complete-inline .head { font-weight: 800; color: #064e3b; margin-bottom: 6px; font-size: 14px; }
        .complete-inline .meta { font-size: 12.5px; color: #065f46; margin-bottom: 12px; line-height: 1.5; }

        @media (max-width: 1024px) { .layout { flex-direction: column; padding: 16px; } .sidebar { width: 100%; position: relative; top: 0; display: flex; flex-wrap: wrap; gap: 8px; padding: 12px; } .side-item { margin: 0; } .instructor-card { display: none; } .main { padding: 20px 0; } }
        @media (max-width: 768px) { .user-name { display: none; } .topbar { padding: 12px 16px; } .cards-row { grid-template-columns: 1fr 1fr; } .block-form { flex-direction: column; align-items: stretch; } }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="topbar">
            <a href="~/InstructorDashboard" runat="server" class="logo">
                <span class="logo-name">IKHWEZI<span>.</span></span>
                <span class="logo-sub">DRIVING SCHOOL</span>
            </a>
            <div class="topbar-right">
                <a href="~/PersonalInfo" runat="server" class="profile-wrap" title="My Profile">
                    <span class="user-name"><asp:Literal ID="litStaffName" runat="server" /></span>
                    <span class="avatar"><asp:Literal ID="litInitials" runat="server" /></span>
                </a>
            </div>
        </div>

        <div class="layout">
            <aside class="sidebar">
                <a href="~/InstructorDashboard" runat="server" class="side-item selected">
                    <svg viewBox="0 0 24 24"><rect x="3" y="3" width="7" height="7"></rect><rect x="14" y="3" width="7" height="7"></rect><rect x="14" y="14" width="7" height="7"></rect><rect x="3" y="14" width="7" height="7"></rect></svg>
                    Dashboard
                </a>
                <a href="~/Support" runat="server" class="side-item">
                    <svg viewBox="0 0 24 24"><path d="M3 18v-6a9 9 0 0 1 18 0v6"></path><path d="M21 19a2 2 0 0 1-2 2h-1a2 2 0 0 1-2-2v-3a2 2 0 0 1 2-2h3zM3 19a2 2 0 0 0 2 2h1a2 2 0 0 0 2-2v-3a2 2 0 0 0-2-2H3z"></path></svg>
                    Support
                </a>
                <a href="javascript:void(0);" onclick="document.getElementById('<%= btnLogoutSide.ClientID %>').click(); return false;" class="side-item">
                    <svg viewBox="0 0 24 24"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"></path><polyline points="16 17 21 12 16 7"></polyline><line x1="21" y1="12" x2="9" y2="12"></line></svg>
                    Logout
                </a>
                <asp:LinkButton ID="btnLogoutSide" runat="server" style="display:none;" OnClick="btnLogoutSide_Click">Logout</asp:LinkButton>
                
                <div class="instructor-card">
                    <div class="title">Teaching Resources</div>
                    <p>Quick access to the K53 syllabus and student policies.</p>
                    <a href="~/LearnersGuide" runat="server" class="res-link">
                        <svg viewBox="0 0 24 24"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20"></path><path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z"></path></svg>
                        K53 Learner's Guide
                    </a>
                </div>
            </aside>

            <main class="main">
                <h2 class="page-title">Today's Teaching Overview</h2>
                <p class="page-sub">Your assigned students and lessons at a glance.</p>

                <asp:Label ID="lblMsg" runat="server" Visible="false" />

                <div class="cards-row">
                    <asp:LinkButton ID="cardStudents" runat="server" CssClass="stat-card" OnClick="cardStudents_Click" CausesValidation="false">
                        <span class="stat-value"><asp:Literal ID="lblCardStudents" runat="server" /></span>
                        <span class="stat-label">Assigned Students</span>
                    </asp:LinkButton>
                    <asp:LinkButton ID="cardToday" runat="server" CssClass="stat-card hot" OnClick="cardToday_Click" CausesValidation="false">
                        <span class="stat-value"><asp:Literal ID="lblCardToday" runat="server" /></span>
                        <span class="stat-label">Today's Sessions</span>
                    </asp:LinkButton>
                    <asp:LinkButton ID="cardAwaiting" runat="server" CssClass="stat-card warn" OnClick="cardAwaiting_Click" CausesValidation="false">
                        <span class="stat-value"><asp:Literal ID="lblCardUnsettled" runat="server" /></span>
                        <span class="stat-label">Awaiting Deposit</span>
                    </asp:LinkButton>
                    <asp:LinkButton ID="cardCompleted" runat="server" CssClass="stat-card green" OnClick="cardCompleted_Click" CausesValidation="false">
                        <span class="stat-value"><asp:Literal ID="lblCardCompleted" runat="server" /></span>
                        <span class="stat-label">Completed Lessons</span>
                    </asp:LinkButton>
                </div>

                <div class="block-card">
                    <h3>
                        <svg viewBox="0 0 24 24"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect><line x1="16" y1="2" x2="16" y2="6"></line><line x1="8" y1="2" x2="8" y2="6"></line><line x1="3" y1="10" x2="21" y2="10"></line></svg>
                        Manage My Availability
                    </h3>
                    <div class="block-form">
                        <div class="f-group">
                            <label>Select Date to Block</label>
                            <asp:TextBox ID="txtBlockDate" runat="server" TextMode="Date" />
                        </div>
                        <div class="f-group">
                            <label>Reason (Optional)</label>
                            <asp:TextBox ID="txtBlockReason" runat="server" placeholder="e.g. Sick Leave" />
                        </div>
                        <asp:Button ID="btnBlockDate" runat="server" Text="Block Date" CssClass="btn-block" OnClick="btnBlockDate_Click" />
                    </div>
                </div>

                <div class="session-card" id="sessions">
                    <div class="session-head">
                        <h3>Session Management</h3>
                        <div>
                            <span class="f-label">Show:</span>
                            <asp:DropDownList ID="ddlFilter" runat="server" CssClass="ddl" AutoPostBack="true" OnSelectedIndexChanged="ddlFilter_SelectedIndexChanged">
                                <asp:ListItem Value="active" Selected="True">Upcoming / Active</asp:ListItem>
                                <asp:ListItem Value="completed">Completed</asp:ListItem>
                                <asp:ListItem Value="all">All Sessions</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                    </div>

                    <div class="session-body">
                        <div class="table-responsive">
                            <asp:Repeater ID="repSessions" runat="server">
                                <HeaderTemplate>
                                    <table class="grid">
                                        <thead>
                                            <tr>
                                                <th>Ref</th>
                                                <th>Date &amp; Time</th>
                                                <th>Student</th>
                                                <th>Status</th>
                                                <th style="text-align:right;">Actions</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <tr>
                                        <td><b>#<%# Eval("BookingID") %></b></td>
                                        <td><%# Convert.ToDateTime(Eval("ScheduledDateTime")).ToString("ddd, d MMM yyyy HH:mm") %></td>
                                        <td>
                                            <div class="student-cell-wrap">
                                                <div class="student-avatar">
                                                    <%# Eval("Initials") %>
                                                    <asp:Image runat="server" ImageUrl='<%# Eval("ProfilePhotoUrl") %>' Visible='<%# IsPhotoAvailable(Eval("ProfilePhotoUrl")) %>' />
                                                </div>
                                                <div class="student-info">
                                                    <div class="student-cell"><%# Eval("StudentName") %></div>
                                                    <%# BuildStudentMeta(Container.DataItem) %>
                                                </div>
                                            </div>
                                        </td>
                                        <td>
                                            <span class='chip <%# Chip(Eval("BookingStatus").ToString()) %>'>
                                                <%# Eval("BookingStatus") %>
                                            </span>
                                        </td>
                                        <td style="text-align:right;">
                                            <asp:PlaceHolder runat="server" Visible='<%# HasPendingRequestUI(Eval("InstructorRequestStatus")) %>'>
                                                <div class="pending-badge">
                                                    <svg viewBox="0 0 24 24"><circle cx="12" cy="12" r="10"></circle><polyline points="12 6 12 12 16 14"></polyline></svg>
                                                    <%# GetRequestLabel(Eval("InstructorRequestStatus")) %>
                                                </div>
                                            </asp:PlaceHolder>

                                            <asp:PlaceHolder runat="server" Visible='<%# !HasPendingRequestUI(Eval("InstructorRequestStatus")) %>'>
                                                <div class="action-group">
                                                    <asp:LinkButton runat="server" CssClass="btn-act btn-complete"
                                                        CommandArgument='<%# Eval("BookingID") %>'
                                                        OnCommand="btnComplete_ShowConfirm"
                                                        Visible='<%# CanBeMarkedCompleted(Eval("BookingStatus").ToString(), Eval("ScheduledDateTime"), Eval("InstructorRequestStatus")) %>'>
                                                        Mark Completed
                                                    </asp:LinkButton>

                                                    <asp:LinkButton runat="server" CssClass="btn-act btn-resched"
                                                        CommandArgument='<%# Eval("BookingID") %>'
                                                        OnCommand="btnResched_Command"
                                                        Visible='<%# IsReschedulable(Eval("BookingStatus").ToString(), Eval("InstructorRequestStatus")) %>'>
                                                        Reschedule
                                                    </asp:LinkButton>

                                                    <details class="dropdown">
                                                        <summary>⋮</summary>
                                                        <div class="dropdown-menu">
                                                            <asp:LinkButton runat="server" CssClass="dropdown-item danger"
                                                                CommandArgument='<%# Eval("BookingID") %>'
                                                                OnCommand="btnRequestCancel_Command"
                                                                Visible='<%# CanRequestCancel(Eval("BookingStatus").ToString(), Eval("InstructorRequestStatus"), Eval("ScheduledDateTime")) %>'>
                                                                Request Cancel
                                                            </asp:LinkButton>

                                                            <asp:LinkButton runat="server" CssClass="dropdown-item"
                                                                CommandArgument='<%# Eval("BookingID") %>'
                                                                OnCommand="btnRequestReplace_Command"
                                                                Visible='<%# CanRequestReplace(Eval("BookingStatus").ToString(), Eval("InstructorRequestStatus"), Eval("ScheduledDateTime")) %>'>
                                                                Request Replacement
                                                            </asp:LinkButton>
                                                        </div>
                                                    </details>
                                                </div>
                                            </asp:PlaceHolder>

                                            <asp:Panel runat="server" CssClass="complete-inline" Visible='<%# IsPendingComplete(Container.DataItem) %>'>
                                                <div class="head">Confirm Lesson Completion</div>
                                                <div class="meta">Are you sure you have completed this lesson? This will deduct 1 lesson from the student's package.</div>
                                                <asp:LinkButton runat="server" CssClass="btn-act btn-complete"
                                                    CommandArgument='<%# Eval("BookingID") %>'
                                                    OnCommand="btnComplete_Confirm"
                                                    style="background:#22b573; color:#fff; border:none;">
                                                    Yes, Mark Completed
                                                </asp:LinkButton>
                                                <asp:LinkButton runat="server" CssClass="btn-act btn-grey"
                                                    OnCommand="btnComplete_Dismiss"
                                                    style="background:#e2e8f0; color:#333; border:none; margin-left:8px;">
                                                    Cancel
                                                </asp:LinkButton>
                                            </asp:Panel>
                                            
                                        </td>
                                    </tr>
                                </ItemTemplate>
                                <FooterTemplate>
                                        </tbody>
                                    </table>
                                </FooterTemplate>
                            </asp:Repeater>
                        </div>

                        <asp:Literal ID="litEmpty" runat="server" Visible="false"
                            Text="<div class='empty'>No sessions found for this filter.</div>" />

                        <asp:Panel ID="pnlReschedule" runat="server" CssClass="action-panel" Visible="false">
                            <b>Reschedule booking <asp:Literal ID="litReschedTarget" runat="server" /></b>
                            <div class="action-row">
                                <div>
                                    <span class="f-label">New date</span>
                                    <asp:TextBox ID="txtNewDate" runat="server" CssClass="f-input" TextMode="Date" style="padding:10px; border:1px solid #e2e8f0; border-radius:6px;" />
                                </div>
                                <div>
                                    <span class="f-label">Start time</span>
                                    <asp:DropDownList ID="ddlNewTime" runat="server" CssClass="ddl" />
                                </div>
                                <asp:Button ID="btnDoResched" runat="server" Text="Confirm Reschedule" CssClass="btn-panel btn-navy" OnClick="btnDoResched_Click" />
                                <asp:Button ID="btnCloseResched" runat="server" Text="Close" CssClass="btn-panel btn-grey" OnClick="btnCloseResched_Click" CausesValidation="false" />
                            </div>
                        </asp:Panel>

                        <asp:Panel ID="pnlRequestCancel" runat="server" CssClass="action-panel" Visible="false">
                            <b>Request cancellation for booking <asp:Literal ID="litRequestCancelTarget" runat="server" /></b>
                            <div style="font-size:13px; color:#64748b; margin-bottom:12px;">Reception will review your request. Please give them at least 1 hour.</div>
                            <asp:TextBox ID="txtRequestCancelReason" runat="server" CssClass="action-textarea" TextMode="MultiLine" placeholder="e.g. I have a medical appointment." MaxLength="500" />
                            <div class="action-row">
                                <asp:Button ID="btnDoRequestCancel" runat="server" Text="Send Request" CssClass="btn-panel btn-red" OnClick="btnDoRequestCancel_Click" />
                                <asp:Button ID="btnCloseRequestCancel" runat="server" Text="Close" CssClass="btn-panel btn-grey" OnClick="btnCloseRequestCancel_Click" CausesValidation="false" />
                            </div>
                        </asp:Panel>

                        <asp:Panel ID="pnlRequestReplace" runat="server" CssClass="action-panel" Visible="false">
                            <b>Request replacement for booking <asp:Literal ID="litRequestReplaceTarget" runat="server" /></b>
                            <div style="font-size:13px; color:#64748b; margin-bottom:12px;">Reception will find another instructor. Please give them at least 1 hour.</div>
                            <asp:TextBox ID="txtRequestReplaceReason" runat="server" CssClass="action-textarea" TextMode="MultiLine" placeholder="e.g. Vehicle unavailable." MaxLength="500" />
                            <div class="action-row">
                                <asp:Button ID="btnDoRequestReplace" runat="server" Text="Send Request" CssClass="btn-panel btn-navy" OnClick="btnDoRequestReplace_Click" />
                                <asp:Button ID="btnCloseRequestReplace" runat="server" Text="Close" CssClass="btn-panel btn-grey" OnClick="btnCloseRequestReplace_Click" CausesValidation="false" />
                            </div>
                        </asp:Panel>

                        <asp:HiddenField ID="hfReschedBooking" runat="server" />
                        <asp:HiddenField ID="hfActionBooking" runat="server" />

                    </div>
                </div>
            </main>
        </div>
    </form>
</body>
</html>