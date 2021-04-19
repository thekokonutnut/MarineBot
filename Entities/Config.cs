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
        internal DatabaseConfig _databaseConfig = new DatabaseConfig();

        [JsonProperty("imgur_clientid")]
        internal string imgurClientID = "";

        [JsonProperty("duda_answers")]
        internal string[] dudaAnswers = {"Yes", "No", "Maybe"};

        [JsonProperty("status_messages")]
        internal string[] statusMessages = { "Gutting demons", "Cleaning hell", "Freeing the Earth" };

        [JsonProperty("administrators")]
        internal string[] administratorsList = { "<insert userid>" };

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
        [JsonProperty("user")]
        internal string User = "";
        [JsonProperty("password")]
        internal string Password = "";
        [JsonProperty("database")]
        internal string Database = "";
    }
}
