namespace TechCorner_ECommerce.ViewModels {
    public class AdminProductOperationResult {
        public bool Succeeded { get; set; }
        public bool NotFound { get; set; }
        public string? FieldName { get; set; }
        public string Message { get; set; } = "";

        public static AdminProductOperationResult Success() {
            return new AdminProductOperationResult {
                Succeeded = true
            };
        }

        public static AdminProductOperationResult Failure(string message, string? fieldName = null) {
            return new AdminProductOperationResult {
                Message = message,
                FieldName = fieldName
            };
        }

        public static AdminProductOperationResult Missing(string message) {
            return new AdminProductOperationResult {
                NotFound = true,
                Message = message
            };
        }
    }
}
