namespace TechCorner_ECommerce.ViewModels {
    public class CategoryCreateVM {
        public string Name { get; set; }
    }

    public class CategoryEditVM {
        public int CategoryId { get; set; }

        public string Name { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        
    }
}
