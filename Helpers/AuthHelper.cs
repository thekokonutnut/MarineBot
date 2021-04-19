using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarineBot.Helpers
{
    internal static class AuthHelper
    {
        static string[] botAdministrators;
        static AuthHelper()
        {
            var config = Config.LoadFromFile("config.json");

            botAdministrators = config.administratorsList;
        }

        public static bool BotOwner(CommandContext ctx)
        {
            var app = ctx.Client.CurrentApplication;
            var me = ctx.Client.CurrentUser;

            if (app != null)
                return app.Owners.Any(x => x.Id == ctx.User.Id);

            return ctx.User.Id == me.Id;
        }

        public static bool BotAdministrator(DiscordUser user)
        {
            if (botAdministrators.Contains(user.Id.ToString()))
                return true;
            return false;
        }
    }
}
