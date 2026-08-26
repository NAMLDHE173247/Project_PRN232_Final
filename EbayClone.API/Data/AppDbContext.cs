using EbayClone.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EbayClone.API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<OrderTable> Orders => Set<OrderTable>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<ShippingInfo> ShippingInfos => Set<ShippingInfo>();
    public DbSet<Dispute> Disputes => Set<Dispute>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public DbSet<ReturnRequest> ReturnRequests => Set<ReturnRequest>();
    public DbSet<AdminAuditLog> AdminAuditLogs => Set<AdminAuditLog>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");
            entity.Property(x => x.Id).HasColumnName("id");
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(100);
            entity.Property(x => x.PasswordHash).HasColumnName("password").HasMaxLength(255);
            entity.Property(x => x.FullName).HasColumnName("username").HasMaxLength(100);
            entity.Property(x => x.Role).HasColumnName("role").HasMaxLength(20);
            entity.Property(x => x.ModerationStatus).HasColumnName("moderationStatus").HasMaxLength(20).IsConcurrencyToken();
            entity.Property(x => x.ModerationReason).HasColumnName("moderationReason").HasMaxLength(500);
            entity.Property(x => x.ModeratedBy).HasColumnName("moderatedBy");
            entity.Property(x => x.ModeratedAtUtc).HasColumnName("moderatedAtUtc").HasColumnType("datetime2(0)");
            entity.Ignore(x => x.CreatedAtUtc);
        });
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Product");
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Name).HasColumnName("title").HasMaxLength(255);
            entity.Property(x => x.Price).HasColumnName("price").HasPrecision(10, 2);
            entity.Property(x => x.SellerId).HasColumnName("sellerId");
            entity.Property(x => x.ModerationStatus).HasColumnName("moderationStatus").HasMaxLength(20).IsConcurrencyToken();
            entity.Property(x => x.ModerationReason).HasColumnName("moderationReason").HasMaxLength(500);
            entity.Property(x => x.ModeratedBy).HasColumnName("moderatedBy");
            entity.Property(x => x.ModeratedAtUtc).HasColumnName("moderatedAtUtc").HasColumnType("datetime2(0)");
            entity.Ignore(x => x.CreatedAtUtc);
        });
        modelBuilder.Entity<OrderTable>(entity =>
        {
            entity.ToTable("OrderTable");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.BuyerId).HasColumnName("buyerId");
            entity.Property(x => x.AddressId).HasColumnName("addressId");
            entity.Property(x => x.TotalPrice).HasColumnName("totalPrice").HasPrecision(10, 2);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(20);
            entity.Property(x => x.OrderDate).HasColumnName("orderDate").HasColumnType("datetime");
            entity.Ignore(x => x.Address);
            entity.Ignore(x => x.Buyer);
            entity.Ignore(x => x.Disputes);
            entity.Ignore(x => x.OrderItems);
            entity.Ignore(x => x.Payments);
            entity.Ignore(x => x.ReturnRequests);
            entity.Ignore(x => x.ShippingInfos);
        });
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItem");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.OrderId).HasColumnName("orderId");
            entity.Property(x => x.ProductId).HasColumnName("productId");
            entity.Property(x => x.Quantity).HasColumnName("quantity");
            entity.Property(x => x.UnitPrice).HasColumnName("unitPrice").HasPrecision(10, 2);
            entity.Ignore(x => x.Order);
            entity.Ignore(x => x.Product);
        });
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payment");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.OrderId).HasColumnName("orderId");
            entity.Property(x => x.UserId).HasColumnName("userId");
            entity.Property(x => x.Amount).HasColumnName("amount").HasPrecision(10, 2);
            entity.Property(x => x.Method).HasColumnName("method").HasMaxLength(50);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(20);
            entity.Property(x => x.PaidAt).HasColumnName("paidAt").HasColumnType("datetime");
            entity.Ignore(x => x.Order);
            entity.Ignore(x => x.User);
        });
        modelBuilder.Entity<ShippingInfo>(entity =>
        {
            entity.ToTable("ShippingInfo");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.OrderId).HasColumnName("orderId");
            entity.Property(x => x.Carrier).HasColumnName("carrier").HasMaxLength(100);
            entity.Property(x => x.TrackingNumber).HasColumnName("trackingNumber").HasMaxLength(100);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(50);
            entity.Property(x => x.EstimatedArrival).HasColumnName("estimatedArrival").HasColumnType("datetime");
            entity.Ignore(x => x.Order);
        });
        modelBuilder.Entity<Dispute>(entity =>
        {
            entity.ToTable("Dispute");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.OrderId).HasColumnName("orderId");
            entity.Property(x => x.RaisedBy).HasColumnName("raisedBy");
            entity.Property(x => x.Description).HasColumnName("description");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).IsConcurrencyToken();
            entity.Property(x => x.Resolution).HasColumnName("resolution");
            entity.Property(x => x.AssignedTo).HasColumnName("assignedTo");
            entity.Property(x => x.AssignedAtUtc).HasColumnName("assignedAtUtc").HasColumnType("datetime2(0)");
            entity.Property(x => x.ReviewStartedAtUtc).HasColumnName("reviewStartedAtUtc").HasColumnType("datetime2(0)");
            entity.Property(x => x.ResolvedBy).HasColumnName("resolvedBy");
            entity.Property(x => x.ResolvedAtUtc).HasColumnName("resolvedAtUtc").HasColumnType("datetime2(0)");
            entity.Ignore(x => x.Order);
            entity.Ignore(x => x.RaisedByNavigation);
        });
        modelBuilder.Entity<Review>(entity =>
        {
            entity.ToTable("Review");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProductId).HasColumnName("productId");
            entity.Property(x => x.ReviewerId).HasColumnName("reviewerId");
            entity.Property(x => x.Rating).HasColumnName("rating");
            entity.Property(x => x.Comment).HasColumnName("comment");
            entity.Property(x => x.CreatedAt).HasColumnName("createdAt").HasColumnType("datetime");
            entity.Property(x => x.ModerationStatus).HasColumnName("moderationStatus").HasMaxLength(20).IsConcurrencyToken();
            entity.Property(x => x.ModerationReason).HasColumnName("moderationReason").HasMaxLength(500);
            entity.Property(x => x.ModeratedBy).HasColumnName("moderatedBy");
            entity.Property(x => x.ModeratedAtUtc).HasColumnName("moderatedAtUtc").HasColumnType("datetime2(0)");
            entity.Ignore(x => x.Product);
            entity.Ignore(x => x.Reviewer);
        });
        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.ToTable("Feedback");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.SellerId).HasColumnName("sellerId");
            entity.Property(x => x.AverageRating).HasColumnName("averageRating").HasPrecision(3, 2);
            entity.Property(x => x.TotalReviews).HasColumnName("totalReviews");
            entity.Property(x => x.PositiveRate).HasColumnName("positiveRate").HasPrecision(5, 2);
            entity.Ignore(x => x.Seller);
        });
        modelBuilder.Entity<ReturnRequest>(entity =>
        {
            entity.ToTable("ReturnRequest");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.OrderId).HasColumnName("orderId");
            entity.Property(x => x.UserId).HasColumnName("userId");
            entity.Property(x => x.Reason).HasColumnName("reason");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).IsConcurrencyToken();
            entity.Property(x => x.CreatedAt).HasColumnName("createdAt").HasColumnType("datetime");
            entity.Ignore(x => x.Order);
            entity.Ignore(x => x.User);
        });
        modelBuilder.Entity<User>().HasMany(x => x.Products).WithOne(x => x.Seller).HasForeignKey(x => x.SellerId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<User>().Ignore(x => x.Orders);
        modelBuilder.Entity<AdminAuditLog>(entity =>
        {
            entity.ToTable("AdminAuditLog");
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.AdminUserId).HasColumnName("adminUserId");
            entity.Property(x => x.Action).HasColumnName("action").HasMaxLength(50);
            entity.Property(x => x.ResourceType).HasColumnName("resourceType").HasMaxLength(50);
            entity.Property(x => x.ResourceId).HasColumnName("resourceId");
            entity.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(500);
            entity.Property(x => x.CreatedAtUtc)
                .HasColumnName("createdAtUtc")
                .HasColumnType("datetime2(0)")
                .HasConversion(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
        });
    }
}
