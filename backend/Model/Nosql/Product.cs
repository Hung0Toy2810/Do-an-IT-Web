using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Model.Nosql
{
    public class ProductDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? MongoId { get; set; }

        [BsonElement("id")]
        public long Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("slug")]
        public string Slug { get; set; } = string.Empty;

        [BsonElement("brand")]
        public string Brand { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("attributeOptions")]
        public Dictionary<string, List<string>> AttributeOptions { get; set; } = new();

        [BsonElement("variants")]
        public List<ProductVariant> Variants { get; set; } = new();

        [BsonElement("isDiscontinued")]
        public bool IsDiscontinued { get; set; } = false;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ProductVariant
    {
        [BsonElement("slug")]
        public string Slug { get; set; } = string.Empty;

        [BsonElement("attributes")]
        public Dictionary<string, string> Attributes { get; set; } = new();

        [BsonElement("stock")]
        public int Stock { get; set; }

        [BsonElement("originalPrice")]
        public decimal OriginalPrice { get; set; }

        [BsonElement("discountedPrice")]
        public decimal DiscountedPrice { get; set; }

        [BsonElement("images")]
        public List<string> Images { get; set; } = new();

        [BsonElement("specifications")]
        public List<Specification> Specifications { get; set; } = new();
    }

    public class Specification
    {
        [BsonElement("label")]
        public string Label { get; set; } = string.Empty;

        [BsonElement("value")]
        public string Value { get; set; } = string.Empty;
    }
}