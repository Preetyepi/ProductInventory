using System.ComponentModel.DataAnnotations;

namespace ProductInventory.Models
{
    public class Product
    {


        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        [Range(0, double.MaxValue)]
        [Required(ErrorMessage ="This field required")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public string? Category { get; set; }

    }
}
