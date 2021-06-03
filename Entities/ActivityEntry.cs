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
        public ulong AddedBy;
        public DiscordActivity Activity;

        public ActivityEntry()
        {
        }

        public ActivityEntry(int id, ulong added, DiscordActivity activity)
        {
            ID = id;
            AddedBy = added;
            Activity = activity;
        }
    }
}
