using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarineBot.Entities
{
    internal class ActivityEntry
    {
        public int ID;
        public int UserID;
        public ActivityType Type;
        public string Status;

        public ulong AddedBy;

        public ActivityEntry()
        {
        }
    }
}
