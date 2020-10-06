using System.Collections.Generic;

namespace MailCheck.TlsRpt.Api.Domain
{
    public class TlsRptInfoListRequest
    {
        public TlsRptInfoListRequest()
        {
            HostNames = new List<string>();
        }

        public List<string> HostNames { get; set; }
    }
}
