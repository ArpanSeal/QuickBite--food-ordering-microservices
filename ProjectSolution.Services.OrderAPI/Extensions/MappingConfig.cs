using ProjectSolution.Services.OrderAPI.Models;
using ProjectSolution.Services.OrderAPI.Models.Dto;

namespace ProjectSolution.Services.OrderAPI.Extensions
{
    public static class MappingConfig
    {
        public static OrderHeaderDto ToOrderHeaderDtoFromCartHeaderDto(this CartHeaderDto CartHeaderDto)
        {
            return new OrderHeaderDto
            {
                UserId = CartHeaderDto.UserId,
                CouponCode = CartHeaderDto.CouponCode,
                Discount = CartHeaderDto.Discount,
                OrderTotal = CartHeaderDto.CartTotal,
                Name = CartHeaderDto.Name,
                Email = CartHeaderDto.Email,
                PhoneNumber = CartHeaderDto.PhoneNumber,
            };
        }
        public static CartHeaderDto ToCartHeaderDtoFromOrderHeaderDto(this OrderHeaderDto orderHeaderDto)
        {
            return new CartHeaderDto
            {
                UserId = orderHeaderDto.UserId,
                CouponCode = orderHeaderDto.CouponCode,
                Discount = orderHeaderDto.Discount,
                CartTotal = orderHeaderDto.OrderTotal,
                Name = orderHeaderDto.Name,
                Email = orderHeaderDto.Email,
                PhoneNumber = orderHeaderDto.PhoneNumber,
            };
        }

        public static OrderDetailsDto ToOrderDetailsDtoFromCartDetailsDto(this CartDetailsDto cartDetailsDto)
        {
            return new OrderDetailsDto
            {
                ProductId = cartDetailsDto.ProductId,
                ProductDto = cartDetailsDto.ProductDto,
                Count = cartDetailsDto.Count,
                ProductName = cartDetailsDto.ProductDto?.ProductName ?? string.Empty,
                ProductPrice = cartDetailsDto.ProductDto?.Price ?? 0,
            };
        }
        public static CartDetailsDto ToCartDetailsDtoFromOrderDetailsDto(this OrderDetailsDto orderDetailsDto)
        {
            return new CartDetailsDto
            {
                ProductId = orderDetailsDto.ProductId,
                ProductDto = orderDetailsDto.ProductDto,
                Count = orderDetailsDto.Count,
            };
        }

        public static OrderHeaderDto ToOrderHeaderDtoFromOrderHeader(this OrderHeader orderHeader)
        {
            return new OrderHeaderDto
            {
                OrderHeaderId = orderHeader.OrderHeaderId,
                UserId = orderHeader.UserId,
                CouponCode = orderHeader.CouponCode,
                Discount = orderHeader.Discount,
                OrderTotal = orderHeader.OrderTotal,
                Name = orderHeader.Name,
                Email = orderHeader.Email,
                PhoneNumber = orderHeader.PhoneNumber,
                OrderTime = orderHeader.OrderTime,
                Status = orderHeader.Status,
                PaymentIntentId = orderHeader.PaymentIntentId,
                StripeSessionId = orderHeader.StripeSessionId,
                OrderDetailsDtoList = orderHeader.OrderDetailsList.Select(od => od.ToOrderDetailsDtoFromOrderDetails()).ToList(),
            };
        }
        public static OrderHeader ToOrderHeaderFromOrderHeaderDto(this OrderHeaderDto orderHeaderDto)
        {
            return new OrderHeader
            {
                OrderHeaderId = orderHeaderDto.OrderHeaderId,
                UserId = orderHeaderDto.UserId,
                CouponCode = orderHeaderDto.CouponCode,
                Discount = orderHeaderDto.Discount,
                OrderTotal = orderHeaderDto.OrderTotal,
                Name = orderHeaderDto.Name,
                Email = orderHeaderDto.Email,
                PhoneNumber = orderHeaderDto.PhoneNumber,
                OrderTime = orderHeaderDto.OrderTime,
                Status = orderHeaderDto.Status,
                PaymentIntentId = orderHeaderDto.PaymentIntentId,
                StripeSessionId = orderHeaderDto.StripeSessionId,
                OrderDetailsList = orderHeaderDto.OrderDetailsDtoList.Select(od => od.ToOrderDetailsFromOrderDetailsDto()).ToList(),
            };
        }

        public static OrderDetailsDto ToOrderDetailsDtoFromOrderDetails(this OrderDetails orderDetails)
        {
            return new OrderDetailsDto
            {
                OrderDetailsId = orderDetails.OrderDetailsId,
                OrderHeaderId = orderDetails.OrderHeaderId,
                ProductId = orderDetails.ProductId,
                ProductDto = orderDetails.ProductDto,
                Count = orderDetails.Count,
                ProductName = orderDetails.ProductName,
                ProductPrice = orderDetails.ProductPrice,
            };
        }
        public static OrderDetails ToOrderDetailsFromOrderDetailsDto(this OrderDetailsDto orderDetailsDto)
        {
            return new OrderDetails
            {
                OrderDetailsId = orderDetailsDto.OrderDetailsId,
                OrderHeaderId = orderDetailsDto.OrderHeaderId,
                ProductId = orderDetailsDto.ProductId,
                ProductDto = orderDetailsDto.ProductDto,
                Count = orderDetailsDto.Count,
                ProductName = orderDetailsDto.ProductName,
                ProductPrice = orderDetailsDto.ProductPrice,
            };
        }
    }
}
