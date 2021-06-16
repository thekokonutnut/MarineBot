using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using MarineBot.Interfaces;
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
            ReloadConfig();
        }
        public static void ReloadConfig()
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
            return BotAdministrator(user.Id.ToString());
        }

        public static bool BotAdministrator(string id)
        {
            if (botAdministrators.Contains(id))
                return true;
            return false;
        }

        public static string[] GetAdministrators() => botAdministrators;
    }

    internal static class CryptoHelper
    {
        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = new UTF8Encoding().GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
