using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MarineBot.Helpers
{
    class EmbedPagination
    {
        int currentOffset = 0;
        int maxOffset = 1;

        CommandContext context;
        DiscordMessage pagingmsg;
        EmbedGeneration generator;

        public delegate DiscordEmbedBuilder EmbedGeneration(int offset);

        Timer ownerTimer;
        Timer timeoutTimer;

        bool ownershipEnabled = true;

        public EmbedPagination(CommandContext ctx, int maxOff, EmbedGeneration handler, DiscordMessage msg = null)
        {
            context = ctx;
            context.Client.ComponentInteractionCreated += HandleInteraction;
            generator = handler;

            maxOffset = maxOff;

            if (msg is not null)
                pagingmsg = msg;

            ownerTimer = new Timer(15 * 1000);
            ownerTimer.AutoReset = false;
            ownerTimer.Elapsed += OwnerTimer_Elapsed;

            timeoutTimer = new Timer(30 * 60 * 1000);
            timeoutTimer.AutoReset = false;
            timeoutTimer.Elapsed += TimeoutTimer_Elapsed;
        }

        private void TimeoutTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            context.Client.ComponentInteractionCreated -= HandleInteraction;

            generator = null;
            context = null;
        }

        private void OwnerTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ownershipEnabled = false;
        }

        public async Task UpdatePaging()
        {
            if (ownershipEnabled)
            {
                ownerTimer.Stop();
                ownerTimer.Start();
            }

            timeoutTimer.Stop();
            timeoutTimer.Start();

            var embed = generator(currentOffset);

            //embed.WithFooter($"Showing {currentOffset} - {currentOffset+maxPerPage} / {_items.Count()}");

            var builder = new DiscordMessageBuilder()
                .WithEmbed(embed.Build())
                .AddComponents(new DiscordComponent[]
                {
                    new DiscordButtonComponent(ButtonStyle.Primary, "paging_first", "<<"),
                    new DiscordButtonComponent(ButtonStyle.Primary, "paging_left", "<"),
                    new DiscordButtonComponent(ButtonStyle.Primary, "paging_right", ">"),
                    new DiscordButtonComponent(ButtonStyle.Primary, "paging_last", ">>")
                });

            if (pagingmsg is null)
            {
                pagingmsg = await context.RespondAsync(builder);
            }
            else
            {
                await pagingmsg.ModifyAsync(builder);
            }
        }

        private async Task HandleInteraction(DSharpPlus.DiscordClient sender, DSharpPlus.EventArgs.ComponentInteractionCreateEventArgs e)
        {
            if (!e.User.IsBot && (e.Channel is null || e.Channel == context.Channel) && e.Message == pagingmsg)
            {
                if (ownershipEnabled && e.User != context.User)
                    return;

                switch (e.Id)
                {
                    case "paging_left":
                        currentOffset--;
                        break;
                    case "paging_right":
                        currentOffset++;
                        break;
                    case "paging_first":
                        currentOffset = 0;
                        break;
                    case "paging_last":
                        currentOffset = maxOffset;
                        break;
                    default:
                        break;
                }
                currentOffset = Math.Clamp(currentOffset, 0, maxOffset);

                await e.Interaction.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredMessageUpdate);
                await UpdatePaging();
            }
        }
    }
}
