
using log4net;

using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Server.Network;
using ACE.Server.WorldObjects;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using MySqlX.XDevAPI.Relational;

namespace ACE.Server.Command.Handlers
{
    internal static class CommandHandlerHelper
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// This will determine where a command handler should output to, the console or a client session.<para />
        /// If the session is null, the output will be sent to the console. If the session is not null, and the session.Player is in the world, it will be sent to the session.<para />
        /// Messages sent to the console will be sent using log.Info()
        /// </summary>
        public static void WriteOutputInfo(Session session, string output, ChatMessageType chatMessageType = ChatMessageType.Broadcast)
        {
            if (session != null)
            {
                if (session.State == Network.Enum.SessionState.WorldConnected && session.Player != null)
                    ChatPacket.SendServerMessage(session, output, chatMessageType);
            }
            else
                log.Info(output);
        }

        /// <summary>
        /// This will determine where a command handler should output to, the console or a client session.<para />
        /// If the session is null, the output will be sent to the console. If the session is not null, and the session.Player is in the world, it will be sent to the session.<para />
        /// Messages sent to the console will be sent using log.Debug()
        /// </summary>
        public static void WriteOutputDebug(Session session, string output, ChatMessageType chatMessageType = ChatMessageType.Broadcast)
        {
            if (session != null)
            {
                if (session.State == Network.Enum.SessionState.WorldConnected && session.Player != null)
                    ChatPacket.SendServerMessage(session, output, chatMessageType);
            }
            else
                log.Debug(output);
        }

        /// <summary>
        /// Returns the last appraised WorldObject
        /// </summary>
        public static WorldObject GetLastAppraisedObject(Session session)
        {
            var targetID = session.Player.RequestedAppraisalTarget;
            if (targetID == null)
            {
                WriteOutputInfo(session, "GetLastAppraisedObject() - no appraisal target");
                return null;
            }

            var target = session.Player.FindObject(targetID.Value, Player.SearchLocations.Everywhere, out _, out _, out _);
            if (target == null)
            {
                WriteOutputInfo(session, $"GetLastAppraisedObject() - couldn't find {targetID:X8}");
                return null;
            }
            return target;
        }

        /// <summary>
        /// This will split a string by \n for rows and | for columns and then pretty print as a text table
        /// </summary>
        public static void PrettyPrint(Session session, string message, bool hasHeader = true, ChatMessageType chatMessageType = ChatMessageType.Broadcast)
        {
            var parsed = new List<List<string>>();

            message = message.Trim().TrimEnd('\n');

            var rows = message.Split('\n');
            foreach(var r in rows)
            {
                var newRow = new List<string>();
                newRow.AddRange(r.Split('|'));
                parsed.Add(newRow);
            }

            PrettyPrint(session, parsed, hasHeader, chatMessageType);
        }

        /// <summary>
        /// This will attempt to format a list of a list of strings into a text table.
        /// TODO: Refactor for less ugliness
        /// </summary>
        public static void PrettyPrint(Session session, List<List<string>> table, bool hasHeader = true, ChatMessageType chatMessageType = ChatMessageType.Broadcast)
        {
            if (table == null || table.Count == 0 || table[0] == null || table[0].Count == 0)
            {
                WriteOutputInfo(session, "", chatMessageType);
                return;
            }

            if (table[0].Count == 1)
            {
                WriteOutputInfo(session, table[0][0], chatMessageType);
                return;
            }

            var pretty = "Generating who list...\n\n";
            var colCount = table.Max(x => x.Count);
            var colWidths = Enumerable.Range(0, colCount)
                .Select(i => table.Max(arr => (arr.ElementAtOrDefault(i) ?? "").Length))
                .ToList();

            var sep = "+".PadRight(colWidths.Sum(x => x) + colCount, '-') + "+\n";

            pretty += sep;

            table.ForEach(row =>
            {
                var line = "|";
                for (var x = 0; x < colCount; x++)
                {
                    var cell = "";
                    if (x < row.Count)
                    {
                        cell = row[x];
                    }
                    line += $"{cell}".PadRight(colWidths[x], ' ') + "|";
                }
                pretty += line + "\n";

                if (hasHeader && table.Count > 1 && pretty.Count(x => x == '\n') == 2)
                {
                    pretty += sep + "\n";
                }
            });

            pretty += sep + "\n\n\n";

            WriteOutputInfo(session, pretty, chatMessageType);
        }
    }
}
