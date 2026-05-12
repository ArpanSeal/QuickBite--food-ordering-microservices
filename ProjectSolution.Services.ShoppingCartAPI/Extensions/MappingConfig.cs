using ProjectSolution.Services.ShoppingCartAPI.Models;
using ProjectSolution.Services.ShoppingCartAPI.Models.Dto;

namespace ProjectSolution.Services.ShoppingCartAPI.Extensions
{
    public static class MappingConfig
    {
        public static CartHeaderDto ToCartHeaderDto(this CartHeader CartHeader)
        {
            return new CartHeaderDto
            {
                CartHeaderId = CartHeader.CartHeaderId,
                UserId = CartHeader.UserId,
                CouponCode = CartHeader.CouponCode,
                Discount = CartHeader.Discount,
                CartTotal = CartHeader.CartTotal,
            };
        }
        public static CartHeader ToCartHeader(this CartHeaderDto CartHeaderDto)
        {
            return new CartHeader
            {
                CartHeaderId = CartHeaderDto.CartHeaderId,
                UserId = CartHeaderDto.UserId,
                CouponCode = CartHeaderDto.CouponCode,
                Discount = CartHeaderDto.Discount,
                CartTotal = CartHeaderDto.CartTotal,
            };
        }
        public static CartDetailsDto ToCartDetailsDto(this CartDetails CartDetails)
        {
            return new CartDetailsDto
            {
                CartDetailsId = CartDetails.CartDetailsId,
                CartHeaderId = CartDetails.CartHeaderId,
                ProductId = CartDetails.ProductId,
                Count = CartDetails.Count
            };
        }
        public static CartDetails ToCartDetails(this CartDetailsDto CartDetailsDto)
        {
            return new CartDetails
            {
                CartDetailsId = CartDetailsDto.CartDetailsId,
                CartHeaderId = CartDetailsDto.CartHeaderId,
                //CartHeader = CartDetailsDto.CartHeaderDto?.ToCartHeader() ?? new(), // do not map CartHeader to avoid circular reference
                ProductId = CartDetailsDto.ProductId,
                //Product = CartDetailsDto.ProductDto ?? new(), // do not map Product to avoid circular reference
                Count = CartDetailsDto.Count
            };
        }
    }
}
