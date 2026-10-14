using Microsoft.AspNetCore.Identity;

namespace TechCorner_ECommerce.ViewModels {
    public class ProfileServiceResult<T> {
        public bool RequiresLogin { get; set; }
        public T? Value { get; set; }
        public IEnumerable<IdentityError> Errors { get; set; } = Enumerable.Empty<IdentityError>();
    }
}
