 <%@ Page Title="My Progress" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MyProgress.aspx.cs" Inherits="WebApplicationM3.MyProgress" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .progress-wrap { max-width: 900px; margin: 40px auto; padding: 0 20px; }

        /* ===== HERO ===== */
        .progress-hero {
            background: linear-gradient(135deg, #0a1e4a, #1a4199);
            border-radius: 20px;
            padding: 50px 44px;
            color: #fff;
            margin-bottom: 40px;
            position: relative;
            overflow: hidden;
        }
        .progress-hero::before {
            content: '';
            position: absolute;
            top: -50%; right: -20%;
            width: 400px; height: 400px;
            background: radial-gradient(circle, rgba(232,40,63,.25), transparent 70%);
            pointer-events: none;
        }
        .progress-kicker {
            color: #ffb3bd;
            font-size: 12px;
            font-weight: 800;
            letter-spacing: 4px;
            text-transform: uppercase;
            margin-bottom: 14px;
        }
        .progress-hero h1 {
            font-size: 2.2em;
            font-weight: 800;
            line-height: 1.15;
            margin-bottom: 16px;
            letter-spacing: -.5px;
        }
        .progress-hero h1 .accent { color: #e8283f; font-style: italic; }
        .progress-hero p {
            font-size: 1.05em;
            color: #cdd9ef;
            line-height: 1.7;
            max-width: 640px;
        }

        /* ===== OVERALL STATS ROW ===== */
        .stats-row {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
            gap: 16px;
            margin-bottom: 40px;
        }
        .stat-box {
            background: #fff;
            border-radius: 14px;
            padding: 24px 20px;
            text-align: center;
            box-shadow: 0 6px 22px rgba(13,43,110,.07);
            border-top: 4px solid #0d2b6e;
        }
        .stat-box.green { border-top-color: #22b573; }
        .stat-box.orange { border-top-color: #e8890c; }
        .stat-box.red { border-top-color: #e8283f; }
        .stat-box .value {
            font-size: 2.2em;
            font-weight: 800;
            color: #0a1e4a;
            line-height: 1;
            letter-spacing: -1px;
        }
        .stat-box.green .value { color: #22b573; }
        .stat-box.orange .value { color: #e8890c; }
        .stat-box.red .value { color: #e8283f; }
        .stat-box .label {
            font-size: 11px;
            font-weight: 800;
            letter-spacing: 1.5px;
            text-transform: uppercase;
            color: #7787a5;
            margin-top: 8px;
        }

        /* ===== MILESTONES LIST ===== */
        .milestones-section {
            background: #fff;
            border-radius: 16px;
            padding: 36px;
            box-shadow: 0 8px 26px rgba(13,43,110,.08);
        }
        .milestones-title {
            color: #0a1e4a;
            font-size: 1.4em;
            font-weight: 800;
            margin-bottom: 8px;
            letter-spacing: -.3px;
        }
        .milestones-sub {
            color: #667;
            font-size: 14px;
            margin-bottom: 30px;
            line-height: 1.6;
        }

        .milestone {
            display: flex;
            align-items: flex-start;
            gap: 20px;
            padding: 22px 0;
            border-bottom: 1px solid #eef2fa;
            opacity: 0.55;
            transition: opacity .3s ease;
        }
        .milestone:last-child { border-bottom: none; }
        .milestone.achieved { opacity: 1; }

        .milestone-icon {
            width: 52px;
            height: 52px;
            border-radius: 50%;
            background: #eef4ff;
            color: #b5bfd0;
            display: flex;
            align-items: center;
            justify-content: center;
            flex-shrink: 0;
            transition: background .3s ease, color .3s ease, transform .3s ease;
        }
        /* SVG Icon Styles */
        .milestone-icon svg {
            width: 24px;
            height: 24px;
            stroke: currentColor;
            stroke-width: 2;
            fill: none;
            stroke-linecap: round;
            stroke-linejoin: round;
        }
        .milestone.achieved .milestone-icon {
            background: #dcf5e8;
            color: #22b573;
        }
        .milestone.achieved:hover .milestone-icon {
            transform: scale(1.1);
        }

        .milestone-content h4 {
            color: #0a1e4a;
            font-size: 1.05em;
            font-weight: 800;
            margin-bottom: 6px;
            letter-spacing: -.2px;
        }
        .milestone-content p {
            color: #667;
            font-size: 14px;
            line-height: 1.6;
            margin: 0;
        }

        .milestone-status {
            margin-left: auto;
            align-self: center;
            font-size: 12px;
            font-weight: 800;
            letter-spacing: 1px;
            text-transform: uppercase;
            color: #b5bfd0;
            white-space: nowrap;
        }
        .milestone.achieved .milestone-status { color: #22b573; }

        @media (max-width: 700px) {
            .progress-wrap { padding: 0 14px; margin: 20px auto; }
            .progress-hero { padding: 34px 22px; }
            .progress-hero h1 { font-size: 1.7em; }
            .milestones-section { padding: 24px 22px; }
            .milestone-status { display: none; } /* Hide the text label on small screens */
            .milestone { gap: 14px; }
            .milestone-icon { width: 44px; height: 44px; }
            .milestone-icon svg { width: 20px; height: 20px; }
        }
    </style>

    <div class="progress-wrap">

        <!-- HERO -->
        <div class="progress-hero">
            <div class="progress-kicker">Your Journey</div>
            <h1>Tracking Your <span class="accent">Progress</span></h1>
            <p>
                Every lesson brings you closer to your licence. Keep an eye on your
                milestones and see how far you've come.
            </p>
        </div>

        <!-- STATS -->
        <div class="stats-row">
            <div class="stat-box">
                <div class="value"><asp:Literal ID="litTotalBooked" runat="server" /></div>
                <div class="label">Lessons Booked</div>
            </div>
            <div class="stat-box green">
                <div class="value"><asp:Literal ID="litCompleted" runat="server" /></div>
                <div class="label">Lessons Completed</div>
            </div>
            <div class="stat-box orange">
                <div class="value"><asp:Literal ID="litUpcoming" runat="server" /></div>
                <div class="label">Upcoming Lessons</div>
            </div>
            <div class="stat-box red">
                <div class="value"><asp:Literal ID="litHours" runat="server" /></div>
                <div class="label">Hours Driven</div>
            </div>
        </div>

        <!-- MILESTONES -->
        <div class="milestones-section">
            <h2 class="milestones-title">Achievements &amp; Milestones</h2>
            <p class="milestones-sub">Hit these milestones to unlock your progress. Locked milestones will turn green once achieved.</p>

            <div class="milestone" id="ms1" runat="server">
                <div class="milestone-icon">
                    <svg viewBox="0 0 24 24"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path><circle cx="12" cy="7" r="4"></circle></svg>
                </div>
                <div class="milestone-content">
                    <h4>Account Created</h4>
                    <p>Welcome to IKHWEZI! Your journey starts here.</p>
                </div>
                <div class="milestone-status">Unlocked</div>
            </div>

            <div class="milestone" id="ms2" runat="server">
                <div class="milestone-icon">
                    <svg viewBox="0 0 24 24"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path><polyline points="14 2 14 8 20 8"></polyline><line x1="16" y1="13" x2="8" y2="13"></line><line x1="16" y1="17" x2="8" y2="17"></line><polyline points="10 9 9 9 8 9"></polyline></svg>
                </div>
                <div class="milestone-content">
                    <h4>ID Uploaded</h4>
                    <p>You uploaded your ID to your documents folder.</p>
                </div>
                <div class="milestone-status" id="ms2Status" runat="server">Locked</div>
            </div>

            <div class="milestone" id="ms3" runat="server">
                <div class="milestone-icon">
                    <svg viewBox="0 0 24 24"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect><line x1="16" y1="2" x2="16" y2="6"></line><line x1="8" y1="2" x2="8" y2="6"></line><line x1="3" y1="10" x2="21" y2="10"></line></svg>
                </div>
                <div class="milestone-content">
                    <h4>First Lesson Booked</h4>
                    <p>You took the first step and booked a lesson!</p>
                </div>
                <div class="milestone-status" id="ms3Status" runat="server">Locked</div>
            </div>

            <div class="milestone" id="ms4" runat="server">
                <div class="milestone-icon">
                    <svg viewBox="0 0 24 24"><path d="M6 9H4.5a2.5 2.5 0 0 1 0-5H6"></path><path d="M18 9h1.5a2.5 2.5 0 0 0 0-5H18"></path><path d="M4 22h16"></path><path d="M10 14.66V17c0 .55-.47.98-.97 1.21C7.85 18.75 7 20.24 7 22"></path><path d="M14 14.66V17c0 .55.47.98.97 1.21C16.15 18.75 17 20.24 17 22"></path><path d="M18 2H6v7a6 6 0 0 0 12 0V2Z"></path></svg>
                </div>
                <div class="milestone-content">
                    <h4>First Lesson Completed</h4>
                    <p>You successfully completed your first driving lesson.</p>
                </div>
                <div class="milestone-status" id="ms4Status" runat="server">Locked</div>
            </div>

            <div class="milestone" id="ms5" runat="server">
                <div class="milestone-icon">
                    <svg viewBox="0 0 24 24"><path d="M8.5 14.5A2.5 2.5 0 0 0 11 12c0-1.38-.5-2-1-3-1.072-2.143-.224-4.054 2-6 .5 2.5 2 4.9 4 6.5 2 1.6 3 3.5 3 5.5a7 7 0 1 1-14 0c0-1.153.433-2.294 1-3a2.5 2.5 0 0 0 2.5 2.5z"></path></svg>
                </div>
                <div class="milestone-content">
                    <h4>5 Lessons Completed</h4>
                    <p>You're building momentum. 5 lessons down!</p>
                </div>
                <div class="milestone-status" id="ms5Status" runat="server">Locked</div>
            </div>

            <div class="milestone" id="ms6" runat="server">
                <div class="milestone-icon">
                    <svg viewBox="0 0 24 24"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"></path></svg>
                </div>
                <div class="milestone-content">
                    <h4>Test Ready (18+ Lessons)</h4>
                    <p>You've completed enough lessons to be ready for the driver's test.</p>
                </div>
                <div class="milestone-status" id="ms6Status" runat="server">Locked</div>
            </div>

            <div class="milestone" id="ms7" runat="server">
                <div class="milestone-icon">
                    <svg viewBox="0 0 24 24"><polyline points="20 12 20 22 4 22 4 12"></polyline><rect x="2" y="7" width="20" height="5"></rect><line x1="12" y1="22" x2="12" y2="7"></line><path d="M12 7H7.5a2.5 2.5 0 0 1 0-5C11 2 12 7 12 7z"></path><path d="M12 7h4.5a2.5 2.5 0 0 0 0-5C13 2 12 7 12 7z"></path></svg>
                </div>
                <div class="milestone-content">
                    <h4>Fully Paid Package</h4>
                    <p>You've settled your package balance. Thank you!</p>
                </div>
                <div class="milestone-status" id="ms7Status" runat="server">Locked</div>
            </div>
        </div>

    </div>

</asp:Content>