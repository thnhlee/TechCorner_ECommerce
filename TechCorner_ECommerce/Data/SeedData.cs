using TechCorner_ECommerce.Models;

namespace TechCorner_ECommerce.Data {
    public class SeedData {
            public static void Seed(AppDbContext db) {
                if (db.ParentProducts.Any()) return; // tránh seed lại

                /* ================= CATEGORY ================= */

                var cate1 = new Category { Name = "Clothing" };
                var cate2 = new Category { Name = "Electronics" };

                db.Categories.AddRange(cate1, cate2);
                db.SaveChanges();

                var sub1 = new SubCategory { Name = "T-Shirts", CategoryId = cate1.CategoryId };
                var sub2 = new SubCategory { Name = "Smartphones", CategoryId = cate2.CategoryId };

                db.SubCategories.AddRange(sub1, sub2);
                db.SaveChanges();

                /* ================= ATTRIBUTE ================= */

                var color = new ProductAttribute { Name = "Color" };
                var size = new ProductAttribute { Name = "Size" };

                db.ProductAttributes.AddRange(color, size);
                db.SaveChanges();

                var red = new AttributeValue { Value = "Red", ProductAttributeId = color.Id };
                var blue = new AttributeValue { Value = "Blue", ProductAttributeId = color.Id };

                var s = new AttributeValue { Value = "S", ProductAttributeId = size.Id };
                var m = new AttributeValue { Value = "M", ProductAttributeId = size.Id };
                db.AttributeValues.AddRange(red, blue, s, m);
                db.SaveChanges();

                /* ================= PARENT PRODUCT ================= */

                var tshirt = new ParentProduct {
                    Name = "Basic T-Shirt",
                    Slug = "basic-tshirt",
                    Description = "Comfortable cotton t-shirt",
                    SubCategoryId = sub1.Id
                };

                db.ParentProducts.Add(tshirt);
                db.SaveChanges();

                /* ================= IMAGES ================= */

                db.ProductImages.AddRange(
                    new ProductImage { ParentProductId = tshirt.Id, ImageUrl = "/images/tshirt1.jpg", IsPrimary = true },
                    new ProductImage { ParentProductId = tshirt.Id, ImageUrl = "/images/tshirt2.jpg", IsPrimary = false }
                );
                db.SaveChanges();

                /* ================= PRODUCTS (SKU) ================= */

                var sku1 = new Product {
                    ParentProductId = tshirt.Id,
                    Price = 10,
                    StockQuantity = 50
                };

                var sku2 = new Product {
                    ParentProductId = tshirt.Id,
                    Price = 12,
                    StockQuantity = 30
                };

                db.Products.AddRange(sku1, sku2);
                db.SaveChanges();

                /* ================= ATTRIBUTE FOR SKU ================= */

                db.ProductAttributeValues.AddRange(
                    // sku1 = Red + S
                    new ProductAttributeValue { ProductId = sku1.Id, AttributeValueId = red.Id },
                    new ProductAttributeValue { ProductId = sku1.Id, AttributeValueId = s.Id },

                    // sku2 = Blue + M
                    new ProductAttributeValue { ProductId = sku2.Id, AttributeValueId = blue.Id },
                    new ProductAttributeValue { ProductId = sku2.Id, AttributeValueId = m.Id }
                );

                db.SaveChanges();

                /* ================= ANOTHER PRODUCT ================= */

                var phone = new ParentProduct {
                    Name = "iPhone 15",
                    Slug = "iphone-15",
                    Description = "Latest Apple smartphone",
                    SubCategoryId = sub2.Id
                };

                db.ParentProducts.Add(phone);
                db.SaveChanges();

                db.ProductImages.Add(
                    new ProductImage {
                        ParentProductId = phone.Id,
                        ImageUrl = "/images/iphone.jpg",
                        IsPrimary = true
                    }
                );
                db.SaveChanges();

                var phoneSku = new Product {
                    ParentProductId = phone.Id,
                    Price = 999,
                    StockQuantity = 10
                };

                db.Products.Add(phoneSku);
                db.SaveChanges();
            }
        }
    }
