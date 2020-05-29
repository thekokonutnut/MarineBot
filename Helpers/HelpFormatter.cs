using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;

namespace MarineBot.Helpers
{
    internal class HelpFormatter : BaseHelpFormatter
    {
        public DiscordEmbedBuilder EmbedBuilder { get; }
        private Command Command { get; set; }

        public HelpFormatter(CommandContext ctx)
            : base(ctx)
        {
            this.EmbedBuilder = new DiscordEmbedBuilder()
                .WithTitle("Help")
                .WithColor(0x007FFF)
                .WithThumbnailUrl(FacesHelper.GetIdleFace());
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            this.Command = command;

            this.EmbedBuilder.WithDescription($"{Formatter.InlineCode(command.Name)}: {command.Description ?? "Sin descripción."}");

            if (command is CommandGroup cgroup && cgroup.IsExecutableWithoutSubcommands)
                this.EmbedBuilder.WithDescription($"{this.EmbedBuilder.Description}\n\nEste grupo puede ejecutarse como un comándo solo.");

            if (command.Aliases?.Any() == true)
                this.EmbedBuilder.AddField("Aliases", string.Join(", ", command.Aliases.Select(Formatter.InlineCode)), false);

            if (command.Overloads?.Any() == true)
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

                this.EmbedBuilder.AddField("Argumentos", sb.ToString().Trim(), false);
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
                        sb.Append('`').Append(cmd.QualifiedName);

                        foreach (var arg in ovl.Arguments)
                            sb.Append(arg.IsOptional || arg.IsCatchAll ? " [" : " <").Append(arg.Name).Append(arg.IsCatchAll ? "..." : "").Append(arg.IsOptional || arg.IsCatchAll ? ']' : '>');

                        sb.Append($"`: {cmd.Description ?? "Sin descripción."}");

                        sb.Append("\n");
                    }
                }

                this.EmbedBuilder.AddField("Comándos", sb.ToString().Trim(), false);
            } else {
                this.EmbedBuilder.AddField("Grupos", string.Join(", ", subcommands.Select(x => Formatter.InlineCode(x.Name))), false);
            }

            return this;
        }

        public override CommandHelpMessage Build()
        {
            if (this.Command == null)
                this.EmbedBuilder.WithDescription("Mostrando todos los grupos de comándos. Especifica un grupo para ver sus comándos.");

            return new CommandHelpMessage(embed: this.EmbedBuilder.Build());
        }
    }
}
