using ProjectSolution.Web.Models;
using ProjectSolution.Web.Service.IService;
using ProjectSolution.Web.Utility;
using static ProjectSolution.Web.Utility.SD;

namespace ProjectSolution.Web.Service
{
    public class CartService : ICartService
    {
        private readonly IBaseService _baseService;

        public CartService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto?> UpsertCartAsync(CartDto cartDto)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = ApiType.POST,
                Url = SD.ShoppingCartAPIBase + "/api/cart/CartUpsert",
                Data = cartDto
            });
        }

        public async Task<ResponseDto?> RemoveFromCartAsync(int cartDetailId)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = ApiType.POST,
                Url = SD.ShoppingCartAPIBase + "/api/cart/RemoveCart",
                Data = cartDetailId
            });
        }

        public async Task<ResponseDto?> GetAllCartsByUserIdAsync(string userId)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = ApiType.GET,
                Url = SD.ShoppingCartAPIBase + $"/api/cart/GetCart/{userId}"
            });
        }

        public async Task<ResponseDto?> ApplyCouponAsync(CartDto cartDto)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = ApiType.POST,
                Url = SD.ShoppingCartAPIBase + "/api/cart/ApplyCoupon",
                Data = cartDto
            });
        }

        public async Task<ResponseDto?> GetCartCountByUserIdAsync(string userId)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = ApiType.GET,
                Url = SD.ShoppingCartAPIBase + $"/api/cart/GetCartCount/{userId}"
            });
        }
    }
}
