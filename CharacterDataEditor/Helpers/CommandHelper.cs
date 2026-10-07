using System;
using System.CommandLine;
using System.Linq;
using CharacterDataEditor.Constants;
using CharacterDataEditor.Options;

namespace CharacterDataEditor.Helpers;

public class CommandHelper
{
    public static int GenerateRootCommandAndExecuteHandler(string[] args, Func<ArgValues, string[], int> handler)
    {
        var rootCommand = new RootCommand(CommandConstants.RootDescription);

        var logAliases = new string[]
        {
            CommandConstants.LogPathCommandUnixLong,
            CommandConstants.LogPathCommandUnixShort,
            CommandConstants.LogPathCommandWindowsLong,
            CommandConstants.LogPathCommandWindowsShort
        };

        var logOption = GenerateOption<string>(
            logAliases,
            CommandConstants.LogPathCommandDescription,
            CommandConstants.LogPathCommandHelpName,
            CommandConstants.LogPathCommandName);

        rootCommand.Options.Add(logOption);

        rootCommand.SetAction(
            parseResult =>
            {
                var log = parseResult.GetValue(logOption);
                var options = ProcessCommandLineResults(log);
                handler(options, args);
            });

        return rootCommand.Parse(args).Invoke();
    }

    private static Option<T> GenerateOption<T>(string[] aliases, string description, string helpName, string name, bool required = false) =>
        new(name, [.. aliases.Where(alias => alias != name)])
        {
            Description = description,
            HelpName = helpName,
            Required = required
        };

    private static ArgValues ProcessCommandLineResults(string logPath)
    {
        if (logPath != null && !logPath.EndsWith("\\") && !logPath.EndsWith('/'))
        {
            //make sure we use the correct slash if we need to use it
            var useBackslash = logPath.Count(x => x.Equals('\\')) > logPath.Count(x => x.Equals('/'));

            logPath += useBackslash ? "\\" : "/";
        }

        return new ArgValues
        {
            LogPath = logPath
        };
    }
}
