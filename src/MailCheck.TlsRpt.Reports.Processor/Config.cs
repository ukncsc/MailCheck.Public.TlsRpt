using System;
using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.TlsRpt.Reports.Processor
{
    public interface IConfig
    {
        TimeSpan TimeoutS3 { get; }
        long MaxS3ObjectSizeKilobytes { get; }

    }

    public interface IDocumentDbConfig
    {
        string ClusterEndpoint { get; set; }
        string Database { get; set; }
        string Username { get; set; }
    }

    public class Config : IConfig, IDocumentDbConfig
    {
        public Config(IEnvironmentVariables environmentVariables)
        {
            TimeoutS3 = TimeSpan.FromSeconds(environmentVariables.GetAsDouble("TimeoutS3Seconds"));
            MaxS3ObjectSizeKilobytes = environmentVariables.GetAsLong("MaxS3ObjectSizeKilobytes");
            ClusterEndpoint = environmentVariables.Get("DocumentDbClusterEndpoint");
            Database = environmentVariables.Get("TlsRptDatabase");
            Username = environmentVariables.Get("DocumentDbUsername");
        }

        public TimeSpan TimeoutS3 { get; }
        public long MaxS3ObjectSizeKilobytes { get; }
        public string ClusterEndpoint { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
    }
}
