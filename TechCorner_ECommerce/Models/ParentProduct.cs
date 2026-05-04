namespace TechCorner_ECommerce.Models {
    public class ParentProduct {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }

        public int SubCategoryId { get; set; }
        public SubCategory SubCategory { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<Product> Products { get; set; }
        public ICollection<ProductImage> Images { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }
}
