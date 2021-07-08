using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using MarineBot.Interfaces;

namespace MarineBot.Database
{
    internal static class LogTable
    {
        private static DBGateway database;

        public static void Initialize(string ConnectionString)
        {
            database = new DBGateway(ConnectionString);
        }

        public static async Task LogActivityInfo(int activityId, int userId, string action)
        {
            var query = "INSERT INTO `activity_log` (`user_id`, `timestamp`, `activity_id`, `action`) " +
                        "VALUES                     (@P0, @P1, @P2, @P3);";

            var result = await database.ExecuteNonQuery(query,
                userId, DateTimeOffset.Now.ToUnixTimeSeconds(), activityId, action);
        }

        public static async Task LogCommandInfo(CommandContext ctx)
        {
            var query = "INSERT INTO `logs` (`discord_user_id`, `username`, `discord_channel_id`, `command`, `timestamp`) " +
                        "VALUES             (@P0, @P1, @P2, @P3, @P4);";

            var result = await database.ExecuteNonQuery(query,
                ctx.User.Id, ctx.User.Username, ctx.Channel.Id, ctx.Message.Content, DateTimeOffset.Now.ToUnixTimeSeconds());
        }
    }
}