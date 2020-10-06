using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MailCheck.TlsRpt.Contracts;
using MailCheck.TlsRpt.Contracts.SharedDomain;
using Uri = System.Uri;

namespace MailCheck.TlsRpt.Evaluator.Rules
{
    public class RuaTagsShouldContainTlsRptServiceMailBox : IRule<TlsRptRecord>
    {
        private readonly Uri _tlsRptMailbox;
        private readonly string _allowedRuaDomain;
        private readonly string _tlsRptMailboxAddress;

        public RuaTagsShouldContainTlsRptServiceMailBox()
        {
            _tlsRptMailbox = new Uri(TlsRptRulesResource.RuaMailbox);
            _allowedRuaDomain = TlsRptRulesResource.AllowedRuaDomain;
            _tlsRptMailboxAddress = $"{_tlsRptMailbox.UserInfo}@{_tlsRptMailbox.Host}";
        }

        public Task<List<Message>> Evaluate(TlsRptRecord t)
        {
            List<Message> messages = new List<Message>();
            Message message = GetMessage(t);
            if (message != null)
            {
                messages.Add(message);
            }
            return Task.FromResult(messages);
        }

        public Message GetMessage(TlsRptRecord record)
        {
            List<RuaTag> ruaTags = record.Tags.OfType<RuaTag>().ToList();

            // If we have duplicate entries for the same tag or malformed/unknown uri
            // There is already an error so disable this rule
            if (ruaTags.Count > 1 || record.Tags.OfType<RuaTag>().ToList().Any(x =>
                    x.Uris.OfType<MalformedUri>().ToList().Count > 0 || x.Uris.OfType<UnknownUri>().ToList().Count > 0))
            {
                return null;
            }

            List<Uri> reportUris = ruaTags.FirstOrDefault()?.Uris?.Where(_ => _ != null).Select(_ => new Uri(_.Value)).ToList().ToList() ?? new List<Uri>();

            List<Uri> mailCheckUris = reportUris.Where(_ => _.Authority == _tlsRptMailbox.Authority).ToList();

            List<Uri> unexpectedMailCheckUris = mailCheckUris.Where(_ => _?.UserInfo != _tlsRptMailbox.UserInfo).ToList();

            int otherAllowedRuaCount = reportUris.Count(_ => _.Authority == _allowedRuaDomain);

            if (unexpectedMailCheckUris.Any() && otherAllowedRuaCount == 0)
            {
                string template = TlsRptRulesMarkDownResource.RuaTagShouldNotHaveMisconfiguredMailCheckMailboxErrorMessage;

                Regex ruaRegex = new Regex("rua=[^;]+;");

                string currentRuaTag = ruaRegex.Match(record.Record).Value;
                string suggestedRuaTag = currentRuaTag.Replace(" ", "");

                foreach (Uri uri in mailCheckUris)
                {
                    suggestedRuaTag = suggestedRuaTag.Replace(uri.OriginalString, "");
                }

                suggestedRuaTag = suggestedRuaTag.Replace(",,", ",");
                suggestedRuaTag = suggestedRuaTag.Replace(",;", ";");
                suggestedRuaTag = suggestedRuaTag.Replace("=,", "=");

                string delimiter = reportUris.Count > mailCheckUris.Count ? "," : "";
                suggestedRuaTag = suggestedRuaTag.Replace("rua=", $"rua=mailto:{_tlsRptMailboxAddress}{delimiter}");

                string suggestedRecord = record.Record;
                if (!string.IsNullOrEmpty(suggestedRecord) && !string.IsNullOrEmpty(currentRuaTag) && !string.IsNullOrEmpty(suggestedRuaTag))
                {
                    suggestedRecord = suggestedRecord.Replace(currentRuaTag, suggestedRuaTag);
                }

                string markdown = string.Format(template, suggestedRecord);

                return new Message(Guid.Parse("85DB50C5-E3DA-479A-A9CE-1B07C81C3747"), "MessageSources.TlsRptEvaluator", MessageType.error, string.Format(
                        TlsRptRulesResource.RuaTagShouldNotHaveMisconfiguredMailCheckMailboxErrorMessage,
                        _tlsRptMailboxAddress,
                        _tlsRptMailbox.OriginalString), markdown
                    );
            }

            if (!mailCheckUris.Any() && otherAllowedRuaCount == 0)
            {
                string tlsRptRecord = record.Record;

                if (tlsRptRecord.Contains("rua="))
                {
                    tlsRptRecord = tlsRptRecord.Replace("rua=", $"rua={TlsRptRulesResource.RuaMailbox},");
                }
                else
                {
                    tlsRptRecord = $"{tlsRptRecord}rua={TlsRptRulesResource.RuaMailbox};";
                }

                return new Message(Guid.Parse("045C9D62-8771-4D8F-B981-EAC70C7B74A2"), MessageSources.TlsRptEvaluator, MessageType.info, string.Format(TlsRptRulesResource.RuaTagsShouldContainTlsRptServiceMailBoxErrorMessage,
                        _tlsRptMailboxAddress,
                        _tlsRptMailbox.OriginalString),
                    string.Format(TlsRptRulesMarkDownResource.RuaTagsShouldContainTlsRptServiceMailBoxErrorMessage, tlsRptRecord));

            }

            if (reportUris.GroupBy(_ => _.OriginalString).Any(_ => _.Count() > 1))
            {
                return new Message(Guid.Parse("354DDFCE-B9DE-4C3B-93BD-A120408ED94A"), MessageSources.TlsRptEvaluator, MessageType.warning,
                    TlsRptRulesResource.RuaTagShouldNotContainDuplicateUrisErrorMessage, string.Empty);
            }

            return null;
        }


        public int SequenceNo => 1;
        public bool IsStopRule => false;
    }
}
