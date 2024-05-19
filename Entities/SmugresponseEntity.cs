using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarineBot.Entities
{
    internal class SmugresponseEntity
    {
        public int ID;
        public int UserID;
        public int Type;
        public string Query;
        public string Answer;
        public string ModAnswer;

        public SmugresponseEntity()
        {
        }
    }
}
