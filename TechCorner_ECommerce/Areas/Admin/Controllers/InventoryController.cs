using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.Helpers;
using TechCorner_ECommerce.ViewModels;
using X.PagedList.Extensions;
using System.IO;

namespace TechCorner_ECommerce.Areas.Admin.Controllers {
    [Authorize]
    [Area("Admin")]
    public class InventoryController : Controller {
        private readonly AppDbContext db;

        public InventoryController(AppDbContext context) {
            db = context;
        }

        // ================= Export Excel =================
        public async Task<IActionResult> ExportToExcel() {
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

            // Header
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Product";
            worksheet.Cell(1, 3).Value = "Attributes";
            worksheet.Cell(1, 4).Value = "Category";
            worksheet.Cell(1, 5).Value = "Subcategory";
            worksheet.Cell(1, 6).Value = "Price";
            worksheet.Cell(1, 7).Value = "Quantity";
            worksheet.Cell(1, 8).Value = "Create At";

            // Data
            int row = 2;
            int stt = 1;
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

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            string fileName = $"DanhSach_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // ================= INVENTORY =================
        public IActionResult Index(int? cate, string keyword, int? page) {

            int pageSize = 3;
            int pageNumber = page ?? 1;

            ViewBag.Cate = cate;
            ViewBag.SearchQuery = keyword;

            //=========== Show ra toàn bộ Product variant ===========
            var products = db.Products
                    .Include(x => x.ParentProduct)
                    .ThenInclude(x => x.SubCategory)
                        .ThenInclude(x => x.Category)

                    .Include(x => x.ParentProduct)
                        .ThenInclude(x => x.Images)

                    .Include(x => x.ProductAttributeValues)
                        .ThenInclude(x => x.AttributeValue)

                    .AsQueryable();

            // FILTER SUBCATEGORY
            if (cate.HasValue) {
                products = products.Where(x => x.ParentProduct.SubCategoryId == cate.Value);
            }

            // SEARCH
            if (!string.IsNullOrWhiteSpace(keyword)) {
                keyword = keyword.Trim();

                products = products.Where(x =>
                    EF.Functions.Like(
                        EF.Functions.Collate(
                            x.ParentProduct.Name,
                            "SQL_Latin1_General_CP1_CI_AI"
                        ),
                        $"%{keyword}%"
                    ));

            }



            // VIEWMODEL MAPPING
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
                                .FirstOrDefault()??
                            x.ParentProduct.Images
                            .Select(i => i.ImageUrl)
                            .FirstOrDefault()??"",

                Attributes = x.ProductAttributeValues
                    .Select(v => new AttributeVM {

                        Name = v.AttributeValue.ProductAttribute.Name,

                        Value = v.AttributeValue.Value

                    })
                    .ToList()
            })
            .ToPagedList(pageNumber, pageSize);

            ViewBag.SubCategories = db.SubCategories.ToList();
            ViewBag.Count = result.Count;

            return View(result);




            ////=========== Show ra Product Parent ===========
            //var products = db.ParentProducts
            //    .Include(p => p.SubCategory)
            //        .ThenInclude(sc => sc.Category)
            //    .Include(p => p.Images)
            //    .Include(p => p.Products)
            //    .AsQueryable();

            //// filter by subcategory
            //if (cate.HasValue) {
            //    products = products.Where(p => p.SubCategoryId == cate.Value);
            //}

            //// search
            //if (!string.IsNullOrEmpty(keyword)) {
            //    //// Search không phân biệt chữ hoa chữ thường
            //    //products = products.Where(p => p.Name.ToLower().Contains(keyword.ToLower()));

            //    //// Search phân biệt chữ hoa chữ thường
            //    //products = products.Where(p => p.Name.Contains(keyword));

            //    //// Search sử dụng EF.Functions.Like để hỗ trợ wildcard và không phân biệt chữ hoa chữ thường
            //    //products = products.Where(p => EF.Functions.Like(p.Name, $"%{keyword}%"));

            //    //// ép kiểu collation để search không phân biệt chữ hoa chữ thường và dấu tiếng Việt
            //    products = products.Where(p => EF.Functions.Like(EF.Functions.Collate(p.Name, "SQL_Latin1_General_CP1_CI_AI"), $"%{keyword}%"
            //        )
            //    );

            //    ViewBag.SearchQuery = keyword;

            //}

            //var result = products.Select(p => new ProductVM {
            //    Id = p.Id,
            //    Name = p.Name,
            //    Description = p.Description,

            //    // lấy giá nhỏ nhất từ Product(Variant)
            //    Price = p.Products.Min(v => (decimal?)v.Price) ?? 0,

            //    // lấy ảnh đại diện
            //    ImageUrl = p.Images.FirstOrDefault(i => i.IsPrimary).ImageUrl ?? p.Images.FirstOrDefault().ImageUrl ?? "",

            //    Stock = p.Products.Sum(v => v.StockQuantity),
            //    CategoryName = p.SubCategory.Category.Name,
            //    SubCategoryName = p.SubCategory.Name
            //})
            //.ToList();
            //ViewBag.Count = result.Count;
            //return View(result);
        }


    }
}
