using MarineBot.Entities;
using MarineBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarineBot.Database
{
    internal class UserTable : ITable
    {
        private string _connectionString;

        private List<AuthUser> _users;

        private DBGateway database;

        public UserTable(string ConnectionString)
        {
            _connectionString = ConnectionString;

            database = new DBGateway(_connectionString);
        }

        public string TableName() => "auth_users";

        public async Task LoadTable()
        {
            await GetUsersDB(true);
        }

        public async Task<int> AddUserDB(AuthUser user, int token_id = -1)
        {
            var query = "INSERT INTO `auth_users` (`discord_id`, `token_id`, `username`, `discriminator`, `avatar_hash`) " +
                        "VALUES                   (@P0, @P1, @P2, @P3, @P4);";

            var result = await database.ExecuteNonQuery(query,
                user.DiscordID, (token_id != -1) ? token_id : user.Token.ID, user.Username, user.Discriminator, user.AvatarHash);

            return (int)result.Command.LastInsertedId;
        }

        public async Task UpdateUserDB(int user_id, AuthUser userData)
        {
            var query = "UPDATE `auth_users` SET `username` = @P0, `discriminator` = @P1, `avatar_hash` = @P2 " +
                        "WHERE  `user_id` = @P3;";

            await database.ExecuteNonQuery(query,
                userData.Username, userData.Discriminator, userData.AvatarHash, user_id);
        }

        public async Task<int> AddTokenDB(AuthToken token)
        {
            var query = "INSERT INTO `auth_tokens` (`access_token`, `session_code`, `retrieved`, `expires_at`, `refresh_token`) " +
                        "VALUES                    (@P0, @P1, @P2, @P3, @P4);";

            var result = await database.ExecuteNonQuery(query,
                token.AccessToken, token.SessionCode, token.Retrieved.ToUnixTimeSeconds(), token.ExpiresAt.ToUnixTimeSeconds(), token.RefreshToken);

            return (int)result.Command.LastInsertedId;
        }

        public async Task<IEnumerable<AuthUser>> GetUsersDB(bool updateLocal = false)
        {
            var usersList = new List<AuthUser>();

            var query = "SELECT * FROM auth_users";
            var result = await database.ExecuteReader(query);
            var rows = result.Rows;

            foreach (var row in rows)
            {
                var columns = row.GetColumns();

                var entry = new AuthUser()
                {
                    ID = Convert.ToInt32(columns["user_id"]),
                    DiscordID = Convert.ToUInt64(columns["discord_id"]),
                    Username = columns["username"].ToString(),
                    Discriminator = columns["discriminator"].ToString(),
                    AvatarHash = columns["avatar_hash"].ToString()
                };

                var token_id = Convert.ToInt32(columns["token_id"]);
                var token = await GetTokenByID(token_id);

                entry.Token = token;

                usersList.Add(entry);
            }

            if (updateLocal)
                _users = usersList;

            return usersList;
        }

        public async Task<AuthToken> GetTokenByID(int id)
        {
            var query = "SELECT * FROM auth_tokens WHERE token_id = @P0";
            var result = await database.ExecuteReader(query, id);
            var rows = result.Rows;

            if (rows.Count <= 0)
                return null;

            var columns = rows.First().GetColumns();

            var retrieved_timestamp = Convert.ToInt32(columns["retrieved"]);
            var expires_timestamp = Convert.ToInt32(columns["retrieved"]);

            var entry = new AuthToken()
            {
                ID = Convert.ToInt32(columns["token_id"]),
                AccessToken = columns["access_token"].ToString(),
                SessionCode = columns["session_code"].ToString(),
                Retrieved = DateTimeOffset.FromUnixTimeSeconds(retrieved_timestamp),
                ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(expires_timestamp),
                RefreshToken = columns["refresh_token"].ToString()
            };

            return entry;
        }
    }
}
