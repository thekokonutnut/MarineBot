﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Exceptions;
using MarineBot.Attributes;
using MarineBot.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MarineBot.Commands
{
    [Group("admin")]
    [Description("Comandos de administración.")]
    [Hidden, RequireBotAdministrator]
    internal class AdminCommands : BaseCommandModule
    {
        public Bot _botApp { private get; set; }

        [GroupCommand(), Hidden()]
        public async Task MainCommand(CommandContext ctx)
        {
            var cmds = ctx.CommandsNext;
            var context = cmds.CreateContext(ctx.Message, ctx.Prefix, cmds.FindCommand("help", out _), ctx.Command.QualifiedName);
            await cmds.ExecuteCommandAsync(context);
        }

        [Command("shutdown"), Description("Apaga el bot.")]
        public async Task ShutdownCommand(CommandContext ctx)
        {
            try
            {
                await MessageHelper.SendInfoEmbed(ctx, "Apagando el bot...");
                _botApp.RequestShutdown();
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
            }
        }

        [Command("restart"), Description("Reinicia el bot.")]
        public async Task RestartCommand(CommandContext ctx)
        {
            try
            {
                await MessageHelper.SendInfoEmbed(ctx, "Reiniciando el bot...");
                _botApp.RequestRestart();
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
            }
        }

        [Command("listadmins"), Description("Lista los administradores del bot.")]
        public async Task ListAdminsCommand(CommandContext ctx)
        {
            try
            {
                string[] admins = AuthHelper.GetAdministrators();
                await MessageHelper.SendInfoEmbed(ctx, string.Join("\n", admins));
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
            }
        }
    }
}