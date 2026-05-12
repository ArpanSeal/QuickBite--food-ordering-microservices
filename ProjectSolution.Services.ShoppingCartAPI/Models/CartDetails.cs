using ProjectSolution.Services.ShoppingCartAPI.Models.Dto;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectSolution.Services.ShoppingCartAPI.Models
{
    public class CartDetails
    {
        [Key]
        public int CartDetailsId { get; set; }
        public int CartHeaderId { get; set; }

        [ForeignKey("CartHeaderId")]
        public CartHeader? CartHeader { get; set; } //Navigation Property

        public int ProductId { get; set; }

        [NotMapped]
        public ProductDto? Product { get; set; } //Navigation Property
        public int Count { get; set; }
    }
}
