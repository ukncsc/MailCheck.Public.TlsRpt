using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using MailCheck.TlsRpt.Reports.Api.Config;
using MongoDB.Driver;

namespace MailCheck.TlsRpt.Reports.Api.Dao
{
    public interface IMongoClientProvider
    {
        Task<IMongoClient> GetMongoClient();
    }

    public class MongoClientProvider : IMongoClientProvider
    {
        private readonly IDocumentDbConfig _config;
        private readonly string _connectionString;
        private readonly IAmazonSimpleSystemsManagement _parameterStoreClient;

        public MongoClientProvider(IDocumentDbConfig config, IAmazonSimpleSystemsManagement parameterStoreClient)
        {
            _config = config;
            _parameterStoreClient = parameterStoreClient;
        }

        internal MongoClientProvider(IDocumentDbConfig config, string connectionString)
        {
            _config = config;
            _connectionString = connectionString;
        }

        public async Task<IMongoClient> GetMongoClient()
        {
            MongoClientSettings settings;

            if (_connectionString == null)
            {
                string username = _config.Username;
                GetParameterResponse response = await _parameterStoreClient.GetParameterAsync(new GetParameterRequest { Name = username, WithDecryption = true });
                string password = response.Parameter.Value;

                settings = MongoClientSettings.FromUrl(new MongoUrl($"mongodb://{username}:{password}@{_config.ClusterEndpoint}/"));
                settings.AllowInsecureTls = true;
                settings.UseTls = true;
                settings.ReadPreference = ReadPreference.SecondaryPreferred;
                settings.ReplicaSetName = "rs0";
            }
            else
            {
                settings = MongoClientSettings.FromUrl(new MongoUrl(_connectionString));
            }

            return new MongoClient(settings);
        }
    }
}
