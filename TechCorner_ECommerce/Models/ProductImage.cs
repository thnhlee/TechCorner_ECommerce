using System.ComponentModel.DataAnnotations;

namespace TechCorner_ECommerce.Models {
    public class ProductImage {
        public int Id { get; set; }

        public int ParentProductId { get; set; }
        public ParentProduct ParentProduct { get; set; }

        public string ImageUrl { get; set; }
        public bool IsPrimary { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
