using Microsoft.AspNetCore.Identity;
using X.PagedList;

namespace TechCorner_ECommerce.ViewModels {
    public class AdminUserOperationResult {
        public bool Succeeded { get; set; }
        public bool NotFound { get; set; }
        public bool Forbidden { get; set; }
        public string Message { get; set; } = "";
        public IEnumerable<IdentityError> Errors { get; set; } = Enumerable.Empty<IdentityError>();

        public static AdminUserOperationResult Success(string message = "") {
            return new AdminUserOperationResult {
                Succeeded = true,
                Message = message
            };
        }

        public static AdminUserOperationResult Failure(string message) {
            return new AdminUserOperationResult {
                Message = message
            };
        }

        public static AdminUserOperationResult Missing() {
            return new AdminUserOperationResult {
                NotFound = true
            };
        }

        public static AdminUserOperationResult Denied(string message) {
            return new AdminUserOperationResult {
                Forbidden = true,
                Message = message
            };
        }
    }

    public class AdminUserIndexResult {
        public IPagedList<UserListVM>? Items { get; set; }
        public int Count { get; set; }
    }
}
