using CharacterDataEditor.Constants;
using CharacterDataEditor.Options;
using System;
using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;

namespace CharacterDataEditor.Helpers
{
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

            rootCommand.AddOption(logOption);

            rootCommand.SetHandler(
                (string log) =>
                {
                    var options = ProcessCommandLineResults(log);
                    handler(options, args);
                },
                logOption);

            return rootCommand.Invoke(args);
        }

        private static Option<T> GenerateOption<T>(string[] aliases, string description, string helpName, string name, bool required = false)
        {
            var option = new Option<T>(aliases, description);
            option.ArgumentHelpName = helpName;
            option.Name = name;
            option.IsRequired = required;

            return option;
        }

        private static ArgValues ProcessCommandLineResults(string logPath)
        {
            if (logPath != null && !logPath.EndsWith("\\") && !logPath.EndsWith('/'))
            {
                //make sure we use the correct slash if we need to use it
                bool useBackslash = logPath.Count(x => x.Equals('\\')) > logPath.Count(x => x.Equals('/'));

                logPath += useBackslash ? "\\" : "/";
            }

            return new ArgValues
            {
                LogPath = logPath
            };
        }
    }
}
