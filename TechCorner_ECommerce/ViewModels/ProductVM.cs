namespace TechCorner_ECommerce.ViewModels {
    public class ProductVM {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Description { get; set; } 

        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public string CategoryName { get; set; }

        public int Stock { get; set; }
        public List<VariantVM> Variants { get; set; } = new List<VariantVM>();
    }


}
