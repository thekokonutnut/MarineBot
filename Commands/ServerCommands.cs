using MarineBot.Helpers;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using MarineBot.Entities;

namespace MarineBot.Commands
{
    [Group("server"), Aliases("s")]
    [Description("Comandos de servidor.")]
    internal class ServerCommands : BaseCommandModule
    {
        public ServerCommands(IServiceProvider serviceProvider)
        {

        }
    }
}
