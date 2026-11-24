 <%@ Page Title="About Us" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="WebApplicationM3.About" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        /* ================= ABOUT HERO (brand navy) ================= */
        .about-hero {
            background: linear-gradient(135deg, #0d2b6e 0%, #163c8f 100%);
            color: white;
            padding: 60px 20px;
            text-align: center;
            border-radius: 0 0 20px 20px;
            margin-top: -20px;
            margin-bottom: 50px;
            border-bottom: 4px solid #e8283f;
        }
        .about-hero .kicker {
            display: inline-block;
            color: #ff9aa8;
            font-size: 13px;
            font-weight: 700;
            letter-spacing: 3px;
            text-transform: uppercase;
            margin-bottom: 12px;
        }
        .about-hero h1 {
            font-size: 2.8em;
            font-weight: 800;
            margin-bottom: 15px;
        }
        .about-hero h1 .accent { color: #ff5a6e; font-style: italic; }
        .about-hero p {
            font-size: 1.2em;
            color: #dbe6f7;
            font-weight: 300;
            max-width: 800px;
            margin: 0 auto;
        }

        /* ================= STATS ROW ================= */
        .stats-row {
            display: flex;
            justify-content: center;
            gap: 26px;
            flex-wrap: wrap;
            margin-bottom: 55px;
        }
        .stat-card {
            background: white;
            width: 220px;
            padding: 26px 15px;
            border-radius: 12px;
            text-align: center;
            box-shadow: 0 6px 22px rgba(13,43,110,.08);
            border-top: 3px solid #0d2b6e;
        }
        .stat-card.hot { border-top-color: #e8283f; }
        .stat-number {
            font-size: 2.3em;
            font-weight: 800;
            color: #0d2b6e;
            display: block;
        }
        .stat-number .star { color: #ffb400; }
        .stat-label {
            font-size: .95em;
            color: #7787a5;
            font-weight: 600;
            letter-spacing: .5px;
            text-transform: uppercase;
        }

        /* ================= CONTENT SECTIONS ================= */
        .content-section { margin-bottom: 55px; }
        .section-title {
            color: #0d2b6e;
            font-size: 2em;
            font-weight: 800;
            margin-bottom: 30px;
            text-align: center;
            position: relative;
            padding-bottom: 10px;
        }
        .section-title::after {
            content: '';
            display: block;
            width: 64px;
            height: 4px;
            background-color: #e8283f;
            margin: 12px auto 0;
            border-radius: 2px;
        }

        .intro-text {
            font-size: 1.12em;
            color: #4a5568;
            line-height: 1.8;
            max-width: 900px;
            margin: 0 auto;
            text-align: center;
        }
        .intro-text strong { color: #0d2b6e; }

        /* ================= WHY CHOOSE US ================= */
        .features-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 22px;
        }
        .feature-card {
            background: white;
            padding: 30px 26px;
            border-radius: 12px;
            box-shadow: 0 6px 22px rgba(13,43,110,.08);
            border-top: 3px solid #0d2b6e;
            transition: transform .28s ease, box-shadow .28s ease, border-color .28s ease;
        }
        .feature-card:hover {
            transform: translateY(-5px);
            box-shadow: 0 16px 36px rgba(13,43,110,.15);
            border-top-color: #e8283f;
        }
        .feature-card h3 {
            color: #0d2b6e;
            font-size: 1.15em;
            font-weight: 700;
            margin-bottom: 12px;
        }
        .feature-card p {
            color: #7787a5;
            font-size: .92em;
            line-height: 1.65;
            margin: 0;
        }

        /* ================= SERVICES (clean numbered, matches Home cards) ================= */
        .services-box {
            background: linear-gradient(180deg, #f2f6fd, #e8effb);
            padding: 40px 34px;
            border-radius: 16px;
            max-width: 940px;
            margin: 0 auto;
        }
        .service-item {
            background: #fff;
            border-radius: 12px;
            padding: 22px 26px;
            margin-bottom: 16px;
            border-left: 3px solid #0d2b6e;
            display: flex;
            align-items: flex-start;
            gap: 20px;
            box-shadow: 0 4px 14px rgba(13,43,110,.06);
            transition: border-color .25s ease;
        }
        .service-item:last-child { margin-bottom: 0; }
        .service-item:hover { border-left-color: #e8283f; }
        .service-num {
            font-size: 13px;
            font-weight: 800;
            letter-spacing: 3px;
            color: #e8283f;
            min-width: 34px;
            padding-top: 4px;
        }
        .service-text h4 {
            color: #0d2b6e;
            font-size: 1.08em;
            font-weight: 700;
            margin: 0 0 5px;
        }
        .service-text p {
            color: #7787a5;
            font-size: .92em;
            margin: 0;
            line-height: 1.55;
        }

        /* ================= CALL TO ACTION ================= */
        .about-cta {
            text-align: center;
            margin-top: 60px;
            margin-bottom: 40px;
        }
        .about-cta h2 { color: #0d2b6e; font-weight: 800; margin-bottom: 22px; }
        .btn-cta {
            background: #e8283f;
            color: #fff;
            padding: 15px 44px;
            font-size: 1.1em;
            font-weight: 700;
            border-radius: 30px;
            text-decoration: none;
            display: inline-block;
            box-shadow: 0 8px 24px rgba(232,40,63,.35);
            transition: all .25s ease;
        }
        .btn-cta:hover {
            background: #c81e33;
            color: #fff;
            text-decoration: none;
            transform: translateY(-2px);
        }
        .btn-cta-ghost {
            display: inline-block;
            margin-left: 12px;
            background: transparent;
            color: #0d2b6e;
            padding: 15px 40px;
            font-size: 1.1em;
            font-weight: 700;
            border-radius: 30px;
            border: 2px solid #0d2b6e;
            text-decoration: none;
            transition: all .25s ease;
        }
        .btn-cta-ghost:hover {
            background: #0d2b6e;
            color: #fff;
            text-decoration: none;
        }

        @media (max-width: 768px) {
            .about-hero h1 { font-size: 2em; }
            .btn-cta-ghost { margin-left: 0; margin-top: 12px; }
        }
    </style>

    <!-- Hero Section -->
    <div class="about-hero">
        <span class="kicker">Est. 2018 &middot; Durban</span>
        <h1>About <span class="accent">Khwezi</span> Driving School</h1>
        <p>Your trusted partner on the road to driving independence. Established in February 2018 and proudly based at UKZN Howard College Campus.</p>
    </div>

    <div class="container">

        <!-- Stats Section -->
        <div class="stats-row">
            <div class="stat-card hot">
                <span class="stat-number"><span class="star">&#9733;</span> 5.0</span>
                <span class="stat-label">Customer Rating</span>
            </div>
            <div class="stat-card">
                <span class="stat-number">90%</span>
                <span class="stat-label">First-Time Pass Rate</span>
            </div>
            <div class="stat-card">
                <span class="stat-number">2018</span>
                <span class="stat-label">Established</span>
            </div>
        </div>

        <!-- Introduction Text -->
        <div class="content-section">
            <p class="intro-text">
                Welcome to Khwezi Driving School, your trusted partner on the road to driving independence. Conveniently based at the University of KwaZulu-Natal (UKZN) Howard College Campus in Durban, we have proudly helped hundreds of students transform from nervous beginners into safe, confident, and licensed drivers.
                <br /><br />
                Led by director <strong>Mandlenkosi Patrick Kumalo</strong>, our school is built on the pillars of patience, professionalism, and affordability. We maintain a perfect 5.0-star customer satisfaction rating, reflecting our absolute commitment to your success and safety behind the wheel.
            </p>
        </div>

        <!-- Why Choose Us -->
        <div class="content-section">
            <h2 class="section-title">Why Choose Khwezi Driving School?</h2>
            <div class="features-grid">
                <div class="feature-card">
                    <h3>Proven Track Record</h3>
                    <p>We boast an incredible 90% pass rate for our learners' licence preparation programme, ensuring you are fully prepared to pass the first time.</p>
                </div>
                <div class="feature-card">
                    <h3>Flexible Packages</h3>
                    <p>From 10-lesson starter packs to full courses across Code 08, Code 10 and Automatic — training tailored to your pace and budget.</p>
                </div>
                <div class="feature-card">
                    <h3>Ultimate Convenience</h3>
                    <p>To make your learning experience completely stress-free, we provide individual pick-up and drop-off services for all your lessons.</p>
                </div>
                <div class="feature-card">
                    <h3>Inclusive &amp; Welcoming</h3>
                    <p>We take immense pride in maintaining a safe, respectful, and LGBTQ+/transgender-friendly learning environment for all students.</p>
                </div>
            </div>
        </div>

        <!-- Services Section -->
        <div class="content-section">
            <h2 class="section-title">Our Comprehensive Services</h2>
            <div class="services-box">
                <div class="service-item">
                    <div class="service-num">01</div>
                    <div class="service-text">
                        <h4>Learner's Licence Preparation</h4>
                        <p>A structured K53 theory class with complete assistance on test booking, studying, and writing preparation to pass the first time.</p>
                    </div>
                </div>
                <div class="service-item">
                    <div class="service-num">02</div>
                    <div class="service-text">
                        <h4>Code 08 Training</h4>
                        <p>Professional manual lessons plus specialised automatic vehicle training (no clutch, no stalling) for light motor vehicles.</p>
                    </div>
                </div>
                <div class="service-item">
                    <div class="service-num">03</div>
                    <div class="service-text">
                        <h4>Code 10 Training</h4>
                        <p>Medium truck driving lessons for those expanding their personal or professional licensing capabilities.</p>
                    </div>
                </div>
                <div class="service-item">
                    <div class="service-num">04</div>
                    <div class="service-text">
                        <h4>Vehicle Hire for Tests</h4>
                        <p>Use our maintained vehicles for your test — car hire available on Code 08 and Automatic packages, truck hire on Code 10.</p>
                    </div>
                </div>
                <div class="service-item">
                    <div class="service-num">05</div>
                    <div class="service-text">
                        <h4>Flexible Scheduling</h4>
                        <p>Focused lesson blocks designed to fit perfectly around busy university or work schedules, with 2-month payment plans.</p>
                    </div>
                </div>
            </div>
        </div>

        <!-- Call to Action -->
        <div class="about-cta">
            <h2>Ready to start your journey?</h2>
            <a runat="server" href="~/Home.aspx#findus" class="btn-cta">Contact Us Today</a>
            <a runat="server" href="~/BookSession" class="btn-cta-ghost">Book a Session</a>
        </div>

    </div>

</asp:Content>