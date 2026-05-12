using Newtonsoft.Json;
using ProjectSolution.Services.ShoppingCartAPI.Models.Dto;
using ProjectSolution.Services.ShoppingCartAPI.Models.Dtos;
using ProjectSolution.Services.ShoppingCartAPI.Services.IService;

namespace ProjectSolution.Services.ShoppingCartAPI.Services
{
    public class CouponService : ICouponService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CouponService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<CouponDto> GetCouponByName(string couponCode)
        {
            var httpClient = _httpClientFactory.CreateClient("CouponAPI");
            HttpResponseMessage responseMessage = await httpClient.GetAsync($"/api/coupon/GetByCode/{couponCode}");
            string? responseContent = await responseMessage.Content.ReadAsStringAsync();
            ResponseDto? responseDto = JsonConvert.DeserializeObject<ResponseDto>(responseContent)!;
            if (responseDto != null && responseDto.IsSuccess)
            {
                CouponDto couponDto = JsonConvert.DeserializeObject<CouponDto>(responseDto.Result!.ToString()!)!;
                return couponDto;
            }
            return new CouponDto();
        }
    }
}
