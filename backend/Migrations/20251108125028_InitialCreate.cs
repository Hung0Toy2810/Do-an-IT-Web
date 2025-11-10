using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Administrators",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Administrators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, collation: "Vietnamese_CI_AS"),
                    Slug = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CustomerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StandardShippingAddress_ProvinceId = table.Column<int>(type: "int", nullable: false),
                    StandardShippingAddress_ProvinceCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StandardShippingAddress_ProvinceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StandardShippingAddress_DistrictId = table.Column<int>(type: "int", nullable: false),
                    StandardShippingAddress_DistrictValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StandardShippingAddress_DistrictName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StandardShippingAddress_WardsId = table.Column<int>(type: "int", nullable: false),
                    StandardShippingAddress_WardsName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StandardShippingAddress_DetailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    HashPassword = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    AvtURL = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubCategories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, collation: "Vietnamese_CI_AS"),
                    Slug = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    CategoryId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrackingCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReceiverName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReceiverPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ShippingAddress_ProvinceId = table.Column<int>(type: "int", nullable: false),
                    ShippingAddress_ProvinceCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingAddress_ProvinceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingAddress_DistrictId = table.Column<int>(type: "int", nullable: false),
                    ShippingAddress_DistrictValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingAddress_DistrictName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingAddress_WardsId = table.Column<int>(type: "int", nullable: false),
                    ShippingAddress_WardsName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingAddress_DetailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Carrier = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EstimatedDelivery = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubCategoryId = table.Column<long>(type: "bigint", nullable: false),
                    Rating = table.Column<float>(type: "real", nullable: false),
                    TotalRatings = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_SubCategories_SubCategoryId",
                        column: x => x.SubCategoryId,
                        principalTable: "SubCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceStatusHistories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceStatusHistories_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VNPayPayments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<long>(type: "bigint", nullable: false),
                    TransactionCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    ResponseCode = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VNPayPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VNPayPayments_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Carts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    VariantSlug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carts", x => x.Id);
                    table.CheckConstraint("CK_Cart_Quantity_Positive", "[Quantity] > 0");
                    table.ForeignKey(
                        name: "FK_Carts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Carts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductDailyStats",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ViewsCount = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductDailyStats", x => x.Id);
                    table.CheckConstraint("CK_ProductDailyStat_ViewsCount_NonNegative", "[ViewsCount] >= 0");
                    table.ForeignKey(
                        name: "FK_ProductDailyStats_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecentlyViews",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    ViewedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecentlyViews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecentlyViews_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecentlyViews_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShipmentBatches",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    ImportedQuantity = table.Column<int>(type: "int", nullable: false),
                    RemainingQuantity = table.Column<int>(type: "int", nullable: false),
                    ImportPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ImportedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VariantSlug = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShipmentBatches_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceDetails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    VariantSlug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ShipmentBatchId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceDetails", x => x.Id);
                    table.CheckConstraint("CK_InvoiceDetail_Quantity_Positive", "[Quantity] > 0");
                    table.ForeignKey(
                        name: "FK_InvoiceDetails_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceDetails_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceDetails_ShipmentBatches_ShipmentBatchId",
                        column: x => x.ShipmentBatchId,
                        principalTable: "ShipmentBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Administrator_Status",
                table: "Administrators",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Administrator_Username",
                table: "Administrators",
                column: "Username",
                unique: true,
                filter: "Status = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Cart_CustomerId_ProductId_Option",
                table: "Carts",
                columns: new[] { "CustomerId", "ProductId", "VariantSlug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cart_Option",
                table: "Carts",
                column: "VariantSlug");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_ProductId",
                table: "Carts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Category_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Category_Slug",
                table: "Categories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comment_CreatedAt",
                table: "Comments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_Rating",
                table: "Comments",
                column: "Rating");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_CustomerId",
                table: "Comments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ProductId",
                table: "Comments",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_CustomerName",
                table: "Customers",
                column: "CustomerName");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_Email",
                table: "Customers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_PhoneNumber",
                table: "Customers",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_Status",
                table: "Customers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceDetail_InvoiceId_ProductId",
                table: "InvoiceDetails",
                columns: new[] { "InvoiceId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceDetail_Option",
                table: "InvoiceDetails",
                column: "VariantSlug");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceDetails_ProductId",
                table: "InvoiceDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceDetails_ShipmentBatchId",
                table: "InvoiceDetails",
                column: "ShipmentBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_CreatedAt",
                table: "Invoices",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_CustomerId",
                table: "Invoices",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_ReceiverPhone",
                table: "Invoices",
                column: "ReceiverPhone");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_Status",
                table: "Invoices",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_TrackingCode",
                table: "Invoices",
                column: "TrackingCode");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceStatusHistory_CreatedAt",
                table: "InvoiceStatusHistories",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceStatusHistory_InvoiceId",
                table: "InvoiceStatusHistories",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductDailyStat_Date",
                table: "ProductDailyStats",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_ProductDailyStat_ProductId_Date",
                table: "ProductDailyStats",
                columns: new[] { "ProductId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Product_SubCategoryId",
                table: "Products",
                column: "SubCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RecentlyView_CustomerId_ProductId",
                table: "RecentlyViews",
                columns: new[] { "CustomerId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecentlyView_CustomerId_ViewedAt",
                table: "RecentlyViews",
                columns: new[] { "CustomerId", "ViewedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RecentlyViews_ProductId",
                table: "RecentlyViews",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentBatch_BatchCode",
                table: "ShipmentBatches",
                column: "BatchCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentBatch_ProductId",
                table: "ShipmentBatches",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategory_CategoryId_Name",
                table: "SubCategories",
                columns: new[] { "CategoryId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubCategory_CategoryId_Slug",
                table: "SubCategories",
                columns: new[] { "CategoryId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubCategory_Name",
                table: "SubCategories",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_VNPayPayment_InvoiceId",
                table: "VNPayPayments",
                column: "InvoiceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VNPayPayment_TransactionCode",
                table: "VNPayPayments",
                column: "TransactionCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Administrators");

            migrationBuilder.DropTable(
                name: "Carts");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "InvoiceDetails");

            migrationBuilder.DropTable(
                name: "InvoiceStatusHistories");

            migrationBuilder.DropTable(
                name: "ProductDailyStats");

            migrationBuilder.DropTable(
                name: "RecentlyViews");

            migrationBuilder.DropTable(
                name: "VNPayPayments");

            migrationBuilder.DropTable(
                name: "ShipmentBatches");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "SubCategories");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
