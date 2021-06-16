using MarineBot.Entities;
using MarineBot.Helpers;
using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarineBot.Threads;
using MySqlConnector;
using MarineBot.Interfaces;
using System.Linq;

namespace MarineBot.Database
{
    internal class ReminderTable : ITable
    {
        private string _connectionString;
        private List<Reminder> _reminders;

        public ReminderTable(string ConnectionString)
        {
            _connectionString = ConnectionString;
        }

        public string TableName() => "reminders";

        public async Task LoadTable()
        {
            _reminders = await GetRemindersDB();
        }
        public async Task SaveChanges()
        {
            var dbReminders = await GetRemindersDB();
            var localChangesAdditions = _reminders.Where(p => !dbReminders.Any(l => p.Name == l.Name)).ToList();
            var localChangesDeletions = dbReminders.Where(p => !_reminders.Any(l => p.Name == l.Name)).Select(e => e.Name).ToList();
            await AddReminderDB(localChangesAdditions);
            await RemoveReminderDB(localChangesDeletions);
        }

        public bool ReminderExists(string name)
        {
            return _reminders.Any(p => p.Name == name);
        }
        public List<Reminder> GetReminders()
        {
            return _reminders;
        }
        public void AddReminder(Reminder reminder)
        {
            _reminders.Add(reminder);
        }
        public bool RemoveReminder(string name)
        {
            if (!ReminderExists(name))
                return false;
            _reminders.RemoveAt(_reminders.FindIndex(p => p.Name == name));
            return true;
        }

        public async Task<bool> ReminderExistsDB(string name)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT * FROM `reminders` WHERE `Name` = @name";
                    cmd.Parameters.AddWithValue("name", name);

                    using (var reader = cmd.ExecuteReaderAsync())
                        if (reader.Result.HasRows)
                            return true;
                        else
                            return false;
                }
            }
        }
        public async Task<List<Reminder>> GetRemindersDB()
        {
            var remindersList = new List<Reminder>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new MySqlCommand("SELECT * FROM reminders", conn))
                    try
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                            while (await reader.ReadAsync())
                            {
                                var remind = new Reminder(reader["Name"].ToString(), reader["Description"].ToString(),
                                                          reader["Time"].ToString(), Convert.ToUInt64(reader["Guild"]),
                                                          Convert.ToUInt64(reader["Channel"]));
                                remindersList.Add(remind);
                            }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
            }

            return remindersList;
        }
        public async Task AddReminderDB(Reminder reminder)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO `reminders` (`Name`, `Description`, `Time`, `Guild`, `Channel`) VALUES (@name, @desc, @time, @guild, @channel);";
                    cmd.Parameters.AddWithValue("name", reminder.Name);
                    cmd.Parameters.AddWithValue("desc", reminder.Description);
                    cmd.Parameters.AddWithValue("time", reminder.Time);
                    cmd.Parameters.AddWithValue("guild", reminder.Guild);
                    cmd.Parameters.AddWithValue("channel", reminder.Channel);

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
        public async Task AddReminderDB(List<Reminder> reminders)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                foreach (var reminder in reminders)
                {
                    using (var cmd = new MySqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "INSERT INTO `reminders` (`Name`, `Description`, `Time`, `Guild`, `Channel`) VALUES (@name, @desc, @time, @guild, @channel);";
                        cmd.Parameters.AddWithValue("name", reminder.Name);
                        cmd.Parameters.AddWithValue("desc", reminder.Description);
                        cmd.Parameters.AddWithValue("time", reminder.Time);
                        cmd.Parameters.AddWithValue("guild", reminder.Guild);
                        cmd.Parameters.AddWithValue("channel", reminder.Channel);

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
        public async Task RemoveReminderDB(string name)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "DELETE FROM `reminders` WHERE `Name` = @name";
                    cmd.Parameters.AddWithValue("name", name);

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
        public async Task RemoveReminderDB(List<string> names)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                foreach (var name in names)
                {
                    using (var cmd = new MySqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "DELETE FROM `reminders` WHERE `Name` = @name";
                        cmd.Parameters.AddWithValue("name", name);

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
