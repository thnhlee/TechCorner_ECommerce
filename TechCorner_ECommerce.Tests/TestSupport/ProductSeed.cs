using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.Models;

namespace TechCorner_ECommerce.Tests.TestSupport;

public static class ProductSeed {
    public static Product AddProduct(
        AppDbContext db,
        int stockQuantity = 5,
        decimal price = 100m,
        string productName = "Gaming Mouse") {
        var category = new Category {
            Name = "Mouse"
        };

        var productAttribute = new ProductAttribute {
            Category = category,
            Name = "Color"
        };

        var attributeValue = new AttributeValue {
            ProductAttribute = productAttribute,
            Value = "Black"
        };

        var subCategory = new SubCategory {
            Category = category,
            Name = "Wireless"
        };

        var parent = new ParentProduct {
            PublicId = "PP-TEST",
            Name = productName,
            Slug = "gaming-mouse",
            Description = "Mouse description",
            SubCategory = subCategory,
            Images = new List<ProductImage> {
                new ProductImage {
                    ImageUrl = "/images/mouse.png",
                    IsPrimary = true
                }
            }
        };

        var product = new Product {
            SkuCode = "SKU-TEST",
            ParentProduct = parent,
            Price = price,
            StockQuantity = stockQuantity,
            ProductAttributeValues = new List<ProductAttributeValue> {
                new ProductAttributeValue {
                    AttributeValue = attributeValue
                }
            }
        };

        db.Products.Add(product);
        db.SaveChanges();

        return product;
    }
}
