using System;
using System.IO;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using MailCheck.Common.Util;
using MailCheck.TlsRpt.Reports.Entity.Config;
using MailCheck.TlsRpt.Reports.Contracts;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MailCheck.TlsRpt.Reports.Entity.Dao
{
    public interface ITlsRptReportsEntityDao
    {
        Task Create(TlsRptReportsEntityState state);
        Task<TlsRptReportsEntityState> Read(string domain);
        Task Update(TlsRptReportsEntityState state);
        Task Delete(string domain);
    }

    public class TlsRptReportsEntityDao : ITlsRptReportsEntityDao
    {
        private readonly IDocumentDbConfig _config;
        private readonly IClock _clock;
        private readonly ILogger<TlsRptReportsEntityDao> _logger;
        private readonly IMongoClientProvider _mongoClientProvider;
        private const string CollectionName = "entities";

        public TlsRptReportsEntityDao(IDocumentDbConfig config,
            IClock clock,
            ILogger<TlsRptReportsEntityDao> logger,
            IMongoClientProvider mongoClientProvider)
        {
            _config = config;
            _clock = clock;
            _logger = logger;
            _mongoClientProvider = mongoClientProvider;
        }

        public async Task Create(TlsRptReportsEntityState state)
        {
            Stopwatch stopwatch = new Stopwatch();
            _logger.LogInformation($"Beginning {nameof(Create)} for domain {state.Domain}");
            stopwatch.Start();

            IMongoCollection<RawBsonDocument> collection = await GetCollection();

            await collection.InsertOneAsync(Serialize(state));
            _logger.LogInformation($"{nameof(Create)} completed in {stopwatch.ElapsedMilliseconds}");
            stopwatch.Stop();
        }

        public async Task<TlsRptReportsEntityState> Read(string domain)
        {
            Stopwatch stopwatch = new Stopwatch();
            _logger.LogInformation($"Beginning {nameof(Read)} for domain {domain}");
            stopwatch.Start();

            IMongoCollection<RawBsonDocument> collection = await GetCollection();

            var filter = new BsonDocument
            {
                ["Domain"] = domain
            };
            try
            {
                RawBsonDocument doc = await collection
                    .Find(filter)
                    .FirstOrDefaultAsync();
                TlsRptReportsEntityState state = Deserialize(doc);

                _logger.LogInformation($"{nameof(Read)} retrievied in {stopwatch.ElapsedMilliseconds}");
                stopwatch.Stop();

                return state;
                
            }
            catch (InvalidOperationException e)
            {
                _logger.LogInformation($"{nameof(Read)} resulted in {nameof(InvalidOperationException)} in {stopwatch.ElapsedMilliseconds}");
                stopwatch.Stop();
                return null;
            }
        }

        public async Task Update(TlsRptReportsEntityState state)
        {
            Stopwatch stopwatch = new Stopwatch();
            _logger.LogInformation($"Beginning {nameof(Update)} for domain {state.Domain}");
            stopwatch.Start();

            int prevVersion = state.Version;
            state.Version++;
            DateTime currentTime = _clock.GetDateTimeUtc();
            state.LastUpdated = currentTime;
            IMongoCollection<RawBsonDocument> collection = await GetCollection();

            var rawBsonDocument = Serialize(state);

            var filter = new BsonDocument
            {
                ["Domain"] = state.Domain,
                ["Version"] = prevVersion
            };
            var result = await collection.ReplaceOneAsync(filter, rawBsonDocument);
            if (result.IsAcknowledged && result.ModifiedCount == 1)
            {
                _logger.LogInformation($"{nameof(Update)} completed in {stopwatch.ElapsedMilliseconds}");
                stopwatch.Stop();
                return;
            }

            throw new Exception($"Failed to Update entity for {state.Domain}, version mismatch occured.");
        }

        public async Task Delete(string domain)
        {
            Stopwatch stopwatch = new Stopwatch();
            _logger.LogInformation($"Beginning {nameof(Delete)} for domain {domain}");
            stopwatch.Start();

            IMongoCollection<RawBsonDocument> collection = await GetCollection();

            var filter = new BsonDocument
            {
                ["Domain"] = domain,
            };
            await collection.FindOneAndDeleteAsync(filter);
            _logger.LogInformation($"{nameof(Delete)} completed in {stopwatch.ElapsedMilliseconds}");
            stopwatch.Stop();
        }

        private TlsRptReportsEntityState Deserialize(RawBsonDocument doc)
        {
            if (doc == null) return null;
            
            using (var stream = new ByteBufferStream(doc.Slice))
            using (var reader = new Newtonsoft.Json.Bson.BsonReader(stream))
            {
                var ser = new JsonSerializer();
                return ser.Deserialize<TlsRptReportsEntityState>(reader);
            }
        }

        private static RawBsonDocument Serialize(TlsRptReportsEntityState state)
        {
            byte[] data;
            using (MemoryStream ms = new MemoryStream())
            using (BsonDataWriter writer = new BsonDataWriter(ms))
            {
                JsonSerializer jsonSerializer = new JsonSerializer();
                jsonSerializer.Serialize(writer, state);
                writer.Flush();
                data = ms.ToArray();
            }

            RawBsonDocument rawDocument = new RawBsonDocument(data);
            return rawDocument;
        }

        private async Task<IMongoCollection<RawBsonDocument>> GetCollection()
        {
            IMongoClient client = await _mongoClientProvider.GetMongoClient();

            IMongoDatabase database = client.GetDatabase(_config.Database);
            IMongoCollection<RawBsonDocument> collection = database.GetCollection<RawBsonDocument>(CollectionName);
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