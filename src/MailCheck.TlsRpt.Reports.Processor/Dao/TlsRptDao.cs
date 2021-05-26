using System.IO;
using System.Threading.Tasks;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using MailCheck.TlsRpt.Reports.Contracts;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Serialization;

namespace MailCheck.TlsRpt.Reports.Processor.Dao
{
    public interface ITlsRptDao
    {
        Task Persist(ReportInfo jsonReport);
    }

    internal class TlsRptDao : ITlsRptDao
    {
        private readonly IDocumentDbConfig _config;
        private readonly ILogger<TlsRptDao> _logger;
        private readonly IAmazonSimpleSystemsManagement _parameterStoreClient;
        private const string CollectionName = "reports";

        public TlsRptDao(IDocumentDbConfig config, ILogger<TlsRptDao> logger, IAmazonSimpleSystemsManagement parameterStoreClient)
        {
            _config = config;
            _logger = logger;
            _parameterStoreClient = parameterStoreClient;
        }

        public async Task Persist(ReportInfo jsonReport)
        {
            const string template = "mongodb://{0}:{1}@{2}/";
            string username = _config.Username;
            GetParameterResponse response = await _parameterStoreClient.GetParameterAsync(new GetParameterRequest { Name = username, WithDecryption = true });
            string password = response.Parameter.Value;
            string clusterEndpoint = _config.ClusterEndpoint;
            string connectionString = string.Format(template, username, password, clusterEndpoint);
            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));
            settings.AllowInsecureTls = true;
            settings.UseTls = true;

            MongoClient client = new MongoClient(settings);

            IMongoDatabase database = client.GetDatabase(_config.Database);
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(CollectionName);

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new KebabToPascalNamingStrategy
                {
                    ProcessDictionaryKeys = true
                }
            };

            byte[] data;
            using (MemoryStream ms = new MemoryStream())
            using (BsonDataWriter writer = new BsonDataWriter(ms))
            {
                JsonSerializer jsonSerializer = new JsonSerializer { ContractResolver = contractResolver };
                jsonSerializer.Serialize(writer, jsonReport);
                await writer.FlushAsync();
                data = ms.ToArray();
            }

            try
            {
                RawBsonDocument rawDocument = new RawBsonDocument(data);
                BsonDocument bsonDocument = rawDocument.Materialize(BsonBinaryReaderSettings.Defaults);
                await collection.InsertOneAsync(bsonDocument);
            }
            catch (MongoDuplicateKeyException)
            {
                _logger.LogInformation($"Insert failed due to duplicate key for report {jsonReport.Source}");
            }
        }
    }
}