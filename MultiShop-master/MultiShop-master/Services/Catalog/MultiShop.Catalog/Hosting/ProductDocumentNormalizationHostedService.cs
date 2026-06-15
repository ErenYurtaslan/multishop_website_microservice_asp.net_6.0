using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MultiShop.Catalog.Settings;

namespace MultiShop.Catalog.Hosting
{
    /// <summary>
    /// "stock" / "productImageUrl" camelCase alanlarini C# <see cref="Entites.Product"/> ile
    /// uyumlu "Stock" / "ProductImageUrl" alanlarina toplar; yinelenen camel anahtarlarini siler.
    /// </summary>
    public class ProductDocumentNormalizationHostedService : IHostedService
    {
        private readonly IDatabaseSettings _db;
        private readonly ILogger<ProductDocumentNormalizationHostedService>? _log;

        public ProductDocumentNormalizationHostedService(
            IOptions<DatabaseSettings> options,
            ILogger<ProductDocumentNormalizationHostedService> log)
        {
            _db = options.Value;
            _log = log;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(() => NormalizeAsync(cancellationToken), cancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private async Task NormalizeAsync(CancellationToken cancellationToken)
        {
            try
            {
                var client = new MongoClient(_db.ConnectionString);
                var database = client.GetDatabase(_db.DatabaseName);
                var coll = database.GetCollection<BsonDocument>(_db.ProductCollectionName);
                var all = await coll.Find(FilterDefinition<BsonDocument>.Empty)
                    .ToListAsync(cancellationToken);

                var touched = 0;
                foreach (var doc in all)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    var setDoc = new BsonDocument();
                    var stockP = GetInt32(doc, "Stock");
                    var stockC = GetInt32(doc, "stock");
                    var effectiveStock = stockP > 0 ? stockP : stockC;
                    if (effectiveStock > 0 && GetInt32(doc, "Stock") != effectiveStock)
                        setDoc["Stock"] = effectiveStock;

                    var imgP = GetString(doc, "ProductImageUrl");
                    var imgC = GetString(doc, "productImageUrl");
                    if (string.IsNullOrWhiteSpace(imgP) && !string.IsNullOrWhiteSpace(imgC))
                        setDoc["ProductImageUrl"] = imgC!.Trim();

                    var brandP = GetString(doc, "BrandId");
                    var brandC = GetString(doc, "brandId");
                    if (string.IsNullOrWhiteSpace(brandP) && !string.IsNullOrWhiteSpace(brandC))
                        setDoc["BrandId"] = brandC!.Trim();

                    var colorCodeP = GetString(doc, "ColorCode");
                    var colorCodeC = GetString(doc, "colorCode");
                    if (string.IsNullOrWhiteSpace(colorCodeP) && !string.IsNullOrWhiteSpace(colorCodeC))
                        setDoc["ColorCode"] = colorCodeC!.Trim();

                    var updates = new List<UpdateDefinition<BsonDocument>>();
                    if (setDoc.ElementCount > 0)
                        updates.Add(new BsonDocumentUpdateDefinition<BsonDocument>(new BsonDocument("$set", setDoc)));
                    if (doc.Contains("stock"))
                        updates.Add(Builders<BsonDocument>.Update.Unset("stock"));
                    if (doc.Contains("productImageUrl") &&
                        (setDoc.Contains("ProductImageUrl") || !string.IsNullOrWhiteSpace(GetString(doc, "ProductImageUrl"))))
                        updates.Add(Builders<BsonDocument>.Update.Unset("productImageUrl"));
                    if (doc.Contains("brandId") &&
                        (setDoc.Contains("BrandId") || !string.IsNullOrWhiteSpace(GetString(doc, "BrandId"))))
                        updates.Add(Builders<BsonDocument>.Update.Unset("brandId"));
                    if (doc.Contains("colorCode") &&
                        (setDoc.Contains("ColorCode") || !string.IsNullOrWhiteSpace(GetString(doc, "ColorCode"))))
                        updates.Add(Builders<BsonDocument>.Update.Unset("colorCode"));

                    if (updates.Count == 0) continue;

                    var filter = Builders<BsonDocument>.Filter.Eq("_id", doc["_id"]);
                    await coll.UpdateOneAsync(
                        filter,
                        Builders<BsonDocument>.Update.Combine(updates),
                        cancellationToken: cancellationToken);
                    touched++;
                }

                _log?.LogInformation("ProductDocumentNormalization: {Touched} belge (toplam {Total}).", touched, all.Count);
            }
            catch (Exception ex)
            {
                _log?.LogWarning(ex, "ProductDocumentNormalization basarisiz; Catalog yine de ayaga kalkar.");
            }
        }

        private static int GetInt32(BsonDocument d, string name)
        {
            if (!d.Contains(name) || d[name].IsBsonNull) return 0;
            var v = d[name];
            return v.BsonType switch
            {
                BsonType.Int32 => v.AsInt32,
                BsonType.Int64 => (int)Math.Clamp(v.AsInt64, int.MinValue, int.MaxValue),
                BsonType.Double => (int)Math.Round(v.AsDouble, MidpointRounding.AwayFromZero),
                BsonType.Decimal128 => (int)Math.Round((decimal)v.AsDecimal, MidpointRounding.AwayFromZero),
                _ => 0
            };
        }

        private static string? GetString(BsonDocument d, string name) =>
            d.Contains(name) && d[name] is { IsBsonNull: false, BsonType: BsonType.String }
                ? d[name].AsString
                : null;
    }
}
