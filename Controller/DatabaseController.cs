using MarineBot.Threads;
using MarineBot.Database;
using MarineBot.Entities;
using MarineBot.Interfaces;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MarineBot.Helpers;

namespace MarineBot.Controller
{
    internal class DatabaseController
    {
        private DatabaseConfig _config;
        private string ConnectionString = "";
        private List<ITable> databaseTables;

        public DatabaseController(DatabaseConfig config)
        {
            _config = config;
            ConnectionString = $"Server={_config.Server};Port={_config.Port};User ID={_config.User};Password={_config.Password};Database={_config.Database}";

            databaseTables = new List<ITable>();
            databaseTables.Add(new ReminderTable(ConnectionString));
            databaseTables.Add(new PollTable(ConnectionString));
            databaseTables.Add(new ActivityTable(ConnectionString));
            databaseTables.Add(new SmugresponsesTable(ConnectionString));
            databaseTables.Add(new UserTable(ConnectionString));

            LogTable.Initialize(ConnectionString);
        }

        public T GetTable<T>()
        {
            return databaseTables.OfType<T>().FirstOrDefault();
        }

        public bool TestConnection()
        {
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[System] Database connection failed.");
                Console.WriteLine(e.ToString());
                return false;
            }
            Console.WriteLine("[System] Connection to the database was successful.");
            return true;
        }

        public async Task LoadEverything()
        {
            Console.WriteLine("[System] Loading data from database.");
            foreach (var table in databaseTables)
            {
                await table.LoadTable();
            }
        }
    }
}
