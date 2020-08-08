﻿using MarineBot.Entities;
using MarineBot.Interfaces;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarineBot.Database
{
    internal class PollTable : ITable
    {
        private string _connectionString; 
        private List<Poll> _polls;

        public PollTable(string ConnectionString)
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
                    using (var cmd = new MySqlCommand("SELECT * FROM polls", conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        Console.WriteLine("[Database] Table Polls exists.");
                        return;
                    }
                }
                catch (MySqlException e)
                {
                    if (e.Number == 1146)
                        Console.WriteLine("[Database] Table Polls doesn't exist. Creating it.");
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
                    $@"CREATE TABLE `doombot`.`polls` ( 
	                    `ID` INT(10) NOT NULL 
	                    , `Title` VARCHAR(256) NOT NULL 
	                    , `Options` VARCHAR(512) NOT NULL 
	                    , `Time` INT(10) NOT NULL 
                        , `StartTime` BIGINT(12) NOT NULL 
	                    , `MessageID` BIGINT(12) NOT NULL 
	                    , `Guild` BIGINT(12) NOT NULL 
	                    , `Channel` BIGINT(12) NOT NULL 
	                    , PRIMARY KEY (`ID`)
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
        public async Task LoadTable()
        {
            _polls = await GetPollsDB();
        }
        public async Task SaveChanges()
        {
            var dbPolls = await GetPollsDB();
            var localChangesAdditions = _polls.Where(p => !dbPolls.Any(l => p.ID == l.ID)).ToList();
            var localChangesDeletions = dbPolls.Where(p => !_polls.Any(l => p.ID == l.ID)).Select(e => e.ID).ToList();
            await AddPollDB(localChangesAdditions);
            await RemovePollDB(localChangesDeletions);
        }

        private uint GetNextID()
        {
            return _polls.Count == 0 ? 0 : _polls.Max(p => p.ID) + 1;
        }
        public bool PollExists(uint id)
        {
            return _polls.Any(p => p.ID == id);
        }
        public List<Poll> GetPolls()
        {
            return _polls;
        }
        public void CreatePoll(Poll poll)
        {
            poll.ID = GetNextID();
            _polls.Add(poll);
        }
        public bool RemovePoll(uint id)
        {
            if (!PollExists(id))
                return false;
            _polls.RemoveAt(_polls.FindIndex(p => p.ID == id));
            return true;
        }

        public List<string> DeserializeOptions(string options)
        {
            string[] _splitoptions = options.Split(",");
            return new List<string>(_splitoptions);
        }
        private string SerializeOptions(List<string> options)
        {
            return string.Join(",", options.ToArray());
        }
        public async Task<List<Poll>> GetPollsDB()
        {
            var pollsList = new List<Poll>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new MySqlCommand("SELECT * FROM polls", conn))
                {
                    try
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var poll = new Poll(Convert.ToUInt64(reader["MessageID"]),  Convert.ToUInt64(reader["Guild"]), 
                                                    Convert.ToUInt64(reader["Channel"]),    reader["Title"].ToString(), 
                                                    Convert.ToUInt32(reader["Time"]),       Convert.ToInt64(reader["StartTime"]),
                                                    DeserializeOptions(reader["Options"].ToString()));
                                poll.ID = Convert.ToUInt32(reader["ID"]);

                                pollsList.Add(poll);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        throw;
                    }
                }
            }

            return pollsList;
        }
        public async Task AddPollDB(List<Poll> polls)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                foreach (var poll in polls)
                {
                    using (var cmd = new MySqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "INSERT INTO `polls` (`ID`, `MessageID`, `Guild`, `Channel`, `Title`, `Time`, `StartTime`, `Options`) " +
                                          "VALUES              (@ID , @MessageID , @Guild , @Channel , @Title , @Time , @StartTime , @Options );";

                        cmd.Parameters.AddWithValue("ID", poll.ID);
                        cmd.Parameters.AddWithValue("MessageID", poll.MessageID);
                        cmd.Parameters.AddWithValue("Guild", poll.Guild);
                        cmd.Parameters.AddWithValue("Channel", poll.ChannelID);
                        cmd.Parameters.AddWithValue("Title", poll.Title);
                        cmd.Parameters.AddWithValue("Time", poll.Time);
                        cmd.Parameters.AddWithValue("StartTime", poll.StartTime);
                        cmd.Parameters.AddWithValue("Options", SerializeOptions(poll.Options));

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
        public async Task RemovePollDB(List<uint> ids)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                foreach (var id in ids)
                {
                    using (var cmd = new MySqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "DELETE FROM `polls` WHERE `ID` = @ID";
                        cmd.Parameters.AddWithValue("ID", id);

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
}
