using X.PagedList;

namespace TechCorner_ECommerce.ViewModels {
    public class AdminCategoryOperationResult {
        public bool Succeeded { get; set; }
        public bool NotFound { get; set; }
        public string? FieldName { get; set; }
        public string Message { get; set; } = "";
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Value { get; set; }

        public static AdminCategoryOperationResult Success(int? id = null, string? name = null, string? value = null) {
            return new AdminCategoryOperationResult {
                Succeeded = true,
                Id = id,
                Name = name,
                Value = value
            };
        }

        public static AdminCategoryOperationResult Failure(string message, string? fieldName = null) {
            return new AdminCategoryOperationResult {
                Message = message,
                FieldName = fieldName
            };
        }

        public static AdminCategoryOperationResult Missing(string message) {
            return new AdminCategoryOperationResult {
                NotFound = true,
                Message = message
            };
        }
    }

    public class CategoryIndexResult {
        public IPagedList<MenuCategoryVM>? Items { get; set; }
        public string? Search { get; set; }
    }

    public class SubCategoryIndexResult {
        public IPagedList<MenuSubCategoryVM>? Items { get; set; }
        public List<Models.Category> Categories { get; set; } = new();
        public int? CategoryId { get; set; }
        public string? Search { get; set; }
    }
}
