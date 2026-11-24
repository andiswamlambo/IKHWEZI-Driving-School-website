 <%@ Page Title="Learners Guide" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="LearnersGuide.aspx.cs" Inherits="WebApplicationM3.LearnersGuide" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .guide-wrap { max-width: 900px; margin: 40px auto; padding: 0 20px; }

        /* ===== HERO ===== */
        .guide-hero {
            background: linear-gradient(135deg, #0a1e4a, #1a4199);
            border-radius: 20px;
            padding: 56px 44px;
            color: #fff;
            margin-bottom: 40px;
            position: relative;
            overflow: hidden;
        }
        .guide-hero::before {
            content: '';
            position: absolute;
            top: -50%; right: -20%;
            width: 400px; height: 400px;
            background: radial-gradient(circle, rgba(232,40,63,.25), transparent 70%);
            pointer-events: none;
        }
        .guide-kicker {
            color: #ffb3bd;
            font-size: 12px;
            font-weight: 800;
            letter-spacing: 4px;
            text-transform: uppercase;
            margin-bottom: 14px;
        }
        .guide-hero h1 {
            font-size: 2.4em;
            font-weight: 800;
            line-height: 1.15;
            margin-bottom: 16px;
            letter-spacing: -.5px;
        }
        .guide-hero h1 .accent { color: #e8283f; font-style: italic; }
        .guide-hero p {
            font-size: 1.05em;
            color: #cdd9ef;
            line-height: 1.7;
            max-width: 640px;
        }

        /* ===== TOC ===== */
        .toc {
            background: #fff;
            border-radius: 16px;
            padding: 26px 30px;
            box-shadow: 0 8px 26px rgba(13,43,110,.08);
            border-left: 4px solid #e8283f;
            margin-bottom: 40px;
        }
        .toc-title {
            color: #0a1e4a;
            font-size: 13px;
            font-weight: 800;
            letter-spacing: 1.5px;
            text-transform: uppercase;
            margin-bottom: 14px;
        }
        .toc-list {
            list-style: none;
            padding: 0; margin: 0;
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 8px 24px;
        }
        .toc-list a {
            color: #0a1e4a;
            text-decoration: none;
            font-size: 14px;
            font-weight: 600;
            display: inline-block;
            padding: 6px 0;
            transition: color .2s ease, transform .2s ease;
        }
        .toc-list a:hover {
            color: #e8283f;
            transform: translateX(4px);
        }

        /* ===== SECTIONS ===== */
        .guide-section {
            background: #fff;
            border-radius: 16px;
            padding: 40px 38px;
            box-shadow: 0 8px 26px rgba(13,43,110,.08);
            margin-bottom: 26px;
            scroll-margin-top: 100px;
        }
        .section-kicker {
            color: #e8283f;
            font-size: 11.5px;
            font-weight: 800;
            letter-spacing: 3px;
            text-transform: uppercase;
            margin-bottom: 10px;
        }
        .guide-section h2 {
            color: #0a1e4a;
            font-size: 1.7em;
            font-weight: 800;
            margin-bottom: 20px;
            letter-spacing: -.3px;
            line-height: 1.25;
        }
        .guide-section h3 {
            color: #0a1e4a;
            font-size: 1.15em;
            font-weight: 800;
            margin: 26px 0 12px;
            letter-spacing: -.2px;
        }
        .guide-section p {
            color: #445;
            font-size: 15px;
            line-height: 1.75;
            margin-bottom: 16px;
        }
        .guide-section a {
            color: #0d2b6e;
            font-weight: 700;
            text-decoration: underline;
        }
        .guide-section a:hover { color: #e8283f; }

        /* ===== FACT GRID ===== */
        .fact-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
            gap: 16px;
            margin: 24px 0 8px;
        }
        .fact-card {
            background: #f7faff;
            border: 1px solid #dbe4f6;
            border-radius: 12px;
            padding: 18px 20px;
            text-align: center;
        }
        .fact-label {
            color: #7787a5;
            font-size: 11px;
            font-weight: 800;
            letter-spacing: 1.5px;
            text-transform: uppercase;
            margin-bottom: 6px;
        }
        .fact-value {
            color: #0a1e4a;
            font-size: 1.6em;
            font-weight: 800;
            line-height: 1.1;
            letter-spacing: -.5px;
        }
        .fact-note {
            color: #667;
            font-size: 12px;
            margin-top: 4px;
        }

        /* ===== TIPS LIST ===== */
        .tips-list { counter-reset: tip; }
        .tip {
            display: flex;
            gap: 20px;
            padding: 22px 0;
            border-bottom: 1px solid #eef2fa;
        }
        .tip:last-child { border-bottom: none; }
        .tip-num {
            flex-shrink: 0;
            width: 44px;
            height: 44px;
            border-radius: 50%;
            background: linear-gradient(135deg, #0d2b6e, #1a4199);
            color: #fff;
            display: flex;
            align-items: center;
            justify-content: center;
            font-weight: 800;
            font-size: 16px;
        }
        .tip-body h4 {
            color: #0a1e4a;
            font-size: 1.05em;
            font-weight: 800;
            margin-bottom: 6px;
            letter-spacing: -.2px;
        }
        .tip-body p {
            color: #556;
            font-size: 14.5px;
            line-height: 1.7;
            margin: 0;
        }

        /* ===== MISTAKES ===== */
        .mistake {
            background: #fdf0f1;
            border-left: 4px solid #e8283f;
            border-radius: 8px;
            padding: 16px 20px;
            margin-bottom: 14px;
        }
        .mistake h4 {
            color: #a3141f;
            font-size: 14.5px;
            font-weight: 800;
            margin-bottom: 6px;
            display: flex;
            align-items: center;
            gap: 8px;
        }
        .mistake h4 svg {
            width: 16px;
            height: 16px;
            stroke: #a3141f;
            stroke-width: 2;
            fill: none;
            flex-shrink: 0;
        }
        .mistake p {
            color: #556;
            font-size: 14px;
            line-height: 1.65;
            margin: 0;
        }

        /* ===== CHECKLIST ===== */
        .checklist {
            background: #effaf4;
            border: 2px dashed #bfe8d4;
            border-radius: 14px;
            padding: 26px 30px;
            margin-top: 22px;
        }
        .checklist h4 {
            color: #14663f;
            font-size: 1em;
            font-weight: 800;
            margin-bottom: 14px;
            letter-spacing: -.1px;
        }
        .checklist ul {
            list-style: none;
            padding: 0; margin: 0;
        }
        .checklist li {
            color: #22355f;
            font-size: 14.5px;
            line-height: 1.7;
            padding-left: 28px;
            position: relative;
            margin-bottom: 10px;
        }
        /* SVG Checkbox */
        .checklist li::before {
            content: '';
            position: absolute;
            left: 0; top: 4px;
            width: 16px;
            height: 16px;
            background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='%2322b573' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Crect x='3' y='3' width='18' height='18' rx='2' ry='2'/%3E%3C/svg%3E");
            background-size: contain;
            background-repeat: no-repeat;
        }

        /* ===== CTA ===== */
        .guide-cta {
            background: linear-gradient(135deg, #0a1e4a, #1a4199);
            border-radius: 20px;
            padding: 44px 40px;
            color: #fff;
            text-align: center;
            margin-top: 40px;
        }
        .guide-cta h2 {
            font-size: 1.7em;
            font-weight: 800;
            margin-bottom: 10px;
            letter-spacing: -.3px;
        }
        .guide-cta p {
            color: #cdd9ef;
            font-size: 15px;
            line-height: 1.65;
            margin-bottom: 26px;
            max-width: 520px;
            margin-left: auto;
            margin-right: auto;
        }
        .btn-cta {
            display: inline-block;
            padding: 15px 40px;
            background: #e8283f;
            color: #fff;
            border-radius: 30px;
            font-weight: 800;
            font-size: 15px;
            text-decoration: none;
            letter-spacing: .3px;
            box-shadow: 0 10px 26px rgba(232,40,63,.4);
            transition: all .2s ease;
        }
        .btn-cta:hover {
            background: #c81e33;
            color: #fff;
            text-decoration: none;
            transform: translateY(-2px);
        }

        /* ===== RESPONSIVE ===== */
        @media (max-width: 700px) {
            .guide-wrap { padding: 0 14px; margin: 20px auto; }
            .guide-hero { padding: 36px 24px; }
            .guide-hero h1 { font-size: 1.8em; }
            .guide-section { padding: 28px 22px; }
            .guide-section h2 { font-size: 1.4em; }
            .toc-list { grid-template-columns: 1fr; }
            .tip { gap: 14px; }
            .tip-num { width: 38px; height: 38px; font-size: 14px; }
        }
    </style>

    <div class="guide-wrap">

        <!-- ============ HERO ============ -->
        <div class="guide-hero">
            <div class="guide-kicker">Complete Study Guide</div>
            <h1>How to Ace Your <span class="accent">Learner's Licence</span> Test</h1>
            <p>
                Everything you need to know about the K53 learner's test — the format,
                the preparation, the strategies, and the mistakes to avoid. Written by
                instructors who've helped hundreds of students pass first time.
            </p>
        </div>

        <!-- ============ TOC ============ -->
        <div class="toc">
            <div class="toc-title">On This Page</div>
            <ul class="toc-list">
                <li><a href="#about">What is the K53 Learner's Test?</a></li>
                <li><a href="#requirements">What You Need Before You Apply</a></li>
                <li><a href="#strategies">10 Strategies to Ace the Test</a></li>
                <li><a href="#mistakes">Common Mistakes to Avoid</a></li>
                <li><a href="#resources">Free Study Resources</a></li>
                <li><a href="#checklist">Final Pre-Test Checklist</a></li>
            </ul>
        </div>

        <!-- ============ SECTION 1 ============ -->
        <div class="guide-section" id="about">
            <div class="section-kicker">Background</div>
            <h2>What is the K53 Learner's Test?</h2>
            <p>
                The K53 learner's licence test is South Africa's standard examination for anyone
                who wants to drive on public roads. It tests your knowledge of road signs,
                traffic rules, vehicle controls, and safe driving practices.
            </p>
            <p>
                The test is set by the <strong>Department of Transport</strong> and administered by
                provincial licensing departments (such as the KZN Department of Transport). You must
                pass it before you can book your practical driver's licence test.
            </p>

            <h3>The Format at a Glance</h3>
            <div class="fact-grid">
                <div class="fact-card">
                    <div class="fact-label">Questions</div>
                    <div class="fact-value">~64</div>
                    <div class="fact-note">Multiple choice (A, B, C)</div>
                </div>
                <div class="fact-card">
                    <div class="fact-label">Duration</div>
                    <div class="fact-value">60 min</div>
                    <div class="fact-note">Computer or paper</div>
                </div>
                <div class="fact-card">
                    <div class="fact-label">Pass Mark</div>
                    <div class="fact-value">75%</div>
                    <div class="fact-note">Must pass all sections</div>
                </div>
                <div class="fact-card">
                    <div class="fact-label">Cost</div>
                    <div class="fact-value">R 108</div>
                    <div class="fact-note">Learner's licence fee</div>
                </div>
            </div>

            <h3>What Topics Are Tested?</h3>
            <p>The test covers four main areas:</p>
            <ul style="color: #445; font-size: 15px; line-height: 1.9; padding-left: 24px;">
                <li><strong>Road Signs</strong> — around 30% of the test. You'll see an image of a sign and must pick the correct meaning.</li>
                <li><strong>Rules of the Road</strong> — speed limits, right of way, parking rules, and right-of-way at intersections.</li>
                <li><strong>Vehicle Controls</strong> — what each pedal, lever, and dashboard light means.</li>
                <li><strong>Safety and Emergencies</strong> — how to handle brake failure, tyre blowouts, and other emergency situations.</li>
            </ul>
        </div>

        <!-- ============ SECTION 2 ============ -->
        <div class="guide-section" id="requirements">
            <div class="section-kicker">Before You Apply</div>
            <h2>What You Need Before You Apply</h2>
            <p>
                Before you book your learner's test, make sure you have everything on this list.
                Missing documents are the #1 reason students are turned away at the test centre.
            </p>

            <h3>Documents You Must Bring</h3>
            <ul style="color: #445; font-size: 15px; line-height: 1.9; padding-left: 24px;">
                <li>Valid South African ID book, ID card, or passport</li>
                <li>Proof of residence (utility bill, lease agreement, or affidavit — less than 3 months old)</li>
                <li>Two black-and-white ID-sized photographs</li>
                <li>Completed <strong>LL1 application form</strong> (available at any DLTC)</li>
                <li>Completed <strong>eye test report</strong> from an optometrist or DLTC eye test</li>
            </ul>

            <h3>Basic Requirements</h3>
            <ul style="color: #445; font-size: 15px; line-height: 1.9; padding-left: 24px;">
                <li><strong>Age:</strong> You must be at least <strong>17 years old</strong> for a Code 8 (light motor vehicle) licence</li>
                <li><strong>Age:</strong> You must be at least <strong>18 years old</strong> for a Code 10 (heavy motor vehicle) licence</li>
                <li><strong>Health:</strong> You must pass a basic eye test and declare any medical conditions that could affect driving</li>
            </ul>
        </div>

        <!-- ============ SECTION 3 ============ -->
        <div class="guide-section" id="strategies">
            <div class="section-kicker">Study Strategies</div>
            <h2>10 Strategies to Ace the Test</h2>
            <p>
                These strategies come from years of helping students pass. They work — and they're
                the difference between a 70% and a 90% score.
            </p>

            <div class="tips-list">

                <div class="tip">
                    <div class="tip-num">1</div>
                    <div class="tip-body">
                        <h4>Start with Road Signs</h4>
                        <p>Road signs make up ~30% of the test and are the easiest section to master. Learn the 5 shapes (circle, triangle, diamond, rectangle, octagon) and their meanings first. Once you know the shapes, you can guess most signs correctly even if you've never seen them.</p>
                    </div>
                </div>

                <div class="tip">
                    <div class="tip-num">2</div>
                    <div class="tip-body">
                        <h4>Use the K53 Manual as Your Bible</h4>
                        <p>The official K53 manual is available free from the Department of Transport. Everything in the test comes from this book — no exceptions. Download it, read it, highlight it. Don't rely on random online quizzes; they're often wrong.</p>
                    </div>
                </div>

                <div class="tip">
                    <div class="tip-num">3</div>
                    <div class="tip-body">
                        <h4>Practice Every Day for Two Weeks</h4>
                        <p>Consistency beats cramming. Study 30 minutes every day for two weeks instead of four hours the night before. Your brain remembers information better when it's spaced out.</p>
                    </div>
                </div>

                <div class="tip">
                    <div class="tip-num">4</div>
                    <div class="tip-body">
                        <h4>Read Every Question Twice</h4>
                        <p>Many questions use similar wording. Words like <em>"always"</em>, <em>"never"</em>, <em>"unless"</em>, and <em>"except"</em> change the answer. Slow down and read every question carefully before you look at the answers.</p>
                    </div>
                </div>

                <div class="tip">
                    <div class="tip-num">5</div>
                    <div class="tip-body">
                        <h4>Eliminate Wrong Answers First</h4>
                        <p>In a 3-option multiple choice, you have a 33% chance by guessing. But if you can eliminate one obviously-wrong answer first, your odds jump to 50%. Use this technique when you're unsure.</p>
                    </div>
                </div>

                <div class="tip">
                    <div class="tip-num">6</div>
                    <div class="tip-body">
                        <h4>Take Practice Tests on Real Equipment</h4>
                        <p>If your test is computer-based, use a computer for practice. If it's paper-based, print out questions and answer with a pencil. Simulate the real environment as closely as possible.</p>
                    </div>
                </div>

                <div class="tip">
                    <div class="tip-num">7</div>
                    <div class="tip-body">
                        <h4>Time Yourself</h4>
                        <p>You have about 60 minutes for 64 questions — that's roughly 55 seconds per question. Practice under time pressure so you're not rushed on test day. If a question is taking too long, flag it and come back.</p>
                    </div>
                </div>

                <div class="tip">
                    <div class="tip-num">8</div>
                    <div class="tip-body">
                        <h4>Focus on Road Sign Images, Not Descriptions</h4>
                        <p>The real test shows you an <strong>image of a sign</strong> and asks what it means. Don't memorise descriptions like "a red octagon with STOP." Instead, look at pictures and drill your visual memory. Print signs, stick them on your fridge, and quiz yourself.</p>
                    </div>
                </div>

                <div class="tip">
                    <div class="tip-num">9</div>
                    <div class="tip-body">
                        <h4>Get a Good Night's Sleep</h4>
                        <p>Do NOT study the night before. Your brain consolidates memory during sleep. A well-rested brain scores 10-15% higher than a tired one. Eat a proper breakfast on test day.</p>
                    </div>
                </div>

                <div class="tip">
                    <div class="tip-num">10</div>
                    <div class="tip-body">
                        <h4>Arrive Early and Stay Calm</h4>
                        <p>Arrive at the DLTC at least 45 minutes before your appointment. Bring all your documents. Take deep breaths. If you fail, you can retake the test after 7 days — but with good preparation, you won't need to.</p>
                    </div>
                </div>

            </div>
        </div>

        <!-- ============ SECTION 4 ============ -->
        <div class="guide-section" id="mistakes">
            <div class="section-kicker">Watch Out For</div>
            <h2>Common Mistakes to Avoid</h2>
            <p>
                These are the top reasons students fail the K53 learner's test. Learn them, avoid them, and you'll save yourself time, money, and stress.
            </p>

            <div class="mistake">
                <h4>
                    <svg viewBox="0 0 24 24"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
                    Not bringing the correct documents
                </h4>
                <p>You cannot write the test without a valid ID, proof of residence, and completed eye test report. Check, double-check, and triple-check before you leave home.</p>
            </div>

            <div class="mistake">
                <h4>
                    <svg viewBox="0 0 24 24"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
                    Skipping the eye test
                </h4>
                <p>The eye test is mandatory. Book it in advance — the DLTC is often fully booked for weeks. Or get it done at any optometrist and bring the report with you.</p>
            </div>

            <div class="mistake">
                <h4>
                    <svg viewBox="0 0 24 24"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
                    Relying on free online quizzes alone
                </h4>
                <p>Many free quizzes have inaccurate answers. Use the <strong>official K53 manual</strong> as your primary source, and use quizzes only as a supplement.</p>
            </div>

            <div class="mistake">
                <h4>
                    <svg viewBox="0 0 24 24"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
                    Cramming the night before
                </h4>
                <p>You cannot memorise 64 questions and 100+ signs in one night. Start at least two weeks before your test date.</p>
            </div>

            <div class="mistake">
                <h4>
                    <svg viewBox="0 0 24 24"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
                    Reading too fast
                </h4>
                <p>Many students fail because they misread questions. Slow down. Read every word. "Which of the following is NOT a safe practice" is very different from "Which of the following IS a safe practice."</p>
            </div>

            <div class="mistake">
                <h4>
                    <svg viewBox="0 0 24 24"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
                    Ignoring the "why" behind the rules
                </h4>
                <p>Don't just memorise answers — understand why the rule exists. Understanding makes the answer obvious, and you'll retain it longer.</p>
            </div>

            <div class="mistake">
                <h4>
                    <svg viewBox="0 0 24 24"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
                    Test-day stress eating or sleeping poorly
                </h4>
                <p>Eat a light, protein-rich breakfast. Avoid heavy carbs and caffeine that make you jittery. Get a full night's sleep the night before.</p>
            </div>

        </div>

        <!-- ============ SECTION 5 ============ -->
        <div class="guide-section" id="resources">
            <div class="section-kicker">Study Materials</div>
            <h2>Free Study Resources</h2>
            <p>
                Everything below is <strong>free, official, and accurate</strong>. Start with the K53 manual —
                it's the source of every question on the test.
            </p>

            <h3>Official Resources</h3>
            <ul style="color: #445; font-size: 15px; line-height: 1.9; padding-left: 24px;">
                <li><strong>K53 Learner's Manual</strong> — available free from the Department of Transport or at any DLTC</li>
                <li><strong>Your Provincial DLTC website</strong> — for booking, fees, and updates on requirements</li>
                <li><strong>SA Road Traffic Signs Manual</strong> — the official reference for every road sign in South Africa</li>
            </ul>

            <h3>Additional Practice</h3>
            <ul style="color: #445; font-size: 15px; line-height: 1.9; padding-left: 24px;">
                <li>Ask your instructor for practice tests — IKHWEZI has printed versions</li>
                <li>Study the signs in your street, on the way to school or work</li>
                <li>Watch free YouTube tutorials on road signs and rules (search "K53 learner's test South Africa")</li>
            </ul>

            <h3>At IKHWEZI</h3>
            <p>
                We offer <strong>K53 theory classes</strong> taught by qualified instructors.
                These are perfect if you want structured learning with an expert who can
                answer your questions and correct your mistakes.
            </p>
            <p style="text-align: center; margin-top: 26px;">
                <a href="<%= ResolveUrl("~/LearnersClass") %>"
                   style="display:inline-block; padding: 14px 36px; background: #e8283f;
                          color: #fff; border-radius: 28px; font-weight: 800;
                          text-decoration: none; letter-spacing: .3px;">
                    Book a K53 Theory Class &rarr;
                </a>
            </p>
        </div>

        <!-- ============ SECTION 6 ============ -->
        <div class="guide-section" id="checklist">
            <div class="section-kicker">Final Prep</div>
            <h2>Your Pre-Test Checklist</h2>
            <p>
                Print this list and tick off every item before you leave for the test centre.
                Missing even one document can mean losing your booking.
            </p>

            <div class="checklist">
                <h4>Documents to Bring</h4>
                <ul>
                    <li>Valid SA ID book, ID card, or passport</li>
                    <li>Proof of residence (less than 3 months old)</li>
                    <li>Two black-and-white ID-sized photographs</li>
                    <li>Completed LL1 application form</li>
                    <li>Eye test report (from optometrist or DLTC)</li>
                    <li>Booking receipt / confirmation (if you booked online)</li>
                </ul>
            </div>

            <div class="checklist" style="margin-top: 18px;">
                <h4>Day Before</h4>
                <ul>
                    <li>Sleep at least 7 hours</li>
                    <li>Charge your phone (in case you need to call about a delay)</li>
                    <li>Plan your route to the DLTC and check for traffic</li>
                    <li>Set an alarm 2 hours before your test time</li>
                </ul>
            </div>

            <div class="checklist" style="margin-top: 18px;">
                <h4>Test Day</h4>
                <ul>
                    <li>Eat a light, protein-rich breakfast</li>
                    <li>Arrive 45 minutes early</li>
                    <li>Bring a black pen and a notebook</li>
                    <li>Take deep breaths — you've prepared for this</li>
                </ul>
            </div>

        </div>

        <!-- ============ CTA ============ -->
        <div class="guide-cta">
            <h2>Ready to Book Your Learner's Test?</h2>
            <p>
                Once you've passed, book your practical lessons at IKHWEZI.
                We'll get you from learner to licensed driver — one lesson at a time.
            </p>
            <a href="<%= ResolveUrl("~/LearnersClass") %>" class="btn-cta">Book a K53 Theory Class</a>
        </div>

    </div>

</asp:Content>