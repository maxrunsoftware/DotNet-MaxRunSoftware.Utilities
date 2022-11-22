// Copyright (c) 2022 Max Run Software (dev@maxrunsoftware.com)
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

namespace MaxRunSoftware.Utilities;

public record CommandLineArgumentParserArg(
    bool IsArgument,
    bool IsOption,
    bool IsFlag,
    int Index,
    string ArgRaw,
    string Name,
    string Value
)
{
    public const string ARGUMENT_NAME = "~ARGUMENT~";
    public const string FLAG_VALUE = "~FLAG~";

    public static CommandLineArgumentParserArg CreateArgument(int index, string argRaw, string value) => new(
        IsArgument: true,
        IsOption: false,
        IsFlag: false,
        Index: index,
        ArgRaw: argRaw,
        Name: ARGUMENT_NAME,
        Value: value
    );

    public static CommandLineArgumentParserArg CreateOption(int index, string argRaw, string name, string value) => new(
        IsArgument: false,
        IsOption: true,
        IsFlag: false,
        Index: index,
        ArgRaw: argRaw,
        Name: name,
        Value: value
    );


    public static CommandLineArgumentParserArg CreateFlag(int index, string argRaw, string name) => new(
        IsArgument: false,
        IsOption: false,
        IsFlag: true,
        Index: index,
        ArgRaw: argRaw,
        Name: name,
        Value: FLAG_VALUE
    );
}

public record CommandLineArgumentsParserResult(
    List<CommandLineArgumentParserArg> Args,
    List<string> Arguments,
    DictionaryIndexed<string, List<string>> Options,
    List<string> Flags
)
{
    public static CommandLineArgumentsParserResult Create(bool optionsCaseSensitive = false)
    {
        return new CommandLineArgumentsParserResult(
            Args: new List<CommandLineArgumentParserArg>(),
            Arguments: new List<string>(),
            Options: new DictionaryIndexed<string, List<string>>(optionsCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase),
            Flags: new List<string>()
        );
    }
}

public record CommandLineArgumentsParserOptions(
    char OptionIdentifier,
    char OptionDelimiter,
    bool OptionsCaseSensitive
);

public class CommandLineArgumentsParser
{
    private static Tuple<string, string?>? ParseOption(CommandLineArgumentsParserOptions options, string arg)
    {
        arg.CheckNotNull(); // sanity check
        if (!arg.Contains(options.OptionIdentifier)) return null; // quick short circuit


        var name = new StringBuilder(100);
        var value = new StringBuilder(500);

        const int parsingUnknown = 0;
        const int parsingIdentifier = 1;
        const int parsingName = 2;
        const int parsingValue = 3;
        var state = parsingUnknown;

        var q = new Queue<char>(arg.ToCharArray());
        while (q.Count > 0)
        {
            var c = q.Dequeue();
            if (state is parsingUnknown)
            {
                if (char.IsWhiteSpace(c)) continue;
                if (c != options.OptionIdentifier) return null; // We got a character that is not whitespace or options.OptionIdentifier so not an Option
                state = parsingIdentifier;
            }

            else if (state is parsingIdentifier)
            {
                if (char.IsWhiteSpace(c)) continue;
                if (c == options.OptionIdentifier) continue;
                name.Append(c);
                state = parsingName;
            }

            else if (state is parsingName)
            {
                if (c == options.OptionDelimiter)
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


        if (v == null && n.Contains(options.OptionDelimiter)) return null; // arg="-=foo" should not be a flag
        return new Tuple<string, string?>(n, v);
    }

    public static CommandLineArgumentsParserResult Parse(CommandLineArgumentsParserOptions options, params string?[]? args)
    {
        var result = CommandLineArgumentsParserResult.Create(options.OptionsCaseSensitive);
        var index = 0;
        foreach (var arg in args.OrEmpty().TrimOrNull().WhereNotNull())
        {
            var opt = ParseOption(options, arg);
            if (opt == null) // argument
            {
                var v = arg.CheckNotNullTrimmed();
                result.Args.Add(CommandLineArgumentParserArg.CreateArgument(index, arg, v));
                result.Arguments.Add(arg);
            }
            else if (opt.Item2 == null) // flag
            {
                var n = opt.Item1.CheckNotNullTrimmed();
                result.Args.Add(CommandLineArgumentParserArg.CreateFlag(index, arg, n));
                result.Flags.Add(n);
            }
            else // option
            {
                var n = opt.Item1.CheckNotNullTrimmed();
                var v = opt.Item2.CheckNotNullTrimmed();
                result.Args.Add(CommandLineArgumentParserArg.CreateOption(index, arg, n, v));

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
