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

        private HashSet<int> updatedEntries;

        public ActivityTable(string ConnectionString)
        {
            _connectionString = ConnectionString;

            updatedEntries = new HashSet<int>();
        }

        public string TableName() => "activities";

        public async Task LoadTable()
        {
            await GetActivitiesDB(true);
        }

        public async Task SaveChanges()
        {
            var dbActs = await GetActivitiesDB();

            // add entries that are in the local list but not in the database

            var localChangesAdditions = _activities.Where(p => !dbActs.Any(l => p.ID == l.ID)).ToList();
            await AddActivitiesDB(localChangesAdditions);

            // remove entries that are in the database but not in the local list

            var localChangesDeletions = dbActs.Where(p => !_activities.Any(l => p.ID == l.ID)).Select(e => e.ID).ToList();
            await RemoveActivitiesDB(localChangesDeletions);

            //update edited entries

            await UpdateActivitiesDB(updatedEntries);
            updatedEntries.Clear();
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
            int index = _activities.FindIndex(p => p.ID == id);
            _activities.RemoveAt(index);
            return true;
        }
        public bool UpdateEntry(int id, ActivityEntry newEntry)
        {
            if (!EntryExists(id))
                return false;
            int index = _activities.FindIndex(p => p.ID == id);
            _activities[index] = newEntry;

            updatedEntries.Add(id);

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
                                var activity = new DiscordActivity(reader["status"].ToString(), (ActivityType)Convert.ToInt32(reader["type"]));
                                var entry = new ActivityEntry(Convert.ToInt32(reader["activity_id"]), Convert.ToUInt64(reader["user_id"]), activity);

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

        public async Task AddActivitiesDB(IEnumerable<ActivityEntry> activities)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                foreach (var activity in activities)
                {
                    using (var cmd = new MySqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "INSERT INTO `activities` (`activity_id`, `user_id`, `type`, `status`) " +
                                          "VALUES                   (@ID , @User , @Type , @Text);";

                        cmd.Parameters.AddWithValue("ID", activity.ID);
                        cmd.Parameters.AddWithValue("User", activity.AddedBy);
                        cmd.Parameters.AddWithValue("Type", activity.Activity.ActivityType);
                        cmd.Parameters.AddWithValue("Text", activity.Activity.Name);

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
                    cmd.CommandText = "DELETE FROM `activities` WHERE `activity_id` = @ID";
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

        public async Task RemoveActivitiesDB(IEnumerable<int> ids)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                foreach (var id in ids)
                {
                    using (var cmd = new MySqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "DELETE FROM `activities` WHERE `activity_id` = @ID";
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

        public async Task UpdateActivitiesDB(IEnumerable<int> ids)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                foreach (var id in ids)
                {
                    if (!EntryExists(id))
                        continue;
                    int index = _activities.FindIndex(p => p.ID == id);
                    var activity = _activities[index];

                    using (var cmd = new MySqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "UPDATE `activities` SET `type` = @Type, `status` = @Text " +
                                          "WHERE  `activity_id` = @ID;";

                        cmd.Parameters.AddWithValue("ID", activity.ID);
                        cmd.Parameters.AddWithValue("Type", activity.Activity.ActivityType);
                        cmd.Parameters.AddWithValue("Text", activity.Activity.Name);

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

