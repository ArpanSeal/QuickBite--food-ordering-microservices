using System.ComponentModel.DataAnnotations;

namespace ProjectSolution.Web.Models
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        [Display(Name = "Product Name")]
        [MaxLength(100, ErrorMessage = "Product name must be 100 characters or less.")]
        public string ProductName { get; set; } = string.Empty; // ensures that ProductName is never null

        [Range(1, 100000, ErrorMessage = "Price must be between 1 and 100000.")]
        public decimal Price { get; set; }

        [MaxLength(500, ErrorMessage = "Description must be 500 characters or less.")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Category")]
        [MaxLength(100, ErrorMessage = "Category name must be 100 characters or less.")]
        public string CategoryName { get; set; } = string.Empty; //Category will define the type of product

        public string? ImageUrl { get; set; }

        public string? ImageLocalPath { get; set; }

        public IFormFile? ImageFile { get; set; }

        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100.")]
        public int Quantity { get; set; } = 1;
    }
}

/* DateTime Types:

[Range(typeof(DateTime), "1/1/2000", "12/31/2100", ErrorMessage = "Date must be between 1/1/2000 and 12/31/2100.")]
[DataType(DataType.Date)] // specifies that the data type is a date only (no time component).
public DateTime ManufactureDate { get; set; }

*/