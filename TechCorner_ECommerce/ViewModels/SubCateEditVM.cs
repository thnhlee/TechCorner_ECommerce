namespace TechCorner_ECommerce.ViewModels {
    public class SubCategoryEditVM {
        public int Id { get; set; }

        public string Name { get; set; }

        public int CategoryId { get; set; }

        public List<CategoryVM> Categories { get; set; } = new();
    }
}
