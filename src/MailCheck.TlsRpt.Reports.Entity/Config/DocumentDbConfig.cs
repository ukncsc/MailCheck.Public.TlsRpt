using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.TlsRpt.Reports.Entity.Config
{
    public interface IDocumentDbConfig
    {
        string ClusterEndpoint { get; set; }
        string Database { get; set; }
        string Username { get; set; }
    }

    public class DocumentDbConfig : IDocumentDbConfig
    {
        public DocumentDbConfig(IEnvironmentVariables environmentVariables)
        {
            ClusterEndpoint = environmentVariables.Get("DocumentDbClusterEndpoint");
            Database = environmentVariables.Get("TlsRptDatabase");
            Username = environmentVariables.Get("DocumentDbUsername");
        }

        public string ClusterEndpoint { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
    }
}