using System.Security.Claims;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Services {
    public interface IProfileService {
        Task<ProfileServiceResult<ProfileVM>> GetProfileAsync(ClaimsPrincipal principal);
        Task<ProfileServiceResult<object>> UpdateProfileAsync(ClaimsPrincipal principal, ProfileVM model);
    }
}
