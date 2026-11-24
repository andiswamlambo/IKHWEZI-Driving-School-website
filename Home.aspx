 <%@ Page Title="Home" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="WebApplicationM3._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        html, body { overflow-x: hidden; }

        /* =========================================================
           HERO
           ========================================================= */
        .hero-section {
            position: relative;
            width: 100vw;
            margin-left: calc(-50vw + 50%);
            height: 84vh;
            min-height: 540px;
            overflow: hidden;
            background: #0a1e4a;
            display: flex;
            align-items: center;
            justify-content: center;
            text-align: center;
        }
        .hero-slide {
            position: absolute;
            top: 0; left: 0;
            width: 100%; height: 100%;
            object-fit: cover;
            opacity: 0;
            animation: heroFade 9s infinite;
        }
        .hero-slide.s1 { animation-delay: 0s; }
        .hero-slide.s2 { animation-delay: 3s; }
        .hero-slide.s3 { animation-delay: 6s; }
        @keyframes heroFade {
            0%   { opacity: 0; transform: scale(1); }
            4%   { opacity: 1; }
            30%  { opacity: 1; transform: scale(1.06); }
            36%  { opacity: 0; transform: scale(1.06); }
            100% { opacity: 0; transform: scale(1.06); }
        }
        .hero-overlay {
            position: absolute;
            top: 0; left: 0;
            width: 100%; height: 100%;
            background: linear-gradient(180deg,
                rgba(10,30,74,.55) 0%,
                rgba(10,30,74,.35) 45%,
                rgba(10,30,74,.65) 100%);
        }
        .hero-content {
            position: relative;
            z-index: 2;
            max-width: 900px;
            padding: 0 24px;
        }
        .hero-kicker {
            display: inline-block;
            color: #a9c4ee;
            font-size: 12px;
            font-weight: 700;
            letter-spacing: 4px;
            text-transform: uppercase;
            margin-bottom: 20px;
            padding-bottom: 12px;
            border-bottom: 1px solid rgba(255,255,255,.25);
            text-shadow: 0 1px 8px rgba(0,0,0,.6);
        }
        .hero-title {
            color: #fff;
            font-size: 3.2em;
            font-weight: 800;
            line-height: 1.12;
            margin-bottom: 20px;
            letter-spacing: -.5px;
            text-shadow: 0 3px 22px rgba(0,0,0,.6);
        }
        .hero-title .accent { color: #ff5566; font-style: italic; font-weight: 800; }
        .hero-tagline {
            color: #ffffff;
            font-size: 1.1em;
            font-weight: 400;
            line-height: 1.7;
            margin: 0 auto 34px;
            max-width: 640px;
            text-shadow: 0 2px 12px rgba(0,0,0,.65);
        }
        .hero-ctas {
            display: flex;
            gap: 16px;
            justify-content: center;
            flex-wrap: wrap;
        }
        .btn-hero-solid {
            background: #e8283f; color: #fff;
            padding: 15px 40px; border-radius: 32px;
            font-size: 1em; font-weight: 700; letter-spacing: .3px;
            text-decoration: none;
            box-shadow: 0 10px 30px rgba(232,40,63,.5);
            transition: transform .2s ease, box-shadow .2s ease, background .2s ease;
            border: 2px solid #e8283f;
        }
        .btn-hero-solid:hover {
            background: #c81e33; border-color: #c81e33; color: #fff;
            text-decoration: none; transform: translateY(-2px);
            box-shadow: 0 14px 36px rgba(232,40,63,.6);
        }
        .btn-hero-ghost {
            background: rgba(255,255,255,.1); color: #fff;
            padding: 15px 40px; border-radius: 32px;
            border: 2px solid rgba(255,255,255,.85);
            font-size: 1em; font-weight: 700; letter-spacing: .3px;
            text-decoration: none;
            transition: all .2s ease;
            backdrop-filter: blur(4px);
        }
        .btn-hero-ghost:hover {
            background: rgba(255,255,255,.25);
            border-color: #fff; color: #fff; text-decoration: none;
        }

        /* =========================================================
           TRUST STRIP
           ========================================================= */
        .trust-strip {
            position: relative;
            z-index: 5;
            margin-top: -30px;
            background: #fff;
            border-radius: 16px;
            box-shadow: 0 18px 44px rgba(13,43,110,.14);
            display: flex;
            flex-wrap: wrap;
            justify-content: space-around;
            padding: 26px 15px;
            border: 1px solid #eef2fa;
        }
        .trust-item {
            text-align: center;
            padding: 8px 22px;
            border-right: 1px solid #eef2fa;
        }
        .trust-item:last-child { border-right: none; }
        .trust-item .t-value {
            font-size: 1.4em;
            font-weight: 800;
            color: #0a1e4a;
            letter-spacing: -.3px;
        }
        .trust-item .t-value .stars { color: #e8b400; font-size: .9em; letter-spacing: 1px; }
        .trust-item .t-label {
            font-size: 11px;
            color: #7787a5;
            font-weight: 700;
            letter-spacing: 1.4px;
            text-transform: uppercase;
            margin-top: 4px;
        }

        /* =========================================================
           SECTION HEADERS
           ========================================================= */
        .section-pad { margin-top: 72px; }
        .section-head { text-align: center; margin-bottom: 42px; }
        .section-kicker {
            color: #e8283f;
            font-size: 12px;
            font-weight: 800;
            letter-spacing: 3px;
            text-transform: uppercase;
            margin-bottom: 10px;
        }
        .section-head h2 {
            color: #0a1e4a;
            font-weight: 800;
            font-size: 2em;
            margin-bottom: 10px;
            letter-spacing: -.3px;
        }
        .section-head .rule {
            width: 56px;
            height: 3px;
            background: #e8283f;
            border-radius: 2px;
            margin: 14px auto 0;
        }
        .section-head .section-sub {
            color: #667;
            font-size: .98em;
            max-width: 560px;
            margin: 12px auto 0;
            line-height: 1.6;
        }

        /* =========================================================
           HOW WE HELP YOU PASS
           ========================================================= */
        .pass-steps {
            display: flex;
            flex-wrap: wrap;
            justify-content: center;
            gap: 24px;
        }
        .pass-step {
            background: #fff;
            width: 300px;
            padding: 36px 28px 32px;
            border-radius: 18px;
            text-align: center;
            box-shadow: 0 8px 28px rgba(13,43,110,.09);
            border-bottom: 4px solid #0a1e4a;
            transition: transform .3s ease, box-shadow .3s ease, border-color .3s ease;
            position: relative;
        }
        .pass-step:hover {
            transform: translateY(-6px);
            box-shadow: 0 22px 48px rgba(13,43,110,.16);
            border-bottom-color: #e8283f;
        }
        .pass-step-icon {
            width: 68px;
            height: 68px;
            border-radius: 50%;
            background: linear-gradient(135deg, #0d2b6e, #1a4199);
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0 auto 20px;
            box-shadow: 0 8px 20px rgba(13,43,110,.25);
        }
        .pass-step-icon svg { width: 30px; height: 30px; }
        .pass-step-num {
            position: absolute;
            top: 16px; right: 20px;
            font-size: 11px;
            font-weight: 800;
            letter-spacing: 2px;
            color: #e8283f;
        }
        .pass-step h3 {
            font-size: 1.15em;
            color: #0a1e4a;
            font-weight: 800;
            margin-bottom: 10px;
            letter-spacing: -.2px;
        }
        .pass-step p { font-size: .9em; color: #556; line-height: 1.65; margin: 0; }

        /* =========================================================
           GALLERY
           ========================================================= */
        .gallery-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
            gap: 22px;
        }
        .gallery-card {
            background: #fff;
            border-radius: 16px;
            overflow: hidden;
            box-shadow: 0 8px 26px rgba(13,43,110,.1);
            transition: transform .3s ease, box-shadow .3s ease;
        }
        .gallery-card:hover {
            transform: translateY(-6px);
            box-shadow: 0 20px 44px rgba(13,43,110,.18);
        }
        .gallery-img-wrap {
            position: relative;
            width: 100%;
            height: 240px;
            overflow: hidden;
            background: #0d2b6e;
        }
        .gallery-img-wrap img {
            width: 100%; height: 100%;
            object-fit: cover;
            display: block;
            transition: transform .4s ease;
        }
        .gallery-card:hover .gallery-img-wrap img { transform: scale(1.06); }
        .gallery-caption { padding: 20px 22px 22px; }
        .gallery-caption h3 {
            font-size: 1.05em;
            color: #0a1e4a;
            font-weight: 800;
            margin-bottom: 8px;
            letter-spacing: -.2px;
        }
        .gallery-caption p { font-size: .88em; color: #667; line-height: 1.6; margin: 0; }

        /* =========================================================
           ACTION CARDS
           ========================================================= */
        .action-container {
            display: flex; flex-wrap: wrap; justify-content: center; gap: 22px;
        }
        .action-link {
            background: #fff;
            width: 250px;
            padding: 32px 26px 26px;
            border-radius: 14px;
            text-align: left;
            text-decoration: none;
            color: #0a1e4a;
            box-shadow: 0 6px 24px rgba(13,43,110,.07);
            border-top: 3px solid #0a1e4a;
            transition: transform .28s ease, box-shadow .28s ease, border-color .28s ease;
            display: block;
            position: relative;
        }
        .action-link:hover {
            transform: translateY(-6px);
            box-shadow: 0 20px 44px rgba(13,43,110,.16);
            border-top-color: #e8283f;
            text-decoration: none;
            color: #0a1e4a;
        }
        .action-num {
            font-size: 11px; font-weight: 800; letter-spacing: 3px;
            color: #e8283f; margin-bottom: 12px;
        }
        .action-link h3 {
            font-size: 1.05em; font-weight: 700; margin: 0 0 10px;
            color: #0a1e4a; letter-spacing: -.2px;
        }
        .action-link p { font-size: .85em; color: #7787a5; line-height: 1.6; margin: 0 0 16px; }
        .action-more {
            font-size: .82em; font-weight: 700; letter-spacing: .5px;
            color: #0a1e4a; transition: color .2s ease;
        }
        .action-link:hover .action-more { color: #e8283f; }

        /* =========================================================
           PRICING
           ========================================================= */
        .pricing-section {
            background: linear-gradient(180deg, #f5f8fd, #e7eef9);
            padding: 64px 34px; border-radius: 20px;
            text-align: center; margin-top: 72px;
        }
        .pricing-kicker {
            color: #e8283f; font-size: 12px; font-weight: 800;
            letter-spacing: 3px; text-transform: uppercase; margin-bottom: 12px;
        }
        .pricing-slogan {
            color: #0a1e4a; font-size: 2em; font-weight: 800;
            letter-spacing: -.4px; margin-bottom: 10px;
        }
        .pricing-sub {
            color: #667; font-size: 1em; margin-bottom: 40px;
            max-width: 620px; margin-left: auto; margin-right: auto;
            line-height: 1.65;
        }
        .package-row {
            display: flex; justify-content: center; gap: 24px;
            flex-wrap: wrap; margin-bottom: 36px;
        }
        .package-card {
            background: #fff; width: 290px; padding: 34px 26px;
            border-radius: 16px; border-top: 4px solid #0a1e4a;
            box-shadow: 0 8px 26px rgba(13,43,110,.08);
            transition: transform .3s ease, box-shadow .3s ease;
            display: block; text-decoration: none; color: inherit; position: relative;
        }
        .package-card:hover {
            transform: translateY(-8px);
            box-shadow: 0 22px 46px rgba(13,43,110,.16);
            text-decoration: none; color: inherit;
        }
        .package-card.hot {
            border-top-color: #e8283f;
            box-shadow: 0 12px 36px rgba(232,40,63,.14);
        }
        .package-kicker {
            display: block; min-height: 16px;
            font-size: 10.5px; font-weight: 800; letter-spacing: 2.4px;
            color: #e8283f; text-transform: uppercase; margin-bottom: 12px;
        }
        .package-title {
            font-size: 1.5em; color: #0a1e4a; font-weight: 800;
            margin-bottom: 6px; letter-spacing: -.4px;
        }
        .package-lessons { font-size: .9em; color: #7787a5; margin-bottom: 20px; }
        .package-price {
            font-size: 2em; color: #0a1e4a; font-weight: 800; letter-spacing: -.6px;
        }
        .package-price .from {
            font-size: .42em; font-weight: 600; color: #7787a5;
            vertical-align: middle; letter-spacing: 1px;
            text-transform: uppercase; margin-right: 6px;
        }
        .package-cta {
            margin-top: 18px; color: #e8283f; font-weight: 700;
            font-size: .9em; letter-spacing: .3px;
        }
        .payment-line {
            color: #0a1e4a; font-weight: 600; font-size: 1em;
            letter-spacing: .3px; padding-top: 24px;
            border-top: 1px solid #dbe4f6; max-width: 720px; margin: 0 auto;
        }
        .payment-line .dot { color: #e8283f; font-weight: 800; margin: 0 14px; }

        /* =========================================================
           FIND US  (only one contact block now)
           ========================================================= */
        .findus-section {
            scroll-margin-top: 90px; margin-top: 72px;
            background: #0a1e4a; border-radius: 20px;
            padding: 52px 40px;
            display: flex; flex-wrap: wrap; align-items: stretch;
            gap: 40px;
            box-shadow: 0 20px 50px rgba(10,30,74,.22);
        }
        .findus-map {
            flex: 1.15; min-width: 340px; min-height: 420px;
            border-radius: 14px; overflow: hidden;
            box-shadow: 0 10px 30px rgba(0,0,0,.28);
            background: #142a5c;
        }
        .findus-map iframe {
            width: 100%; height: 100%; border: 0;
            display: block; min-height: 420px;
        }
        .findus-info {
            flex: 1; min-width: 300px; padding: 16px 8px;
            display: flex; flex-direction: column; justify-content: center;
        }
        .findus-kicker {
            color: #a9c4ee; font-size: 11px; font-weight: 800;
            letter-spacing: 3.5px; text-transform: uppercase; margin-bottom: 10px;
        }
        .findus-title {
            font-size: 2em; font-weight: 800;
            margin-bottom: 30px; color: #fff;
            letter-spacing: -.4px; line-height: 1.15;
        }
        .findus-title .accent { color: #e8283f; font-style: italic; }
        .findus-item {
            margin-bottom: 18px; padding-bottom: 18px;
            border-bottom: 1px solid rgba(255,255,255,.09);
        }
        .findus-item:last-of-type { border-bottom: none; }
        .findus-label {
            font-size: 10.5px; letter-spacing: 2.5px; color: #8ba4d4;
            text-transform: uppercase; font-weight: 800; margin-bottom: 6px;
        }
        .findus-value {
            font-size: 1em; color: #eef3fc;
            line-height: 1.65; font-weight: 400;
        }
        .findus-value a {
            color: #8ec6ff; text-decoration: none; font-weight: 600;
            transition: color .2s ease;
        }
        .findus-value a:hover { color: #ffffff; text-decoration: underline; }
        .btn-directions {
            display: inline-block; margin-top: 12px;
            padding: 12px 32px; border-radius: 28px;
            text-decoration: none; font-weight: 700; font-size: .92em;
            letter-spacing: .5px; background: #e8283f;
            border: 2px solid #e8283f; color: #fff;
            align-self: flex-start; transition: all .25s ease;
        }
        .btn-directions:hover {
            background: #c81e33; border-color: #c81e33; color: #fff;
            text-decoration: none; transform: translateY(-2px);
        }

        /* =========================================================
           CONTACT FORM  (single, no side duplicates)
           ========================================================= */
        .contact-section {
            margin-top: 72px;
            background: linear-gradient(180deg, #f5f8fd, #eef4fb);
            border-radius: 20px;
            padding: 56px 40px;
            border: 1px solid #e3ecf7;
        }
        .contact-grid {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 40px;
            max-width: 1000px;
            margin: 0 auto;
        }
        .contact-form-card {
            background: #ffffff;
            border-radius: 16px;
            padding: 32px 30px 28px;
            box-shadow: 0 12px 34px rgba(13,43,110,.09);
            border-top: 4px solid #0d2b6e;
        }
        .form-row { margin-bottom: 18px; }
        .form-label {
            display: block;
            font-size: 12px; font-weight: 700; letter-spacing: .8px;
            color: #0a1e4a; text-transform: uppercase; margin-bottom: 8px;
        }
        .form-input {
            width: 100%; padding: 12px 15px;
            border: 1.5px solid #d5e0f2; border-radius: 10px;
            font-size: 14px; color: #0a1e4a;
            background: #f9fbff; outline: none;
            font-family: inherit;
            transition: border-color .2s ease, background .2s ease, box-shadow .2s ease;
        }
        .form-input:focus {
            border-color: #0d2b6e; background: #ffffff;
            box-shadow: 0 0 0 4px rgba(13,43,110,.08);
        }
        .form-input::placeholder { color: #94a3bd; }
        .form-textarea {
            resize: vertical; min-height: 130px; line-height: 1.55;
        }
        .form-submit {
            display: block; width: 100%;
            padding: 14px 20px;
            background: #e8283f; color: #ffffff;
            border: none; border-radius: 12px;
            font-weight: 800; font-size: 14.5px;
            letter-spacing: .5px; cursor: pointer;
            margin-top: 6px;
            box-shadow: 0 10px 24px rgba(232,40,63,.28);
            transition: background .2s ease, transform .2s ease, box-shadow .2s ease;
        }
        .form-submit:hover {
            background: #c81e33; transform: translateY(-1px);
            box-shadow: 0 14px 30px rgba(232,40,63,.38);
        }
        .form-submit:active { transform: translateY(0); }
        .form-message {
            display: block;
            padding: 12px 15px; border-radius: 10px;
            font-size: 13.5px; font-weight: 600;
            margin-bottom: 20px; line-height: 1.5;
        }
        .form-message.ok {
            background: #effaf4; border: 1px solid #bfe8d4; color: #14663f;
        }
        .form-message.err {
            background: #fdf0f1; border: 1px solid #f3c2c9; color: #a3141f;
        }

        .contact-info {
            display: flex; flex-direction: column; justify-content: center;
            gap: 22px; padding: 8px 4px;
        }
        .contact-info-block {
            padding-bottom: 18px;
            border-bottom: 1px solid #dbe4f6;
        }
        .contact-info-block:last-of-type { border-bottom: none; }
        .contact-info-kicker {
            font-size: 10.5px; font-weight: 800; letter-spacing: 2.5px;
            text-transform: uppercase; color: #e8283f; margin-bottom: 8px;
        }
        .contact-info-value {
            display: block; font-size: 1em; color: #0a1e4a;
            font-weight: 700; text-decoration: none;
            line-height: 1.7; word-break: break-word;
            transition: color .2s ease;
        }
        .contact-info-value:hover { color: #e8283f; text-decoration: none; }
        .contact-info-note {
            font-size: .95em; color: #445; line-height: 1.7;
        }

        /* =========================================================
           RESPONSIVE
           ========================================================= */
        @media (max-width: 900px) {
            .hero-section { height: auto; min-height: 460px; padding: 70px 20px; }
            .hero-title { font-size: 2em; }
            .hero-tagline { font-size: .98em; }
            .hero-ctas { flex-direction: column; align-items: center; }
            .btn-hero-solid, .btn-hero-ghost { width: 100%; max-width: 340px; text-align: center; }

            .trust-strip { margin-top: -22px; padding: 18px 10px; }
            .trust-item { border-right: none; border-bottom: 1px solid #eef2fa; padding: 12px 10px; }
            .trust-item:last-child { border-bottom: none; }

            .findus-section { padding: 36px 22px; gap: 26px; }
            .findus-map { min-height: 300px; min-width: 100%; }
            .findus-map iframe { min-height: 300px; }
            .findus-title { font-size: 1.65em; }

            .contact-section { padding: 40px 22px; }
            .contact-grid { grid-template-columns: 1fr; gap: 30px; }
            .contact-form-card { padding: 26px 22px; }

            .pricing-section { padding: 44px 20px; }
            .pricing-slogan { font-size: 1.65em; }
            .section-pad { margin-top: 54px; }
            .pass-step { width: 100%; max-width: 420px; }
            .gallery-img-wrap { height: 220px; }
        }
        @media (max-width: 480px) {
            .hero-title { font-size: 1.7em; }
            .section-head h2 { font-size: 1.5em; }
            .action-link { width: 100%; }
            .package-card { width: 100%; }
        }
    </style>

    <!-- ================= HERO ================= -->
    <div class="hero-section">
        <img class="hero-slide s1" src="<%= ResolveUrl("~/Images/happy_customer.png") %>" alt="Graduate holding new licence" />
        <img class="hero-slide s2" src="<%= ResolveUrl("~/Images/khwezi_person1.png") %>" alt="Instructor and student" />
        <img class="hero-slide s3" src="<%= ResolveUrl("~/Images/khwezi_person2.png") %>" alt="Practical training" />

        <div class="hero-overlay"></div>

        <div class="hero-content">
            <span class="hero-kicker">Durban &middot; UKZN Concourse</span>
            <h1 class="hero-title">
                Your Journey to the<br />
                <span class="accent">Driver's Seat</span> Starts Here
            </h1>
            <p class="hero-tagline">
                Professional instruction from licensed experts.
                90% first-time pass rate across Code 08, Code 10, and Automatic.
            </p>
            <div class="hero-ctas">
                <a runat="server" href="~/BookSession" class="btn-hero-solid">Book a Session</a>
                <a runat="server" href="~/Offers" class="btn-hero-ghost">View Packages</a>
            </div>
        </div>
    </div>

    <div class="container">

        <!-- ================= TRUST STRIP ================= -->
        <div class="trust-strip">
            <div class="trust-item">
                <div class="t-value"><span class="stars">&#9733;&#9733;&#9733;&#9733;&#9733;</span> 5.0</div>
                <div class="t-label">Client Rating</div>
            </div>
            <div class="trust-item">
                <div class="t-value">90%</div>
                <div class="t-label">First-Time Pass Rate</div>
            </div>
            <div class="trust-item">
                <div class="t-value">Code 08 &middot; 10 &middot; Automatic</div>
                <div class="t-label">Licence Classes</div>
            </div>
            <div class="trust-item">
                <div class="t-value">50% / 60 Days</div>
                <div class="t-label">Deposit &middot; Payment Plan</div>
            </div>
        </div>

        <!-- ================= HOW WE HELP YOU PASS ================= -->
        <div class="section-pad">
            <div class="section-head">
                <div class="section-kicker">Our Method</div>
                <h2>How We Help You Pass</h2>
                <div class="rule"></div>
                <p class="section-sub">
                    Three simple stages. A proven process. From your first theory
                    session to your final pass certificate.
                </p>
            </div>

            <div class="pass-steps">
                <div class="pass-step">
                    <span class="pass-step-num">01</span>
                    <div class="pass-step-icon">
                        <svg viewBox="0 0 24 24" fill="none" stroke="#fff" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                            <path d="M2 3h6a4 4 0 0 1 4 4v14a3 3 0 0 0-3-3H2z"/>
                            <path d="M22 3h-6a4 4 0 0 0-4 4v14a3 3 0 0 1 3-3h7z"/>
                        </svg>
                    </div>
                    <h3>Learn the Theory</h3>
                    <p>Our K53 theory sessions prepare you for the learner's test with structured practice and qualified instructors.</p>
                </div>
                <div class="pass-step">
                    <span class="pass-step-num">02</span>
                    <div class="pass-step-icon">
                        <svg viewBox="0 0 24 24" fill="none" stroke="#fff" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                            <circle cx="12" cy="12" r="10"/>
                            <polygon points="10 8 16 12 10 16 10 8"/>
                        </svg>
                    </div>
                    <h3>Practice Behind the Wheel</h3>
                    <p>Hands-on lessons with patient instructors in real traffic. Learn at your pace, in the vehicle you'll test in.</p>
                </div>
                <div class="pass-step">
                    <span class="pass-step-num">03</span>
                    <div class="pass-step-icon">
                        <svg viewBox="0 0 24 24" fill="none" stroke="#fff" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                            <circle cx="12" cy="8" r="7"/>
                            <polyline points="8.21 13.89 7 23 12 20 17 23 15.79 13.88"/>
                        </svg>
                    </div>
                    <h3>Pass and Drive</h3>
                    <p>We prepare you for test day with mock exams, route practice, and confidence-building. We don't pass until you do.</p>
                </div>
            </div>
        </div>

        <!-- ================= GALLERY ================= -->
        <div class="section-pad">
            <div class="section-head">
                <div class="section-kicker">In Action</div>
                <h2>Real Students, Real Success</h2>
                <div class="rule"></div>
                <p class="section-sub">
                    Every lesson is a step toward independence. Here's what that journey looks like at IKHWEZI.
                </p>
            </div>

            <div class="gallery-grid">
                <div class="gallery-card">
                    <div class="gallery-img-wrap">
                        <img src="<%= ResolveUrl("~/Images/khwezi_person1.png") %>" alt="Instructor coaching a student" />
                    </div>
                    <div class="gallery-caption">
                        <h3>One-on-One Coaching</h3>
                        <p>Our instructors sit beside you from day one &mdash; correcting, encouraging, and building your confidence behind the wheel.</p>
                    </div>
                </div>
                <div class="gallery-card">
                    <div class="gallery-img-wrap">
                        <img src="<%= ResolveUrl("~/Images/happy_customer.png") %>" alt="Student holding new driver's licence" />
                    </div>
                    <div class="gallery-caption">
                        <h3>Licence in Hand</h3>
                        <p>The moment it all pays off. Our 90% first-time pass rate speaks for itself &mdash; and for the students we've helped.</p>
                    </div>
                </div>
                <div class="gallery-card">
                    <div class="gallery-img-wrap">
                        <img src="<%= ResolveUrl("~/Images/khwezi_person2.png") %>" alt="Practical driving lesson in traffic" />
                    </div>
                    <div class="gallery-caption">
                        <h3>Real-World Driving</h3>
                        <p>We train on real roads, in real traffic, in the same vehicles you'll be tested in &mdash; no surprises on test day.</p>
                    </div>
                </div>
            </div>
        </div>

        <!-- ================= SERVICE OPTIONS ================= -->
        <div class="section-pad">
            <div class="section-head">
                <div class="section-kicker">Our Services</div>
                <h2>How Can We Help You Today?</h2>
                <div class="rule"></div>
                <p class="section-sub">
                    From your first theory lesson to your final pass, we guide every step of the journey.
                </p>
            </div>

            <div class="action-container">
                <a runat="server" href="~/LearnersClass" class="action-link">
                    <div class="action-num">01</div>
                    <h3>Learner's Licence Prep</h3>
                    <p>K53 theory classes for Codes 08 &amp; 10, taught by qualified instructors.</p>
                    <span class="action-more">Book a class &rsaquo;</span>
                </a>
                <a runat="server" href="~/BookSession" class="action-link">
                    <div class="action-num">02</div>
                    <h3>Driver's Licence Prep</h3>
                    <p>Practical driving lessons with patient, qualified instructors.</p>
                    <span class="action-more">Book a session &rsaquo;</span>
                </a>
                <a runat="server" href="~/Offers" class="action-link">
                    <div class="action-num">03</div>
                    <h3>Explore Our Packages</h3>
                    <p>From 10 lessons to full courses &mdash; transparent pricing for every licence code.</p>
                    <span class="action-more">View packages &rsaquo;</span>
                </a>
                <a href="#" onclick="openPaymentModal(); return false;" class="action-link">
                    <div class="action-num">04</div>
                    <h3>Make Payments Here</h3>
                    <p>Secure deposits and instalments &mdash; online payments coming soon.</p>
                    <span class="action-more">Payment info &rsaquo;</span>
                </a>
                <a runat="server" href="~/LearnersGuide" class="action-link">
                    <div class="action-num">05</div>
                    <h3>Free Learner's Guide</h3>
                    <p>Complete study guide for the K53 test &mdash; tips, strategies, and checklists.</p>
                    <span class="action-more">Read the guide &rsaquo;</span>
                </a>
            </div>
        </div>

        <!-- ================= PRICING ================= -->
        <div class="pricing-section">
            <div class="pricing-kicker">Transparent Pricing</div>
            <div class="pricing-slogan">We Don't Pass Until You Do</div>
            <p class="pricing-sub">
                Real packages from our price list. Pay 50% deposit and spread the balance
                over 60 days &mdash; no hidden fees.
            </p>

            <div class="package-row">
                <a runat="server" href="~/Offers" class="package-card">
                    <span class="package-kicker">&nbsp;</span>
                    <div class="package-title">CODE 08</div>
                    <div class="package-lessons">Light Motor Vehicle</div>
                    <div class="package-price"><span class="from">From</span>R 1,600</div>
                    <div class="package-cta">View 5 packages &rsaquo;</div>
                </a>
                <a runat="server" href="~/Offers" class="package-card hot">
                    <span class="package-kicker">Most Popular</span>
                    <div class="package-title">CODE 10</div>
                    <div class="package-lessons">Medium Truck</div>
                    <div class="package-price"><span class="from">From</span>R 1,800</div>
                    <div class="package-cta">View 5 packages &rsaquo;</div>
                </a>
                <a runat="server" href="~/Offers" class="package-card">
                    <span class="package-kicker">&nbsp;</span>
                    <div class="package-title">AUTOMATIC</div>
                    <div class="package-lessons">Automatic Light Motor Vehicle</div>
                    <div class="package-price"><span class="from">From</span>R 2,000</div>
                    <div class="package-cta">View 5 packages &rsaquo;</div>
                </a>
            </div>

            <div class="payment-line">
                Pay 50% Deposit <span class="dot">&middot;</span>
                60 Days to Pay <span class="dot">&middot;</span>
                Free Learner's Licence on Full Courses
            </div>
        </div>

        <!-- ================= FIND US ================= -->
        <div class="findus-section" id="findus">
            <div class="findus-map">
                <iframe src="https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3463.106244746098!2d31.0293!3d-29.8686!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x1ef7a80001e5e3e3%3A0x7c8c6f0a6c5e3e2c!2sUniversity%20of%20KwaZulu-Natal!5e0!3m2!1sen!2sza!4v1640000000000!5m2!1sen!2sza"
                    allowfullscreen="" loading="lazy"></iframe>
            </div>

            <div class="findus-info">
                <div class="findus-kicker">Visit Us</div>
                <div class="findus-title">Find <span class="accent">Us</span></div>

                <div class="findus-item">
                    <div class="findus-label">Address</div>
                    <div class="findus-value">UKZN Concourse, University of KwaZulu-Natal<br />Durban, South Africa</div>
                </div>
                <div class="findus-item">
                    <div class="findus-label">Phone &amp; WhatsApp</div>
                    <div class="findus-value">
                        <a href="tel:+27311234567">+27 31 123 4567</a>
                        &nbsp;&middot;&nbsp;
                        <a href="https://wa.me/27812345678" target="_blank" rel="noopener">+27 81 234 5678</a>
                    </div>
                </div>
                <div class="findus-item">
                    <div class="findus-label">Email</div>
                    <div class="findus-value">
                        <a href="mailto:info@khwezidriving.co.za?subject=Enquiry%20from%20website">info@khwezidriving.co.za</a>
                    </div>
                </div>
                <div class="findus-item">
                    <div class="findus-label">Operating Hours</div>
                    <div class="findus-value">
                        Mon &ndash; Fri: 08:00 &ndash; 17:00<br />
                        Sat: 08:00 &ndash; 13:00 &nbsp;&middot;&nbsp; Sun: Closed
                    </div>
                </div>

                <a class="btn-directions"
                   href="https://www.google.com/maps/search/?api=1&query=University+of+KwaZulu-Natal+Durban"
                   target="_blank" rel="noopener">Get Directions</a>
            </div>
        </div>

        <!-- ================= CONTACT FORM ================= -->
        <div class="contact-section">
            <div class="section-head">
                <div class="section-kicker">Get In Touch</div>
                <h2>Send Us a Message</h2>
                <div class="rule"></div>
                <p class="section-sub">
                    Questions about lessons, packages or pricing? We'll reply within one business day.
                </p>
            </div>

            <div class="contact-grid">

                <!-- Form -->
                <div class="contact-form-card">
                    <asp:Label ID="lblContactMsg" runat="server" Visible="false" />

                    <div class="form-row">
                        <label class="form-label" for="<%= txtContactName.ClientID %>">Your Name</label>
                        <asp:TextBox ID="txtContactName" runat="server" CssClass="form-input"
                            placeholder="e.g. Thabo Dlamini" MaxLength="100" />
                    </div>

                    <div class="form-row">
                        <label class="form-label" for="<%= txtContactEmail.ClientID %>">Your Email</label>
                        <asp:TextBox ID="txtContactEmail" runat="server" CssClass="form-input"
                            TextMode="Email" placeholder="you@email.com" MaxLength="200" />
                    </div>

                    <div class="form-row">
                        <label class="form-label" for="<%= txtContactPhone.ClientID %>">Your Number</label>
                        <asp:TextBox ID="txtContactPhone" runat="server" CssClass="form-input"
                            placeholder="e.g. 082 123 4567" MaxLength="20" />
                    </div>

                    <div class="form-row">
                        <label class="form-label" for="<%= txtContactMessage.ClientID %>">Your Message</label>
                        <asp:TextBox ID="txtContactMessage" runat="server" CssClass="form-input form-textarea"
                            TextMode="MultiLine" placeholder="How can we help?" MaxLength="2000" Rows="6" />
                    </div>

                    <asp:Button ID="btnContactSubmit" runat="server" Text="Submit"
                        CssClass="form-submit" OnClick="btnContactSubmit_Click" />
                </div>

                <!-- Info -->
                <aside class="contact-info">
                    <div class="contact-info-block">
                        <div class="contact-info-kicker">Call / WhatsApp</div>
                        <a class="contact-info-value" href="tel:+27311234567">+27 31 123 4567</a>
                        <a class="contact-info-value" href="https://wa.me/27812345678" target="_blank" rel="noopener">+27 81 234 5678</a>
                    </div>

                    <div class="contact-info-block">
                        <div class="contact-info-kicker">Email</div>
                        <a class="contact-info-value" href="mailto:info@khwezidriving.co.za?subject=Enquiry%20from%20website">
                            info@khwezidriving.co.za
                        </a>
                    </div>

                    <div class="contact-info-block">
                        <div class="contact-info-kicker">Office Hours</div>
                        <div class="contact-info-note">
                            Mon &ndash; Fri: 08:00 &ndash; 17:00<br />
                            Sat: 08:00 &ndash; 13:00<br />
                            Sun: Closed
                        </div>
                    </div>

                    <div class="contact-info-block">
                        <div class="contact-info-kicker">Prefer to Book Directly?</div>
                        <a runat="server" href="~/BookSession" class="contact-info-value">Book a Session &rsaquo;</a>
                    </div>
                </aside>

            </div>
        </div>

    </div>

    <!-- ================= PAYMENT MODAL ================= -->
    <div id="paymentModal" class="pay-modal-overlay" onclick="if(event.target===this) closePaymentModal();"
         style="position:fixed;inset:0;background:rgba(10,30,74,.7);backdrop-filter:blur(4px);display:none;align-items:center;justify-content:center;padding:20px;z-index:99999;">
        <div class="pay-modal-card"
             style="background:#fff;border-radius:20px;max-width:520px;width:100%;padding:42px 38px 34px;box-shadow:0 30px 80px rgba(0,0,0,.4);position:relative;text-align:center;border-top:4px solid #e8283f;">
            <button type="button" class="pay-modal-close" onclick="closePaymentModal();" aria-label="Close"
                    style="position:absolute;top:14px;right:16px;background:transparent;border:none;color:#7787a5;font-size:28px;line-height:1;cursor:pointer;padding:4px 8px;">&times;</button>

            <h2 style="color:#0a1e4a;font-size:1.5em;font-weight:800;margin-bottom:14px;">Online Payments Coming Soon</h2>
            <p style="color:#556;font-size:15px;line-height:1.7;margin-bottom:26px;">
                We're putting the finishing touches on secure online payments.
                Until then, please make your 50% deposit at reception using
                <b>Cash</b>, <b>Card</b>, or <b>EFT</b>.
            </p>

            <div style="display:flex;flex-wrap:wrap;gap:10px;justify-content:center;">
                <a href="https://wa.me/27812345678?text=Hi%20IKHWEZI%2C%20I%27d%20like%20to%20pay%20my%20deposit"
                   target="_blank" rel="noopener"
                   style="display:inline-block;padding:13px 28px;border-radius:26px;font-weight:800;font-size:14px;text-decoration:none;background:#25d366;color:#fff;">
                    WhatsApp Reception
                </a>
                <a href="tel:+27311234567"
                   style="display:inline-block;padding:13px 28px;border-radius:26px;font-weight:800;font-size:14px;text-decoration:none;background:transparent;color:#0d2b6e;border:2px solid #0d2b6e;">
                    Call Us
                </a>
            </div>
        </div>
    </div>

    <!-- ================= SCRIPTS ================= -->
    <script>
        function openPaymentModal() {
            var m = document.getElementById('paymentModal');
            if (m) { m.style.display = 'flex'; document.body.style.overflow = 'hidden'; }
        }
        function closePaymentModal() {
            var m = document.getElementById('paymentModal');
            if (m) { m.style.display = 'none'; document.body.style.overflow = ''; }
        }
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' || e.keyCode === 27) closePaymentModal();
        });
    </script>

</asp:Content>