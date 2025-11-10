using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;  

namespace Backend.Model.Nosql.ViettelPost
{
    public class ProvinceDocument
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("provinceId")]
        public int ProvinceId { get; set; }

        [BsonElement("provinceCode")]
        public string ProvinceCode { get; set; } = string.Empty;

        [BsonElement("provinceName")]
        public string ProvinceName { get; set; } = string.Empty;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
    public class Province
    {
        [JsonPropertyName("PROVINCE_ID")]
        public int ProvinceId { get; set; }

        [JsonPropertyName("PROVINCE_CODE")]
        public string ProvinceCode { get; set; } = string.Empty;

        [JsonPropertyName("PROVINCE_NAME")]
        public string ProvinceName { get; set; } = string.Empty;
    }
    public class District
    {
        [JsonPropertyName("DISTRICT_ID")]
        public int DistrictId { get; set; }

        [JsonPropertyName("DISTRICT_VALUE")]
        public string DistrictValue { get; set; } = string.Empty;

        [JsonPropertyName("DISTRICT_NAME")]
        public string DistrictName { get; set; } = string.Empty;

        [JsonPropertyName("PROVINCE_ID")]
        public int ProvinceId { get; set; }
    }
    public class Ward
    {
        [JsonPropertyName("WARDS_ID")]
        public int WardId { get; set; }

        [JsonPropertyName("WARDS_NAME")]
        public string WardName { get; set; } = string.Empty;

        [JsonPropertyName("DISTRICT_ID")]
        public int DistrictId { get; set; }
    }

    public class DistrictDocument
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("districtId")]
        public int DistrictId { get; set; }

        [BsonElement("districtValue")]
        public string DistrictValue { get; set; } = string.Empty;

        [BsonElement("districtName")]
        public string DistrictName { get; set; } = string.Empty;

        [BsonElement("provinceId")]
        public int ProvinceId { get; set; }

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
    public class WardDocument
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("wardId")]
        public int WardId { get; set; }

        [BsonElement("wardName")]
        public string WardName { get; set; } = string.Empty;

        [BsonElement("districtId")]
        public int DistrictId { get; set; }

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
    public class AddressValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public Province? Province { get; set; }
        public District? District { get; set; }
        public Ward? Ward { get; set; }
    }
}