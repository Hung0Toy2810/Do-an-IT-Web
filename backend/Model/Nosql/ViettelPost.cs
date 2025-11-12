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

    /// <summary>
    /// Yêu cầu tạo đơn hàng gửi lên ViettelPost
    /// Tài liệu: https://partner.viettelpost.vn/v2/order/createOrder
    /// </summary>
    public class ViettelPostOrderRequest
    {
        [JsonPropertyName("ORDER_NUMBER")]
        [Required]
        public string OrderNumber { get; set; } = string.Empty;

        [JsonPropertyName("GROUPADDRESS_ID")]
        public int GroupAddressId { get; set; }

        [JsonPropertyName("CUS_ID")]
        public int CusId { get; set; }

        [JsonPropertyName("DELIVERY_DATE")]
        public string DeliveryDate { get; set; } = DateTime.Now.AddDays(1).ToString("dd/MM/yyyy HH:mm:ss");

        // === NGƯỜI GỬI ===
        [JsonPropertyName("SENDER_FULLNAME")]
        public string SenderFullname { get; set; } = "Shop";

        [JsonPropertyName("SENDER_ADDRESS")]
        public string SenderAddress { get; set; } = "Hà Nội";

        [JsonPropertyName("SENDER_PHONE")]
        public string SenderPhone { get; set; } = "0901234567";

        [JsonPropertyName("SENDER_EMAIL")]
        public string SenderEmail { get; set; } = "shop@example.com";

        [JsonPropertyName("SENDER_WARD")]
        public int SenderWard { get; set; } = 0;

        [JsonPropertyName("SENDER_DISTRICT")]
        public int SenderDistrict { get; set; }

        [JsonPropertyName("SENDER_PROVINCE")]
        public int SenderProvince { get; set; }

        [JsonPropertyName("SENDER_LATITUDE")]
        public double SenderLatitude { get; set; } = 0;

        [JsonPropertyName("SENDER_LONGITUDE")]
        public double SenderLongitude { get; set; } = 0;

        // === NGƯỜI NHẬN ===
        [JsonPropertyName("RECEIVER_FULLNAME")]
        public string ReceiverFullname { get; set; } = string.Empty;

        [JsonPropertyName("RECEIVER_ADDRESS")]
        public string ReceiverAddress { get; set; } = string.Empty;

        [JsonPropertyName("RECEIVER_PHONE")]
        public string ReceiverPhone { get; set; } = string.Empty;

        [JsonPropertyName("RECEIVER_EMAIL")]
        public string ReceiverEmail { get; set; } = string.Empty;

        [JsonPropertyName("RECEIVER_WARD")]
        public int ReceiverWard { get; set; } = 0;

        [JsonPropertyName("RECEIVER_DISTRICT")]
        public int ReceiverDistrict { get; set; }

        [JsonPropertyName("RECEIVER_PROVINCE")]
        public int ReceiverProvince { get; set; }

        [JsonPropertyName("RECEIVER_LATITUDE")]
        public double ReceiverLatitude { get; set; } = 0;

        [JsonPropertyName("RECEIVER_LONGITUDE")]
        public double ReceiverLongitude { get; set; } = 0;

        // === THÔNG TIN HÀNG HÓA ===
        [JsonPropertyName("PRODUCT_NAME")]
        public string ProductName { get; set; } = "Sản phẩm";

        [JsonPropertyName("PRODUCT_DESCRIPTION")]
        public string ProductDescription { get; set; } = string.Empty;

        [JsonPropertyName("PRODUCT_QUANTITY")]
        public int ProductQuantity { get; set; } = 1;

        [JsonPropertyName("PRODUCT_PRICE")]
        public int ProductPrice { get; set; } = 0;

        [JsonPropertyName("PRODUCT_WEIGHT")]
        public int ProductWeight { get; set; } = 1000; // gram

        [JsonPropertyName("PRODUCT_LENGTH")]
        public int ProductLength { get; set; } = 20; // cm

        [JsonPropertyName("PRODUCT_WIDTH")]
        public int ProductWidth { get; set; } = 15; // cm

        [JsonPropertyName("PRODUCT_HEIGHT")]
        public int ProductHeight { get; set; } = 10; // cm

        [JsonPropertyName("PRODUCT_TYPE")]
        public string ProductType { get; set; } = "HH"; // HH: Hàng hóa

        // === DỊCH VỤ & GHI CHÚ ===
        [JsonPropertyName("ORDER_PAYMENT")]
        public int OrderPayment { get; set; } = 3; // 1: Người gửi trả, 3: Người nhận trả

        [JsonPropertyName("ORDER_SERVICE")]
        public string OrderService { get; set; } = "VCN"; // Vận chuyển nhanh

        [JsonPropertyName("ORDER_SERVICE_ADD")]
        public string OrderServiceAdd { get; set; } = string.Empty;

        [JsonPropertyName("ORDER_VOUCHER")]
        public string OrderVoucher { get; set; } = string.Empty;

        [JsonPropertyName("ORDER_NOTE")]
        public string OrderNote { get; set; } = "Cho xem hàng, không thử";

        // === TIỀN ===
        [JsonPropertyName("MONEY_COLLECTION")]
        public int MoneyCollection { get; set; } = 0; // Thu hộ

        [JsonPropertyName("MONEY_TOTALFEE")]
        public int MoneyTotalFee { get; set; } = 0;

        [JsonPropertyName("MONEY_FEECOD")]
        public int MoneyFeeCod { get; set; } = 0;

        [JsonPropertyName("MONEY_FEEVAS")]
        public int MoneyFeeVas { get; set; } = 0;

        [JsonPropertyName("MONEY_FEEINSURRANCE")]
        public int MoneyFeeInsurance { get; set; } = 0;

        [JsonPropertyName("MONEY_FEE")]
        public int MoneyFee { get; set; } = 0;

        [JsonPropertyName("MONEY_FEEOTHER")]
        public int MoneyFeeOther { get; set; } = 0;

        [JsonPropertyName("MONEY_TOTALVAT")]
        public int MoneyTotalVat { get; set; } = 0;

        [JsonPropertyName("MONEY_TOTAL")]
        public int MoneyTotal { get; set; } = 0;

        // === DANH SÁCH SẢN PHẨM CHI TIẾT ===
        [JsonPropertyName("LIST_ITEM")]
        public List<ViettelPostOrderItem> ListItem { get; set; } = new();
    }

    public class ViettelPostOrderItem
    {
        [JsonPropertyName("PRODUCT_NAME")]
        public string ProductName { get; set; } = "Sản phẩm";

        [JsonPropertyName("PRODUCT_PRICE")]
        public int ProductPrice { get; set; } = 0;

        [JsonPropertyName("PRODUCT_WEIGHT")]
        public int ProductWeight { get; set; } = 1000; // gram

        [JsonPropertyName("PRODUCT_QUANTITY")]
        public int ProductQuantity { get; set; } = 1;
    }

    /// <summary>
    /// Phản hồi từ ViettelPost khi tạo đơn thành công
    /// </summary>
    public class ViettelPostOrderResponse
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("error")]
        public bool Error { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public ViettelPostOrderData? Data { get; set; }
    }

    public class ViettelPostOrderData
    {
        [JsonPropertyName("ORDER_NUMBER")]
        public string OrderNumber { get; set; } = string.Empty;

        [JsonPropertyName("MONEY_COLLECTION")]
        public int MoneyCollection { get; set; }

        [JsonPropertyName("EXCHANGE_WEIGHT")]
        public int ExchangeWeight { get; set; }

        [JsonPropertyName("MONEY_TOTAL")]
        public int MoneyTotal { get; set; }

        [JsonPropertyName("MONEY_TOTAL_FEE")]
        public int MoneyTotalFee { get; set; }

        [JsonPropertyName("MONEY_FEE")]
        public int MoneyFee { get; set; }

        [JsonPropertyName("MONEY_COLLECTION_FEE")]
        public int MoneyCollectionFee { get; set; }

        [JsonPropertyName("MONEY_OTHER_FEE")]
        public int MoneyOtherFee { get; set; }

        [JsonPropertyName("MONEY_VAS")]
        public object? MoneyVas { get; set; }

        [JsonPropertyName("MONEY_VAT")]
        public int MoneyVat { get; set; }

        [JsonPropertyName("KPI_HT")]
        public int KpiHt { get; set; }
    }
}