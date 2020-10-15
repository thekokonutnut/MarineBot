using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MarineBot.Converters
{
    class DictConverter : IArgumentConverter<Dictionary<string, string>>
    {
        public Task<Optional<Dictionary<string, string>>> ConvertAsync(string value, CommandContext ctx)
        {
            Match match = Regex.Match(value, @"([^\s]+):([a-zA-Z0-9]+)", RegexOptions.IgnoreCase);

            Dictionary<string, string> _result = new Dictionary<string, string>();

            while (match.Success)
            {
                _result.Add(match.Groups[1].Value.Trim(), match.Groups[2].Value.Trim());
                match = match.NextMatch();
            }

            return Task.FromResult(Optional.FromValue(_result));
        }
    }
}
