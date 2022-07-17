using MarineBot.Entities;
using MarineBot.Threads;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace MarineBot.Helpers
{
    internal class Config
    {
        [JsonProperty("token")]
        internal string Token = "";

        [JsonProperty("prefix")]
        internal string Prefix = "'";

        [JsonProperty("databaseconfig")]
        internal DatabaseConfig databaseConfig = new DatabaseConfig();

        [JsonProperty("facesEndpoint")]
        internal string facesEndpoint = "";

        [JsonProperty("imgur_clientid")]
        internal string imgurClientID = "";

        [JsonProperty("duda_answers")]
        internal string[] dudaAnswers = {"Yes", "No", "Maybe"};

        [JsonProperty("status_messages")]
        internal string[] statusMessages = { "Gutting demons", "Cleaning hell", "Freeing the Earth" };

        [JsonProperty("faces_config")]
        internal FacesConfig facesConfig = new FacesConfig();

        [JsonProperty("administrators")]
        internal string[] administratorsList = { "<insert userid>" };

        [JsonProperty("oauth2")]
        internal OAuth2Config oauth2Config = new OAuth2Config();
        
        [JsonProperty("enable_lavalink")]
        internal bool enableLavalink = true;

        [JsonProperty("saucenao_apikey")]
        internal string saucenaoApiKey = "";

        public static Config LoadFromFile(string path)
        {
            using (var sr = new StreamReader(path))
            {
                return JsonConvert.DeserializeObject<Config>(sr.ReadToEnd());
            }
        }

        public void SaveToFile(string path)
        {
            using (var sw = new StreamWriter(path))
            {
                sw.Write(JsonConvert.SerializeObject(this));
            }
        }
    }
    internal class DatabaseConfig
    {
        [JsonProperty("server")]
        internal string Server = "";
        [JsonProperty("port")]
        internal string Port = "3306";
        [JsonProperty("user")]
        internal string User = "";
        [JsonProperty("password")]
        internal string Password = "";
        [JsonProperty("database")]
        internal string Database = "";
    }

    internal class FacesConfig
    {
        [JsonProperty("idle")]
        internal string[] IdleFaces = { "Idle_0.png" };
        [JsonProperty("success")]
        internal string[] SuccessFaces = { "Happy_0.png" };
        [JsonProperty("warning")]
        internal string[] WarningFaces = { "Confused_0.png" };
        [JsonProperty("error")]
        internal string[] ErrorFaces = { "Sad_0.png" };
    }

    internal class OAuth2Config
    {
        [JsonProperty("api_endpoint")]
        internal string API_Endpoint = "https://discord.com/api/v8";
        [JsonProperty("client_id")]
        internal string Client_ID = "";
        [JsonProperty("client_secret")]
        internal string Client_Secret = "";
        [JsonProperty("redirect_uri")]
        internal string Redirect_URI = "http://localhost:17701/auth";
    }
}
