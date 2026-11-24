 
<%@ Page Title="Student Profile" Language="C#" AutoEventWireup="true" CodeBehind="ReceptionistStudentDetail.aspx.cs" Inherits="WebApplicationM3.ReceptionistStudentDetail" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <title>Student Profile - IKHWEZI</title>
    <style>
        body { font-family: 'Segoe UI', Arial, sans-serif; background: #f0f4f8; padding: 40px; }
        .card { background: #fff; border-radius: 12px; padding: 30px; max-width: 800px; margin: 0 auto; box-shadow: 0 4px 15px rgba(0,0,0,0.05); }
        h2 { color: #0d2b6e; margin-bottom: 20px; border-bottom: 2px solid #e2e8f0; padding-bottom: 10px; }

        /* NEW: Profile picture */
        .profile-photo-section {
            display: flex;
            align-items: center;
            justify-content: center;
            margin-bottom: 25px;
        }
        .profile-photo {
            width: 125px;
            height: 125px;
            border-radius: 50%;
            object-fit: cover;
            border: 4px solid #e2e8f0;
            background: #f8fafc;
        }
        .profile-placeholder {
            width: 125px;
            height: 125px;
            border-radius: 50%;
            background: #e2e8f0;
            color: #64748b;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 48px;
            font-weight: 700;
        }

        .details-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; margin-bottom: 30px; }
        .field { margin-bottom: 10px; }
        .lbl { font-weight: 700; color: #64748b; font-size: 12px; text-transform: uppercase; display: block; }
        .val { color: #1e293b; font-size: 15px; font-weight: 600; display: block; margin-top: 4px; }

        .pkg-card { background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; padding: 20px; margin-bottom: 30px; }
        .pkg-title { color: #0d2b6e; font-size: 16px; font-weight: 800; margin-bottom: 10px; }
        .pkg-stats { display: flex; gap: 20px; }
        .stat { font-size: 14px; color: #64748b; }
        .stat strong { color: #22b573; font-size: 18px; display: block; }

        .btn-back { display: inline-block; margin-top: 20px; padding: 12px 24px; background: #e2e8f0; color: #1e293b; border-radius: 8px; font-weight: 700; text-decoration: none; transition: 0.2s; }
        .btn-back:hover { background: #cbd5e1; text-decoration: none; }

        .msg { padding: 14px; border-radius: 8px; margin-bottom: 20px; font-weight: 600; font-size: 14px; display: block; }
        .msg.err { background: #fef2f2; color: #b91c1c; border: 1px solid #fecaca; }

        @media (max-width: 600px) {
            .details-grid { grid-template-columns: 1fr; }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="card">
            <h2>Student Profile</h2>

            <asp:Label ID="lblMsg" runat="server" Visible="false" />

            <!-- NEW: READ-ONLY PROFILE PICTURE -->
            <div class="profile-photo-section">
                <asp:Image
                    ID="imgStudentPhoto"
                    runat="server"
                    CssClass="profile-photo"
                    AlternateText="Student profile picture"
                    Visible="false" />

                <asp:Panel
                    ID="pnlPhotoPlaceholder"
                    runat="server"
                    CssClass="profile-placeholder">
                    <asp:Literal ID="litPhotoInitials" runat="server" />
                </asp:Panel>
            </div>

            <div class="details-grid">
                <div class="field">
                    <span class="lbl">Name</span>
                    <span class="val"><asp:Literal ID="litName" runat="server" /></span>
                </div>
                <div class="field">
                    <span class="lbl">Student Number</span>
                    <span class="val"><asp:Literal ID="litNumber" runat="server" /></span>
                </div>
                <div class="field">
                    <span class="lbl">Email</span>
                    <span class="val"><asp:Literal ID="litEmail" runat="server" /></span>
                </div>
                <div class="field">
                    <span class="lbl">Phone</span>
                    <span class="val"><asp:Literal ID="litPhone" runat="server" /></span>
                </div>
                <div class="field">
                    <span class="lbl">Account Status</span>
                    <span class="val"><asp:Literal ID="litStatus" runat="server" /></span>
                </div>
            </div>

            <!-- ACTIVE PACKAGE INFO -->
            <asp:Panel ID="pnlPackage" runat="server" CssClass="pkg-card" Visible="false">
                <div class="pkg-title"><asp:Literal ID="litPkgName" runat="server" /></div>
                <div class="pkg-stats">
                    <div class="stat">
                        Purchased
                        <strong><asp:Literal ID="litPkgTotal" runat="server" /></strong>
                    </div>
                    <div class="stat">
                        Used
                        <strong><asp:Literal ID="litPkgUsed" runat="server" /></strong>
                    </div>
                    <div class="stat">
                        Remaining
                        <strong><asp:Literal ID="litPkgRemaining" runat="server" /></strong>
                    </div>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlNoPackage" runat="server" CssClass="pkg-card" Visible="false">
                <div class="pkg-title" style="color:#64748b;">
                    No active package found.
                </div>
            </asp:Panel>

            <!-- ACTIONS -->
            <a href="AdminStudents.aspx" class="btn-back">
                &larr; Back to Directory
            </a>
        </div>
    </form>
</body>
</html>
