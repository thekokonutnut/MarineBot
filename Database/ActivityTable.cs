﻿using DSharpPlus.Entities;
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
        private List<ActivityEntry> _activities;

        private DBGateway database;

        public ActivityTable(string ConnectionString)
        {
            database = new DBGateway(ConnectionString);
        }

        public string TableName() => "activities";

        public async Task LoadTable()
        {
            await GetActivitiesDB(true);
        }

        public IEnumerable<ActivityEntry> GetEntries()
        {
            return _activities;
        }

        public async Task<IEnumerable<ActivityEntry>> GetActivitiesDB(bool updateLocal = false)
        {
            var activitiesList = new List<ActivityEntry>();

            var query = "SELECT * FROM activities";
            var result = await database.ExecuteReader(query);
            var rows = result.Rows;

            foreach (var row in rows)
            {
                var columns = row.GetColumns();

                var entry = new ActivityEntry()
                {
                    ID = Convert.ToInt32(columns["activity_id"]),
                    UserID = Convert.ToInt32(columns["user_id"]),
                    Type = (ActivityType)Convert.ToInt32(columns["type"]),
                    Status = columns["status"].ToString()
                };

                activitiesList.Add(entry);
            }

            if (updateLocal)
                _activities = activitiesList;

            return activitiesList;
        }

        public async Task<int> AddActivity(ActivityEntry activity)
        {
            var query = "INSERT INTO `activities` (`user_id`, `type`, `status`) " +
                        "VALUES                   (@P0, @P1, @P2);";

            var result = await database.ExecuteNonQuery(query,
                activity.UserID, activity.Type, activity.Status);

            return (int)result.Command.LastInsertedId;
        }

        public async Task AddActivitiesDB(IEnumerable<ActivityEntry> activities)
        {
            foreach (var activity in activities)
            {
                await AddActivity(activity);
            }
        }

        public async Task RemoveActivity(int id)
        {
            var query = "DELETE FROM `activities` WHERE `activity_id` = @P0";
            await database.ExecuteNonQuery(query, id);
        }

        public async Task RemoveActivitiesDB(IEnumerable<int> ids)
        {
            foreach (var id in ids)
            {
                await RemoveActivity(id);
            }
        }

        public async Task UpdateActivity(int id, ActivityEntry newEntry)
        {
            var query = "UPDATE `activities` SET `type` = @P0, `status` = @P1 " +
                        "WHERE  `activity_id` = @P2;";

            await database.ExecuteNonQuery(query,
                newEntry.Type, newEntry.Status, id);
        }
    }
}

