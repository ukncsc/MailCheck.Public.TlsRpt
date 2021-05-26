using System.Runtime.Serialization;

namespace MailCheck.TlsRpt.Reports.Contracts.Schema.Rfc8460201809
{
    public enum PolicyType
    {
        [EnumMember(Value = "tlsa")]
        Tlsa,
        [EnumMember(Value = "sts")]
        Sts,
        [EnumMember(Value = "no-policy-found")]
        NoPolicyFound
    }
}