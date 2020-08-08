using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MarineBot.Entities
{
    internal class Poll
    {
        public uint ID = 0;
        public string Title = "";
        public List<string> Options;
        public ulong MessageID = 0;
        public ulong ChannelID = 0;
        public ulong Guild = 0;
        public uint Time = 0;
        public long StartTime = 0;

        public bool Initialized = false;
        public bool Terminated = false;
        public DiscordChannel Channel = null;
        public DiscordMessage Message = null;

        public Poll(ulong messageid, ulong guild, ulong channel, string title, uint time, long starttime, List<string> options)
        {
            MessageID = messageid;
            Guild = guild;
            ChannelID = channel;
            Title = title;
            Time = time;
            StartTime = starttime;
            Options = options;
        }
    }
}
