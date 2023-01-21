namespace CharacterDataEditor.Constants
{
    public class CommandConstants
    {
        public const string RootDescription = "Knockout Arcade Character Data Editor";

        public const string LogPathCommandDescription = "Outputs the log to file at the path specified, log file will be named plexmatch.log in the directory specified";
        public const string LogPathCommandHelpName = "Log Path";
        public const string LogPathCommandName = "log";
        public const string LogPathCommandShortName = "l";
        public const string LogPathCommandUnixLong = $"--{LogPathCommandName}";
        public const string LogPathCommandUnixShort = $"-{LogPathCommandShortName}";
        public const string LogPathCommandWindowsLong = $"/{LogPathCommandName}";
        public const string LogPathCommandWindowsShort = $"/{LogPathCommandShortName}";
    }
}
