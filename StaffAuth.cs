 using System;
using System.Web;
using System.Web.UI;

namespace WebApplicationM3
{
    // =========================================================
    // STAFF AUTH — role model matches the Admin_Staff table.
    //
    // Only three roles exist in your DB:
    //   • Receptionist — operations (bookings, students, payments)
    //   • Manager      — everything a Receptionist can do,
    //                    PLUS notify staff & view instructor dashboard
    //   • Instructor   — their own dashboard only
    //
    // "Admin" is not a real role — we treat Manager as the top
    // level for backwards compatibility with any code that
    // requests "Admin".
    // =========================================================
    public static class StaffAuth
    {
        public static string Email
        {
            get { return HttpContext.Current.Session["StaffEmail"] as string ?? ""; }
        }

        public static string Name
        {
            get { return HttpContext.Current.Session["StaffName"] as string ?? "Staff"; }
        }

        public static string Role
        {
            get { return HttpContext.Current.Session["StaffRole"] as string ?? ""; }
        }

        public static bool IsSignedIn
        {
            get { return !string.IsNullOrEmpty(Email); }
        }

        // =========================================================
        // True for roles with day-to-day operational access.
        // Used by Students / Payments / Audit / Bookings pages.
        // =========================================================
        public static bool IsOperations()
        {
            string r = Role;
            return r.Equals("Receptionist", StringComparison.OrdinalIgnoreCase) ||
                   r.Equals("Manager", StringComparison.OrdinalIgnoreCase) ||
                   r.Equals("Admin", StringComparison.OrdinalIgnoreCase); // legacy
        }

        // =========================================================
        // True only for Manager (a.k.a. Admin). Used for sensitive
        // actions: bulk staff emails, instructor view, etc.
        // =========================================================
        public static bool IsManagerOrHigher()
        {
            string r = Role;
            return r.Equals("Manager", StringComparison.OrdinalIgnoreCase) ||
                   r.Equals("Admin", StringComparison.OrdinalIgnoreCase); // legacy
        }


        public static string HomeFor(string role)
        {
            if (string.IsNullOrEmpty(role)) return "~/Account/Login.aspx";  // ← fixed
            if (role.Equals("Instructor", StringComparison.OrdinalIgnoreCase))
                return "~/InstructorDashboard.aspx";
            return "~/AdminDashboard.aspx";
        }

        public static string HomeFor()
        {
            return HomeFor(Role);
        }

        // =========================================================
        // Page access check.
        //
        //   CheckAccess("Admin")      → Receptionist, Manager (both)
        //   CheckAccess("Manager")    → Manager only
        //   CheckAccess("Instructor") → Instructor or Manager
        // =========================================================
        public static bool CheckAccess(string requiredRole)
        {
            var ctx = HttpContext.Current;
            var page = ctx.CurrentHandler as Page;


            if (!IsSignedIn)
            {
                string returnUrl = ctx.Request.RawUrl;
                ctx.Response.Redirect("~/Account/Login.aspx?return=" +
                    HttpUtility.UrlEncode(returnUrl), false);
                ctx.ApplicationInstance.CompleteRequest();
                return false;
            }

            if (string.IsNullOrEmpty(requiredRole))
                return true;

            string role = Role;

            // Manager passes every check
            if (role.Equals("Manager", StringComparison.OrdinalIgnoreCase))
                return true;

            // "Admin" area = "operations" area → Receptionist or Manager
            if (requiredRole.Equals("Admin", StringComparison.OrdinalIgnoreCase) &&
                (role.Equals("Receptionist", StringComparison.OrdinalIgnoreCase) ||
                 role.Equals("Manager", StringComparison.OrdinalIgnoreCase)))
                return true;

            // "Manager" area → Manager only (already handled above)
            if (requiredRole.Equals("Manager", StringComparison.OrdinalIgnoreCase))
                return false; // only Manager gets here, and Manager is handled above

            // "Instructor" area → Instructor or Manager
            if (requiredRole.Equals("Instructor", StringComparison.OrdinalIgnoreCase) &&
                role.Equals("Instructor", StringComparison.OrdinalIgnoreCase))
                return true;

            // Exact match (future-proof)
            if (role.Equals(requiredRole, StringComparison.OrdinalIgnoreCase))
                return true;

            // Denied
            if (page != null)
            {
                page.Response.StatusCode = 403;
                page.Response.Write(
                    "<html><body style='font-family:Segoe UI,Arial,sans-serif;" +
                    "padding:40px;background:#eef2fa;text-align:center;'>" +
                    "<h1 style='color:#e8283f;'>Access Denied</h1>" +
                    "<p>Your role (<b>" + HttpUtility.HtmlEncode(Role) +
                    "</b>) doesn't have permission to open this page.</p>" +
                    "<p><a href='Login.aspx' style='color:#0d2b6e;'>Sign in with another account</a></p>" +
                    "</body></html>");
                ctx.ApplicationInstance.CompleteRequest();
            }

            return false;
        }

        public static void SetStaffSession(string email, string fullName, string role)
        {
            var session = HttpContext.Current.Session;
            session["StaffEmail"] = (email ?? "").Trim().ToLowerInvariant();
            session["StaffName"] = (fullName ?? "").Trim();
            session["StaffRole"] = (role ?? "").Trim();
        }

        public static void SignOutStaff()
        {
            var session = HttpContext.Current.Session;
            session.Remove("StaffEmail");
            session.Remove("StaffName");
            session.Remove("StaffRole");
        }
    }
}