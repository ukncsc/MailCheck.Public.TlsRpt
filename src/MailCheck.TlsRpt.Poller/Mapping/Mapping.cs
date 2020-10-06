using System.Linq;
using MailCheck.TlsRpt.Contracts;
using MailCheck.TlsRpt.Contracts.Poller;
using MailCheck.TlsRpt.Poller.Domain;
using ContractTag = MailCheck.TlsRpt.Contracts.SharedDomain.Tag;
using ContractMessage = MailCheck.TlsRpt.Contracts.SharedDomain.Message;
using ContractMessageType = MailCheck.TlsRpt.Contracts.SharedDomain.MessageType;
using ContractUnknownTag = MailCheck.TlsRpt.Contracts.SharedDomain.UnknownTag;
using ContractVersion = MailCheck.TlsRpt.Contracts.SharedDomain.VersionTag;
using ContractTlsRptRecords = MailCheck.TlsRpt.Contracts.SharedDomain.TlsRptRecords;
using ContractTlsRptRecord = MailCheck.TlsRpt.Contracts.SharedDomain.TlsRptRecord;
using ContractMalformedUri = MailCheck.TlsRpt.Contracts.SharedDomain.MalformedUri;
using ContractUnknownUri = MailCheck.TlsRpt.Contracts.SharedDomain.UnknownUri;
using ContractMalformedTag = MailCheck.TlsRpt.Contracts.SharedDomain.MalformedTag;
using ContractHttpsUri = MailCheck.TlsRpt.Contracts.SharedDomain.HttpsUri;
using ContractMailToUri = MailCheck.TlsRpt.Contracts.SharedDomain.MailToUri;
using ContractRuaTag = MailCheck.TlsRpt.Contracts.SharedDomain.RuaTag;
using ContractUri = MailCheck.TlsRpt.Contracts.SharedDomain.Uri;



namespace MailCheck.TlsRpt.Poller.Mapping
{
    public static class Mapping
    {
        public static TlsRptRecordsPolled ToTlsRptRecordsPolled(this TlsRptPollResult tlsRptPollResult)
        {
            return new TlsRptRecordsPolled(tlsRptPollResult.Id,
                tlsRptPollResult.TlsRptRecords?.ToContract(),
                tlsRptPollResult.Errors.Select(_ => _.ToContract()).ToList());
        }

        private static ContractTlsRptRecords ToContract(this TlsRptRecords tlsRptRecords)
        {
            return new ContractTlsRptRecords(tlsRptRecords.Domain,
                tlsRptRecords.Records.Select(x => x.ToContractTlsRptRecord()).ToList(), 
                tlsRptRecords.MessageSize);
        }

        private static ContractTlsRptRecord ToContractTlsRptRecord(this TlsRptRecord tlsRptRecord)
        {
            return new ContractTlsRptRecord(tlsRptRecord.Domain,
                tlsRptRecord.RecordsParts,
                tlsRptRecord.Tags.Select(x => x.ToContract()).ToList());
        }

        private static ContractTag ToContract(this Tag tag)
        {
            switch (tag)
            {
                case MalformedTag malformedTag:
                    return malformedTag.ToContract();
                case RuaTag ruaTag:
                    return ruaTag.ToContract();
                case VersionTag version:
                    return version.ToContract();
            }

            return new ContractUnknownTag(tag.RawValue);
        }

        private static ContractMessage ToContract(this Error error)
        {
            return new ContractMessage(error.Id, MessageSources.TlsRptPoller, error.ErrorType.ToContract(), error.Message, error.Markdown);
        }

        private static ContractMessageType ToContract(this ErrorType errorType)
        {
            switch (errorType)
            {
                case ErrorType.Error:
                    return ContractMessageType.error;
                case ErrorType.Warning:
                    return ContractMessageType.warning;
                case ErrorType.Info:
                    return ContractMessageType.info;
                default:
                    return ContractMessageType.error;
            }
        }

        private static ContractMalformedUri ToContract(this MalformedUri malformedUri)
        {
            return new ContractMalformedUri(malformedUri.Value);
        }

        private static ContractMalformedTag ToContract(this MalformedTag malformedTag)
        {
            return new ContractMalformedTag(malformedTag.RawValue);
        }

        private static ContractHttpsUri ToContract(this HttpsUri httpsUri)
        {
            return new ContractHttpsUri(httpsUri.Value);
        }

        private static ContractMailToUri ToContract(this MailToUri mailToUri)
        {
            return new ContractMailToUri(mailToUri.Value);
        }

        private static ContractRuaTag ToContract(this RuaTag ruaTag)
        {
            return new ContractRuaTag(ruaTag.RawValue, ruaTag.Uris.Select(_ => _.ToContract()).ToList());
        }
        
        private static ContractUri ToContract(this Uri uri)
        {
            switch (uri)
            {
                case HttpsUri httpsUri:
                    return httpsUri.ToContract();
                case MailToUri mailToUri:
                    return mailToUri.ToContract();
                case MalformedUri malformedUri:
                    return malformedUri.ToContract();
            }

            return new ContractUnknownUri(uri.Value);
        }

        private static ContractVersion ToContract(this VersionTag version)
        {
            return new ContractVersion(version.RawValue, version.Value);
        }
    }
}
