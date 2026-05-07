using TechCorner_ECommerce.Models;

namespace TechCorner_ECommerce.Data {
    public class SeedData {
        public static void Seed(AppDbContext db) {
            // tránh seed lại
            if (db.ParentProducts.Any()) return; 

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

            // Clothing attributes
            var color = new ProductAttribute {
                Name = "Color",
                CategoryId = cate1.CategoryId
            };

            var size = new ProductAttribute {
                Name = "Size",
                CategoryId = cate1.CategoryId
            };

            // Electronics attributes
            var storage = new ProductAttribute {
                Name = "Storage",
                CategoryId = cate2.CategoryId
            };

            db.ProductAttributes.AddRange(color, size, storage);
            db.SaveChanges();

            // Clothing values
            var red = new AttributeValue {
                Value = "Red",
                ProductAttributeId = color.Id
            };

            var blue = new AttributeValue {
                Value = "Blue",
                ProductAttributeId = color.Id
            };

            var s = new AttributeValue {
                Value = "S",
                ProductAttributeId = size.Id
            };

            var m = new AttributeValue {
                Value = "M",
                ProductAttributeId = size.Id
            };

            // Electronics values
            var storage128 = new AttributeValue {
                Value = "128GB",
                ProductAttributeId = storage.Id
            };

            var storage256 = new AttributeValue {
                Value = "256GB",
                ProductAttributeId = storage.Id
            };

            db.AttributeValues.AddRange(
                red, blue,
                s, m,
                storage128, storage256
            );

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
                new ProductImage { ParentProductId = tshirt.Id, ImageUrl = "/images/4eede2ae-18ce-4603-aae9-b41abc5543a2.jpg", IsPrimary = true },
                new ProductImage { ParentProductId = tshirt.Id, ImageUrl = "/images/a9f860e9-961a-4792-b42c-e9510d217ef4.png", IsPrimary = false }
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
                    ImageUrl = "/images/a9f860e9-961a-4792-b42c-e9510d217ef4.png",
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

            db.ProductAttributeValues.Add(
                new ProductAttributeValue {
                    ProductId = phoneSku.Id,
                    AttributeValueId = storage128.Id
                }
            );

            db.SaveChanges();
        }
    }
}
    
