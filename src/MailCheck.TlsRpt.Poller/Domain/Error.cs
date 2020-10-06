using System;

namespace MailCheck.TlsRpt.Poller.Domain
{
    public class Error
    {
        public Error(Guid id, ErrorType errorType, string message, string markdown)
        {
            Id = id;
            ErrorType = errorType;
            Message = message;
            Markdown = markdown;
        }

        public Guid Id { get; }
        public ErrorType ErrorType { get; }
        public string Message { get; }
        public string Markdown { get; }
    }
}