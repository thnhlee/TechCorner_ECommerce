using Microsoft.IdentityModel.Logging;
using TechCorner_ECommerce.Data;

namespace TechCorner_ECommerce.Helpers {
    public class SlugService : ISlugService {
        private readonly AppDbContext _context;

        public SlugService(AppDbContext context) {
            _context = context;
        }

        public string CreateSlug(string name) {
            var baseSlug = SlugGenerate.GenerateSlug(name);
            var slug = baseSlug;
            int i = 1;

            while (_context.ParentProducts.Any(x => x.Slug == slug)) {
                slug = $"{baseSlug}-{i++}";
            }

            return slug;
        }
    }
}
