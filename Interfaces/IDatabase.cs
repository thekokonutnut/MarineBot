using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MarineBot.Interfaces
{
    interface IDatabase
    {
        public Task TestConnection();
        public Task CreateTableIfNull();
        public Task LoadDatabase();
        public Task SaveChanges();
    }
}
