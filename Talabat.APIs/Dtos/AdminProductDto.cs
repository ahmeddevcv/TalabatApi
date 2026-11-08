using System.ComponentModel.DataAnnotations;

namespace Talabat.APIs.Dtos
{
    public class AdminProductDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        public string PictureUrl { get; set; }

        [Required]
        public int ProductBrandId { get; set; }

        [Required]
        public int ProductTypeId { get; set; }
    }
}
