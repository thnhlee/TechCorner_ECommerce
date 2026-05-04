using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechCorner_ECommerce.Models;

namespace TechCorner_ECommerce.Data {
    public class AppDbContext : IdentityDbContext {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }

        public DbSet<ParentProduct> ParentProducts { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }

        public DbSet<ProductAttribute> ProductAttributes { get; set; }
        public DbSet<AttributeValue> AttributeValues { get; set; }
        public DbSet<ProductAttributeValue> ProductAttributeValues { get; set; }

        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        public DbSet<Payment> Payments { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            /* =========================PARENT PRODUCT========================= */

            modelBuilder.Entity<ParentProduct>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(x => x.Slug)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasIndex(x => x.Slug)
                    .IsUnique();

                //  SubCategory 1 - n ParentProduct
                entity.HasOne(x => x.SubCategory)
                    .WithMany(x => x.ParentProducts)
                    .HasForeignKey(x => x.SubCategoryId);
            });

            /* =========================PRODUCT========================= */

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Price)
                    .HasColumnType("decimal(18,2)");

                entity.Property(x => x.StockQuantity)
                    .HasDefaultValue(0);

                //  ParentProduct 1 - n Product
                entity.HasOne(x => x.ParentProduct)
                    .WithMany(x => x.Products)
                    .HasForeignKey(x => x.ParentProductId);
            });

            /* =========================PRODUCT IMAGE========================= */

            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.HasKey(x => x.Id);

                //  ParentProduct 1 - n Image
                entity.HasOne(x => x.ParentProduct)
                    .WithMany(x => x.Images)
                    .HasForeignKey(x => x.ParentProductId);
            });

            /* =========================ATTRIBUTE========================= */

            modelBuilder.Entity<ProductAttribute>(entity =>
            {
                entity.HasKey(x => x.Id);
            });

            modelBuilder.Entity<AttributeValue>(entity =>
            {
                entity.HasKey(x => x.Id);

                //  Attribute 1 - n AttributeValue
                entity.HasOne(x => x.ProductAttribute)
                    .WithMany(x => x.Values)
                    .HasForeignKey(x => x.ProductAttributeId);
            });

            /* =========================PRODUCT - ATTRIBUTE VALUE (M:N)========================= */

            modelBuilder.Entity<ProductAttributeValue>(entity =>
            {
                entity.HasKey(x => x.Id);

                //  Product 1 - n ProductAttributeValue
                entity.HasOne(x => x.Product)
                    .WithMany(x => x.ProductAttributeValues)
                    .HasForeignKey(x => x.ProductId);

                //  AttributeValue 1 - n ProductAttributeValue
                entity.HasOne(x => x.AttributeValue)
                    .WithMany(x => x.ProductAttributeValues)
                    .HasForeignKey(x => x.AttributeValueId);

                // unique constraint to prevent duplicate attribute values for the same product
                entity.HasIndex(x => new { x.ProductId, x.AttributeValueId })
                    .IsUnique();
            });

            /* =========================CART========================= */

            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(x => x.Id);

                //  User 1 - n Cart
                entity.HasOne(x => x.User)
                    .WithMany(x => x.Carts)
                    .HasForeignKey(x => x.UserId);
            });

            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(x => x.Id);

                //  Cart 1 - n CartItem
                entity.HasOne(x => x.Cart)
                    .WithMany(x => x.CartItems)
                    .HasForeignKey(x => x.CartId);

                //  Product 1 - n CartItem
                entity.HasOne(x => x.Product)
                    .WithMany(x => x.CartItems)
                    .HasForeignKey(x => x.ProductId);
            });

            /* =========================ORDER========================= */

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(x => x.Id);

                //  User 1 - n Order
                entity.HasOne(x => x.User)
                    .WithMany(x => x.Orders)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                //  Order 1 - 1 Address 
                entity.HasOne(x => x.Address)
                    .WithMany()
                    .HasForeignKey(x => x.AddressId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasKey(x => x.Id);

                //  Order 1 - n OrderDetail
                entity.HasOne(x => x.Order)
                    .WithMany(x => x.OrderDetails)
                    .HasForeignKey(x => x.OrderId)
                    .OnDelete(DeleteBehavior.NoAction);

                //  Product 1 - n OrderDetail
                entity.HasOne(x => x.Product)
                    .WithMany(x => x.OrderDetails)
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.Property(x => x.Price)
                    .HasColumnType("decimal(18,2)");
            });

            /* =========================PAYMENT========================= */

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(x => x.Id);

                //  Order 1 - n Payment
                entity.HasOne(x => x.Order)
                    .WithMany(x => x.Payments)
                    .HasForeignKey(x => x.OrderId);
            });

            /* =========================ADDRESS========================= */

            modelBuilder.Entity<Address>(entity =>
            {
                entity.HasKey(x => x.Id);

                //  User 1 - n Address
                entity.HasOne(x => x.User)
                    .WithMany(x => x.Addresses)
                    .HasForeignKey(x => x.UserId);
            });

            /* =========================REVIEW========================= */

            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(x => x.Id);

                //  User 1 - n Review
                entity.HasOne(x => x.User)
                    .WithMany(x => x.Reviews)
                    .HasForeignKey(x => x.UserId);

                //  ParentProduct 1 - n Review
                entity.HasOne(x => x.ParentProduct)
                    .WithMany(x => x.Reviews)
                    .HasForeignKey(x => x.ParentProductId);
            });
        }

    }
}

