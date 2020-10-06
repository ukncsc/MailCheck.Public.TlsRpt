using MailCheck.TlsRpt.Contracts.SharedDomain;

namespace MailCheck.TlsRpt.Evaluator.Rules
{
    public class Error
    {
        public Error(ErrorType errorType, string message, MessageDisplay messageDisplay = MessageDisplay.Standard)
        {
            ErrorType = errorType;
            Message = message;
            MessageDisplay = messageDisplay;
        }

        public ErrorType ErrorType { get; }

        public string Message { get; }

        public MessageDisplay MessageDisplay { get; }

        public override string ToString()
        {
            return $"{nameof(ErrorType)}: {MessageDisplay} - {ErrorType}, {nameof(Message)}: {Message}";
        }
    }
}