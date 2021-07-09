using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.EventHandling;
using MarineBot.Controller;
using MarineBot.Database;
using MarineBot.Entities;
using MarineBot.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MarineBot.Threads
{
    class PollThread
    {
        private CancellationTokenSource _cts;
        private PollTable _pollTable;
        private DiscordClient _client;

        private string[] _reactions = { ":green_square:", ":red_square:", ":yellow_square:", ":blue_square:", ":purple_square:" };

        bool Running = false;

        public PollThread(IServiceProvider serviceProvider)
        {
            var controller  = serviceProvider.GetService<DatabaseController>();
            _pollTable      = controller.GetTable<PollTable>();
            _cts            = serviceProvider.GetService<CancellationTokenSource>();
            _client         = serviceProvider.GetService<DiscordClient>();
        }

        private bool ValidUser(DiscordUser user)
        {
            return !user.IsBot;
        }
        private DiscordEmbedBuilder GenerateEmbed(Poll poll, int[] reactionCount, bool terminated = false)
        {
            var embed = new DiscordEmbedBuilder()
                .WithColor(0x3275a8)
                .WithTitle(poll.Title)
                .WithThumbnail(terminated ? FacesHelper.GetSuccessFace() : FacesHelper.GetIdleFace());

            var options = poll.Options;

            if (terminated)
            {
                for (int i = 0; i < options.Count; i++)
                {
                    embed.AddField($"{_reactions[i]}\t**{options[i]}**", $"{reactionCount[i]}");
                }
                embed.WithDescription("\n**Poll completed.**");
            } 
            else
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < options.Count; i++)
                {
                    builder.Append($"{_reactions[i]}\t**{options[i]}**\n\n");
                }
                var timeLeft = poll.Time - (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - poll.StartTime);
                builder.Append($"\n***Ends in: {timeLeft} seconds***");
                embed.WithDescription(builder.ToString());
            }

            return embed;
        }
        private async Task CheckPoll(Poll poll)
        {
            int[] reactionsCount = { 0, 0, 0, 0, 0 };
            if (!poll.Initialized)
            {
                poll.Channel = await _client.GetChannelAsync(poll.ChannelID);
                poll.Message = await poll.Channel.GetMessageAsync(poll.MessageID);

                var embed = GenerateEmbed(poll, reactionsCount);
                await poll.Message.ModifyAsync(embed: embed.Build());
                if (poll.Message.Reactions.Count == 0)
                {
                    for (int i = 0; i < poll.Options.Count; i++)
                    {
                        await poll.Message.CreateReactionAsync(DiscordEmoji.FromName(_client, _reactions[i]));
                    }
                }

                poll.Initialized = true;
            }
            else
            {
                var timeLeft = poll.Time - (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - poll.StartTime);

                if (timeLeft <= 0)
                {
                    for (int i = 0; i < poll.Options.Count; i++)
                    {
                        var usersReact = await poll.Message.GetReactionsAsync(DiscordEmoji.FromName(_client, _reactions[i]));
                        reactionsCount[i] += usersReact.Count(p => ValidUser(p));
                    }

                    poll.Terminated = true;

                    var embed = GenerateEmbed(poll, reactionsCount, true);
                    await poll.Message.ModifyAsync(embed: embed.Build());
                    await TerminatePoll(poll);
                }
            }
        }
        private async Task TerminatePoll(Poll poll)
        {
            await poll.Message.DeleteAllReactionsAsync("Poll terminated.");
            //_pollTable.RemovePoll(poll.ID);
        }

        public async Task RunAsync()
        {
            if (Running)
                return;

            Running = true;

            Console.WriteLine("[System] Poll Thread running.");

            while (!_cts.IsCancellationRequested)
            {
                var polls = _pollTable.GetPolls();
                var terminated = new List<uint>();

                foreach (var poll in polls)
                {
                    await CheckPoll(poll);
                    if (poll.Terminated) terminated.Add(poll.ID);
                }
                foreach (var id in terminated)
                    _pollTable.RemovePoll(id);

                Thread.Sleep(1000);
            }

        }
    }
}