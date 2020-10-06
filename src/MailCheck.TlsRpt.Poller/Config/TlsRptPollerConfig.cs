﻿using System;
using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.TlsRpt.Poller.Config
{
    public interface ITlsRptPollerConfig
    {
        string SnsTopicArn { get; }
        string NameServer { get; }
        bool AllowNullResults { get; }
        TimeSpan DnsRecordLookupTimeout { get; }
    }

    public class TlsRptPollerConfig : ITlsRptPollerConfig
    {
        public TlsRptPollerConfig(IEnvironmentVariables environmentVariables)
        {
            SnsTopicArn = environmentVariables.Get("SnsTopicArn");
            DnsRecordLookupTimeout = TimeSpan.FromSeconds(environmentVariables.GetAsLong("DnsRecordLookupTimeoutSeconds"));
            NameServer = environmentVariables.Get("NameServer", false);
            AllowNullResults = environmentVariables.GetAsBoolOrDefault("AllowNullResults");
        }

        public string SnsTopicArn { get; }
        public TimeSpan DnsRecordLookupTimeout { get; }
        public string NameServer { get; }
        public bool AllowNullResults { get; }
    }
}
