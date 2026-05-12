using Newtonsoft.Json;
using ProjectSolution.Services.OrderAPI.Models.Dto;
using ProjectSolution.Services.OrderAPI.Services.IService;

namespace ProjectSolution.Services.OrderAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        public ProductService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<IEnumerable<ProductDto>> GetProductDtos()
        {
            HttpClient client = _httpClientFactory.CreateClient("ProductAPI");
            HttpRequestMessage requestMessage = new();
            requestMessage.Headers.Add("Accept", "application/json");
            requestMessage.Method = HttpMethod.Get;
            requestMessage.RequestUri = new Uri(_config["Services:ProductAPI"] + "/api/product");
            // no content to send in get request

            HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);
            var responseContent = await responseMessage.Content.ReadAsStringAsync();
            ResponseDto responseDto = JsonConvert.DeserializeObject<ResponseDto>(responseContent)!;
            if (responseDto.IsSuccess && responseDto.Result is not null)
            {
                IEnumerable<ProductDto> productDtos = JsonConvert.DeserializeObject<IEnumerable<ProductDto>>(Convert.ToString(responseDto.Result)!)!;
                return productDtos;
            }
            return new List<ProductDto>();
        }
    }
}
