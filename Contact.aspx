 <%@ Page Title="Contact Us" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Contact.aspx.cs" Inherits="WebApplicationM3.Contact" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        /* Contact Page Premium Styling */
        .contact-hero {
            background: linear-gradient(135deg, #003366 0%, #007bff 100%);
            color: white;
            padding: 60px 20px;
            text-align: center;
            border-radius: 0 0 20px 20px;
            margin-top: -20px;
            margin-bottom: 40px;
        }
        .contact-hero h1 {
            font-size: 2.8em;
            font-weight: 800;
            margin-bottom: 10px;
        }
        .contact-hero p {
            font-size: 1.2em;
            opacity: 0.9;
        }

        .contact-wrapper {
            display: flex;
            flex-wrap: wrap;
            gap: 40px;
            margin-bottom: 50px;
        }

        /* Left Side: Info & Map */
        .info-column {
            flex: 1;
            min-width: 300px;
        }
        .info-card {
            background: white;
            padding: 30px;
            border-radius: 12px;
            box-shadow: 0 4px 15px rgba(0,0,0,0.05);
            margin-bottom: 30px;
        }
        .info-card h3 {
            color: #003366;
            font-weight: 700;
            margin-bottom: 20px;
            border-bottom: 2px solid #e6f2ff;
            padding-bottom: 10px;
        }
        .info-item {
            display: flex;
            align-items: flex-start;
            margin-bottom: 20px;
        }
        .info-icon {
            font-size: 1.5em;
            color: #007bff;
            margin-right: 15px;
            min-width: 25px;
        }
        .info-text h4 {
            font-size: 1.1em;
            color: #003366;
            margin: 0 0 5px 0;
        }
        .info-text p {
            margin: 0;
            color: #555;
            line-height: 1.5;
        }
        .map-container {
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 4px 15px rgba(0,0,0,0.05);
            height: 300px;
        }
        .map-container iframe {
            width: 100%;
            height: 100%;
            border: 0;
        }

        /* Right Side: Contact Form */
        .form-column {
            flex: 1;
            min-width: 300px;
        }
        .form-card {
            background: white;
            padding: 40px;
            border-radius: 12px;
            box-shadow: 0 4px 15px rgba(0,0,0,0.05);
        }
        .form-group {
            margin-bottom: 25px;
        }
        .form-card label {
            font-weight: 600;
            color: #003366;
            display: block;
            margin-bottom: 8px;
        }
        .form-control {
            width: 100%;
            padding: 12px 15px;
            border: 1px solid #ccc;
            border-radius: 6px;
            font-size: 1em;
            transition: border-color 0.3s;
        }
        .form-control:focus {
            border-color: #007bff;
            outline: none;
            box-shadow: 0 0 5px rgba(0,123,255,0.3);
        }
        .btn-submit {
            background-color: #007bff;
            color: white;
            padding: 14px 30px;
            border: none;
            border-radius: 6px;
            font-size: 1.1em;
            font-weight: 600;
            cursor: pointer;
            transition: background-color 0.3s;
            width: 100%;
        }
        .btn-submit:hover {
            background-color: #0056b3;
        }
    </style>

    <!-- Hero Header -->
    <div class="contact-hero">
        <h1>Get in Touch</h1>
        <p>Have a question about our packages? We'd love to hear from you!</p>
    </div>

    <div class="contact-wrapper">
        
        <!-- Left Column: Business Info & Map -->
        <div class="info-column">
            <div class="info-card">
                <h3>Visit Our Office</h3>
                
                <div class="info-item">
                    <div class="info-icon">📍</div>
                    <div class="info-text">
                        <h4>Our Location</h4>
                        <p>Durban UKZN Concourse<br>University of KwaZulu-Natal<br>Durban, South Africa</p>
                    </div>
                </div>

                <div class="info-item">
                    <div class="info-icon">📞</div>
                    <div class="info-text">
                        <h4>Phone & WhatsApp</h4>
                        <p>+27 31 123 4567<br>+27 81 234 5678</p>
                    </div>
                </div>

                <div class="info-item">
                    <div class="info-icon">✉️</div>
                    <div class="info-text">
                        <h4>Email Us</h4>
                        <p>info@khwezidriving.co.za</p>
                    </div>
                </div>

                <div class="info-item">
                    <div class="info-icon">🕒</div>
                    <div class="info-text">
                        <h4>Operating Hours</h4>
                        <p>Monday - Friday: 8:00 AM - 5:00 PM<br>Saturday: 8:00 AM - 1:00 PM<br>Sunday: Closed</p>
                    </div>
                </div>
            </div>

            <!-- Google Map Embed (UKZN Durban) -->
            <div class="map-container">
                <iframe src="https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3463.106244746098!2d31.0293!3d-29.8686!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x1ef7a80001e5e3e3%3A0x7c8c6f0a6c5e3e2c!2sUniversity%20of%20KwaZulu-Natal!5e0!3m2!1sen!2sza!4v1640000000000!5m2!1sen!2sza" allowfullscreen="" loading="lazy"></iframe>
            </div>
        </div>

        <!-- Right Column: Contact Form -->
        <div class="form-column">
            <div class="form-card">
                <h3 style="color: #003366; font-weight: 700; margin-bottom: 20px;">Send Us a Message</h3>
                
                <div class="form-group">
                    <label for="txtName">Full Name</label>
                    <asp:TextBox ID="txtName" runat="server" CssClass="form-control" placeholder="e.g. Sipho Dlamini"></asp:TextBox>
                </div>

                <div class="form-group">
                    <label for="txtEmail">Email Address</label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" placeholder="you@example.com"></asp:TextBox>
                </div>

                <div class="form-group">
                    <label for="txtPhone">Phone Number</label>
                    <asp:TextBox ID="txtPhone" runat="server" CssClass="form-control" TextMode="Phone" placeholder="+27 12 345 6789"></asp:TextBox>
                </div>

                <div class="form-group">
                    <label for="txtMessage">How can we help you?</label>
                    <asp:TextBox ID="txtMessage" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="5" placeholder="I would like to inquire about Code 08 lessons..."></asp:TextBox>
                </div>

                <asp:Button ID="btnSend" runat="server" Text="Send Message" CssClass="btn-submit" OnClick="btnSend_Click" />
                <asp:Label ID="lblSuccess" runat="server" ForeColor="Green" Font-Bold="true" style="display:block; margin-top:15px;"></asp:Label>
            </div>
        </div>

    </div>

</asp:Content>