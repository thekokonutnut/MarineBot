using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarineBot.Database
{
    internal class DBGateway
    {
        private string _connectionString;

        public DBGateway(string ConnectionString)
        {
            _connectionString = ConnectionString;
        }

        public async Task<DBQueryResult> ExecuteNonQuery(string query, params object[] parameters)
        {
            MySqlCommand cmd;
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                cmd = new MySqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = query;

                for (int i = 0; i < parameters.Length; i++)
                {
                    cmd.Parameters.AddWithValue($"@P{i}", parameters[i]);
                }

                try
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            return new DBQueryResult(null, cmd);
        }

        public async Task<DBQueryResult> ExecuteReader(string query, params object[] parameters)
        {
            MySqlCommand cmd;
            var rows = new List<DBRow>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                cmd = new MySqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = query;

                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i] == null) continue;
                    cmd.Parameters.AddWithValue($"@P{i}", parameters[i]);
                }

                try
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var row = new DBRow();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row.AddColumn(reader.GetName(i), reader.GetValue(i));
                            }
                            rows.Add(row);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            return new DBQueryResult(rows, cmd);
        }
    }

    internal class DBQueryResult
    {
        public IReadOnlyCollection<DBRow> Rows;
        public MySqlCommand Command;

        public DBQueryResult(IReadOnlyCollection<DBRow> rows, MySqlCommand command)
        {
            Rows = rows;
            Command = command;
        }
    }

    internal class DBRow
    {
        Dictionary<string, object> Columns;
        public DBRow()
        {
            Columns = new Dictionary<string, object>();
        }

        public void AddColumn(string name, object value)
        {
            Columns.Add(name, value);
        }

        public IReadOnlyDictionary<string, object> GetColumns()
            => Columns;
    }
}
