using MarineBot.Controller;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MarineBot.Controller
{
    internal class DatabaseController
    {
        private DatabaseConfig _config;
        public string ConnectionString { get; private set; } = "";
        public DatabaseController(DatabaseConfig config)
        {
            _config = config;
            ConnectionString = $"Server={_config.Server};User ID={_config.User};Password={_config.Password};Database={_config.Database}";
        }
    }
}
