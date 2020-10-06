using System;

namespace MailCheck.TlsRpt.Poller.Exceptions
{
    public class TlsRptPollerException : Exception
    {
        public TlsRptPollerException()
        {
        }

        public TlsRptPollerException(string formatString, params object[] values)
            : base(string.Format(formatString, values))
        {
        }

        public TlsRptPollerException(string message)
            : base(message)
        {
        }
    }
}
