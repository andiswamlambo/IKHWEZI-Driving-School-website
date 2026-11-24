 <%@ Page Title="Offers" Language="C#" AutoEventWireup="true" CodeBehind="Offers.aspx.cs" Inherits="WebApplicationM3.Offers" %>

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
        .avatar { width: 34px; height: 34px; border-radius: 50%; background: #e8283f; color: #fff; display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: 13px; }
        
        .layout { display: flex; gap: 20px; padding: 20px 24px; align-items: flex-start; }
        .sidebar { width: 235px; flex-shrink: 0; background: #fff; border-radius: 12px; padding: 14px; box-shadow: 0 4px 15px rgba(13,43,110,.08); }
        
        /* Side Items with SVG */
        .side-item { display: flex; align-items: center; gap: 10px; padding: 10px 12px; margin-bottom: 6px; border-radius: 8px; color: #0d2b6e; font-size: 13.5px; font-weight: 600; text-decoration: none; transition: 0.2s; }
        .side-item svg { width: 18px; height: 18px; stroke: currentColor; stroke-width: 2; fill: none; stroke-linecap: round; stroke-linejoin: round; }
        .side-item:hover { background: #eef4ff; }
        .side-item.selected, .side-item.selected:hover { background: #0d2b6e; color: #fff; }
        
        .subscription { margin-top: 18px; background: #0d2b6e; color: #fff; border-radius: 10px; padding: 16px; text-align: center; }
        .sub-title { font-weight: 700; font-size: 15px; margin-bottom: 6px; }
        .subscription p { font-size: 11.5px; color: #c3d2ee; }
        .btn-upgrade { width: 100%; margin-top: 10px; padding: 9px; background: #e8283f; color: #fff; border: none; border-radius: 16px; font-weight: 700; font-size: 13px; cursor: pointer; }
        .btn-upgrade:hover { background: #c81e33; }
        
        .main { flex: 1; background: #fff; border-radius: 12px; padding: 22px; box-shadow: 0 4px 15px rgba(13,43,110,.08); }
        .page-header { display: flex; align-items: center; gap: 12px; margin-bottom: 18px; }
        .back-btn { width: 30px; height: 30px; border-radius: 50%; background: #f1f4fb; color: #0d2b6e; display: flex; align-items: center; justify-content: center; text-decoration: none; font-weight: 700; font-size: 16px; }
        .back-btn:hover { background: #e2e8f5; color: #0d2b6e; text-decoration: none; }
        .page-title { color: #0d2b6e; font-size: 20px; font-weight: 700; }

        .guest-notice { background: #fffbe8; border: 1px solid #f0e0a0; border-radius: 8px; padding: 12px 16px; margin-bottom: 16px; font-size: 13px; color: #7a5c00; }
        .guest-notice a { color: #0d2b6e; font-weight: 700; }

        .tagline { font-size: 13px; color: #555; margin-bottom: 16px; }
        .offers-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 16px; }
        .offer-card { background: #fff; border: 1px solid #e3eaf6; border-radius: 10px; padding: 18px; display: flex; flex-direction: column; }
        .offer-head { display: flex; justify-content: space-between; align-items: flex-start; gap: 10px; }
        .offer-title { color: #0d2b6e; font-size: 15.5px; font-weight: 700; }
        .offer-price { color: #22b573; font-weight: 800; font-size: 16px; white-space: nowrap; }
        .offer-desc { font-size: 12.5px; color: #555; margin: 8px 0 14px; flex: 1; }
        .offer-foot { display: flex; justify-content: space-between; align-items: center; }
        .offer-extra { font-size: 11.5px; color: #888; }
        .btn-book-offer { background: #e8283f; color: #fff; padding: 7px 18px; border-radius: 16px; text-decoration: none; font-weight: 700; font-size: 12.5px; }
        .btn-book-offer:hover { background: #c81e33; color: #fff; text-decoration: none; }

        @media (max-width: 950px) { .layout { flex-direction: column; } .sidebar { width: 100%; } }
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
                <asp:PlaceHolder ID="phUserChip" runat="server" Visible="false">
                    <!-- Notification Bell Icon -->
                    <a href="#" class="top-icon" title="Notifications">
                        <svg viewBox="0 0 24 24">
                            <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"></path>
                            <path d="M13.73 21a2 2 0 0 1-3.46 0"></path>
                        </svg>
                    </a>
                    <span class="user-name"><asp:Literal ID="litStudentName" runat="server" /></span>
                    <span class="avatar"><asp:Literal ID="litInitials" runat="server" /></span>
                </asp:PlaceHolder>
            </div>
        </div>

        <div class="layout">
            <asp:PlaceHolder ID="phStudentNav" runat="server" Visible="false">
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
                    <a href="~/Offers" runat="server" class="side-item selected">
                        <svg viewBox="0 0 24 24"><polyline points="20 12 20 22 4 22 4 12"></polyline><rect x="2" y="7" width="20" height="5"></rect><line x1="12" y1="22" x2="12" y2="7"></line><path d="M12 7H7.5a2.5 2.5 0 0 1 0-5C11 2 12 7 12 7z"></path><path d="M12 7h4.5a2.5 2.5 0 0 0 0-5C13 2 12 7 12 7z"></path></svg>
                        Offers
                    </a>
                    
                    <!-- Book Icon -->
                    <a href="#" class="side-item">
                        <svg viewBox="0 0 24 24"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20"></path><path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z"></path></svg>
                        Theory (K53)
                    </a>
                    
                    <!-- Document Icon -->
                    <a href="#" class="side-item">
                        <svg viewBox="0 0 24 24"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path><polyline points="14 2 14 8 20 8"></polyline><line x1="16" y1="13" x2="8" y2="13"></line><line x1="16" y1="17" x2="8" y2="17"></line><polyline points="10 9 9 9 8 9"></polyline></svg>
                        My Documents
                    </a>
                    
                    <!-- Headphones Icon -->
                    <a href="#" class="side-item">
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
                            OnClientClick="alert('Premium coming soon!'); return false;" />
                    </div>
                </aside>
            </asp:PlaceHolder>

            <main class="main">
                <asp:PlaceHolder ID="phGuestNotice" runat="server" Visible="false">
                    <div class="guest-notice">
                        You're browsing as a guest.
                        <a runat="server" href="~/Account/Login?return=Offers">Log in</a> or
                        <a runat="server" href="~/Account/SignUp">sign up</a>
                        to book any of these packages.
                    </div>
                </asp:PlaceHolder>

                <div class="page-header">
                    <a href="javascript:history.back()" class="back-btn">&lsaquo;</a>
                    <h2 class="page-title">Offers &amp; Packages</h2>
                </div>

                <p class="tagline">Choose a package that suits your journey. Flexible 50% deposits and monthly payment plans available — prices in Rand (ZAR).</p>

                <div class="offers-grid">
                    <asp:Repeater ID="repOffers" runat="server">
                        <ItemTemplate>
                            <div class="offer-card">
                                <div class="offer-head">
                                    <h3 class="offer-title"><%# Eval("Title") %></h3>
                                    <span class="offer-price"><%# Eval("PriceText") %></span>
                                </div>
                                <p class="offer-desc"><%# Eval("Description") %></p>
                                <div class="offer-foot">
                                    <span class="offer-extra"><%# Eval("Extra") %></span>
                                    <a runat="server" href="~/BookSession" class="btn-book-offer">Book Now</a>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </main>
        </div>

    </form>
</body>
</html>