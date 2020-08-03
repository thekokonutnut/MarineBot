using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordDoomBot.Entities
{
    internal class Member
    {
        public uint UserID;
        public string Username;
        public int AdminLevel;

        public Member(string username, uint userid, int adminlevel)
        {
            Username = username;
            UserID = userid;
            AdminLevel = adminlevel;
        }
    }
}
