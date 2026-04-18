using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TechCorner_ECommerce.Data {
    public class AppDbContext : IdentityDbContext {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            // Category -> SubCategory
            modelBuilder.Entity<SubCategory>()
                .HasOne(sc => sc.Category)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(sc => sc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // SubCategory -> Product 
            modelBuilder.Entity<Product>()
                .HasOne(p => p.SubCategory)
                .WithMany(sc => sc.Products)
                .HasForeignKey(p => p.SubCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product -> Category
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);

            ////////////////////////////////////////// SEED DATA //////////////////////////////////////////
            base.OnModelCreating(modelBuilder);

            // ===================== CATEGORY =====================
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Laptop" },
                new Category { Id = 2, Name = "PC" },
                new Category { Id = 3, Name = "Main - CPU - VGA" },
                new Category { Id = 4, Name = "Gaming Gear" }
            );

            // ===================== SUBCATEGORY =====================
            modelBuilder.Entity<SubCategory>().HasData(
                // Laptop brands
                new SubCategory { Id = 1, Name = "Acer", CategoryId = 1 },
                new SubCategory { Id = 2, Name = "Asus", CategoryId = 1 },
                new SubCategory { Id = 3, Name = "MSI", CategoryId = 1 },
                new SubCategory { Id = 4, Name = "Lenovo", CategoryId = 1 },

                // PC types
                new SubCategory { Id = 5, Name = "Gaming PC", CategoryId = 2 },
                new SubCategory { Id = 6, Name = "Workstation PC", CategoryId = 2 },

                // Components
                new SubCategory { Id = 7, Name = "Mainboard", CategoryId = 3 },
                new SubCategory { Id = 8, Name = "CPU", CategoryId = 3 },
                new SubCategory { Id = 9, Name = "VGA", CategoryId = 3 },

                // Gaming gear
                new SubCategory { Id = 10, Name = "Keyboard", CategoryId = 4 },
                new SubCategory { Id = 11, Name = "Mouse", CategoryId = 4 },
                new SubCategory { Id = 12, Name = "Headset", CategoryId = 4 }
            );

            // ===================== PRODUCTS =====================
            modelBuilder.Entity<Product>().HasData(
                // Laptop
                new Product {
                    Id = 1,
                    Name = "Acer Nitro 5",
                    Description = "Gaming laptop RTX 3050",
                    Price = 1200,
                    Stock = 10,
                    ImageUrl = "acer_nitro5.jpg",
                    CategoryId = 1,
                    SubCategoryId = 1
                },
                new Product {
                    Id = 2,
                    Name = "Asus ROG Strix G15",
                    Description = "High performance gaming laptop",
                    Price = 1500,
                    Stock = 8,
                    ImageUrl = "asus_rog_g15.jpg",
                    CategoryId = 1,
                    SubCategoryId = 2
                },
                new Product {
                    Id = 3,
                    Name = "MSI Katana GF66",
                    Description = "Gaming laptop RTX 3060",
                    Price = 1400,
                    Stock = 6,
                    ImageUrl = "msi_katana.jpg",
                    CategoryId = 1,
                    SubCategoryId = 3
                },

                // PC
                new Product {
                    Id = 4,
                    Name = "Gaming PC RTX 4060",
                    Description = "Custom build gaming PC",
                    Price = 2000,
                    Stock = 5,
                    ImageUrl = "pc_gaming.jpg",
                    CategoryId = 2,
                    SubCategoryId = 5
                },

                // Components
                new Product {
                    Id = 5,
                    Name = "Intel Core i7 13700K",
                    Description = "13th Gen CPU",
                    Price = 450,
                    Stock = 15,
                    ImageUrl = "cpu_i7.jpg",
                    CategoryId = 3,
                    SubCategoryId = 8
                },
                new Product {
                    Id = 6,
                    Name = "RTX 4070",
                    Description = "NVIDIA Graphics Card",
                    Price = 700,
                    Stock = 7,
                    ImageUrl = "rtx4070.jpg",
                    CategoryId = 3,
                    SubCategoryId = 9
                },

                // Gaming Gear
                new Product {
                    Id = 7,
                    Name = "Razer BlackWidow",
                    Description = "Mechanical gaming keyboard",
                    Price = 150,
                    Stock = 20,
                    ImageUrl = "keyboard_razer.jpg",
                    CategoryId = 4,
                    SubCategoryId = 10
                },
                new Product {
                    Id = 8,
                    Name = "Logitech G Pro X",
                    Description = "Gaming mouse",
                    Price = 120,
                    Stock = 25,
                    ImageUrl = "mouse_logitech.jpg",
                    CategoryId = 4,
                    SubCategoryId = 11
                }
            );
        }






    }
}
