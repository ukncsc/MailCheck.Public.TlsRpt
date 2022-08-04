using System.Diagnostics;
using System.Threading.Tasks;
using MailCheck.TlsRpt.Reports.Api.Config;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Bson;
using Newtonsoft.Json;
using MongoDB.Bson.IO;
using MailCheck.TlsRpt.Reports.Api.Domain;

namespace MailCheck.TlsRpt.Reports.Api.Dao
{
    public interface IReportsApiEntitiesDao
    {
        Task<TlsRptReportsEntityResponse> Read(string domain);
    }

    public class ReportsApiEntitiesDao : IReportsApiEntitiesDao
    {
        private readonly IDocumentDbConfig _config;
        private readonly ILogger<ReportsApiDao> _logger;
        private readonly IMongoClientProvider _mongoClientProvider;

        public ReportsApiEntitiesDao(IDocumentDbConfig config, ILogger<ReportsApiDao> logger, IMongoClientProvider mongoClientProvider)
        {
            _config = config;
            _logger = logger;
            _mongoClientProvider = mongoClientProvider;
        }

        public async Task<TlsRptReportsEntityResponse> Read(string domain)
        {
            Stopwatch stopwatch = new Stopwatch();
            _logger.LogInformation($"Beginning {nameof(Read)} for domain {domain}");
            stopwatch.Start();

            IMongoCollection<RawBsonDocument> collection = await GetCollection();

            BsonDocument filter = new BsonDocument
            {
                ["Domain"] = domain
            };

            RawBsonDocument doc = await collection
                .Find(filter)
                .FirstOrDefaultAsync();

            if (doc != null)
            {
                using (ByteBufferStream stream = new ByteBufferStream(doc.Slice))
                using (var reader = new Newtonsoft.Json.Bson.BsonReader(stream))
                {
                    JsonSerializer ser = new JsonSerializer();
                    _logger.LogInformation($"{nameof(Read)} retrieved from database in {stopwatch.ElapsedMilliseconds}");
                    stopwatch.Stop();
                    return ser.Deserialize<TlsRptReportsEntityResponse>(reader);
                }
            }

            _logger.LogInformation($"{nameof(Read)} returned null from database in {stopwatch.ElapsedMilliseconds}");
            stopwatch.Stop();
            return null;
        }

        private async Task<IMongoCollection<RawBsonDocument>> GetCollection()
        {
            IMongoClient client = await _mongoClientProvider.GetMongoClient();

            IMongoDatabase database = client.GetDatabase(_config.Database);
            IMongoCollection<RawBsonDocument> collection = database.GetCollection<RawBsonDocument>("entities");
            var indexKeys = new BsonDocument
            {
                ["Domain"] = 1
            };
            BsonDocumentIndexKeysDefinition<RawBsonDocument> keysDefinition =
                new BsonDocumentIndexKeysDefinition<RawBsonDocument>(indexKeys);
            await collection.Indexes.CreateOneAsync(new CreateIndexModel<RawBsonDocument>(keysDefinition, new CreateIndexOptions { Unique = true }));
            return collection;
        }
    }
}