using CodingSeb.ExpressionEvaluator;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity.Extensions;
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
        public DiscordClient _client { private get; set; }

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

        [Command("eval"), Description("Evalua una expresión.")]
        public async Task EvalCommand(CommandContext ctx, [Description("Expresión"), RemainingText()] string expresion)
        {
            if (expresion == null) throw new ArgumentException();
            try
            {
                ExpressionEvaluator mEvaluator = new ExpressionEvaluator();
                await MessageHelper.SendSuccessEmbed(ctx, $"`{expresion}:` {mEvaluator.Evaluate(expresion)}");
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
            }
        }

        [Command("test"), Description("xd.")]
        public async Task TestComman(CommandContext ctx)
        {
            _client.ComponentInteractionCreated += _client_ComponentInteractionCreated;

            var builder = new DiscordMessageBuilder();

            builder
                .WithContent("Buttons! Coming soon:tm:");

            for (int i = 1; i <= 5; i++)
            {
                var pbtns = new DiscordComponent[]
                {
                    new DiscordButtonComponent(ButtonStyle.Primary, $"poll{i*5}", $"{i*5}"),
                    new DiscordButtonComponent(ButtonStyle.Primary, $"poll{i*5+1}", $"{i*5+1}"),
                    new DiscordButtonComponent(ButtonStyle.Primary, $"poll{i*5+2}", $"{i*5+2}"),
                    new DiscordButtonComponent(ButtonStyle.Primary, $"poll{i*5+3}", $"{i*5+3}"),
                    new DiscordButtonComponent(ButtonStyle.Primary, $"poll{i*5+4}", $"{i*5+4}")
                };

                builder.WithComponents(pbtns);
            }

            await builder.SendAsync(ctx.Channel);
        }

        private async Task _client_ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            var x = e;
            var se = sender;

            await e.Interaction.CreateResponseAsync(InteractionResponseType.DefferedMessageUpdate);

            Console.WriteLine("interaction");
        }
    }
}
