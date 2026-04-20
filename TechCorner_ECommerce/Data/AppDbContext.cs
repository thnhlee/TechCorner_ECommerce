using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechCorner_ECommerce.Models;

namespace TechCorner_ECommerce.Data {
    public class AppDbContext : IdentityDbContext {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {
        }


        public DbSet<Category> Categories { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }

        public DbSet<ProductAttribute> ProductAttributes { get; set; }
        public DbSet<AttributeValue> AttributeValues { get; set; }
        public DbSet<VariantAttributeValue> VariantAttributeValues { get; set; }

        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        public DbSet<Payment> Payments { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            // Category -> SubCategory
            modelBuilder.Entity<SubCategory>()
                .HasOne(sc => sc.Category)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(sc => sc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product -> SubCategory 
            modelBuilder.Entity<Product>()
                .HasOne(p => p.SubCategory)
                .WithMany(sc => sc.Products)
                .HasForeignKey(p => p.SubCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // ProductImage
            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ProductVariant
            modelBuilder.Entity<ProductVariant>()
                .HasOne(pv => pv.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(pv => pv.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // AttributeValue -> Attribute 
            modelBuilder.Entity<AttributeValue>()
                .HasOne(av => av.Attribute)
                .WithMany(a => a.Values)
                .HasForeignKey(av => av.AttributeId)
                .OnDelete(DeleteBehavior.Cascade);

            // VariantAttributeValue -> ProductVariant & AttributeValue (n - n)
            modelBuilder.Entity<VariantAttributeValue>()
                .HasOne(vav => vav.ProductVariant)
                .WithMany(pv => pv.VariantAttributeValues)
                .HasForeignKey(vav => vav.ProductVariantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VariantAttributeValue>()
                .HasOne(vav => vav.AttributeValue)
                .WithMany(av => av.VariantAttributeValues)
                .HasForeignKey(vav => vav.AttributeValueId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cart -> User (1 - 1)
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithOne(u => u.Cart)
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // CartItem -> Cart & ProductVariant (n - 1)
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.ProductVariant)
                .WithMany(pv => pv.CartItems)
                .HasForeignKey(ci => ci.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order -> User & Address (n - 1)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Address)
                .WithMany()
                .HasForeignKey(o => o.AddressId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrderDetail -> Order & ProductVariant (n - 1)
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.ProductVariant)
                .WithMany(pv => pv.OrderDetails)
                .HasForeignKey(od => od.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment -> Order (n - 1)
            //modelBuilder.Entity<Payment>()
            //    .HasOne(p => p.Order)
            //    .WithMany(o => o.Payments)
            //    .HasForeignKey(p => p.OrderId)
            //    .OnDelete(DeleteBehavior.Cascade);

            // Address -> User (n - 1)
            modelBuilder.Entity<Address>()
                .HasOne(a => a.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Review -> Product & User (n - 1)
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<Review>()
            //    .HasOne(r => r.User)
            //    .WithMany(u => u.Reviews)
            //    .HasForeignKey(r => r.UserId)
            //    .OnDelete(DeleteBehavior.Restrict);

            // 1 User - 1 Cart
            modelBuilder.Entity<Cart>()
                .HasIndex(c => c.UserId)
                .IsUnique();
            base.OnModelCreating(modelBuilder);

            ////////////////////////////////////////// SEED DATA //////////////////////////////////////////
            modelBuilder.Entity<Category>().HasData(
                new Category {
                    Id = 1,
                    Name = "Gaming Gear",
                     
                    
                },
                new Category {
                    Id = 2,
                    Name = "PC Components",
                     
                   
                },
                new Category {
                    Id = 3,
                    Name = "Monitors & Displays",
                     
                    
                },
                new Category {
                    Id = 4,
                    Name = "Accessories",
                     
                    
                }
            );

            modelBuilder.Entity<SubCategory>().HasData(

                // Gaming Gear
                new SubCategory { Id = 1, Name = "Mouse", CategoryId = 1  },
                new SubCategory { Id = 2, Name = "Keyboard", CategoryId = 1  },
                new SubCategory { Id = 3, Name = "Headset", CategoryId = 1  },
                new SubCategory { Id = 4, Name = "Controller", CategoryId = 1  },

                // PC Components
                new SubCategory { Id = 5, Name = "GPU", CategoryId = 2  },
                new SubCategory { Id = 6, Name = "CPU", CategoryId = 2  },
                new SubCategory { Id = 7, Name = "RAM", CategoryId = 2  },
                new SubCategory { Id = 8, Name = "SSD", CategoryId = 2  },

                // Monitors
                new SubCategory { Id = 9, Name = "Gaming Monitor", CategoryId = 3  },
                new SubCategory { Id = 10, Name = "Ultrawide Monitor", CategoryId = 3  },

                // Accessories
                new SubCategory { Id = 11, Name = "Mousepad", CategoryId = 4  },
                new SubCategory { Id = 12, Name = "Cable & Hub", CategoryId = 4  }
            );
            modelBuilder.Entity<Product>().HasData(

                // Mouse
                new Product { Id = 1, Name = "Logitech G Pro X Superlight", Description = "Ultra-light esports mouse", SubCategoryId = 1  },
                new Product { Id = 2, Name = "Razer DeathAdder V3", Description = "Ergonomic FPS mouse", SubCategoryId = 1  },
                new Product { Id = 3, Name = "Glorious Model O Wireless", Description = "Lightweight honeycomb mouse", SubCategoryId = 1  },

                // Keyboard 
                new Product { Id = 4, Name = "Keychron K8 Pro", Description = "Wireless mechanical keyboard", SubCategoryId = 2  },
                new Product { Id = 5, Name = "Razer Huntsman Mini", Description = "60% optical keyboard", SubCategoryId = 2  },
                new Product { Id = 6, Name = "Ducky One 3 TKL", Description = "Premium mechanical keyboard", SubCategoryId = 2  },

                // Headset
                new Product { Id = 7, Name = "HyperX Cloud II", Description = "7.1 surround headset", SubCategoryId = 3  },
                new Product { Id = 8, Name = "SteelSeries Arctis 7", Description = "Wireless gaming headset", SubCategoryId = 3  },

                // Controller 
                new Product { Id = 9, Name = "Xbox Wireless Controller", Description = "Official Xbox controller", SubCategoryId = 4  },
                new Product { Id = 10, Name = "DualSense PS5 Controller", Description = "Sony PS5 controller", SubCategoryId = 4  },

                // GPU 
                new Product { Id = 11, Name = "RTX 4060 Ti", Description = "Mid-range GPU", SubCategoryId = 5  },
                new Product { Id = 12, Name = "RTX 4070 Super", Description = "High-end gaming GPU", SubCategoryId = 5  },
                new Product { Id = 13, Name = "RX 7800 XT", Description = "AMD performance GPU", SubCategoryId = 5  },

                // CPU 
                new Product { Id = 14, Name = "Intel i5-13600K", Description = "Mid-high gaming CPU", SubCategoryId = 6  },
                new Product { Id = 15, Name = "Intel i7-14700K", Description = "High-end CPU", SubCategoryId = 6  },

                // RAM
                new Product { Id = 16, Name = "Corsair Vengeance 16GB", Description = "DDR5 RAM kit", SubCategoryId = 7  },

                // SSD 
                new Product { Id = 17, Name = "Samsung 980 Pro 1TB", Description = "NVMe SSD", SubCategoryId = 8  },

                // Monitor
                new Product { Id = 18, Name = "LG 27GN800", Description = "2K 144Hz monitor", SubCategoryId = 9  },

                // Ultrawide
                new Product { Id = 19, Name = "Samsung Odyssey G9", Description = "49-inch ultrawide gaming monitor", SubCategoryId = 10  },

                // Mousepad 
                new Product { Id = 20, Name = "Artisan Hien Mousepad", Description = "Pro esports mousepad", SubCategoryId = 11  }
            );

            modelBuilder.Entity<ProductVariant>().HasData(

                new ProductVariant { Id = 1, ProductId = 1, Price = 2500000, StockQuantity = 50  },
                new ProductVariant { Id = 2, ProductId = 2, Price = 1800000, StockQuantity = 40  },
                new ProductVariant { Id = 3, ProductId = 3, Price = 2100000, StockQuantity = 35  },
                new ProductVariant { Id = 4, ProductId = 4, Price = 2200000, StockQuantity = 30  },
                new ProductVariant { Id = 5, ProductId = 5, Price = 2400000, StockQuantity = 25  },
                new ProductVariant { Id = 6, ProductId = 6, Price = 3000000, StockQuantity = 20  },
                new ProductVariant { Id = 7, ProductId = 7, Price = 1500000, StockQuantity = 50  },
                new ProductVariant { Id = 8, ProductId = 8, Price = 2800000, StockQuantity = 35  },
                new ProductVariant { Id = 9, ProductId = 9, Price = 1200000, StockQuantity = 60  },
                new ProductVariant { Id = 10, ProductId = 10, Price = 2200000, StockQuantity = 40  },

                new ProductVariant { Id = 11, ProductId = 11, Price = 9000000, StockQuantity = 25  },
                new ProductVariant { Id = 12, ProductId = 12, Price = 13000000, StockQuantity = 20  },
                new ProductVariant { Id = 13, ProductId = 13, Price = 12000000, StockQuantity = 18  },
                new ProductVariant { Id = 14, ProductId = 14, Price = 7500000, StockQuantity = 30  },
                new ProductVariant { Id = 15, ProductId = 15, Price = 9500000, StockQuantity = 22  },
                new ProductVariant { Id = 16, ProductId = 16, Price = 2500000, StockQuantity = 40  },
                new ProductVariant { Id = 17, ProductId = 17, Price = 3000000, StockQuantity = 45  },
                new ProductVariant { Id = 18, ProductId = 18, Price = 6500000, StockQuantity = 15  },
                new ProductVariant { Id = 19, ProductId = 19, Price = 18000000, StockQuantity = 10  },
                new ProductVariant { Id = 20, ProductId = 20, Price = 1600000, StockQuantity = 55  }
            );

            modelBuilder.Entity<ProductImage>().HasData(

                new ProductImage { Id = 1, ProductId = 1, ImageUrl = "/img/p1.jpg", IsPrimary = true },
                new ProductImage { Id = 2, ProductId = 2, ImageUrl = "/img/p2.jpg", IsPrimary = true },
                new ProductImage { Id = 3, ProductId = 3, ImageUrl = "/img/p3.jpg", IsPrimary = true },
                new ProductImage { Id = 4, ProductId = 4, ImageUrl = "/img/p4.jpg", IsPrimary = true },
                new ProductImage { Id = 5, ProductId = 5, ImageUrl = "/img/p5.jpg", IsPrimary = true },
                new ProductImage { Id = 6, ProductId = 6, ImageUrl = "/img/p6.jpg", IsPrimary = true },
                new ProductImage { Id = 7, ProductId = 7, ImageUrl = "/img/p7.jpg", IsPrimary = true },
                new ProductImage { Id = 8, ProductId = 8, ImageUrl = "/img/p8.jpg", IsPrimary = true },
                new ProductImage { Id = 9, ProductId = 9, ImageUrl = "/img/p9.jpg", IsPrimary = true },
                new ProductImage { Id = 10, ProductId = 10, ImageUrl = "/img/p10.jpg", IsPrimary = true },

                new ProductImage { Id = 11, ProductId = 11, ImageUrl = "/img/p11.jpg", IsPrimary = true },
                new ProductImage { Id = 12, ProductId = 12, ImageUrl = "/img/p12.jpg", IsPrimary = true },
                new ProductImage { Id = 13, ProductId = 13, ImageUrl = "/img/p13.jpg", IsPrimary = true },
                new ProductImage { Id = 14, ProductId = 14, ImageUrl = "/img/p14.jpg", IsPrimary = true },
                new ProductImage { Id = 15, ProductId = 15, ImageUrl = "/img/p15.jpg", IsPrimary = true },
                new ProductImage { Id = 16, ProductId = 16, ImageUrl = "/img/p16.jpg", IsPrimary = true },
                new ProductImage { Id = 17, ProductId = 17, ImageUrl = "/img/p17.jpg", IsPrimary = true },
                new ProductImage { Id = 18, ProductId = 18, ImageUrl = "/img/p18.jpg", IsPrimary = true },
                new ProductImage { Id = 19, ProductId = 19, ImageUrl = "/img/p19.jpg", IsPrimary = true },
                new ProductImage { Id = 20, ProductId = 20, ImageUrl = "/img/p20.jpg", IsPrimary = true }
            );

            modelBuilder.Entity<ProductAttribute>().HasData(
                new ProductAttribute { Id = 1, Name = "Color" },
                new ProductAttribute { Id = 2, Name = "DPI" },
                new ProductAttribute { Id = 3, Name = "Switch Type" }
            );

            modelBuilder.Entity<AttributeValue>().HasData(
                new AttributeValue { Id = 1, Value = "Black", AttributeId = 1 },
                new AttributeValue { Id = 2, Value = "White", AttributeId = 1 },
                new AttributeValue { Id = 3, Value = "Pink", AttributeId = 1 },
                new AttributeValue { Id = 4, Value = "800 DPI", AttributeId = 2 },
                new AttributeValue { Id = 5, Value = "1600 DPI", AttributeId = 2 },
                new AttributeValue { Id = 6, Value = "3200 DPI", AttributeId = 2 },
                new AttributeValue { Id = 7, Value = "8000 DPI", AttributeId = 2 },
                new AttributeValue { Id = 8, Value = "Red Switch", AttributeId = 3 },
                new AttributeValue { Id = 9, Value = "Blue Switch", AttributeId = 3 },
                new AttributeValue { Id = 10, Value = "Brown Switch", AttributeId = 3 }
            );

            modelBuilder.Entity<VariantAttributeValue>().HasData(

                // Product 1 
                new VariantAttributeValue { Id = 1, ProductVariantId = 1, AttributeValueId = 1 }, // Black
                new VariantAttributeValue { Id = 2, ProductVariantId = 1, AttributeValueId = 4 }, // 800 DPI

                // Product 2
                new VariantAttributeValue { Id = 3, ProductVariantId = 2, AttributeValueId = 2 }, // White
                new VariantAttributeValue { Id = 4, ProductVariantId = 2, AttributeValueId = 5 }, // 1600 DPI

                //  Product 3
                new VariantAttributeValue { Id = 5, ProductVariantId = 3, AttributeValueId = 1 },
                new VariantAttributeValue { Id = 6, ProductVariantId = 3, AttributeValueId = 6 },

                //  Keyboard 1
                new VariantAttributeValue { Id = 7, ProductVariantId = 4, AttributeValueId = 8 }, // Red Switch

                //  Keyboard 2
                new VariantAttributeValue { Id = 8, ProductVariantId = 5, AttributeValueId = 9 }, // Blue Switch

                //  Keyboard 3
                new VariantAttributeValue { Id = 9, ProductVariantId = 6, AttributeValueId = 10 } // Brown Switch
            );
        }
    }
}
