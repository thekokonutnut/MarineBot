using DSharpPlus.Entities;
using MarineBot.Entities;
using MarineBot.Interfaces;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarineBot.Database
{
    internal class ActivityTable : ITable
    {
        private string _connectionString;
        private List<ActivityEntry> _activities;

        public ActivityTable(string ConnectionString)
        {
            _connectionString = ConnectionString;
        }

        public async Task CreateTableIfNull()
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                try
                {
                    using (var cmd = new MySqlCommand("SELECT * FROM activities", conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        Console.WriteLine("[Database] Table Activity exists.");
                        return;
                    }
                }
                catch (MySqlException e)
                {
                    if (e.Number == 1146)
                        Console.WriteLine("[Database] Table Activity doesn't exist. Creating it.");
                    else
                    {
                        Console.WriteLine(e.ToString());
                    }
                }

                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText =
                    $@"CREATE TABLE `activities` (
                          `ID` int(16) NOT NULL AUTO_INCREMENT,
                          `AddedBy` bigint(12) NOT NULL,
                          `ActivityType` int(1) NOT NULL,
                          `ActivityText` varchar(128) NOT NULL,
                          PRIMARY KEY (`ID`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
                    try
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }
        }

        public async Task LoadTable()
        {
            await GetActivitiesDB();
        }

        public async Task SaveChanges()
        {
            var dbActs = await GetActivitiesDB();

            var localChangesAdditions = _activities.Where(p => !dbActs.Any(l => p.ID == l.ID)).ToList();
            var localChangesDeletions = dbActs.Where(p => !_activities.Any(l => p.ID == l.ID)).Select(e => e.ID).ToList();

            await AddActivitiesDB(localChangesAdditions);
            await RemoveActivitiesDB(localChangesDeletions);
        }

        private int GetNextID()
        {
            return _activities.Count == 0 ? 0 : _activities.Max(p => p.ID) + 1;
        }
        public bool EntryExists(int id)
        {
            return _activities.Any(p => p.ID == id);
        }
        public List<ActivityEntry> GetEntries()
        {
            return _activities;
        }
        public int CreateEntry(ActivityEntry entry)
        {
            entry.ID = GetNextID();
            _activities.Add(entry);

            return entry.ID;
        }
        public bool RemoveEntry(int id)
        {
            if (!EntryExists(id))
                return false;
            _activities.RemoveAt(_activities.FindIndex(p => p.ID == id));
            return true;
        }

        public async Task<List<ActivityEntry>> GetActivitiesDB(bool updateLocal = false)
        {
            var activitiesList = new List<ActivityEntry>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new MySqlCommand("SELECT * FROM activities", conn))
                {
                    try
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var activity = new DiscordActivity(reader["ActivityText"].ToString(), (ActivityType)Convert.ToInt32(reader["ActivityType"]));
                                var entry = new ActivityEntry(Convert.ToInt32(reader["ID"]), Convert.ToUInt64(reader["AddedBy"]), activity);

                                activitiesList.Add(entry);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }

            if (updateLocal)
                _activities = activitiesList;

            return activitiesList;
        }

        public async Task AddActivitiesDB(List<ActivityEntry> activities)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                foreach (var activity in activities)
                {
                    using (var cmd = new MySqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "INSERT INTO `activities` (`ID`, `AddedBy`, `ActivityType`, `ActivityText`) " +
                                          "VALUES                   (@ID , @AddedBy , @ActivityType , @ActivityText);";

                        cmd.Parameters.AddWithValue("ID", activity.ID);
                        cmd.Parameters.AddWithValue("AddedBy", activity.AddedBy);
                        cmd.Parameters.AddWithValue("ActivityType", activity.Activity.ActivityType);
                        cmd.Parameters.AddWithValue("ActivityText", activity.Activity.Name);

                        try
                        {
                            await cmd.ExecuteNonQueryAsync();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                    }
                }
            }
        }

        public async Task RemoveActivity(int id)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "DELETE FROM `activities` WHERE `ID` = @ID";
                    cmd.Parameters.AddWithValue("ID", id);

                    try
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }
        }

        public async Task RemoveActivitiesDB(List<int> ids)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                foreach (var id in ids)
                {
                    using (var cmd = new MySqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "DELETE FROM `activities` WHERE `ID` = @ID";
                        cmd.Parameters.AddWithValue("ID", id);

                        try
                        {
                            await cmd.ExecuteNonQueryAsync();

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                    }
                }
            }
        }
    }
}

