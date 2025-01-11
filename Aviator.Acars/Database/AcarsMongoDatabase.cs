using Aviator.Acars.Config;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Compression;
using MongoDB.Driver.Core.Configuration;

namespace Aviator.Acars.Database;

public class AcarsMongoDatabase(MongoDbConfig config) : IAcarsDatabase
{
    private readonly MongoClient _client = new(config.ConnectionString);

    private async Task SaveAcarsAsBsonAsync(string jsonString, CancellationToken cancellationToken = default)
    {
        var db = _client.GetDatabase(config.Database);
        var collection = db.GetCollection<BsonDocument>(config.Collection);

        await collection.InsertOneAsync(BsonDocument.Parse(jsonString), cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task InsertAsync(byte[] bytes, CancellationToken cancellationToken = default)
    {
        var byteString = System.Text.Encoding.Default.GetString(bytes);
        await SaveAcarsAsBsonAsync(byteString, cancellationToken).ConfigureAwait(false);
    }
}