namespace CharacterDataEditor.Constants
{
    public class MessageConstants
    {
        public const string LoggerAttachedMessage = "Logger attached. Startup complete. Running...";
        public const string CompletedMessage = "Operation Completed";
        public const string FolderMissingOrInvalid = "Missing or Invalid Folder: {path}";
        public const string UnsavedMessage = "Unsaved data detected. Do you want to [S]ave, E[x]it, or [C]ancel?";
        public const string UpgradeNeededMessage = "Upgrade Needed. What action do you want to take? [U]pgrade or [C]lose Project?";

        //exception messages
        public const string ExceptionHeaderMessage = "An unhandeled exception occurred details below:";
        public const string ExceptionTypeMessage = "Exception Type: {exceptionType}";
        public const string ExceptionMessageMessage = "Exception Message: {exceptionMessage}";
        public const string ExceptionInnerExceptionTypeMessage = "Inner Exception Type: {innerType}";
        public const string ExceptionInnerExceptionMessageMessage = "Inner Exception Message: {innerMessage}";
        public const string ExceptionSourceMessage = "Exception Source: {exceptionSource}";
        public const string ExceptionStackTraceMessage = "Exception Stack Trace: {stackTrace}";

        //upgrade messages
        public const string OriginalTo094UpgradeMessage = "DATA LOSS WARNING:\nMovement Data has been changed to Ground and Air movement data.\n" +
            "Migration is not possible, this data will be lost.\nAll other data will be migrated properly.";
    }
}
