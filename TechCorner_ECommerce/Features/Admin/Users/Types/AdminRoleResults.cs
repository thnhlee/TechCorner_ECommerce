using Microsoft.AspNetCore.Identity;
using X.PagedList;

namespace TechCorner_ECommerce.ViewModels {
    public class AdminRoleOperationResult {
        public bool Succeeded { get; set; }
        public bool NotFound { get; set; }
        public string? FieldName { get; set; }
        public string Message { get; set; } = "";
        public IEnumerable<IdentityError> Errors { get; set; } = Enumerable.Empty<IdentityError>();

        public static AdminRoleOperationResult Success() {
            return new AdminRoleOperationResult {
                Succeeded = true
            };
        }

        public static AdminRoleOperationResult Failure(string message, string? fieldName = null) {
            return new AdminRoleOperationResult {
                Message = message,
                FieldName = fieldName
            };
        }

        public static AdminRoleOperationResult Missing(string message = "") {
            return new AdminRoleOperationResult {
                NotFound = true,
                Message = message
            };
        }
    }

    public class AdminRoleIndexResult {
        public IPagedList<RoleVM>? Items { get; set; }
        public int Count { get; set; }
    }
}
