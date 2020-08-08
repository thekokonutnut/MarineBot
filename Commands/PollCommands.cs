using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using MarineBot.Threads;
using MarineBot.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MarineBot.Entities;
using MarineBot.Helpers;

namespace MarineBot.Commands
{
    [Group("Polls"), Aliases("p")]
    [Description("Comandos de utilidad variada.")]
    internal class PollCommands : BaseCommandModule
    {
        private PollTable _pollTable;

        public PollCommands(IServiceProvider serviceProvider)
        {
            var controller = (DatabaseController)serviceProvider.GetService(typeof(DatabaseController));

            _pollTable = controller.GetTable<PollTable>();
        }

        [Command("create")]
        [Description("Crea una encuesta.")]
        public async Task CreatePollCommand(CommandContext ctx, [Description("Titulo de la encuesta.")] string title, 
                          [Description("Tiempo en segundos.")] uint time = 10, [Description("Opciones.")] string opciones = "Yes,No")
        {
            if (title.Length == 0)
            {
                await MessageHelper.SendErrorEmbed(ctx, "Necesitas especificar un titulo.");
                return;
            }
            if (time < 10)
            {
                await MessageHelper.SendErrorEmbed(ctx, "El tiempo debe ser almenos 10 segundos.");
                return;
            }
            if (opciones.Split(",").Length < 2 || opciones.Split(",").Length > 5)
            {
                await MessageHelper.SendErrorEmbed(ctx, "Necesitas especificar almenos 2 opciones y máximo 5.");
                return;
            }

            var pollMsg = await MessageHelper.SendInfoEmbed(ctx, "Cargando encuesta...");
            var poll = new Poll(pollMsg.Id, pollMsg.Channel.GuildId, pollMsg.ChannelId, title, time, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), _pollTable.DeserializeOptions(opciones));
            _pollTable.CreatePoll(poll);
        }
    }
}
