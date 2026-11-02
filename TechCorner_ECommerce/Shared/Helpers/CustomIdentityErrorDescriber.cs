using Microsoft.AspNetCore.Identity;

namespace TechCorner_ECommerce.Helpers {
    public class CustomIdentityErrorDescriber : IdentityErrorDescriber {
        public override IdentityError DuplicateUserName(string email) {
            return new IdentityError {
                Code = nameof(DuplicateUserName),
                Description = $"Email '{email}' is already taken"
            };
        }
    }
}
