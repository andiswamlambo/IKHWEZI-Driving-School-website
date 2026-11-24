 
<%@ Page Title="Document Review" Language="C#" AutoEventWireup="true"
    CodeBehind="AdminDocuments.aspx.cs" Inherits="WebApplicationM3.AdminDocuments" %>

<!DOCTYPE html>
<html lang="en">
<head id="head1" runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title><%: Page.Title %> - IKHWEZI</title>

    <style>
        * { box-sizing: border-box; margin: 0; padding: 0; }

        body {
            font-family: 'Segoe UI', Arial, sans-serif;
            background: linear-gradient(180deg,#e9f1fc,#d9e5f5);
            min-height: 100vh;
        }

        .topbar {
            background: #0d2b6e;
            display: flex;
            align-items: center;
            justify-content: space-between;
            padding: 10px 24px;
        }

        .logo {
            text-decoration: none;
            display: flex;
            flex-direction: column;
            line-height: 1.1;
        }

        .logo-name {
            font-size: 22px;
            font-weight: 800;
            font-style: italic;
            color: #fff;
            letter-spacing: 1px;
        }

        .logo-name span { color: #e8283f; }

        .logo-sub {
            font-size: 9px;
            color: #c3d2ee;
            letter-spacing: 3px;
        }

        .topbar-right {
            display: flex;
            align-items: center;
            gap: 14px;
        }

        .role-badge {
            background: #e8283f;
            color: #fff;
            padding: 4px 12px;
            border-radius: 12px;
            font-size: 11px;
            font-weight: 800;
            letter-spacing: .5px;
            text-transform: uppercase;
        }

        .user-name {
            color: #fff;
            font-size: 14px;
            font-weight: 600;
        }

        .avatar {
            width: 34px;
            height: 34px;
            border-radius: 50%;
            background: #e8283f;
            color: #fff;
            display: flex;
            align-items: center;
            justify-content: center;
            font-weight: 700;
            font-size: 13px;
        }

        .layout {
            display: flex;
            gap: 20px;
            padding: 20px 24px;
            align-items: flex-start;
        }

        .sidebar {
            width: 235px;
            flex-shrink: 0;
            background: #fff;
            border-radius: 12px;
            padding: 14px;
            box-shadow: 0 4px 15px rgba(13,43,110,.08);
        }

        .side-item {
            display: flex;
            align-items: center;
            gap: 10px;
            padding: 10px 12px;
            margin-bottom: 6px;
            border-radius: 8px;
            color: #0d2b6e;
            font-size: 13.5px;
            font-weight: 600;
            text-decoration: none;
        }

        /* SVG icons match AdminDashboard */
        .side-item svg {
            width: 18px;
            height: 18px;
            stroke: currentColor;
            stroke-width: 2;
            fill: none;
            stroke-linecap: round;
            stroke-linejoin: round;
        }

        .side-item:hover { background: #eef4ff; }

        .side-item.selected,
        .side-item.selected:hover {
            background: #0d2b6e;
            color: #fff;
        }

        .main {
            flex: 1;
            min-width: 0;
        }

        .page-title {
            color: #0d2b6e;
            font-size: 20px;
            font-weight: 700;
            margin-bottom: 4px;
        }

        .page-sub {
            font-size: 13px;
            color: #555;
            margin-bottom: 16px;
        }

        .cards-row {
            display: grid;
            grid-template-columns: repeat(auto-fit,minmax(160px,1fr));
            gap: 14px;
            margin-bottom: 18px;
        }

        .stat-card {
            background: #fff;
            border-radius: 12px;
            padding: 18px 20px;
            box-shadow: 0 4px 15px rgba(13,43,110,.08);
            border-top: 3px solid #e8890c;
        }

        .stat-value {
            font-size: 2em;
            font-weight: 800;
            color: #e8890c;
            display: block;
        }

        .stat-label {
            font-size: 11.5px;
            color: #7787a5;
            font-weight: 700;
            letter-spacing: 1px;
            text-transform: uppercase;
        }

        .session-card {
            background: #fff;
            border-radius: 12px;
            box-shadow: 0 4px 15px rgba(13,43,110,.08);
            overflow: hidden;
        }

        .session-head {
            background: #0d2b6e;
            color: #fff;
            padding: 14px 20px;
            display: flex;
            align-items: center;
            justify-content: space-between;
            flex-wrap: wrap;
            gap: 10px;
        }

        .session-head h3 { font-size: 16px; }

        .session-body {
            padding: 18px 20px;
            overflow-x: auto;
        }

        .msg {
            display: block;
            border-radius: 8px;
            padding: 11px 15px;
            font-size: 13.5px;
            font-weight: 600;
            margin-bottom: 14px;
        }

        .msg.ok {
            background: #effaf4;
            border: 1px solid #bfe8d4;
            color: #14663f;
        }

        .msg.err {
            background: #fdf0f1;
            border: 1px solid #f3c2c9;
            color: #a3141f;
        }

        .grid {
            width: 100%;
            border-collapse: collapse;
            font-size: 13.5px;
        }

        .grid th {
            text-align: left;
            background: #f2f6fd;
            color: #0d2b6e;
            font-size: 11.5px;
            letter-spacing: .5px;
            text-transform: uppercase;
            padding: 10px 12px;
            border-bottom: 2px solid #dbe4f6;
        }

        .grid td {
            padding: 11px 12px;
            border-bottom: 1px solid #eef2fa;
            color: #333;
            vertical-align: middle;
        }

        .grid tr:hover td { background: #f8fbff; }

        .thumb {
            width: 54px;
            height: 54px;
            border-radius: 8px;
            object-fit: cover;
            border: 1px solid #dbe4f6;
        }

        .chip {
            display: inline-block;
            padding: 4px 12px;
            border-radius: 12px;
            font-size: 11.5px;
            font-weight: 700;
            background: #fff3d6;
            color: #8a6100;
        }

        .btn-act {
            border: none;
            border-radius: 14px;
            padding: 6px 14px;
            font-weight: 700;
            font-size: 11.5px;
            cursor: pointer;
            text-decoration: none;
            display: inline-block;
            margin-right: 6px;
            margin-bottom: 4px;
        }

        .btn-confirm {
            background: #22b573;
            color: #fff;
        }

        .btn-confirm:hover {
            background: #1d9e63;
            color: #fff;
            text-decoration: none;
        }

        .btn-cancel {
            background: #e8283f;
            color: #fff;
        }

        .btn-cancel:hover {
            background: #c81e33;
            color: #fff;
            text-decoration: none;
        }

        .btn-view {
            background: #0d2b6e;
            color: #fff;
        }

        .btn-view:hover {
            background: #1a4199;
            color: #fff;
            text-decoration: none;
        }

        .empty {
            text-align: center;
            color: #7787a5;
            font-size: 13.5px;
            padding: 22px 0 6px;
        }

        @media (max-width:950px) {
            .layout { flex-direction: column; }
            .sidebar { width: 100%; }
        }
    </style>
</head>

<body>
<form id="form1" runat="server">

    <!-- TOP BAR -->
    <div class="topbar">
        <a href="~/AdminDashboard" runat="server" class="logo">
            <span class="logo-name">
                IKHWEZI<span>.</span>
            </span>
            <span class="logo-sub">DRIVING SCHOOL</span>
        </a>

        <div class="topbar-right">
            <span class="role-badge">
                <asp:Literal ID="litRoleBadge" runat="server" />
            </span>

            <span class="user-name">
                <asp:Literal ID="litStaffName" runat="server" />
            </span>

            <span class="avatar">
                <asp:Literal ID="litInitials" runat="server" />
            </span>
        </div>
    </div>

    <div class="layout">

        <!-- SIDEBAR -->
        <aside class="sidebar">

            <a href="~/AdminDashboard" runat="server"
               class="side-item">
                <svg viewBox="0 0 24 24">
                    <rect x="3" y="4" width="18" height="18"
                          rx="2" ry="2"></rect>
                    <line x1="16" y1="2" x2="16" y2="6"></line>
                    <line x1="8" y1="2" x2="8" y2="6"></line>
                    <line x1="3" y1="10" x2="21" y2="10"></line>
                </svg>
                Bookings
            </a>

            <a href="~/AdminDocuments" runat="server"
               class="side-item selected">
                <svg viewBox="0 0 24 24">
                    <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
                    <polyline points="14 2 14 8 20 8"></polyline>
                    <line x1="16" y1="13" x2="8" y2="13"></line>
                    <line x1="16" y1="17" x2="8" y2="17"></line>
                    <polyline points="10 9 9 9 8 9"></polyline>
                </svg>
                Documents
            </a>

            <asp:PlaceHolder ID="phManagerOnly" runat="server">

                <a href="~/Account/StaffNotify" runat="server"
                   class="side-item">
                    <svg viewBox="0 0 24 24">
                        <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"></path>
                        <polyline points="22,6 12,13 2,6"></polyline>
                    </svg>
                    Notify Staff
                </a>

                <a href="~/InstructorDashboard" runat="server"
                   class="side-item">
                    <svg viewBox="0 0 24 24">
                        <path d="M2 3h6a4 4 0 0 1 4 4v14a3 3 0 0 0-3-3H2z"></path>
                        <path d="M22 3h-6a4 4 0 0 0-4 4v14a3 3 0 0 1 3-3h7z"></path>
                    </svg>
                    Instructor View
                </a>

            </asp:PlaceHolder>

        </aside>

        <!-- MAIN CONTENT -->
        <main class="main">

            <h2 class="page-title">Document Review</h2>

            <p class="page-sub">
                Approve or reject ID, learner's licence,
                proof of address and profile photos.
            </p>

            <!-- PENDING REVIEW CARD -->
            <div class="cards-row">
                <div class="stat-card">

                    <span class="stat-value">
                        <asp:Literal ID="lblPendingCount"
                            runat="server" />
                    </span>

                    <span class="stat-label">
                        Pending Review
                    </span>

                </div>
            </div>

            <!-- DOCUMENT TABLE -->
            <div class="session-card">

                <div class="session-head">

                    <h3>Documents Awaiting Approval</h3>

                    <div>
                        <span class="f-label"
                            style="color:#c3d2ee;margin-right:8px;">
                            Filter:
                        </span>

                        <asp:DropDownList
                            ID="ddlFilter"
                            runat="server"
                            AutoPostBack="true"
                            OnSelectedIndexChanged="ddlFilter_SelectedIndexChanged"
                            style="padding:8px 10px;border-radius:6px;border:1px solid #ccd7ec;font-size:13px;">

                            <asp:ListItem Value="pending" Selected="True">
                                Pending only
                            </asp:ListItem>

                            <asp:ListItem Value="photo">
                                Profile photos only
                            </asp:ListItem>

                            <asp:ListItem Value="all">
                                All documents (last 90 days)
                            </asp:ListItem>

                        </asp:DropDownList>
                    </div>

                </div>

                <div class="session-body">

                    <asp:Label ID="lblMsg"
                        runat="server"
                        Visible="false" />

                    <asp:Repeater
                        ID="repDocs"
                        runat="server"
                        OnItemCommand="repDocs_ItemCommand">

                        <HeaderTemplate>
                            <table class="grid">
                                <thead>
                                    <tr>
                                        <th>Preview</th>
                                        <th>Student</th>
                                        <th>Type</th>
                                        <th>File</th>
                                        <th>Size</th>
                                        <th>Uploaded</th>
                                        <th>Status</th>
                                        <th>Actions</th>
                                    </tr>
                                </thead>
                                <tbody>
                        </HeaderTemplate>

                        <ItemTemplate>
                            <tr>

                                <!-- DOCUMENT PREVIEW -->
                                <td>
                                    <asp:Image
                                        runat="server"
                                        CssClass="thumb"
                                        ImageUrl='<%# ResolveUrl((string)Eval("FilePath")) %>'
                                        Visible='<%# ((string)Eval("FilePath")).ToLower().EndsWith(".jpg") ||
                                                    ((string)Eval("FilePath")).ToLower().EndsWith(".jpeg") ||
                                                    ((string)Eval("FilePath")).ToLower().EndsWith(".png") %>' />
                                </td>

                                <!-- STUDENT -->
                                <td>
                                    <strong>
                                        <%#: Eval("StudentName") %>
                                    </strong>
                                    <br />

                                    <span style="font-size:11.5px;color:#7787a5;">
                                        <%#: Eval("StudEmail") %>
                                    </span>
                                </td>

                                <!-- DOCUMENT TYPE -->
                                <td>
                                    <b><%#: Eval("DocumentType") %></b>
                                </td>

                                <!-- FILE -->
                                <td>
                                    <a href='<%# ResolveUrl((string)Eval("FilePath")) %>'
                                       target="_blank"
                                       rel="noopener noreferrer">
                                        <%#: Eval("FileName") %>
                                    </a>
                                </td>

                                <!-- SIZE -->
                                <td>
                                    <%#: Eval("FileSizeKB") %> KB
                                </td>

                                <!-- UPLOADED DATE -->
                                <td>
                                    <%#: Eval("UploadedOnText") %>
                                </td>

                                <!-- STATUS -->
                                <td>
                                    <span class="chip">
                                        <%#: Eval("Status") %>
                                    </span>
                                </td>

                                <!-- ACTIONS -->
                                <td>

                                    <!-- APPROVE -->
                                    <asp:LinkButton
                                        runat="server"
                                        CssClass="btn-act btn-confirm"
                                        CommandName="Approve"
                                        CommandArgument='<%# Eval("DocumentID") %>'
                                        Visible='<%# Eval("Status").ToString() == "Pending" %>'
                                        OnClientClick="return confirm('Approve this document?');">
                                        Approve
                                    </asp:LinkButton>

                                    <!-- REJECT -->
                                    <asp:LinkButton
                                        runat="server"
                                        CssClass="btn-act btn-cancel"
                                        CommandName="Reject"
                                        CommandArgument='<%# Eval("DocumentID") %>'
                                        Visible='<%# Eval("Status").ToString() == "Pending" %>'
                                        OnClientClick="return confirm('Reject this document?');">
                                        Reject
                                    </asp:LinkButton>

                                    <!-- PROFILE -->
                                    <asp:LinkButton
                                        runat="server"
                                        CssClass="btn-act btn-view"
                                        CommandName="Profile"
                                        CommandArgument='<%# Eval("StudNumber") %>'>
                                        Profile
                                    </asp:LinkButton>

                                    <!-- DOWNLOAD ONE DOCUMENT -->
                                    <asp:LinkButton
                                        ID="btnDownload"
                                        runat="server"
                                        CssClass="btn-act btn-view"
                                        CommandName="Download"
                                        CommandArgument='<%# Eval("DocumentID") %>'
                                        CausesValidation="false">
                                        Download
                                    </asp:LinkButton>

                                    <!-- DOWNLOAD ALL STUDENT DOCUMENTS -->
                                    <asp:LinkButton
                                        ID="btnDownloadAll"
                                        runat="server"
                                        CssClass="btn-act btn-view"
                                        CommandName="DownloadAll"
                                        CommandArgument='<%# Eval("StudNumber") %>'
                                        CausesValidation="false">
                                        Download All
                                    </asp:LinkButton>

                                </td>
                            </tr>
                        </ItemTemplate>

                        <FooterTemplate>
                                </tbody>
                            </table>
                        </FooterTemplate>

                    </asp:Repeater>

                    <asp:Literal
                        ID="litEmpty"
                        runat="server"
                        Visible="false"
                        Text="<div class='empty'>No documents match this filter.</div>" />

                </div>
            </div>

        </main>
    </div>

</form>
</body>
</html>
