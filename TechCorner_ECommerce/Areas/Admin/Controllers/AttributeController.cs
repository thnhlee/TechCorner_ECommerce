using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.Models;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Areas.Admin.Controllers {
    [Authorize]
    [Area("Admin")]
    public class AttributeController : Controller {
        private readonly AppDbContext db;

        public AttributeController(AppDbContext context) {
            db = context;
        }

        public IActionResult Index(int? categoryId) {
            var model = new AttributeManageVM {
                SelectedCategoryId = categoryId,

                Categories = db.Categories
                    .Select(c => new CategoryVM {
                        Id = c.CategoryId,
                        Name = c.Name
                    })
                    .ToList()
            };

            if (categoryId.HasValue) {
                model.Attributes = db.ProductAttributes
                    .Where(a => a.CategoryId == categoryId.Value)
                    .Include(a => a.AttributeValues)
                    .Select(a => new ProductAttributeVM {
                        Id = a.Id,
                        Name = a.Name,
                        CategoryId = a.CategoryId,
                        Values = a.AttributeValues.Select(v => new AttributeValueVM {
                            Id = v.Id,
                            Value = v.Value
                        }).ToList()
                    })
                    .ToList();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddAttribute(CreateAttributeVM model) {
            if (string.IsNullOrWhiteSpace(model.Name)) {
                return Json(new {
                    success = false,
                    message = "Attribute name is required"
                });
            }

            bool exists = db.ProductAttributes.Any(x =>
                x.CategoryId == model.CategoryId &&
                x.Name.ToLower() == model.Name.Trim().ToLower()
            );

            if (exists) {
                return Json(new {
                    success = false,
                    message = "Attribute already exists in this category"
                });
            }

            var attr = new ProductAttribute {
                CategoryId = model.CategoryId,
                Name = model.Name.Trim()
            };

            db.ProductAttributes.Add(attr);
            db.SaveChanges();

            return Json(new {
                success = true,
                id = attr.Id,
                name = attr.Name
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddAttributeValue(CreateAttributeValueVM model) {
            if (string.IsNullOrWhiteSpace(model.Value)) {
                return Json(new {
                    success = false,
                    message = "Value is required"
                });
            }

            bool exists = db.AttributeValues.Any(x =>
                x.ProductAttributeId == model.AttributeId &&
                x.Value.ToLower() == model.Value.Trim().ToLower()
            );

            if (exists) {
                return Json(new {
                    success = false,
                    message = "Value already exists"
                });
            }

            var value = new AttributeValue {
                ProductAttributeId = model.AttributeId,
                Value = model.Value.Trim()
            };

            db.AttributeValues.Add(value);
            db.SaveChanges();

            return Json(new {
                success = true,
                id = value.Id,
                value = value.Value
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAttribute(int id) {
            var attr = db.ProductAttributes
                .Include(x => x.AttributeValues)
                .FirstOrDefault(x => x.Id == id);

            if (attr == null) {
                return Json(new {
                    success = false,
                    message = "Attribute not found"
                });
            }

            bool isUsed = db.ProductAttributeValues
                .Any(x => attr.AttributeValues
                    .Select(v => v.Id)
                    .Contains(x.AttributeValueId));

            if (isUsed) {
                return Json(new {
                    success = false,
                    message = "This attribute cannot be deleted because it is already used."
                });
            }

            db.AttributeValues.RemoveRange(attr.AttributeValues);
            db.ProductAttributes.Remove(attr);

            db.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAttributeValue(int id) {
            var value = db.AttributeValues
                .FirstOrDefault(x => x.Id == id);

            if (value == null) {
                return Json(new {
                    success = false,
                    message = "Value not found"
                });
            }

            bool isUsed = db.ProductAttributeValues
                .Any(x => x.AttributeValueId == id);

            if (isUsed) {
                return Json(new {
                    success = false,
                    message = "This value cannot be deleted because it is already used."
                });
            }

            db.AttributeValues.Remove(value);
            db.SaveChanges();

            return Json(new { success = true });
        }
    }
}