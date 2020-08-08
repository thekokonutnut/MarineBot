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
        internal string[] dudaAnswers = {"si", "no", "perhaps"};

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
}
