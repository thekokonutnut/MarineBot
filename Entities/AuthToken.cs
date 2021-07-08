using MarineBot.Helpers;
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
        public string AccessToken;
        public string SessionCode;
        public DateTimeOffset Retrieved;
        public DateTimeOffset ExpiresAt;
        public string RefreshToken;

        public AuthToken()
        {

        }
        public AuthToken(OAuth2Response response)
        {
            AccessToken = response.Token;
            SessionCode = CryptoHelper.CreateMD5(response.Token);
            Retrieved = response.Requested;
            ExpiresAt = response.Requested.AddSeconds(response.Lifespan);
            RefreshToken = response.RefreshToken;
        }
    }
}
