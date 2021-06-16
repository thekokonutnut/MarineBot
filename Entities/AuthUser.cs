using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarineBot.Entities
{
    internal class AuthUser
    {
        public int ID;
        public long DiscordID;
        public AuthToken Token;
        public string Username;
        public string Discriminator;
    }
}
