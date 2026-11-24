 
<%@ Page Title="Reception Dashboard" Language="C#" AutoEventWireup="true" CodeBehind="AdminDashboard.aspx.cs" Inherits="WebApplicationM3.AdminDashboard" %>
<!DOCTYPE html>
<html lang="en">
<head id="head1" runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title><%: Page.Title %> - IKHWEZI</title>
    <style>
        * { box-sizing: border-box; margin: 0; padding: 0; }
        body { font-family: 'Segoe UI', Arial, sans-serif; background: linear-gradient(180deg, #e9f1fc, #d9e5f5); min-height: 100vh; }

        /* ===== TOP BAR ===== */
        .topbar { background: #0d2b6e; display: flex; align-items: center; justify-content: space-between; padding: 10px 24px; }
        .logo { text-decoration: none; display: flex; flex-direction: column; line-height: 1.1; }
        .logo-name { font-size: 22px; font-weight: 800; font-style: italic; color: #fff; letter-spacing: 1px; }
        .logo-name span { color: #e8283f; }
        .logo-sub { font-size: 9px; color: #c3d2ee; letter-spacing: 3px; }
        .topbar-right { display: flex; align-items: center; gap: 14px; }
        .role-badge { background: #e8283f; color: #fff; padding: 4px 12px; border-radius: 12px; font-size: 11px; font-weight: 800; letter-spacing: .5px; text-transform: uppercase; }
        .user-name { color: #fff; font-size: 14px; font-weight: 600; }
        .avatar { width: 34px; height: 34px; border-radius: 50%; background: #e8283f; color: #fff; display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: 13px; }

        /* ===== LAYOUT & SIDEBAR ===== */
        .layout { display: flex; gap: 20px; padding: 20px 24px; align-items: flex-start; }
        .sidebar { width: 235px; flex-shrink: 0; background: #fff; border-radius: 12px; padding: 14px; box-shadow: 0 4px 15px rgba(13,43,110,.08); }
        .side-item { display: flex; align-items: center; gap: 10px; padding: 10px 12px; margin-bottom: 6px; border-radius: 8px; color: #0d2b6e; font-size: 13.5px; font-weight: 600; text-decoration: none; transition: 0.2s; }
        .side-item svg { width: 18px; height: 18px; stroke: currentColor; stroke-width: 2; fill: none; stroke-linecap: round; stroke-linejoin: round; }
        .side-item:hover { background: #eef4ff; }
        .side-item.selected, .side-item.selected:hover { background: #0d2b6e; color: #fff; }

        /* ===== MAIN CONTENT ===== */
        .main { flex: 1; min-width: 0; }
        .page-title { color: #0d2b6e; font-size: 20px; font-weight: 700; margin-bottom: 4px; }
        .page-sub { font-size: 13px; color: #555; margin-bottom: 16px; }

        /* SEARCH BAR */
        .search-box { background: #fff; border-radius: 12px; padding: 10px; margin-bottom: 20px; display: flex; gap: 10px; box-shadow: 0 4px 15px rgba(13,43,110,.08); }
        .search-input { flex: 1; padding: 12px 16px; border: 1px solid #ccd7ec; border-radius: 8px; font-size: 14px; outline: none; }
        .search-input:focus { border-color: #0d2b6e; }
        .btn-search { background: #0d2b6e; color: #fff; border: none; padding: 0 24px; border-radius: 8px; font-weight: 700; cursor: pointer; }
        .btn-search:hover { background: #1a4199; }

        /* STAT CARDS */
        .cards-row { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 14px; margin-bottom: 18px; }
        .stat-card { background: #fff; border-radius: 12px; padding: 18px 20px; box-shadow: 0 4px 15px rgba(13,43,110,.08); border-top: 3px solid #0d2b6e; }
        .stat-card.warn { border-top-color: #e8890c; }
        .stat-card.hot { border-top-color: #e8283f; }
        .stat-card.green { border-top-color: #22b573; }
        .stat-value { font-size: 2em; font-weight: 800; color: #0d2b6e; display: block; }
        .stat-card.warn .stat-value { color: #e8890c; }
        .stat-card.hot .stat-value { color: #e8283f; }
        .stat-card.green .stat-value { color: #22b573; }
        .stat-label { font-size: 11.5px; color: #7787a5; font-weight: 700; letter-spacing: 1px; text-transform: uppercase; }

        /* SESSION GRID */
        .session-card { background: #fff; border-radius: 12px; box-shadow: 0 4px 15px rgba(13,43,110,.08); overflow: hidden; }
        .session-head { background: #0d2b6e; color: #fff; padding: 14px 20px; display: flex; align-items: center; justify-content: space-between; flex-wrap: wrap; gap: 10px; }
        .session-head h3 { font-size: 16px; }
        .session-head .f-label { color: #c3d2ee; margin-bottom: 0; display: inline-block; margin-right: 8px; }
        .ddl { padding: 8px 10px; border: 1px solid #ccd7ec; border-radius: 6px; font-size: 13px; background: #fff; }
        .session-body { padding: 18px 20px; }

        .msg { display: block; border-radius: 8px; padding: 11px 15px; font-size: 13.5px; font-weight: 600; margin-bottom: 14px; }
        .msg.ok { background: #effaf4; border: 1px solid #bfe8d4; color: #14663f; }
        .msg.err { background: #fdf0f1; border: 1px solid #f3c2c9; color: #a3141f; }

        .grid { width: 100%; border-collapse: collapse; font-size: 13.5px; }
        .grid th { text-align: left; background: #f2f6fd; color: #0d2b6e; font-size: 11.5px; letter-spacing: .5px; text-transform: uppercase; padding: 10px 12px; border-bottom: 2px solid #dbe4f6; }
        .grid td { padding: 11px 12px; border-bottom: 1px solid #eef2fa; color: #333; vertical-align: top; }
        .grid tr:hover td { background: #f8fbff; }

        .chip { display: inline-block; padding: 4px 12px; border-radius: 12px; font-size: 11.5px; font-weight: 700; }
        .chip.unsettled { background: #fde3c8; color: #8a4b00; }
        .chip.pending { background: #fff3d6; color: #8a6100; }
        .chip.confirmed { background: #dcebfd; color: #0d2b6e; }
        .chip.completed { background: #dcf5e8; color: #14663f; }
        .chip.cancelled { background: #fde3e7; color: #c81e33; }
        .chip.rescheduled { background: #efe7fd; color: #5b2d9e; }
        .chip.overdue { background: #fde3c8; color: #8a4b00; }
        .chip.suspended { background: #fde3e7; color: #c81e33; }

        .btn-act { border: none; border-radius: 14px; padding: 6px 14px; font-weight: 700; font-size: 11.5px; cursor: pointer; text-decoration: none; display: inline-block; margin-right: 6px; margin-bottom: 4px; }
        .btn-confirm { background: #22b573; color: #fff; }
        .btn-confirm:hover { background: #1d9e63; color: #fff; text-decoration: none; }
        .btn-cancel { background: #e8283f; color: #fff; }
        .btn-cancel:hover { background: #c81e33; color: #fff; text-decoration: none; }
        .btn-view { background: #0d2b6e; color: #fff; }
        .btn-view:hover { background: #1a4199; color: #fff; text-decoration: none; }

        /* REQUEST BADGES */
        .req-badge { display: inline-flex; align-items: center; gap: 4px; background: #fffbeb; border: 1px solid #fde68a; color: #b45309; padding: 4px 8px; border-radius: 6px; font-size: 11px; font-weight: 700; margin-top: 6px; }
        .req-reason { font-size: 11px; color: #b91c1c; font-style: italic; display: block; margin-top: 4px; }
        .btn-req-approve { background: #0d2b6e; color: #fff; border-radius: 6px; padding: 6px 12px; font-size: 12px; font-weight: 600; text-decoration: none; display: inline-block; margin-right: 4px; }
        .btn-req-approve:hover { background: #1a4199; color: #fff; text-decoration: none; }
        .btn-req-decline { background: #f1f5f9; color: #475569; border: 1px solid #cbd5e1; border-radius: 6px; padding: 6px 12px; font-size: 12px; font-weight: 600; text-decoration: none; display: inline-block; }
        .btn-req-decline:hover { background: #e2e8f0; color: #0f172a; text-decoration: none; }

        .empty { text-align: center; color: #7787a5; font-size: 13.5px; padding: 22px 0 6px; }
        .ref-note { font-size: 11.5px; color: #99a3b5; margin-top: 12px; }

        @media (max-width: 950px) {
            .layout { flex-direction: column; }
            .sidebar { width: 100%; }
        }

        /* FRIENDLY IN-WEBSITE CONFIRMATION */
        .ikhwezi-overlay {
            position: fixed;
            inset: 0;
            z-index: 99999;
            display: none;
            align-items: center;
            justify-content: center;
            padding: 20px;
            background: rgba(7, 20, 48, 0.70);
        }
        .ikhwezi-overlay.visible { display: flex; }
        .ikhwezi-dialog {
            width: 100%;
            max-width: 430px;
            padding: 28px;
            background: #fff;
            border-radius: 18px;
            box-shadow: 0 24px 65px rgba(0,0,0,.28);
            text-align: center;
        }
        .ikhwezi-dialog-icon {
            display: flex;
            align-items: center;
            justify-content: center;
            width: 60px;
            height: 60px;
            border-radius: 50%;
            margin: 0 auto 17px;
            background: #eaf1ff;
            color: #0d2b6e;
            font-size: 32px;
            font-weight: 800;
        }
        .ikhwezi-dialog h2 {
            color: #0d2b6e;
            font-size: 22px;
            margin-bottom: 12px;
        }
        .ikhwezi-dialog p {
            color: #53627a;
            font-size: 14px;
            line-height: 1.6;
            margin-bottom: 24px;
        }
        .ikhwezi-dialog-actions {
            display: flex;
            gap: 12px;
        }
        .ikhwezi-dialog-actions button {
            flex: 1;
            padding: 12px;
            border: 0;
            border-radius: 9px;
            cursor: pointer;
            font-size: 13px;
            font-weight: 700;
        }
        .ikhwezi-no {
            background: #edf1f7;
            color: #344054;
        }
        .ikhwezi-yes {
            background: #0d2b6e;
            color: white;
        }
        .ikhwezi-yes.danger { background: #e8283f; }
        .ikhwezi-dialog-actions button:hover {
            filter: brightness(.94);
        }
    </style>

    <script type="text/javascript">
        var ikhweziPendingAction = null;
        var ikhweziPreviousFocus = null;

        function ikhweziAsk(link, action) {
            var titles = {
                Approve: "Approve instructor request?",
                Decline: "Decline instructor request?",
                Confirm: "Confirm this booking?",
                Cancel: "Cancel this booking?"
            };

            var descriptions = {
                Approve: "This will process the instructor's request and may update the booking or instructor assignment. Do you want to continue?",
                Decline: "The instructor's request will be declined and they will be notified. Do you want to continue?",
                Confirm: "This booking will be marked as confirmed. Do you want to continue?",
                Cancel: "This booking will be cancelled. Do you want to continue?"
            };

            if (!titles[action]) return false;

            ikhweziPendingAction = link;
            ikhweziPreviousFocus = document.activeElement;

            document.getElementById("ikhweziDialogTitle").textContent =
                titles[action];

            document.getElementById("ikhweziDialogText").textContent =
                descriptions[action];

            var yes = document.getElementById("ikhweziDialogYes");
            yes.className = "ikhwezi-yes" +
                ((action === "Cancel" || action === "Decline") ? " danger" : "");

            var overlay = document.getElementById("ikhweziOverlay");
            overlay.classList.add("visible");
            overlay.setAttribute("aria-hidden", "false");
            document.getElementById("ikhweziDialogNo").focus();

            return false;
        }

        function ikhweziClose() {
            var overlay = document.getElementById("ikhweziOverlay");
            overlay.classList.remove("visible");
            overlay.setAttribute("aria-hidden", "true");
            ikhweziPendingAction = null;

            if (ikhweziPreviousFocus &&
                typeof ikhweziPreviousFocus.focus === "function") {
                ikhweziPreviousFocus.focus();
            }
            return false;
        }

        function ikhweziProceed() {
            var link = ikhweziPendingAction;
            ikhweziPendingAction = null;

            document.getElementById("ikhweziOverlay")
                .classList.remove("visible");

            if (!link) return false;

            // Invoke the original ASP.NET LinkButton postback.
            // Do not call link.click(), which would reopen this dialog.
            var href = link.getAttribute("href") || "";

            if (href.indexOf("javascript:") === 0) {
                var script = href.substring("javascript:".length);
                // ASP.NET generated postback links use __doPostBack.
                // Invoke the event target directly instead.
                var match = script.match(
                    /__doPostBack\('([^']*)','([^']*)'\)/
                );

                if (match && typeof __doPostBack === "function") {
                    __doPostBack(match[1], match[2]);
                } else {
                    // Preserve ASP.NET's generated postback action.
                    link.removeAttribute("onclick");
                    link.click();
                }
            } else {
                link.removeAttribute("onclick");
                link.click();
            }

            return false;
        }

        document.addEventListener("keydown", function (event) {
            var overlay = document.getElementById("ikhweziOverlay");
            if (!overlay || !overlay.classList.contains("visible")) return;

            if (event.key === "Escape") {
                event.preventDefault();
                ikhweziClose();
            }
            if (event.key === "Tab") {
                var no = document.getElementById("ikhweziDialogNo");
                var yes = document.getElementById("ikhweziDialogYes");
                if (event.shiftKey && document.activeElement === no) {
                    event.preventDefault();
                    yes.focus();
                } else if (!event.shiftKey && document.activeElement === yes) {
                    event.preventDefault();
                    no.focus();
                }
            }
        });
    </script>
</head>
<body>
    <form id="form1" runat="server">

        <div class="topbar">
            <a href="~/AdminDashboard" runat="server" class="logo">
                <span class="logo-name">IKHWEZI<span>.</span></span>
                <span class="logo-sub">DRIVING SCHOOL</span>
            </a>
            <div class="topbar-right">
                <span class="role-badge"><asp:Literal ID="litRoleBadge" runat="server" /></span>
                <span class="user-name"><asp:Literal ID="litStaffName" runat="server" /></span>
                <span class="avatar"><asp:Literal ID="litInitials" runat="server" /></span>
            </div>
        </div>

        <div class="layout">
            <aside class="sidebar">
                <a href="~/AdminDashboard" runat="server" class="side-item selected">
                    <svg viewBox="0 0 24 24"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect><line x1="16" y1="2" x2="16" y2="6"></line><line x1="8" y1="2" x2="8" y2="6"></line><line x1="3" y1="10" x2="21" y2="10"></line></svg>
                    Bookings
                </a>

                <a href="~/AdminStudents" runat="server" class="side-item">
                    <svg viewBox="0 0 24 24"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path><circle cx="9" cy="7" r="4"></circle><path d="M23 21v-2a4 4 0 0 0-3-3.87"></path><path d="M16 3.13a4 4 0 0 1 0 7.75"></path></svg>
                    Students
                </a>

                <a href="~/AdminDocuments" runat="server" class="side-item">
                    <svg viewBox="0 0 24 24"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path><polyline points="14 2 14 8 20 8"></polyline><line x1="16" y1="13" x2="8" y2="13"></line><line x1="16" y1="17" x2="8" y2="17"></line><polyline points="10 9 9 9 8 9"></polyline></svg>
                    Documents
                </a>

                <asp:PlaceHolder ID="phManagerOnly" runat="server">
                    <a href="~/Account/StaffNotify" runat="server" class="side-item">
                        <svg viewBox="0 0 24 24"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"></path><polyline points="22,6 12,13 2,6"></polyline></svg>
                        Notify Staff
                    </a>

                    <a href="~/InstructorDashboard" runat="server" class="side-item">
                        <svg viewBox="0 0 24 24"><path d="M2 3h6a4 4 0 0 1 4 4v14a3 3 0 0 0-3-3H2z"></path><path d="M22 3h-6a4 4 0 0 0-4 4v14a3 3 0 0 1 3-3h7z"></path></svg>
                        Instructor View
                    </a>
                </asp:PlaceHolder>

                <asp:LinkButton ID="btnLogoutSide" runat="server" CssClass="side-item" OnClick="btnLogoutSide_Click">
                    <svg viewBox="0 0 24 24"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"></path><polyline points="16 17 21 12 16 7"></polyline><line x1="21" y1="12" x2="9" y2="12"></line></svg>
                    Logout
                </asp:LinkButton>
            </aside>

            <main class="main">
                <h2 class="page-title">Reception Workstation</h2>
                <p class="page-sub">Manage student bookings, and confirmations.</p>

                <!-- SEARCH BAR -->
                <div class="search-box">
                    <asp:TextBox ID="txtSearch" runat="server" CssClass="search-input" placeholder="Search by student name, email, or booking #..." />
                    <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn-search" OnClick="btnSearch_Click" />
                    <asp:Button ID="btnClearSearch" runat="server" Text="Clear" CssClass="btn-search" style="background:#a9b4c4;" OnClick="btnClearSearch_Click" />
                </div>

                <!-- STAT CARDS -->
                <div class="cards-row">
                    <div class="stat-card warn">
                        <span class="stat-value"><asp:Literal ID="lblCardUnsettled" runat="server" /></span>
                        <span class="stat-label">Awaiting Deposit</span>
                    </div>
                    <div class="stat-card hot">
                        <span class="stat-value"><asp:Literal ID="lblCardPending" runat="server" /></span>
                        <span class="stat-label">Pending Confirmation</span>
                    </div>
                    <div class="stat-card green">
                        <span class="stat-value"><asp:Literal ID="lblCardConfirmed" runat="server" /></span>
                        <span class="stat-label">Confirmed</span>
                    </div>
                    <div class="stat-card">
                        <span class="stat-value"><asp:Literal ID="lblCardToday" runat="server" /></span>
                        <span class="stat-label">Today's Lessons</span>
                    </div>
                </div>

                <!-- BOOKINGS LIST -->
                <div class="session-card">
                    <div class="session-head">
                        <h3>Booking Management</h3>
                        <div>
                            <span class="f-label">Show:</span>
                            <asp:DropDownList ID="ddlFilter" runat="server" CssClass="ddl" AutoPostBack="true" OnSelectedIndexChanged="ddlFilter_SelectedIndexChanged">
                                <asp:ListItem Value="active" Selected="True">Active (Unsettled / Pending / Confirmed)</asp:ListItem>
                                <asp:ListItem Value="unsettled">Awaiting deposit</asp:ListItem>
                                <asp:ListItem Value="pending">Pending Confirmation</asp:ListItem>
                                <asp:ListItem Value="confirmed">Confirmed</asp:ListItem>
                                <asp:ListItem Value="today">Today (all)</asp:ListItem>
                                <asp:ListItem Value="completed">Completed</asp:ListItem>
                                <asp:ListItem Value="all">Everything</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                    </div>

                    <div class="session-body">
                        <asp:Label ID="lblMsg" runat="server" Visible="false" />

                        <asp:Repeater ID="repBookings" runat="server">
                            <HeaderTemplate>
                                <table class="grid">
                                    <thead>
                                        <tr>
                                            <th>Ref</th>
                                            <th>Student</th>
                                            <th>Scheduled</th>
                                            <th>Instructor</th>
                                            <th>Status</th>
                                            <th>Actions</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                    <td><b>#<%# Eval("BookingID") %></b></td>
                                    <td>
                                        <strong><%# Eval("StudentName") %></strong><br />
                                        <span style="font-size:11.5px;color:#7787a5;"><%# Eval("StudEmail") %></span>
                                    </td>
                                    <td><%# Convert.ToDateTime(Eval("ScheduledDateTime")).ToString("ddd, d MMM yyyy HH:mm") %></td>
                                    <td>
                                        <%# Eval("InstructorName") %>

                                        <!-- REQUEST BADGE -->
                                        <%# HasRequest(Eval("InstructorRequestStatus")) ? "<br/><span class='req-badge'>⚠ " + Eval("InstructorRequestStatus").ToString() + " Requested</span>" : "" %>
                                        <%# HasRequest(Eval("InstructorRequestStatus")) ? "<span class='req-reason'>Reason: " + Server.HtmlEncode(Convert.ToString(Eval("InstructorRequestReason"))) + "</span>" : "" %>
                                    </td>
                                    <td>
                                        <span class='chip <%# Chip(Eval("BookingStatus").ToString()) %>'>
                                            <%# Eval("BookingStatus") %>
                                        </span>
                                    </td>
                                    <td>
                                        <!-- APPROVE / DECLINE -->
                                        <asp:PlaceHolder runat="server" Visible='<%# HasRequest(Eval("InstructorRequestStatus")) %>'>
                                            <asp:LinkButton runat="server" CssClass="btn-req-approve"
                                                CommandArgument='<%# Eval("BookingID") %>'
                                                OnCommand="btnApproveRequest_Command"
                                                OnClientClick="return ikhweziAsk(this, 'Approve');">
                                                Approve
                                            </asp:LinkButton>
                                            <asp:LinkButton runat="server" CssClass="btn-req-decline"
                                                CommandArgument='<%# Eval("BookingID") %>'
                                                OnCommand="btnDeclineRequest_Command"
                                                OnClientClick="return ikhweziAsk(this, 'Decline');">
                                                Decline
                                            </asp:LinkButton>
                                        </asp:PlaceHolder>

                                        <!-- STANDARD ACTIONS -->
                                        <asp:PlaceHolder runat="server" Visible='<%# !HasRequest(Eval("InstructorRequestStatus")) %>'>
                                            <asp:LinkButton runat="server" CssClass="btn-act btn-view"
                                                CommandArgument='<%# Eval("StudEmail") %>'
                                                OnCommand="btnViewStudent_Command">Profile</asp:LinkButton>

                                            <asp:LinkButton runat="server" CssClass="btn-act btn-confirm"
                                                CommandArgument='<%# Eval("BookingID") %>'
                                                OnCommand="btnConfirm_Command"
                                                Visible='<%# IsPending(Eval("BookingStatus").ToString()) %>'
                                                OnClientClick="return ikhweziAsk(this, 'Confirm');">Confirm</asp:LinkButton>

                                            <asp:LinkButton runat="server" CssClass="btn-act btn-cancel"
                                                CommandArgument='<%# Eval("BookingID") %>'
                                                OnCommand="btnCancel_Command"
                                                Visible='<%# IsCancellable(Eval("BookingStatus").ToString()) %>'
                                                OnClientClick="return ikhweziAsk(this, 'Cancel');">Cancel</asp:LinkButton>
                                        </asp:PlaceHolder>
                                    </td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                    </tbody>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>

                        <asp:Literal ID="litEmpty" runat="server" Visible="false"
                            Text="<div class='empty'>No bookings match this filter or search.</div>" />

                        <div class="ref-note">
                            Online PayFast/Paystack payments update automatically. Cash/EFT payments must be recorded in the Desktop App.
                        </div>
                    </div>
                </div>
            </main>
        </div>

        <!-- FRIENDLY CONFIRMATION WINDOW INSIDE WEBSITE -->
        <div id="ikhweziOverlay" class="ikhwezi-overlay"
             aria-hidden="true">
            <div class="ikhwezi-dialog"
                 role="dialog"
                 aria-modal="true"
                 aria-labelledby="ikhweziDialogTitle"
                 aria-describedby="ikhweziDialogText">
                <div class="ikhwezi-dialog-icon">?</div>
                <h2 id="ikhweziDialogTitle">Are you sure?</h2>
                <p id="ikhweziDialogText">
                    Do you want to continue with this action?
                </p>
                <div class="ikhwezi-dialog-actions">
                    <button type="button" id="ikhweziDialogNo"
                            class="ikhwezi-no"
                            onclick="return ikhweziClose();">
                        No, Go Back
                    </button>
                    <button type="button" id="ikhweziDialogYes"
                            class="ikhwezi-yes"
                            onclick="return ikhweziProceed();">
                        Yes, Continue
                    </button>
                </div>
            </div>
        </div>

    </form>
</body>
</html>
