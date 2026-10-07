using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.ViewModels;
using X.PagedList.Extensions;

namespace TechCorner_ECommerce.Services {
    public class InventoryService : IInventoryService {
        private readonly AppDbContext db;

        public InventoryService(AppDbContext context) {
            db = context;
        }

        ///////////////////////// EXPORT EXCEL /////////////////////////
        public async Task<InventoryExportResult> ExportToExcelAsync() {
            var data = await db.Products
                .Select(x => new {
                    Name = x.ParentProduct.Name,
                    Category = x.ParentProduct.SubCategory.Category.Name,
                    SubCategory = x.ParentProduct.SubCategory.Name,
                    Price = x.Price,
                    StockQuantity = x.StockQuantity,
                    CreatedAt = x.CreatedAt,
                    Attributes = string.Join(" / ",
                        x.ProductAttributeValues.Select(v =>
                            v.AttributeValue.ProductAttribute.Name + ": " +
                            v.AttributeValue.Value))
                })
                .ToListAsync();

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Danh sách ");

            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Product";
            worksheet.Cell(1, 3).Value = "Attributes";
            worksheet.Cell(1, 4).Value = "Category";
            worksheet.Cell(1, 5).Value = "Subcategory";
            worksheet.Cell(1, 6).Value = "Price";
            worksheet.Cell(1, 7).Value = "Quantity";
            worksheet.Cell(1, 8).Value = "Create At";

            var row = 2;
            var stt = 1;
            foreach (var item in data) {
                worksheet.Cell(row, 1).Value = stt++;
                worksheet.Cell(row, 2).Value = item.Name;
                worksheet.Cell(row, 3).Value = item.Attributes;
                worksheet.Cell(row, 4).Value = item.Category;
                worksheet.Cell(row, 5).Value = item.SubCategory;
                worksheet.Cell(row, 6).Value = item.Price;
                worksheet.Cell(row, 6).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(row, 7).Value = item.StockQuantity;
                worksheet.Cell(row, 8).Value = item.CreatedAt.ToString("dd/MM/yyyy HH:mm");
                row++;
            }

            worksheet.Columns().AdjustToContents();

            // Controller returns this stream as a downloadable Excel file.
            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return new InventoryExportResult {
                Stream = stream,
                FileName = $"DanhSach_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
            };
        }

        ///////////////////////// INVENTORY LIST /////////////////////////
        public InventoryIndexResult GetInventory(int? categoryId, string? keyword, int pageNumber, int pageSize) {
            var products = db.Products
                .Include(x => x.ParentProduct)
                    .ThenInclude(x => x.SubCategory)
                    .ThenInclude(x => x.Category)
                .Include(x => x.ParentProduct)
                    .ThenInclude(x => x.Images)
                .Include(x => x.ProductAttributeValues)
                    .ThenInclude(x => x.AttributeValue)
                .AsQueryable();

            if (categoryId.HasValue) {
                products = products.Where(x => x.ParentProduct.SubCategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword)) {
                keyword = keyword.Trim();

                products = products.Where(x =>
                    EF.Functions.Like(
                        EF.Functions.Collate(
                            x.ParentProduct.Name,
                            "SQL_Latin1_General_CP1_CI_AI"),
                        $"%{keyword}%"));
            }

            var result = products.Select(x => new ProductVM {
                Id = x.Id,
                ParentProductId = x.ParentProductId,
                PublicId = x.ParentProduct.PublicId,
                SkuCode = x.SkuCode,
                Slug = x.ParentProduct.Slug,
                Name = x.ParentProduct.Name,
                Description = x.ParentProduct.Description,
                Price = x.Price,
                Stock = x.StockQuantity,
                CategoryName = x.ParentProduct.SubCategory.Category.Name,
                SubCategoryName = x.ParentProduct.SubCategory.Name,
                ImageUrl = x.ParentProduct.Images
                    .Where(i => i.IsPrimary)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault()
                    ?? x.ParentProduct.Images
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault()
                    ?? "",
                Attributes = x.ProductAttributeValues
                    .Select(v => new AttributeVM {
                        Name = v.AttributeValue.ProductAttribute.Name,
                        Value = v.AttributeValue.Value
                    })
                    .ToList()
            }).ToPagedList(pageNumber, pageSize);

            return new InventoryIndexResult {
                Items = result,
                Count = result.Count,
                SubCategories = db.SubCategories.ToList()
            };
        }
    }
}
