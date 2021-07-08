using MarineBot.Helpers;
using Newtonsoft.Json;
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
        public ulong DiscordID;
        public AuthToken Token;
        public string Username;
        public string Discriminator;
        public string AvatarHash;

        public AuthUser()
        {

        }
        public AuthUser(OAuth2UserInfo info)
        {
            DiscordID = info.ID;
            Username = info.Username;
            Discriminator = info.Discriminator;
            AvatarHash = info.AvatarHash;
        }
    }

    internal class AuthUserInfo
    {
        [JsonProperty("id")]
        public ulong ID = 0;

        [JsonProperty("username")]
        public string Username = "";

        [JsonProperty("avatar")]
        public string AvatarHash = "";

        [JsonProperty("discriminator")]
        public string Discriminator = "";

        public AuthUserInfo()
        {

        }

        public AuthUserInfo(AuthUser user)
        {
            ID = user.DiscordID;
            Username = user.Username;
            AvatarHash = user.AvatarHash;
            Discriminator = user.Discriminator;
        }
    }
}
