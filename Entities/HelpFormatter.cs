using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using MarineBot.Attributes;
using MarineBot.Helpers;

namespace MarineBot.Entities
{
    internal class HelpFormatter : BaseHelpFormatter
    {
        public DiscordEmbedBuilder EmbedBuilder { get; }
        private Command Command { get; set; }
        private string _prefix;

        public HelpFormatter(CommandContext ctx)
            : base(ctx)
        {
            this.EmbedBuilder = new DiscordEmbedBuilder()
                .WithTitle(":wheelchair::wheelchair::wheelchair: Ayuda")
                .WithColor(0xf08a5d)
                .WithThumbnail(FacesHelper.GetIdleFace());

            var _config = (Config)ctx.CommandsNext.Services.GetService(typeof(Config));
            _prefix = _config.Prefix;
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            this.Command = command;

            this.EmbedBuilder.WithDescription($"{Formatter.InlineCode(command.Name)}: {command.Description ?? "Sin descripción."}");

            if (command.Aliases?.Any() == true)
                this.EmbedBuilder.AddField("Aliases", string.Join(", ", command.Aliases.Select(Formatter.InlineCode)), false);

            if (command.Overloads?.Any() == true && command.Parent != null)
            {
                var sb = new StringBuilder();

                foreach (var ovl in command.Overloads.OrderByDescending(x => x.Priority))
                {
                    sb.Append('`').Append(command.QualifiedName);

                    foreach (var arg in ovl.Arguments)
                        sb.Append(arg.IsOptional || arg.IsCatchAll ? " [" : " <").Append(arg.Name).Append(arg.IsCatchAll ? "..." : "").Append(arg.IsOptional || arg.IsCatchAll ? ']' : '>');

                    sb.Append("`\n");

                    foreach (var arg in ovl.Arguments)
                        sb.Append('`').Append(arg.Name).Append(" (").Append(this.CommandsNext.GetUserFriendlyTypeName(arg.Type)).Append(")`: ").Append(arg.Description ?? "Sin descripción.").Append('\n');

                    sb.Append('\n');
                }

                this.EmbedBuilder.AddField("Modo de uso", sb.ToString().Trim(), false);
                this.EmbedBuilder.WithFooter("<> = Requerido [] = Opcional");
            }

            var exampleAtt = (ExampleAttribute)command.CustomAttributes.FirstOrDefault(att => att is ExampleAttribute);
            if (exampleAtt != null)
            {
                var sb = new StringBuilder();
                foreach (var item in exampleAtt.Example)
                {
                    sb.Append($"`{_prefix}{item}`\n");
                }
                this.EmbedBuilder.AddField("Ejemplos", sb.ToString().Trim(), false);
            }

            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            if (this.Command != null) {
                
                var sb = new StringBuilder();
                foreach (var cmd in (this.Command as CommandGroup).Children)
                {
                    foreach (var ovl in cmd.Overloads.OrderByDescending(x => x.Priority))
                    {
                        sb.Append('`').Append(_prefix + cmd.QualifiedName);

                        foreach (var arg in ovl.Arguments)
                            sb.Append(arg.IsOptional || arg.IsCatchAll ? " [" : " <").Append(arg.Name).Append(arg.IsCatchAll ? "..." : "").Append(arg.IsOptional || arg.IsCatchAll ? ']' : '>');

                        sb.Append($"`: {cmd.Description ?? "Sin descripción."}");

                        sb.Append("\n");
                    }
                }

                this.EmbedBuilder.AddField("Comandos", sb.ToString().Trim(), false);
                this.EmbedBuilder.WithFooter("<> = Requerido [] = Opcional");
            } else {
                this.EmbedBuilder.AddField("Comandos principales sin categorizar", string.Join("\n", subcommands.Select(x => Formatter.InlineCode(_prefix + x.Name))), false);
            }

            return this;
        }

        public override CommandHelpMessage Build()
        {
            if (this.Command == null)
            {
                System.DateTime buildDate = System.IO.File.GetLastWriteTime(System.Reflection.Assembly.GetExecutingAssembly().Location);

                this.EmbedBuilder.WithDescription($"Un bot de propósito misceláneo,\nprobablemente no muy útil.\nCreado para uso personal, por mí.\n\n[Repositorio en GitHub](https://github.com/thekokonutnut/MarineBot)\n[Current build: {buildDate}]\n[actualmente {QuotesHelper.GetRandomStatus()}]");
                this.EmbedBuilder.AddField("⠀", $"Usa {Formatter.InlineCode(_prefix + "help <comando>")} para información\nde uso del comando.\nO solo el comando sin argumentos, eso\ntambién funciona.");
                this.EmbedBuilder.Title = "Smuggier everyday";
            }

            return new CommandHelpMessage(embed: this.EmbedBuilder.Build());
        }
    }
}
