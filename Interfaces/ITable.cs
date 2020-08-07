using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MarineBot.Interfaces
{
    interface ITable
    {
        public Task CreateTableIfNull();
        public Task LoadTable();
        public Task SaveChanges();
    }
}
