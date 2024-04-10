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
    internal class SmugresponsesTable : ITable
    {
        private List<SmugresponseEntity> _smugresponses;

        private DBGateway database;

        public SmugresponsesTable(string ConnectionString)
        {
            database = new DBGateway(ConnectionString);
        }

        public string TableName() => "smugresponses";

        public async Task LoadTable()
        {
            await GetResponsesDB(true);
        }

        public IEnumerable<SmugresponseEntity> GetEntries()
        {
            return _smugresponses;
        }

        public async Task<IEnumerable<SmugresponseEntity>> GetResponsesDB(bool updateLocal = false)
        {
            var responsesList = new List<SmugresponseEntity>();

            var query = "SELECT * FROM smugresponses";
            var result = await database.ExecuteReader(query);
            var rows = result.Rows;

            foreach (var row in rows)
            {
                var columns = row.GetColumns();

                var entry = new SmugresponseEntity()
                {
                    ID = Convert.ToInt32(columns["response_id"]),
                    UserID = Convert.ToInt32(columns["user_id"]),
                    Type = Convert.ToInt32(columns["type"]),
                    Query = columns["query"].ToString(),
                    Answer = columns["answer"].ToString()
                };

                responsesList.Add(entry);
            }

            if (updateLocal)
                _smugresponses = responsesList;

            return responsesList;
        }

        public async Task<int> AddResponse(SmugresponseEntity response)
        {
            var query = "INSERT INTO `smugresponses` (`user_id`, `type`, `query`, `answer`) " +
                        "VALUES                      (@P0, @P1, @P2, @P3);";

            var result = await database.ExecuteNonQuery(query,
                response.UserID, response.Type, response.Query, response.Answer);

            int insertedId = (int)result.Command.LastInsertedId;

            //await LogTable.LogActivityInfo(insertedId, response.UserID, $"Added new response (type {response.Type}): \"{response.Query}\".");

            return insertedId;
        }

        public async Task RemoveResponse(int id, int userId = -1)
        {
            var query = "DELETE FROM `smugresponses` WHERE `response_id` = @P0";
            await database.ExecuteNonQuery(query, id);

            //await LogTable.LogActivityInfo(id, userId, "Deleted entry.");
        }

        public async Task UpdateResponse(int id, SmugresponseEntity newEntry)
        {
            var query = "UPDATE `smugresponses` SET `type` = @P0, `query` = @P1, `answer` = @P2 " +
                        "WHERE  `response_id` = @P3;";

            await database.ExecuteNonQuery(query,
                newEntry.Type, newEntry.Query, newEntry.Answer, id);

            //await LogTable.LogActivityInfo(id, newEntry.UserID, $"Updated entry (type {newEntry.Type}): \"{newEntry.Status}\".");
        }

        
    }
}

