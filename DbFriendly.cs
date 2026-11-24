using System;
using System.Data.SqlClient;

namespace WebApplicationM3
{
    /// <summary>
    /// Central place to turn a raw exception into a user-friendly message
    /// while logging the technical detail for developers.
    ///
    ///   catch (Exception ex) { ShowMessage(DbFriendly.Message(ex, "saving booking")); }
    ///
    /// Rule: never write ex.Message to a page.
    /// </summary>
    public static class DbFriendly
    {
        public const string GenericMessage =
            "Something went wrong on our side. Please try again in a moment, or contact " +
            "reception on +27 83 544 8544.";

        public const string DbUnreachableMessage =
            "We can't reach our database right now. Please try again in a moment. " +
            "If this keeps happening, contact reception on +27 83 544 8544.";

        public const string DuplicateMessage =
            "That record already exists. Please check and try again.";

        public const string TimeoutMessage =
            "Our system is taking longer than usual. Please try again in a moment.";

        public static string Message(Exception ex, string context)
        {
            try
            {
                System.Diagnostics.Trace.TraceError(
                    "DbFriendly [" + (context ?? "unknown") + "]: " + ex);
            }
            catch { }

            if (ex == null) return GenericMessage;

            var sql = ex as SqlException;
            if (sql != null)
            {
                if (sql.Class >= 20) return DbUnreachableMessage;

                switch (sql.Number)
                {
                    case -2:
                    case 1222:
                        return TimeoutMessage;

                    case 2601:
                    case 2627:
                        return DuplicateMessage;

                    case 4060:
                    case 18456:
                    case 53:
                    case 40:
                        return DbUnreachableMessage;

                    case 547:
                        return "That action can't be completed because of a related record. " +
                               "Please check the details and try again.";

                    case 1205:
                        return "The system was busy for a moment. Please try again.";

                    case 8152:
                        return "One of the fields you entered is too long. Please shorten it and try again.";

                    case 242:
                        return "One of the dates you entered isn't valid. Please check and try again.";
                }

                return GenericMessage;
            }

            if (ex is System.Configuration.ConfigurationErrorsException)
                return GenericMessage;

            if (ex is System.Net.WebException || ex is System.Net.Sockets.SocketException)
                return DbUnreachableMessage;

            return GenericMessage;
        }
    }
}