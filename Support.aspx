 <%@ Page Title="Support" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Support.aspx.cs" Inherits="WebApplicationM3.Support" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .support-wrap { max-width: 1000px; margin: 40px auto; padding: 0 20px; }

        /* ===== HERO ===== */
        .support-hero {
            background: linear-gradient(135deg, #0a1e4a, #1a4199);
            border-radius: 20px;
            padding: 56px 44px;
            color: #fff;
            margin-bottom: 34px;
            position: relative;
            overflow: hidden;
        }
        .support-hero::before {
            content: '';
            position: absolute;
            top: -50%; right: -20%;
            width: 400px; height: 400px;
            background: radial-gradient(circle, rgba(232,40,63,.25), transparent 70%);
            pointer-events: none;
        }
        .support-kicker {
            color: #ffb3bd;
            font-size: 12px;
            font-weight: 800;
            letter-spacing: 4px;
            text-transform: uppercase;
            margin-bottom: 14px;
        }
        .support-hero h1 {
            font-size: 2.4em;
            font-weight: 800;
            line-height: 1.15;
            margin-bottom: 16px;
            letter-spacing: -.5px;
        }
        .support-hero h1 .accent { color: #e8283f; font-style: italic; }
        .support-hero p {
            font-size: 1.05em;
            color: #cdd9ef;
            line-height: 1.7;
            max-width: 640px;
        }

        /* ===== CHANNELS ===== */
        .channels {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
            gap: 18px;
            margin-bottom: 40px;
        }
        .channel {
            background: #fff;
            border-radius: 14px;
            padding: 26px 22px;
            text-decoration: none;
            color: inherit;
            box-shadow: 0 8px 26px rgba(13,43,110,.08);
            border-top: 4px solid #0d2b6e;
            transition: transform .25s ease, box-shadow .25s ease;
            display: flex;
            flex-direction: column;
            align-items: center;
            text-align: center;
        }
        .channel:hover {
            transform: translateY(-4px);
            box-shadow: 0 18px 42px rgba(13,43,110,.15);
            text-decoration: none;
            color: inherit;
        }
        .channel.whatsapp { border-top-color: #25d366; }
        .channel.whatsapp:hover .channel-icon { background: #25d366; }
        .channel.phone { border-top-color: #e8283f; }
        .channel.phone:hover .channel-icon { background: #e8283f; }
        .channel.email { border-top-color: #0d2b6e; }
        .channel.email:hover .channel-icon { background: #0d2b6e; }

        .channel-icon {
            width: 54px; height: 54px;
            border-radius: 50%;
            background: #0d2b6e;
            display: flex;
            align-items: center;
            justify-content: center;
            margin-bottom: 14px;
            transition: background .25s ease;
        }
        .channel-icon svg { width: 26px; height: 26px; fill: #fff; }
        .channel-title {
            color: #0a1e4a;
            font-size: 1em;
            font-weight: 800;
            margin-bottom: 6px;
        }
        .channel-value {
            color: #667;
            font-size: .9em;
            line-height: 1.55;
        }

        /* ===== QUICK HELP ===== */
        .quick-help { margin-top: 20px; margin-bottom: 40px; }
        .section-head-small { margin-bottom: 24px; }
        .section-kicker {
            color: #e8283f;
            font-size: 11.5px;
            font-weight: 800;
            letter-spacing: 3px;
            text-transform: uppercase;
            margin-bottom: 8px;
        }
        .section-head-small h2 {
            color: #0a1e4a;
            font-size: 1.5em;
            font-weight: 800;
            letter-spacing: -.3px;
        }
        .quick-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
            gap: 16px;
        }
        .quick-card {
            background: #fff;
            border-radius: 14px;
            padding: 24px 26px;
            box-shadow: 0 6px 22px rgba(13,43,110,.07);
            border-left: 4px solid #e8283f;
            transition: transform .25s ease, box-shadow .25s ease;
        }
        .quick-card:hover {
            transform: translateX(4px);
            box-shadow: 0 14px 36px rgba(13,43,110,.13);
        }
        .quick-card h4 {
            color: #0a1e4a;
            font-size: 1em;
            font-weight: 800;
            margin-bottom: 8px;
        }
        .quick-card p {
            color: #556;
            font-size: .92em;
            line-height: 1.65;
            margin: 0;
        }
        .quick-card a {
            color: #0d2b6e;
            font-weight: 700;
            text-decoration: underline;
        }
        .quick-card a:hover { color: #e8283f; }

        /* ===== FAQ SEARCH ===== */
        .faq-search-wrap { margin-bottom: 30px; max-width: 640px; }
        .faq-search-box {
            position: relative;
            display: flex;
            align-items: center;
            background: #fff;
            border: 2px solid #dbe4f6;
            border-radius: 14px;
            transition: border-color .2s ease, box-shadow .2s ease;
            box-shadow: 0 4px 14px rgba(13,43,110,.05);
        }
        .faq-search-box:focus-within {
            border-color: #0d2b6e;
            box-shadow: 0 0 0 4px rgba(13,43,110,.08);
        }
        .faq-search-icon {
            width: 22px;
            height: 22px;
            color: #7787a5;
            margin-left: 18px;
            flex-shrink: 0;
        }
        .faq-search-input {
            flex: 1;
            border: none;
            outline: none;
            padding: 16px 14px;
            font-size: 15px;
            color: #0a1e4a;
            background: transparent;
            font-family: inherit;
            min-width: 0;
        }
        .faq-search-input::placeholder { color: #94a3bd; }
        .faq-clear-btn {
            background: transparent;
            border: none;
            color: #7787a5;
            font-size: 24px;
            font-weight: 700;
            cursor: pointer;
            padding: 0 18px;
            line-height: 1;
            transition: color .2s ease;
        }
        .faq-clear-btn:hover { color: #e8283f; }
        .faq-search-status {
            margin-top: 12px;
            font-size: 13.5px;
            color: #667;
            font-weight: 600;
            padding: 0 4px;
        }
        .faq-search-status b { color: #0d2b6e; }
        .faq-search-status.empty { color: #a3141f; }

        /* ===== FAQ ACCORDION ===== */
        .faq-section { margin-top: 40px; }
        .faq-category {
            color: #0a1e4a;
            font-size: 1.1em;
            font-weight: 800;
            margin: 32px 0 14px;
            padding-bottom: 10px;
            border-bottom: 2px solid #e8283f;
            display: inline-block;
            transition: opacity 0.2s ease;
        }
        .faq-category:first-of-type { margin-top: 0; }

        .faq-item {
            background: #fff;
            border-radius: 12px;
            margin-bottom: 10px;
            box-shadow: 0 4px 14px rgba(13,43,110,.06);
            overflow: hidden;
            border-left: 3px solid #0d2b6e;
            transition: opacity 0.2s ease;
        }
        .faq-item[open] {
            box-shadow: 0 10px 28px rgba(13,43,110,.11);
            border-left-color: #e8283f;
        }
        .faq-item summary {
            list-style: none;
            cursor: pointer;
            padding: 18px 24px;
            font-size: 15px;
            font-weight: 700;
            color: #0a1e4a;
            display: flex;
            align-items: center;
            gap: 14px;
            user-select: none;
            transition: background .2s ease;
        }
        .faq-item summary::-webkit-details-marker { display: none; }
        .faq-item summary:hover { background: #f7faff; }
        .faq-item[open] summary { background: #f7faff; }

        .faq-arrow {
            width: 26px;
            height: 26px;
            border-radius: 50%;
            background: #eef4ff;
            color: #0d2b6e;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 14px;
            font-weight: 800;
            flex-shrink: 0;
            transition: transform .25s ease, background .25s ease, color .25s ease;
        }
        .faq-item[open] .faq-arrow {
            background: #e8283f;
            color: #fff;
            transform: rotate(45deg);
        }
        .faq-q { flex: 1; }

        .faq-answer {
            padding: 0 24px 22px 64px;
            color: #445;
            font-size: 14.5px;
            line-height: 1.75;
        }
        .faq-answer p { margin: 0 0 10px; }
        .faq-answer p:last-child { margin-bottom: 0; }
        .faq-answer a { color: #0d2b6e; font-weight: 700; text-decoration: underline; }
        .faq-answer a:hover { color: #e8283f; }
        .faq-answer ul { margin: 8px 0 10px 22px; padding: 0; }
        .faq-answer li { margin-bottom: 6px; }

        /* No results block */
        .faq-no-results {
            display: none;
            background: #fff;
            border-radius: 14px;
            padding: 40px 26px;
            text-align: center;
            box-shadow: 0 6px 22px rgba(13,43,110,.07);
            border-left: 4px solid #e8283f;
            margin-top: 14px;
        }
        .faq-no-results.show { display: block; }
        .faq-no-results h4 {
            color: #0a1e4a;
            font-size: 1.1em;
            font-weight: 800;
            margin-bottom: 8px;
        }
        .faq-no-results p {
            color: #667;
            font-size: 14px;
            line-height: 1.65;
            margin-bottom: 20px;
        }
        .faq-no-results a {
            display: inline-block;
            padding: 12px 28px;
            background: #e8283f;
            color: #fff;
            border-radius: 24px;
            text-decoration: none;
            font-weight: 800;
            font-size: 14px;
        }
        .faq-no-results a:hover { background: #c81e33; color: #fff; text-decoration: none; }

        /* ===== ESCALATION ===== */
        .escalation {
            background: linear-gradient(135deg, #0a1e4a, #1a4199);
            border-radius: 20px;
            padding: 44px 40px;
            color: #fff;
            text-align: center;
            margin-top: 50px;
        }
        .escalation h2 {
            font-size: 1.7em;
            font-weight: 800;
            margin-bottom: 10px;
            letter-spacing: -.3px;
        }
        .escalation p {
            color: #cdd9ef;
            font-size: 15px;
            line-height: 1.7;
            margin-bottom: 26px;
            max-width: 560px;
            margin-left: auto;
            margin-right: auto;
        }
        .escalation-btns {
            display: flex;
            flex-wrap: wrap;
            gap: 12px;
            justify-content: center;
        }
        .btn-escalation {
            display: inline-block;
            padding: 14px 32px;
            border-radius: 28px;
            font-weight: 800;
            font-size: 14.5px;
            text-decoration: none;
            letter-spacing: .3px;
            transition: all .2s ease;
        }
        .btn-escalation.solid {
            background: #e8283f;
            color: #fff;
            box-shadow: 0 10px 26px rgba(232,40,63,.4);
        }
        .btn-escalation.solid:hover {
            background: #c81e33;
            color: #fff;
            text-decoration: none;
            transform: translateY(-2px);
        }
        .btn-escalation.ghost {
            background: transparent;
            color: #fff;
            border: 2px solid rgba(255,255,255,.65);
        }
        .btn-escalation.ghost:hover {
            background: rgba(255,255,255,.12);
            border-color: #fff;
            color: #fff;
            text-decoration: none;
        }

        /* ===== RESPONSIVE ===== */
        @media (max-width: 700px) {
            .support-wrap { padding: 0 14px; margin: 20px auto; }
            .support-hero { padding: 36px 24px; }
            .support-hero h1 { font-size: 1.8em; }
            .channels { grid-template-columns: 1fr; }
            .quick-grid { grid-template-columns: 1fr; }
            .faq-answer { padding-left: 24px; }
            .escalation { padding: 34px 22px; }
            .escalation h2 { font-size: 1.4em; }
            .faq-search-input { font-size: 14px; padding: 14px 12px; }
            .faq-search-icon { width: 20px; height: 20px; margin-left: 14px; }
            .faq-clear-btn { padding: 0 14px; }
        }
    </style>

    <div class="support-wrap">

        <!-- ===== HERO ===== -->
        <div class="support-hero">
            <div class="support-kicker">We're Here to Help</div>
            <h1>Support &amp; <span class="accent">Help Centre</span></h1>
            <p>
                Answers to the most common questions about bookings, payments, lessons
                and the K53 test. Can't find what you need? Reach us directly — we
                reply within one business day.
            </p>
        </div>

        <!-- ===== CHANNELS ===== -->
        <div class="channels">
            <a class="channel whatsapp"
               href="https://wa.me/27812345678?text=Hi%20IKHWEZI%2C%20I%20need%20help%20with"
               target="_blank" rel="noopener">
                <div class="channel-icon">
                    <svg viewBox="0 0 24 24"><path d="M12 2a10 10 0 0 0-8.6 15.1L2 22l5.1-1.4A10 10 0 1 0 12 2zm5.6 14.3c-.2.6-1.2 1.2-1.7 1.2-.5.1-1 .1-1.7-.1-1.8-.7-3-2.5-3.7-3.1-.7-.9-1.3-2-1.3-3.1 0-1 .5-1.6.7-1.8.2-.2.5-.3.6-.3h.4c.1 0 .3 0 .5.4l.6 1.5c.1.1.1.3 0 .5l-.3.5c-.1.2-.2.3-.3.5-.1.1-.2.3 0 .5.2.3.7 1.1 1.4 1.8.9.8 1.7 1.1 2 .5.1-.1.2-.2.3-.4.1-.1.3-.2.5-.1l1.5.7c.2.1.3.2.4.3.1.2.1.4 0 .5z"/></svg>
                </div>
                <div class="channel-title">WhatsApp</div>
                <div class="channel-value">+27 81 234 5678<br />Fastest reply</div>
            </a>

            <a class="channel phone" href="tel:+27311234567">
                <div class="channel-icon">
                    <svg viewBox="0 0 24 24"><path d="M22 16.9v3a2 2 0 0 1-2.2 2 19.8 19.8 0 0 1-8.6-3.1 19.5 19.5 0 0 1-6-6A19.8 19.8 0 0 1 2.1 4.2 2 2 0 0 1 4.1 2h3a2 2 0 0 1 2 1.7c.1 1 .4 1.9.7 2.8a2 2 0 0 1-.5 2.1L8.1 9.9a16 16 0 0 0 6 6l1.3-1.3a2 2 0 0 1 2.1-.4c.9.3 1.8.6 2.8.7a2 2 0 0 1 1.7 2z"/></svg>
                </div>
                <div class="channel-title">Call Us</div>
                <div class="channel-value">+27 31 123 4567<br />Mon – Fri · 8am – 5pm</div>
            </a>

            <a class="channel email" href="mailto:info@khwezidriving.co.za?subject=Support%20Request">
                <div class="channel-icon">
                    <svg viewBox="0 0 24 24"><path d="M4 4h16a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2zm0 2v.5l8 5.5 8-5.5V6H4zm16 12V8.8l-8 5.4-8-5.4V18h16z"/></svg>
                </div>
                <div class="channel-title">Email</div>
                <div class="channel-value">info@khwezidriving.co.za<br />Reply within 1 business day</div>
            </a>
        </div>

        <!-- ===== QUICK HELP ===== -->
        <div class="quick-help">
            <div class="section-head-small">
                <div class="section-kicker">Quick Answers</div>
                <h2>Most Common Questions</h2>
            </div>

            <div class="quick-grid">
                <div class="quick-card">
                    <h4>How do I book a lesson?</h4>
                    <p>Go to <a runat="server" href="~/BookSession">Make Booking</a> from your dashboard. Choose a package, pick a date and time, and confirm. You'll get an email with your booking details.</p>
                </div>
                <div class="quick-card">
                    <h4>How do I pay for my package?</h4>
                    <p>Pay the 50% deposit at reception (Cash, Card, or EFT). Once payment is recorded, your booking is confirmed. The balance is spread over 60 days.</p>
                </div>
                <div class="quick-card">
                    <h4>How do I cancel a booking?</h4>
                    <p>Go to your <a runat="server" href="~/StudentDashboard">Dashboard</a>, find the booking, and click <b>Cancel Booking</b>. Cancellations must be made at least 24 hours before the lesson.</p>
                </div>
                <div class="quick-card">
                    <h4>When can I book my learner's test?</h4>
                    <p>You must be 17 for Code 8, or 18 for Code 10. Read our free <a runat="server" href="~/LearnersGuide">Learner's Guide</a> for the full requirements and study tips.</p>
                </div>
                <div class="quick-card">
                    <h4>How do I upload my documents?</h4>
                    <p>Open <a runat="server" href="~/MyDocuments">My Documents</a> from your dashboard. Upload your ID, learner's licence, and proof of address for faster approval.</p>
                </div>
                <div class="quick-card">
                    <h4>What if I'm running late?</h4>
                    <p>Call or WhatsApp us immediately. If you're more than 15 minutes late, the lesson may be treated as a no-show and counted as completed.</p>
                </div>
            </div>
        </div>

        <!-- ===== FAQ ===== -->
        <div class="faq-section">
            <div class="section-head-small">
                <div class="section-kicker">Full FAQ</div>
                <h2>Frequently Asked Questions</h2>
            </div>

            <!-- FAQ Search -->
            <div class="faq-search-wrap">
                <div class="faq-search-box">
                    <svg class="faq-search-icon" viewBox="0 0 24 24" fill="none"
                         stroke="currentColor" stroke-width="2.2"
                         stroke-linecap="round" stroke-linejoin="round">
                        <circle cx="11" cy="11" r="8"/>
                        <line x1="21" y1="21" x2="16.65" y2="16.65"/>
                    </svg>
                    <input id="faqSearch" type="text" class="faq-search-input"
                           placeholder="Search the FAQ — e.g. 'cancel', 'deposit', 'learner's licence'..."
                           autocomplete="off" />
                    <button type="button" id="faqClear" class="faq-clear-btn" title="Clear search" style="display:none;">&times;</button>
                </div>
                <div id="faqSearchStatus" class="faq-search-status" style="display:none;"></div>
            </div>

            <h3 class="faq-category">Booking &amp; Scheduling</h3>

            <!-- Note: We swapped <span> for <div> inside the <summary> tags to fix VS warnings -->
            <details class="faq-item">
                <summary>
                    <div class="faq-arrow">+</div>
                    <div class="faq-q">How far in advance can I book a lesson?</div>
                </summary>
                <div class="faq-answer">
                    <p>You can book up to <b>60 days in advance</b>. We recommend booking at least a week ahead so you can secure your preferred instructor and time slot.</p>
                </div>
            </details>

            <details class="faq-item">
                <summary>
                    <div class="faq-arrow">+</div>
                    <div class="faq-q">Can I book multiple lessons at once?</div>
                </summary>
                <div class="faq-answer">
                    <p>Yes. You can hold up to <b>5 upcoming bookings</b> at a time. This lets you plan a full week or month of lessons in one go.</p>
                </div>
            </details>

            <details class="faq-item">
                <summary>
                    <div class="faq-arrow">+</div>
                    <div class="faq-q">Can I change my instructor?</div>
                </summary>
                <div class="faq-answer">
                    <p>Yes. When you book a new lesson, choose a different instructor from the drop-down list. If you want to permanently switch, contact reception.</p>
                </div>
            </details>

            <h3 class="faq-category">Payments &amp; Refunds</h3>

            <details class="faq-item">
                <summary>
                    <div class="faq-arrow">+</div>
                    <div class="faq-q">How does the 50% deposit work?</div>
                </summary>
                <div class="faq-answer">
                    <p>To confirm a booking, we require a <b>50% deposit</b> of the package price. The remaining balance is due within <b>60 days</b>. There's no interest or hidden fees.</p>
                </div>
            </details>

            <details class="faq-item">
                <summary>
                    <div class="faq-arrow">+</div>
                    <div class="faq-q">What payment methods do you accept?</div>
                </summary>
                <div class="faq-answer">
                    <p>We accept <b>Cash, Card (in person), and EFT (bank transfer)</b>. Payfast online payments are coming soon.</p>
                </div>
            </details>

            <details class="faq-item">
                <summary>
                    <div class="faq-arrow">+</div>
                    <div class="faq-q">Can I get a refund if I want to stop lessons?</div>
                </summary>
                <div class="faq-answer">
                    <p>Refunds are reviewed case-by-case. Contact reception with your reason and we'll discuss the best option — usually a partial refund or package swap.</p>
                </div>
            </details>

            <details class="faq-item">
                <summary>
                    <div class="faq-arrow">+</div>
                    <div class="faq-q">What happens if I don't pay the balance by 60 days?</div>
                </summary>
                <div class="faq-answer">
                    <p>Your booking becomes <b>Overdue</b> and you'll be unable to book new lessons until the balance is settled. If left unresolved for more than 14 days past the deadline, your account is <b>Suspended</b> and you'll need to speak to reception.</p>
                </div>
            </details>

            <h3 class="faq-category">Lessons &amp; Instructors</h3>

            <details class="faq-item">
                <summary>
                    <div class="faq-arrow">+</div>
                    <div class="faq-q">What should I bring to my lesson?</div>
                </summary>
                <div class="faq-answer">
                    <p>Bring your <b>learner's licence</b> and <b>ID</b>. Arrive 10 minutes early. Wear comfortable shoes and avoid heavy meals before driving.</p>
                </div>
            </details>

            <details class="faq-item">
                <summary>
                    <div class="faq-arrow">+</div>
                    <div class="faq-q">How is a lesson counted as completed?</div>
                </summary>
                <div class="faq-answer">
                    <p>The instructor marks the lesson as Completed after it finishes. Only then is one lesson deducted from your package. Cancellations and no-shows do not count as completed.</p>
                </div>
            </details>

            <details class="faq-item">
                <summary>
                    <div class="faq-arrow">+</div>
                    <div class="faq-q">Can I have a lesson on a Sunday?</div>
                </summary>
                <div class="faq-answer">
                    <p>No. We're closed on Sundays. Lessons run <b>Monday to Friday 7am–5pm</b> and <b>Saturday 8am–1pm</b>.</p>
                </div>
            </details>

            <details class="faq-item">
                <summary>
                    <div class="faq-arrow">+</div>
                    <div class="faq-q">What if the instructor is sick on the day?</div>
                </summary>
                <div class="faq-answer">
                    <p>Reception will contact you to reschedule. You'll never lose a lesson due to instructor illness — we move your booking to a new slot that suits you.</p>
                </div>
            </details>

            <h3 class="faq-category">Learner's Licence Test</h3>

            <details class="faq-item">
                <summary>
                    <div class="faq-arrow">+</div>
                    <div class="faq-q">What is the pass mark for the K53 test?</div>
                </summary>
                <div class="faq-answer">
                    <p><b>75%</b>. You must pass every section (Road Signs, Rules, Controls, Safety) — not just the overall average.</p>
                </div>
            </details>

            <details class="faq-item">
                <summary>
                    <div class="faq-arrow">+</div>
                    <div class="faq-q">How much does the learner's licence cost?</div>
                </summary>
                <div class="faq-answer">
                    <p>The government fee is approximately <b>R 108</b>. Additional costs may apply for the eye test and ID photos. See our <a runat="server" href="~/LearnersGuide">Learner's Guide</a> for the full breakdown.</p>
                </div>
            </details>

            <details class="faq-item">
                <summary>
                    <div class="faq-arrow">+</div>
                    <div class="faq-q">Where can I study for the test?</div>
                </summary>
                <div class="faq-answer">
                    <p>Download our free <a runat="server" href="~/LearnersGuide">Learner's Guide</a> with 10 study strategies and 7 common mistakes. Or book a <a runat="server" href="~/LearnersClass">K53 theory class</a> with us.</p>
                </div>
            </details>

            <h3 class="faq-category">Account &amp; Documents</h3>

            <details class="faq-item">
                <summary>
                    <div class="faq-arrow">+</div>
                    <div class="faq-q">How do I reset my password?</div>
                </summary>
                <div class="faq-answer">
                    <p>On the login page, click <b>Forgot your password?</b> and follow the instructions. You'll receive a 6-digit code by email to set a new password.</p>
                </div>
            </details>

            <details class="faq-item">
                <summary>
                    <div class="faq-arrow">+</div>
                    <div class="faq-q">What documents do I need to upload?</div>
                </summary>
                <div class="faq-answer">
                    <p>Upload your <b>ID</b>, <b>learner's licence</b>, and <b>proof of address</b> on the <a runat="server" href="~/MyDocuments">My Documents</a> page. This lets reception approve your bookings faster.</p>
                </div>
            </details>

            <details class="faq-item">
                <summary>
                    <div class="faq-arrow">+</div>
                    <div class="faq-q">How do I update my phone or email?</div>
                </summary>
                <div class="faq-answer">
                    <p>Go to <a runat="server" href="~/PersonalInfo">Personal Info</a> and update your details. Changes are saved instantly.</p>
                </div>
            </details>

            <!-- No results message -->
            <div id="faqNoResults" class="faq-no-results">
                <h4>No matching questions</h4>
                <p>Try a different word or ask us directly — we usually reply within a few minutes.</p>
                <a href="https://wa.me/27812345678?text=Hi%20IKHWEZI%2C%20I%20need%20help%20with"
                   target="_blank" rel="noopener">Chat on WhatsApp</a>
            </div>

        </div>

        <!-- ===== ESCALATION ===== -->
        <div class="escalation">
            <h2>Still Stuck?</h2>
            <p>
                If you can't find an answer here, our reception team is ready to help.
                Reach us on WhatsApp for the fastest reply — usually within a few minutes.
            </p>
            <div class="escalation-btns">
                <a class="btn-escalation solid"
                   href="https://wa.me/27812345678?text=Hi%20IKHWEZI%2C%20I%20need%20help%20with"
                   target="_blank" rel="noopener">Chat on WhatsApp</a>
                <a class="btn-escalation ghost" href="mailto:info@khwezidriving.co.za?subject=Support%20Request">Send an Email</a>
            </div>
        </div>

    </div>

    <!-- =========================================================
         FAQ SEARCH SCRIPT
         ========================================================= -->
    <script>
        (function () {
            document.addEventListener('DOMContentLoaded', function () {
                var input = document.getElementById('faqSearch');
                var clearBtn = document.getElementById('faqClear');
                var status = document.getElementById('faqSearchStatus');
                var noResults = document.getElementById('faqNoResults');
                var faqSection = document.querySelector('.faq-section');

                if (!input || !faqSection) return;

                var items = faqSection.querySelectorAll('.faq-item');
                var categories = faqSection.querySelectorAll('.faq-category');
                var totalCount = items.length;

                var itemData = [];
                items.forEach(function (item) {
                    var q = item.querySelector('.faq-q');
                    var a = item.querySelector('.faq-answer');
                    var text = ((q ? q.textContent : '') + ' ' +
                        (a ? a.textContent : '')).toLowerCase();
                    itemData.push({ el: item, text: text });
                });

                function runSearch(rawQuery) {
                    var query = (rawQuery || '').trim().toLowerCase();

                    if (query.length === 0) {
                        itemData.forEach(function (d) { d.el.style.display = ''; });
                        categories.forEach(function (c) { c.style.display = ''; });
                        status.style.display = 'none';
                        noResults.classList.remove('show');
                        clearBtn.style.display = 'none';
                        return;
                    }

                    clearBtn.style.display = '';

                    var visibleCount = 0;

                    itemData.forEach(function (d) {
                        if (d.text.indexOf(query) >= 0) {
                            d.el.style.display = '';
                            visibleCount++;
                        } else {
                            d.el.style.display = 'none';
                        }
                    });

                    categories.forEach(function (cat) {
                        var hasVisible = false;
                        var sibling = cat.nextElementSibling;
                        while (sibling && !sibling.classList.contains('faq-category')) {
                            if (sibling.classList.contains('faq-item') &&
                                sibling.style.display !== 'none') {
                                hasVisible = true;
                                break;
                            }
                            sibling = sibling.nextElementSibling;
                        }
                        cat.style.display = hasVisible ? '' : 'none';
                    });

                    status.style.display = '';
                    status.classList.toggle('empty', visibleCount === 0);
                    status.innerHTML = visibleCount === 0
                        ? 'No results for <b>\u201C' + escapeHtml(query) + '\u201D</b>'
                        : '<b>' + visibleCount + '</b> of ' + totalCount +
                        ' questions match <b>\u201C' + escapeHtml(query) + '\u201D</b>';

                    noResults.classList.toggle('show', visibleCount === 0);
                }

                function escapeHtml(s) {
                    var d = document.createElement('div');
                    d.textContent = s;
                    return d.innerHTML;
                }

                input.addEventListener('input', function () {
                    runSearch(this.value);
                });

                clearBtn.addEventListener('click', function () {
                    input.value = '';
                    runSearch('');
                    input.focus();
                });

                input.addEventListener('keydown', function (e) {
                    if (e.key === 'Escape' || e.keyCode === 27) {
                        input.value = '';
                        runSearch('');
                    }
                });
            });
        })();
    </script>

</asp:Content>