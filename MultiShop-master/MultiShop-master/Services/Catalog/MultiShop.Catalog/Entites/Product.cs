using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MultiShop.Catalog.Entites
{
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ProductId { get; set; }
        public string ProductName { get; set; }

        // Mongo'da tip karisikligi (Int32 / Double / Decimal128) yuzunden
        // "$lte" ve "$gte" sorgu karsilastirmalari hatali calisiyordu.
        // Decimal128 olarak sabitleyince hem yazimda hem okumada filtre tutarli.
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal ProductPrice { get; set; }
        public string ProductImageUrl { get; set; }
        public string ProductDescription { get; set; }

        public string Color { get; set; }
        public string? ColorCode { get; set; }
        public string Size { get; set; }
        public int Stock { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string CategoryId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? BrandId { get; set; }

        [BsonIgnore]
        public Category Category { get; set; }
    }
}
