using ProjectSolution.Web.Models;
using ProjectSolution.Web.Service.IService;
using ProjectSolution.Web.Utility;
using static ProjectSolution.Web.Utility.SD;

namespace ProjectSolution.Web.Service
{
    public class OrderService : IOrderService
    {
        private readonly IBaseService _baseService;

        public OrderService(IBaseService baseService)
        {
            this._baseService = baseService;
        }
        public async Task<ResponseDto?> CreateOrderAsync(CartDto cartDto)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = ApiType.POST,
                Url = SD.OrderAPIBase + "/api/order/CreateOrder",
                Data = cartDto
            });
        }

        public async Task<ResponseDto?> CreateStripeSessionAsync(StripeRequestDto stripeRequestDto)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = ApiType.POST,
                Url = SD.OrderAPIBase + "/api/order/CreateStripeSession",
                Data = stripeRequestDto
            });
        }

        public async Task<ResponseDto?> ValidateStripeSessionAsync(int orderHeaderId)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = ApiType.POST,
                Url = SD.OrderAPIBase + "/api/order/ValidateStripeSession",
                Data = orderHeaderId
            });
        }

        public async Task<ResponseDto?> GetAllOrdersAsync(string? userId)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = ApiType.GET,
                Url = SD.OrderAPIBase + "/api/order/GetOrders/" + userId,
            });
        }

        public async Task<ResponseDto?> GetOrderByIdAsync(int orderHeaderId)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = ApiType.GET,
                Url = SD.OrderAPIBase + "/api/order/GetOrderById/" + orderHeaderId,
            });
        }

        public async Task<ResponseDto?> UpdateOrderStatusAsync(int orderHeaderId, string newStatus)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = ApiType.POST,
                Url = SD.OrderAPIBase + "/api/order/UpdateOrderStatus/" + orderHeaderId,
                Data = newStatus
            });
        }
    }
}
