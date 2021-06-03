using DSharpPlus.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MarineBot.Helpers
{
    internal class OAuth2Helper
    {
        static string API_ENDPOINT;
        static string CLIENT_ID;
        static string CLIENT_SECRET;
        static string REDIRECT_URI;

        private static readonly HttpClient client = new HttpClient();

        static OAuth2Helper()
        {
            ReloadConfig();
        }
        public static void ReloadConfig()
        {
            var config = Config.LoadFromFile("config.json");

            API_ENDPOINT = config.oauth2Config.API_Endpoint;
            CLIENT_ID = config.oauth2Config.Client_ID;
            CLIENT_SECRET = config.oauth2Config.Client_Secret;
            REDIRECT_URI = config.oauth2Config.Redirect_URI;
        }

        public static async Task<OAuth2UserInfo> RequestUserInfo(string oauth2token)
        {
            using (var requestMessage =
                new HttpRequestMessage(HttpMethod.Get, $"{API_ENDPOINT}/users/@me"))
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", oauth2token);

                var response = await client.SendAsync(requestMessage);
                var text = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    try
                    {
                        dynamic json = JsonConvert.DeserializeObject(text);
                        throw new Exception($"Info request returned: {json["error_description"]}");
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Info request returned {response.StatusCode}");
                    }
                }

                var oauthresponse = JsonConvert.DeserializeObject<OAuth2UserInfo>(text);
                return oauthresponse;
            }
        }

        public static async Task<OAuth2Response> RequestAccessToken(string code)
        {
            var values = new Dictionary<string, string>
            {
                { "client_id", CLIENT_ID},
                { "client_secret", CLIENT_SECRET},
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", REDIRECT_URI }
            };

            var content = new FormUrlEncodedContent(values);

            using (var requestMessage =
                new HttpRequestMessage(HttpMethod.Post, $"{API_ENDPOINT}/oauth2/token"))
            {
                requestMessage.Content = content;
                requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                var response = await client.SendAsync(requestMessage);
                var text = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    try
                    {
                        dynamic json = JsonConvert.DeserializeObject(text);
                        throw new Exception($"Token request returned: {json["error_description"]}");
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Token request returned {response.StatusCode}");
                    }
                }

                var oauthresponse = JsonConvert.DeserializeObject<OAuth2Response>(text);
                oauthresponse.Requested = DateTime.Now;

                return oauthresponse;
            }
        }
    }

    internal class OAuth2Response
    {
        [JsonProperty("access_token")]
        public string Token = "";

        [JsonProperty("expires_in")]
        public int Lifespan = 0;

        [JsonProperty("refresh_token")]
        public string RefreshToken  = "";

        [JsonProperty("scope")]
        public string Scope = "";

        [JsonProperty("token_type")]
        public string TokenType = "";

        public DateTime Requested;
    }

    internal class OAuth2UserInfo
    {
        [JsonProperty("id")]
        public ulong ID;

        [JsonProperty("username")]
        public string Username;

        [JsonProperty("avatar")]
        public string AvatarHash;

        [JsonProperty("discriminator")]
        public string Discriminator;
    }
}
