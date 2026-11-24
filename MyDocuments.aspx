 <%@ Page Title="My Documents" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MyDocuments.aspx.cs" Inherits="WebApplicationM3.MyDocuments" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .docs-wrap { max-width: 900px; margin: 40px auto; padding: 0 20px; }
        .docs-title { color: #0a1e4a; font-size: 2em; font-weight: 800; margin-bottom: 8px; }
        .docs-sub { color: #667; font-size: 1em; margin-bottom: 32px; line-height: 1.6; }
        
        /* Profile Photo Preview */
        .photo-preview { display:flex; align-items:center; gap:18px; margin-bottom:26px;
            background:#fff; border-radius:16px; padding:18px 22px;
            box-shadow:0 8px 26px rgba(13,43,110,.08); border-top: 4px solid #0d2b6e; }
        .photo-avatar { width:88px; height:88px; border-radius:50%;
            object-fit:cover; border:3px solid #0d2b6e;
            box-shadow:0 4px 14px rgba(13,43,110,.15); flex-shrink:0; }
        .photo-info { display:flex; flex-direction:column; gap:4px; }
        .photo-placeholder { color:#7787a5; font-size:13.5px; font-style:italic; }
        .photo-status { font-size:12px; font-weight:700; letter-spacing:.3px; display: flex; align-items: center; gap: 6px; }
        .photo-status svg { width: 14px; height: 14px; stroke: currentColor; stroke-width: 2; fill: none; }
        .photo-status.pending  { color:#8a6100; }
        .photo-status.approved { color:#14663f; }
        .photo-status.rejected { color:#c81e33; }

        .upload-card { background: #fff; border-radius: 16px; padding: 30px;
            box-shadow: 0 8px 26px rgba(13,43,110,.08); border-top: 4px solid #0d2b6e; margin-bottom: 26px; }
        .upload-title { color: #0a1e4a; font-size: 1.1em; font-weight: 800; margin-bottom: 16px; display: flex; align-items: center; gap: 8px; }
        .upload-title svg { width: 20px; height: 20px; stroke: #0d2b6e; stroke-width: 2; fill: none; }
        
        .form-label { display: block; font-size: 12.5px; font-weight: 700;
            letter-spacing: .8px; color: #0a1e4a; text-transform: uppercase; margin-bottom: 8px; }
        .form-select, .form-file { width: 100%; padding: 12px 14px;
            border: 1.5px solid #d5e0f2; border-radius: 10px; font-size: 14.5px;
            background: #f9fbff; margin-bottom: 18px; }
        .btn-upload { background: #e8283f; color: #fff; border: none;
            padding: 13px 34px; border-radius: 24px; font-weight: 800;
            font-size: 14px; cursor: pointer; display: inline-flex; align-items: center; gap: 8px; }
        .btn-upload svg { width: 16px; height: 16px; stroke: currentColor; stroke-width: 2; fill: none; }
        .btn-upload:hover { background: #c81e33; }
        
        .docs-list { background: #fff; border-radius: 16px; padding: 30px;
            box-shadow: 0 8px 26px rgba(13,43,110,.08); }
        .docs-list h3 { color: #0a1e4a; font-size: 1.1em; font-weight: 800; margin-bottom: 18px; }
        
        .docs-table { width: 100%; border-collapse: collapse; font-size: 13.5px; }
        .docs-table th { text-align: left; background: #f2f6fd; color: #0d2b6e;
            font-size: 11.5px; letter-spacing: .5px; text-transform: uppercase;
            padding: 10px 12px; border-bottom: 2px solid #dbe4f6; }
        .docs-table td { padding: 12px; border-bottom: 1px solid #eef2fa; color: #333; }
        
        .chip { display: inline-flex; align-items: center; gap: 6px; padding: 4px 10px; border-radius: 12px;
            font-size: 11.5px; font-weight: 700; }
        .chip svg { width: 12px; height: 12px; stroke: currentColor; stroke-width: 2; fill: none; }
        .chip.pending  { background: #fff3d6; color: #8a6100; }
        .chip.approved { background: #dcf5e8; color: #14663f; }
        .chip.rejected { background: #fde3e7; color: #c81e33; }
        
        .btn-remove { background: transparent; color: #e8283f; border: none;
            font-weight: 700; font-size: 12px; cursor: pointer; display: flex; align-items: center; gap: 4px; }
        .btn-remove svg { width: 14px; height: 14px; stroke: currentColor; stroke-width: 2; fill: none; }
        .btn-remove:hover { text-decoration: underline; }

        .msg { display: block; padding: 12px 16px; border-radius: 10px;
            font-size: 13.5px; font-weight: 600; margin-bottom: 18px; }
        .msg.ok  { background: #effaf4; border: 1px solid #bfe8d4; color: #14663f; }
        .msg.err { background: #fdf0f1; border: 1px solid #f3c2c9; color: #a3141f; }
        
        .hint { font-size: 12px; color: #7787a5; margin-top: -12px; margin-bottom: 18px; line-height: 1.6; }
        
        @media (max-width: 700px) {
            .docs-wrap { padding: 0 14px; margin: 20px auto; }
            .upload-card, .docs-list { padding: 22px; }
            .docs-table { font-size: 12px; }
            .docs-table th, .docs-table td { padding: 8px 6px; }
        }
    </style>

    <div class="docs-wrap">
        <h1 class="docs-title">My Documents</h1>
        <p class="docs-sub">
            Upload your ID, learner's licence, and proof of address so reception
            can approve your booking faster. Files are stored securely.
        </p>

        <!-- PROFILE PHOTO PREVIEW -->
        <div class="photo-preview">
            <asp:Image ID="imgProfile" runat="server" CssClass="photo-avatar" Visible="false" AlternateText="Profile photo" />
            <div class="photo-info">
                <asp:Label ID="lblNoPhoto" runat="server" CssClass="photo-placeholder" Text="No profile photo yet — upload one below." />
                <asp:Label ID="lblPhotoStatus" runat="server" CssClass="photo-status" Visible="false" />
            </div>
        </div>

        <asp:Label ID="lblMsg" runat="server" Visible="false" />

        <!-- UPLOAD CARD -->
        <div class="upload-card">
            <div class="upload-title">
                <svg viewBox="0 0 24 24"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><polyline points="17 8 12 3 7 8"></polyline><line x1="12" y1="3" x2="12" y2="15"></line></svg>
                Upload a Document
            </div>

            <label class="form-label">Document Type</label>
            <asp:DropDownList ID="ddlDocType" runat="server" CssClass="form-select">
                <asp:ListItem Value="">-- Select type --</asp:ListItem>
                <asp:ListItem Value="ID">ID / Passport</asp:ListItem>
                <asp:ListItem Value="LearnersLicence">Learner's Licence</asp:ListItem>
                <asp:ListItem Value="ProofOfAddress">Proof of Address</asp:ListItem>
                <asp:ListItem Value="Photo">Profile Photo</asp:ListItem>
                <asp:ListItem Value="Other">Other</asp:ListItem>
            </asp:DropDownList>

            <label class="form-label">Choose File</label>
            <asp:FileUpload ID="fuDocument" runat="server" CssClass="form-file" />
            <div class="hint">
                PDF, JPG, or PNG. Max 5 MB per file.<br/>
                A <b>Profile Photo</b> appears on your profile immediately, but is still reviewed by reception.
            </div>

            <asp:Button ID="btnUpload" runat="server" Text="Upload Document"
                CssClass="btn-upload" OnClick="btnUpload_Click" />
        </div>

        <!-- DOCUMENTS LIST -->
        <div class="docs-list">
            <h3>Your Uploaded Documents</h3>

            <asp:Repeater ID="repDocuments" runat="server" OnItemCommand="repDocuments_ItemCommand">
                <HeaderTemplate>
                    <table class="docs-table">
                        <thead>
                            <tr>
                                <th>Type</th>
                                <th>File</th>
                                <th>Size</th>
                                <th>Uploaded</th>
                                <th>Status</th>
                                <th></th>
                            </tr>
                        </thead>
                        <tbody>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td><b><%# Eval("DocumentType") %></b></td>
                        <td><%# Eval("FileName") %></td>
                        <td><%# Eval("FileSizeKB") %> KB</td>
                        <td><%# Eval("UploadedOnText") %></td>
                        <td>
                            <span class='chip <%# Eval("StatusClass") %>'>
                                <!-- Dynamic SVG based on Status -->
                                <%# Eval("StatusClass").ToString() == "approved" 
                                    ? "<svg viewBox='0 0 24 24'><polyline points='20 6 9 17 4 12'></polyline></svg>" 
                                    : (Eval("StatusClass").ToString() == "rejected" 
                                        ? "<svg viewBox='0 0 24 24'><line x1='18' y1='6' x2='6' y2='18'></line><line x1='6' y1='6' x2='18' y2='18'></line></svg>" 
                                        : "<svg viewBox='0 0 24 24'><circle cx='12' cy='12' r='10'></circle><polyline points='12 6 12 12 16 14'></polyline></svg>") 
                                %>
                                <%# Eval("Status") %>
                            </span>
                        </td>
                        <td>
                            <asp:LinkButton runat="server" CssClass="btn-remove"
                                CommandName="Delete" CommandArgument='<%# Eval("DocumentID") %>'
                                OnClientClick="return confirm('Delete this document?');">
                                <svg viewBox="0 0 24 24"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                Remove
                            </asp:LinkButton>
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate></tbody></table></FooterTemplate>
            </asp:Repeater>

            <asp:Literal ID="litEmpty" runat="server" Visible="false"
                Text="<div style='text-align:center;color:#7787a5;padding:20px;'>No documents uploaded yet.</div>" />
        </div>
    </div>

</asp:Content>