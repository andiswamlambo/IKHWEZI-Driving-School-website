 <%@ Page Title="Student Directory" Language="C#" AutoEventWireup="true" CodeBehind="AdminStudents.aspx.cs" Inherits="WebApplicationM3.AdminStudents" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Student Directory - IKHWEZI</title>
    <link href="https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700;800&display=swap" rel="stylesheet">
    <style>
        :root { --navy: #0d2b6e; --bg: #f0f4f8; --border: #e2e8f0; }
        * { box-sizing: border-box; margin: 0; padding: 0; }
        body { font-family: 'Plus Jakarta Sans', sans-serif; background: var(--bg); min-height: 100vh; }
        
        /* Topbar */
        .topbar { background: var(--navy); display: flex; align-items: center; justify-content: space-between; padding: 12px 24px; color: #fff; }
        .logo { text-decoration: none; display: flex; flex-direction: column; line-height: 1.1; color: #fff; }
        .logo-name { font-size: 22px; font-weight: 800; font-style: italic; letter-spacing: 1px; }
        .logo-name span { color: #e8283f; }
        .logo-sub { font-size: 9px; color: #c3d2ee; letter-spacing: 3px; }

        /* Layout */
        .layout { display: flex; gap: 24px; padding: 24px; max-width: 1400px; margin: 0 auto; }
        .sidebar { width: 240px; background: #fff; border-radius: 12px; padding: 16px; box-shadow: 0 4px 15px rgba(13,43,110,.05); height: fit-content; }
        .side-item { display: flex; align-items: center; gap: 12px; padding: 12px 16px; margin-bottom: 6px; border-radius: 8px; color: var(--navy); font-size: 14px; font-weight: 600; text-decoration: none; }
        .side-item:hover { background: #f8fafc; text-decoration: none; }
        .side-item.active { background: var(--navy); color: #fff; }
        .side-item svg { width: 18px; height: 18px; stroke: currentColor; stroke-width: 2; fill: none; }

        /* Main */
        .main { flex: 1; background: #fff; border-radius: 12px; padding: 24px; box-shadow: 0 4px 15px rgba(13,43,110,.05); }
        .page-title { color: var(--navy); font-size: 22px; font-weight: 800; margin-bottom: 6px; }
        .page-sub { color: #64748b; font-size: 14px; margin-bottom: 24px; }

        /* Search */
        .search-row { display: flex; gap: 12px; margin-bottom: 24px; }
        .search-box { flex: 1; padding: 12px 16px; border: 1px solid var(--border); border-radius: 8px; font-size: 14px; outline: none; }
        .search-box:focus { border-color: var(--navy); }
        .btn-search { background: var(--navy); color: #fff; border: none; padding: 0 24px; border-radius: 8px; font-weight: 700; cursor: pointer; }

        /* Table */
        .grid { width: 100%; border-collapse: collapse; font-size: 14px; }
        .grid th { text-align: left; background: #f8fafc; color: #64748b; font-size: 11.5px; text-transform: uppercase; padding: 14px 16px; border-bottom: 1px solid var(--border); font-weight: 700; }
        .grid td { padding: 14px 16px; border-bottom: 1px solid var(--border); color: #1e293b; }
        .grid tr:hover td { background: #f8fafc; }
        
        .btn-view { background: #eff6ff; color: #1d4ed8; border: 1px solid #bfdbfe; padding: 6px 12px; border-radius: 6px; font-weight: 600; text-decoration: none; font-size: 12.5px; display: inline-block; }
        .btn-view:hover { background: #dbeafe; text-decoration: none; }
        
        .empty { padding: 30px; text-align: center; color: #64748b; font-weight: 500; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="topbar">
            <a href="AdminDashboard.aspx" class="logo">
                <span class="logo-name">IKHWEZI<span>.</span></span>
                <span class="logo-sub">DRIVING SCHOOL</span>
            </a>
        </div>

        <div class="layout">
            <aside class="sidebar">
                <a href="AdminDashboard.aspx" class="side-item">
                    <svg viewBox="0 0 24 24"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect><line x1="16" y1="2" x2="16" y2="6"></line><line x1="8" y1="2" x2="8" y2="6"></line><line x1="3" y1="10" x2="21" y2="10"></line></svg>
                    Bookings
                </a>
                <a href="AdminStudents.aspx" class="side-item active">
                    <svg viewBox="0 0 24 24"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path><circle cx="9" cy="7" r="4"></circle></svg>
                    Students
                </a>
                <a href="AdminDocuments.aspx" class="side-item">
                    <svg viewBox="0 0 24 24"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path><polyline points="14 2 14 8 20 8"></polyline></svg>
                    Documents
                </a>
            </aside>

            <main class="main">
                <h2 class="page-title">Student Directory</h2>
                <p class="page-sub">Search and view student contact details and package balances.</p>

                <div class="search-row">
                    <asp:TextBox ID="txtSearch" runat="server" CssClass="search-box" placeholder="Search by name, email, or student number..." />
                    <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn-search" OnClick="btnSearch_Click" />
                </div>

                <asp:Repeater ID="repStudents" runat="server">
                    <HeaderTemplate>
                        <table class="grid">
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
                            <td><b>#<%# Eval("StudNumber") %></b></td>
                            <td><%# Eval("FullName") %></td>
                            <td><%# Eval("StudEmailAddress") %></td>
                            <td><%# Eval("PhoneNum") %></td>
                            <td><%# Eval("Status") %></td>
                            <td>
                                <!-- SMART REDIRECT -->
                                <asp:HyperLink runat="server" CssClass="btn-view" 
                                    NavigateUrl='<%# GetProfileUrl(Eval("StudEmailAddress").ToString()) %>' 
                                    Text="View Profile" />
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                            </tbody>
                        </table>
                    </FooterTemplate>
                </asp:Repeater>

                <asp:Literal ID="litEmpty" runat="server" Visible="false" Text="<div class='empty'>No students found.</div>" />
            </main>
        </div>
    </form>
</body>
</html>