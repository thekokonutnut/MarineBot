using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarineBot.Entities
{
    internal class AuthToken
    {
        public int ID;
        public AuthUser User;
        public string AccessToken;
        public string SessionCode;
        public DateTime Retrieved;
        public DateTime ExpiresAt;
        public string RefreshToken;
    }
}
