namespace CharacterDataEditor.Constants
{
    public class MessageConstants
    {
        public const string LoggerAttachedMessage = "Logger attached. Startup complete. Running...";
        public const string CompletedMessage = "Operation Completed";
        public const string FolderMissingOrInvalid = "Missing or Invalid Folder: {path}";

        //exception messages
        public const string ExceptionHeaderMessage = "An unhandeled exception occurred details below:";
        public const string ExceptionTypeMessage = "Exception Type: {exceptionType}";
        public const string ExceptionMessageMessage = "Exception Message: {exceptionMessage}";
        public const string ExceptionInnerExceptionTypeMessage = "Inner Exception Type: {innerType}";
        public const string ExceptionInnerExceptionMessageMessage = "Inner Exception Message: {innerMessage}";
        public const string ExceptionSourceMessage = "Exception Source: {exceptionSource}";
        public const string ExceptionStackTraceMessage = "Exception Stack Trace: {stackTrace}";
    }
}
