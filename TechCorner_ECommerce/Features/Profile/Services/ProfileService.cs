using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.Models;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Services {
    public class ProfileService : IProfileService {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly AppDbContext db;

        public ProfileService(UserManager<ApplicationUser> userManager, AppDbContext context) {
            this.userManager = userManager;
            db = context;
        }

        public async Task<ProfileServiceResult<ProfileVM>> GetProfileAsync(ClaimsPrincipal principal) {
            var user = await userManager.GetUserAsync(principal);

            if (user == null) {
                return new ProfileServiceResult<ProfileVM> {
                    RequiresLogin = true
                };
            }

            var address = db.Addresses.FirstOrDefault(x => x.UserId == user.Id);

            // Profile address is used for checkout autofill only; orders keep their own snapshot.
            return new ProfileServiceResult<ProfileVM> {
                Value = new ProfileVM {
                    Email = user.Email,
                    UserName = string.IsNullOrWhiteSpace(user.UserName)
                        ? user.Email
                        : user.UserName,
                    ReceiverName = address?.ReceiverName,
                    Phone = address?.Phone,
                    FullAddress = address?.FullAddress
                }
            };
        }

        public async Task<ProfileServiceResult<object>> UpdateProfileAsync(ClaimsPrincipal principal, ProfileVM model) {
            var user = await userManager.GetUserAsync(principal);

            if (user == null) {
                return new ProfileServiceResult<object> {
                    RequiresLogin = true
                };
            }

            user.UserName = string.IsNullOrWhiteSpace(model.UserName)
                ? user.Email
                : model.UserName.Trim();

            var updateUserResult = await userManager.UpdateAsync(user);

            if (!updateUserResult.Succeeded) {
                return new ProfileServiceResult<object> {
                    Errors = updateUserResult.Errors
                };
            }

            var address = db.Addresses.FirstOrDefault(x => x.UserId == user.Id);

            if (address == null) {
                address = new Address {
                    UserId = user.Id,
                    ReceiverName = model.ReceiverName?.Trim(),
                    Phone = model.Phone?.Trim(),
                    FullAddress = model.FullAddress?.Trim(),
                    CreatedAt = DateTime.Now
                };

                db.Addresses.Add(address);
            }
            else {
                address.ReceiverName = model.ReceiverName?.Trim();
                address.Phone = model.Phone?.Trim();
                address.FullAddress = model.FullAddress?.Trim();
                address.UpdatedAt = DateTime.Now;
            }

            await db.SaveChangesAsync();

            return new ProfileServiceResult<object> {
                Value = new object()
            };
        }
    }
}
