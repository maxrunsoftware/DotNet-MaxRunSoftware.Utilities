// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace MaxRunSoftware.Utilities.Common;

public static partial class Util
{
    private static Tuple<string, string?>? CommandLineParseOption(string arg, char optionIdentifier, char optionDelimiter)
    {
        arg.CheckNotNull(); // sanity check
        if (!arg.Contains(optionIdentifier)) return null; // quick short circuit

        var name = new StringBuilder();
        var value = new StringBuilder();

        const int parsingUnknown = 0, parsingIdentifier = 1, parsingName = 2, parsingValue = 3;
        var state = parsingUnknown;

        var q = new Queue<char>(arg.ToCharArray());
        while (q.Count > 0)
        {
            var c = q.Dequeue();
            if (state is parsingUnknown)
            {
                if (char.IsWhiteSpace(c)) continue;
                if (c != optionIdentifier) return null; // We got a character that is not whitespace or options.OptionIdentifier so not an Option
                state = parsingIdentifier;
            }
            else if (state is parsingIdentifier)
            {
                if (char.IsWhiteSpace(c)) continue;
                if (c == optionIdentifier) continue;
                name.Append(c);
                state = parsingName;
            }
            else if (state is parsingName)
            {
                if (c == optionDelimiter)
                {
                    state = parsingValue;
                }
                else
                {
                    name.Append(c);
                }
            }
            else if (state is parsingValue)
            {
                value.Append(c);
            }
        }

        var n = name.ToString().TrimOrNull();
        if (n == null) return null;

        var v = value.ToString().TrimOrNull();

        if (v == null && n.Contains(optionDelimiter)) return null; // arg="-=foo" should not be a flag
        return new(n, v);
    }

    [PublicAPI]
    public static CommandLineParseResult CommandLineParse(
        char optionIdentifier = '-',
        char optionDelimiter = '=',
        bool optionsCaseSensitive = false,
        bool flagsCaseSensitive = false,
        params string?[]? args)
    {
        var result = new CommandLineParseResult(optionsCaseSensitive, flagsCaseSensitive);
        var index = 0;
        foreach (var arg in args.OrEmpty().TrimOrNull().WhereNotNull())
        {
            var opt = CommandLineParseOption(arg, optionIdentifier, optionDelimiter);
            if (opt == null) // argument
            {
                var v = arg.CheckNotNullTrimmed();
                var o = new CommandLineArgument { ArgRaw = arg, Index = index, Value = v };
                result.Args.Add(o);
                result.Arguments.Add(arg);
            }
            else if (opt.Item2 == null) // flag
            {
                var n = opt.Item1.CheckNotNullTrimmed();
                var o = new CommandLineFlag { ArgRaw = arg, Index = index, Name = n };
                result.Args.Add(o);
                result.Flags.Add(n);
            }
            else // option
            {
                var n = opt.Item1.CheckNotNullTrimmed();
                var v = opt.Item2.CheckNotNullTrimmed();
                var o = new CommandLineOption { ArgRaw = arg, Index = index, Name = n, Value = v };
                result.Args.Add(o);

                var lis = result.Options.GetValueNullable(n);
                if (lis == null)
                {
                    lis = new List<string>();
                    result.Options.Add(n, lis);
                }

                lis.Add(v);
            }

            index++;
        }

        return result;
    }
}

[PublicAPI]
public enum CommandLineArgType { Argument, Option, Flag }

[PublicAPI]
public abstract class CommandLineArg
{
    public abstract CommandLineArgType Type { get; }
    public int Index { get; set; }
    public string ArgRaw { get; set; } = default!;
}

[PublicAPI]
public class CommandLineArgument : CommandLineArg
{
    public override CommandLineArgType Type => CommandLineArgType.Argument;
    public string Value { get; set; } = default!;
}

[PublicAPI]
public class CommandLineOption : CommandLineArg
{
    public override CommandLineArgType Type => CommandLineArgType.Option;
    public string Name { get; set; } = default!;
    public string Value { get; set; } = default!;
}

[PublicAPI]
public class CommandLineFlag : CommandLineArg
{
    public override CommandLineArgType Type => CommandLineArgType.Flag;
    public string Name { get; set; } = default!;
}

[PublicAPI]
public class CommandLineParseResult
{
    public IList<CommandLineArg> Args { get; set; }
    public IList<string> Arguments { get; set; }
    public IDictionary<string, IList<string>> Options { get; set; }
    public ISet<string> Flags { get; set; }

    public CommandLineParseResult(bool optionsCaseSensitive = false, bool flagsCaseSensitive = false)
    {
        Args = new List<CommandLineArg>();
        Arguments = new List<string>();
        Options = new DictionaryIndexed<string, IList<string>>(optionsCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
        Flags = new HashSet<string>(flagsCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
    }
}
