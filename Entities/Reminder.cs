using System;
using System.Collections.Generic;
using System.Text;

namespace MarineBot.Entities
{
    internal class Reminder
    {
        public string Name = "";
        public string Description = "";
        public string Time = "";
        public int Hour = 0;
        public int Minute = 0;
        public ulong Channel = 0;
        public ulong Guild = 0;
        public bool Called = false;

        public Reminder(string name, string description, string time, ulong guild, ulong channel)
        {
            Name = name;
            Description = description;
            Time = time;
            Guild = guild;
            Channel = channel;
            Hour = Convert.ToInt32(Time.Substring(0, 2));
            Minute = Convert.ToInt32(Time.Substring(3, 2));
        }
    }
}
