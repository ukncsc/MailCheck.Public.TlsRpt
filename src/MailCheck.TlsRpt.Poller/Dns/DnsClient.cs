﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DnsClient;
using DnsClient.Protocol;
using MailCheck.Common.Util;
using MailCheck.TlsRpt.Poller.Domain;
using MailCheck.TlsRpt.Poller.Domain.Errors.Parser;
using Microsoft.Extensions.Logging;

namespace MailCheck.TlsRpt.Poller.Dns
{
    public interface IDnsClient
    {
        Task<TlsRptRecordInfos> GetTlsRptRecords(string domain);
    }

    public class DnsClient : IDnsClient
    {
        private const string NonExistentDomainError = "Non-Existent Domain";
        private const string SERV_FAILURE_ERROR = "Server Failure";
        private readonly ILookupClient _lookupClient;
        private readonly ILogger<DnsClient> _log;

        public DnsClient(ILookupClient lookupClient,
            ILogger<DnsClient> log)
        {
            _lookupClient = lookupClient;
            _log = log;
        }

        public async Task<TlsRptRecordInfos> GetTlsRptRecords(string domain)
        {
            string queryText = $"_smtp._tls.{domain}";
            QueryType queryType = QueryType.TXT;

            IDnsQueryResponse response = await _lookupClient.QueryAsync(queryText, queryType);

            List<TlsRptRecordInfo> tlsRptRecordInfos = response.Answers.OfType<TxtRecord>()
                .Where(_ => _.Text.FirstOrDefault()?.StartsWith("v=TLSRPTv", StringComparison.OrdinalIgnoreCase) ?? false)
                .Select(_ => new TlsRptRecordInfo(domain, _.Text.Select(r => r.Escape()).ToList()))
                .ToList();

            if (response.HasError && response.ErrorMessage != NonExistentDomainError && response.ErrorMessage != SERV_FAILURE_ERROR)
            {
                return new TlsRptRecordInfos(domain, new FailedPollError(response.ErrorMessage) , response.MessageSize, response.NameServer.ToString(), response.AuditTrail);
            }

            return new TlsRptRecordInfos(domain, tlsRptRecordInfos, response.MessageSize);
        }
    }
}