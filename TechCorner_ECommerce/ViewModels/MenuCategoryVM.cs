namespace TechCorner_ECommerce.ViewModels {
    public class MenuCategoryVM {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<MenuSubCategoryVM> SubCategories { get; set; } = new();
    }
}
