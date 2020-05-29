using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MarineBot.Controller
{
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
