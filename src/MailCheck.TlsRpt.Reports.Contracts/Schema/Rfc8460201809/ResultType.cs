using System.Runtime.Serialization;

namespace MailCheck.TlsRpt.Reports.Contracts.Schema.Rfc8460201809
{
    public enum ResultType
    {
        [EnumMember(Value = "starttls-not-supported")]
        StartTlsNotSupported,
        [EnumMember(Value = "certificate-host-mismatch")]
        CertificateHostMismatch,
        [EnumMember(Value = "certificate-expired")]
        CertificateExpired,
        [EnumMember(Value = "certificate-not-trusted")]
        CertificateNotTrusted,
        [EnumMember(Value = "validation-failure")]
        ValidationFailure,
        [EnumMember(Value = "tlsa-invalid")]
        TlsaInvalid,
        [EnumMember(Value = "dnssec-invalid")]
        DnssecInvalid,
        [EnumMember(Value = "dane-required")]
        DaneRequired,
        [EnumMember(Value = "sts-policy-fetch-error")]
        StsPolicyFetchError,
        [EnumMember(Value = "sts-policy-invalid")]
        StsPolicyInvalid,
        [EnumMember(Value = "sts-webpki-invalid")]
        StsWebpkiInvalid
    };
}
