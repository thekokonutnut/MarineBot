using MarineBot.Entities;
using MarineBot.Helpers;
using DSharpPlus.CommandsNext;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MarineBot.Controller;
using MarineBot.Interfaces;

namespace MarineBot.Database
{
    internal class ReminderDatabase : IDatabase
    {
        DatabaseController _dbcontroller;

        

        public ReminderDatabase(DatabaseController databaseController)
        {
            _dbcontroller = databaseController;
        }

        public async Task<bool> ReminderExists(string name)
        {
            using (var conn = new MySqlConnection(_dbcontroller.ConnectionString))
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

        public async Task RemoveReminder(string name, CommandContext ctx = null)
        {
            using (var conn = new MySqlConnection(_dbcontroller.ConnectionString))
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
                        if (ctx != null)
                            await MessageHelper.SendErrorEmbed(ctx, e.Message);
                        throw;
                    }
                }
            }
        }

        public async Task AddReminder(Reminder reminder, CommandContext ctx = null)
        {
            using (var conn = new MySqlConnection(_dbcontroller.ConnectionString))
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
                        if (ctx != null)
                            await MessageHelper.SendErrorEmbed(ctx, e.Message);
                        throw;
                    }
                }
            }
        }

        public async Task TestConnection()
        {
            try
            {
                using (var conn = new MySqlConnection(_dbcontroller.ConnectionString))
                {
                    await conn.OpenAsync();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        public async Task<List<Reminder>> GetReminders(CommandContext ctx = null)
        {
            var remindersList = new List<Reminder>();
            using (var conn = new MySqlConnection(_dbcontroller.ConnectionString))
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
                        if (ctx != null)
                            await MessageHelper.SendErrorEmbed(ctx, e.Message);
                        throw;
                    }
            }

            return remindersList;
        }
        public async Task CreateTableIfNull()
        {
            using (var conn = new MySqlConnection(_dbcontroller.ConnectionString))
            {
                await conn.OpenAsync();

                try
                {
                    using (var cmd = new MySqlCommand("SELECT * FROM reminders", conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        Console.WriteLine("[Database] Table Reminder exists.");
                        return;
                    }
                }
                catch (MySqlException e)
                {
                    if (e.Number == 1146)
                        Console.WriteLine("[Database] Table Reminder doesn't exists. Creating it.");
                    else
                    {
                        Console.WriteLine(e.ToString());
                        throw;
                    }
                }

                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText =
                    $@"CREATE TABLE `doombot`.`reminders` (
	                    `Name` VARCHAR(32) NOT NULL
	                    ,`Description` VARCHAR(128) NOT NULL
	                    ,`Time` VARCHAR(5) NOT NULL
                        ,`Guild` BIGINT (12) UNSIGNED NOT NULL
	                    ,`Channel` BIGINT (12) UNSIGNED NOT NULL
	                    ,PRIMARY KEY (`Name`)
                    ) ENGINE = InnoDB;";
                    try
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        throw;
                    }
                }
            }
        }
    }
}
