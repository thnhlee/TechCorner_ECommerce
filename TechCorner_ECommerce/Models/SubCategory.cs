using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechCorner_ECommerce.Models {
    public class SubCategory {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public Category Category { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public ICollection<ParentProduct> ParentProducts { get; set; } = new List<ParentProduct>();
    }
}