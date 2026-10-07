namespace TechCorner_ECommerce.ViewModels {
    public class MenuSubCategoryVM {
        public int Id { get; set; }
        public string Name { get; set; } = "";

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = "";

        public DateTime CreatedAt { get; set; }

        public int Quantity { get; set; }
    }
}
