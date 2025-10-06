using Microsoft.EntityFrameworkCore;
using Backend.Model.Entity;

namespace Backend.SQLDbContext
{
    public class SQLServerDbContext : DbContext
    {
        public SQLServerDbContext(DbContextOptions<SQLServerDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
        }

        // Khai báo DbSet cho các entity
        public DbSet<Administrator> Administrators { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceDetail> InvoiceDetails { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductDailyStat> ProductDailyStats { get; set; }
        public DbSet<RecentlyView> RecentlyViews { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình cho Administrator
            modelBuilder.Entity<Administrator>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasDefaultValueSql("NEWID()");

                entity.HasIndex(e => e.Username)
                    .IsUnique()
                    .HasDatabaseName("IX_Administrator_Username")
                    .HasFilter("Status = 1"); // Chỉ unique với Status = true

                entity.HasIndex(e => e.Status)
                    .HasDatabaseName("IX_Administrator_Status");
            });

            // Cấu hình cho Customer
            modelBuilder.Entity<Customer>(entity =>
            {
                // Tự động sinh GUID cho Id
                entity.Property(e => e.Id)
                    .HasDefaultValueSql("NEWID()");

                // Index cho PhoneNumber để tối ưu tìm kiếm, không unique
                entity.HasIndex(e => e.PhoneNumber)
                    .HasDatabaseName("IX_Customer_PhoneNumber");

                // Index cho CustomerName để hỗ trợ tìm kiếm
                entity.HasIndex(e => e.CustomerName)
                    .HasDatabaseName("IX_Customer_CustomerName");

                // Index cho Status để tối ưu truy vấn khách hàng active/inactive
                entity.HasIndex(e => e.Status)
                    .HasDatabaseName("IX_Customer_Status");

                // Index cho Email để tối ưu tìm kiếm, và ràng buộc unique
                entity.HasIndex(e => e.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_Customer_Email");
            });

            // Cấu hình cho Category
            modelBuilder.Entity<Category>(entity =>
            {
                // Index cho Name để tối ưu tìm kiếm, và ràng buộc unique
                entity.HasIndex(e => e.Name)
                    .IsUnique()
                    .HasDatabaseName("IX_Category_Name");
            });

            // Cấu hình cho SubCategory
            modelBuilder.Entity<SubCategory>(entity =>
            {
                // Quan hệ 1-n với Category
                entity.HasOne(e => e.Category)
                    .WithMany(e => e.SubCategories)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Index cho Name để tối ưu tìm kiếm
                entity.HasIndex(e => e.Name)
                    .HasDatabaseName("IX_SubCategory_Name");

                // Index composite cho CategoryId và Name, ràng buộc unique để tránh trùng tên subcategory trong cùng category
                entity.HasIndex(e => new { e.CategoryId, e.Name })
                    .IsUnique()
                    .HasDatabaseName("IX_SubCategory_CategoryId_Name");
            });

            // Cấu hình cho Product
            modelBuilder.Entity<Product>(entity =>
            {
                // Quan hệ 1-n với SubCategory
                entity.HasOne(e => e.SubCategory)
                    .WithMany(e => e.Products)
                    .HasForeignKey(e => e.SubCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Index cho SubCategoryId để tối ưu truy vấn sản phẩm theo subcategory
                entity.HasIndex(e => e.SubCategoryId)
                    .HasDatabaseName("IX_Product_SubCategoryId");

                // Giả sử Product có thể có thêm trường như Price hoặc Name trong tương lai, nhưng hiện tại chỉ index trên SubCategoryId
            });

            // Cấu hình cho Cart
            modelBuilder.Entity<Cart>(entity =>
            {
                // Quan hệ 1-n với Customer
                entity.HasOne(e => e.Customer)
                    .WithMany(e => e.Carts)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Quan hệ 1-n với Product
                entity.HasOne(e => e.Product)
                    .WithMany(e => e.Carts)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Index composite cho CustomerId và ProductId để tối ưu truy vấn giỏ hàng, và ràng buộc unique để tránh duplicate item trong cart
                entity.HasIndex(e => new { e.CustomerId, e.ProductId, e.Option })
                    .IsUnique()
                    .HasDatabaseName("IX_Cart_CustomerId_ProductId_Option");

                // Ràng buộc check cho Quantity > 0
                entity.ToTable(tb => tb.HasCheckConstraint("CK_Cart_Quantity_Positive", "[Quantity] > 0"));

                // Index cho Option nếu truy vấn theo option (ví dụ: size, color)
                entity.HasIndex(e => e.Option)
                    .HasDatabaseName("IX_Cart_Option");
            });

            // Cấu hình cho Invoice
            modelBuilder.Entity<Invoice>(entity =>
            {
                // Quan hệ 1-n với Customer
                entity.HasOne(e => e.Customer)
                    .WithMany(e => e.Invoices)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Index cho CreatedAt để tối ưu truy vấn theo thời gian (báo cáo doanh thu)
                entity.HasIndex(e => e.CreatedAt)
                    .HasDatabaseName("IX_Invoice_CreatedAt");

                // Index cho CustomerId để tối ưu truy vấn lịch sử hóa đơn của khách
                entity.HasIndex(e => e.CustomerId)
                    .HasDatabaseName("IX_Invoice_CustomerId");

                // Index cho DeliveryAddress để hỗ trợ tìm kiếm hóa đơn theo địa chỉ (nếu cần thống kê khu vực)
                entity.HasIndex(e => e.DeliveryAddress)
                    .HasDatabaseName("IX_Invoice_DeliveryAddress");
            });

            // Cấu hình cho InvoiceDetail
            modelBuilder.Entity<InvoiceDetail>(entity =>
            {
                // Quan hệ 1-n với Invoice
                entity.HasOne(e => e.Invoice)
                    .WithMany(e => e.InvoiceDetails)
                    .HasForeignKey(e => e.InvoiceId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Quan hệ 1-n với Product
                entity.HasOne(e => e.Product)
                    .WithMany(e => e.InvoiceDetails)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Index composite cho InvoiceId và ProductId để tối ưu chi tiết hóa đơn
                entity.HasIndex(e => new { e.InvoiceId, e.ProductId })
                    .HasDatabaseName("IX_InvoiceDetail_InvoiceId_ProductId");

                // Ràng buộc check cho Quantity > 0
                entity.ToTable(tb => tb.HasCheckConstraint("CK_InvoiceDetail_Quantity_Positive", "[Quantity] > 0"));

                // Index cho Option nếu truy vấn theo option
                entity.HasIndex(e => e.Option)
                    .HasDatabaseName("IX_InvoiceDetail_Option");
            });

            // Cấu hình cho ProductDailyStat
            modelBuilder.Entity<ProductDailyStat>(entity =>
            {
                // Quan hệ 1-n với Product
                entity.HasOne(e => e.Product)
                    .WithMany(e => e.ProductDailyStats)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Index composite cho ProductId và Date để tối ưu truy vấn thống kê, và ràng buộc unique để tránh duplicate stat cho cùng ngày
                entity.HasIndex(e => new { e.ProductId, e.Date })
                    .IsUnique()
                    .HasDatabaseName("IX_ProductDailyStat_ProductId_Date");

                // Ràng buộc check cho ViewsCount >= 0 và PurchasesCount >= 0
                entity.ToTable(tb =>
                {
                    tb.HasCheckConstraint("CK_ProductDailyStat_ViewsCount_NonNegative", "[ViewsCount] >= 0");
                    tb.HasCheckConstraint("CK_ProductDailyStat_PurchasesCount_NonNegative", "[PurchasesCount] >= 0");
                });

                // Index cho Date để tối ưu báo cáo tổng hợp theo ngày
                entity.HasIndex(e => e.Date)
                    .HasDatabaseName("IX_ProductDailyStat_Date");
            });

            // Cấu hình cho RecentlyView
            modelBuilder.Entity<RecentlyView>(entity =>
            {
                // Quan hệ 1-n với Customer
                entity.HasOne(e => e.Customer)
                    .WithMany(e => e.RecentlyViews)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Quan hệ 1-n với Product
                entity.HasOne(e => e.Product)
                    .WithMany(e => e.RecentlyViews)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Index composite cho CustomerId và ProductId để tối ưu, và ràng buộc unique để tránh duplicate view cho cùng product
                entity.HasIndex(e => new { e.CustomerId, e.ProductId })
                    .IsUnique()
                    .HasDatabaseName("IX_RecentlyView_CustomerId_ProductId");

                // Index cho ViewedAt để tối ưu truy vấn theo thời gian (lấy recently viewed)
                entity.HasIndex(e => new { e.CustomerId, e.ViewedAt })
                    .HasDatabaseName("IX_RecentlyView_CustomerId_ViewedAt");
            });
        }
    }
}