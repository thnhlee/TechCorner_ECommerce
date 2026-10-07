namespace TechCorner_ECommerce.Models {
    public class ParentProduct {
        public int Id { get; set; }

        public string PublicId { get; set; } = "";
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsDeleted { get; set; }

        public int SubCategoryId { get; set; }
        public SubCategory SubCategory { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
