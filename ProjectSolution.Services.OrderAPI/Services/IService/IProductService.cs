using ProjectSolution.Services.OrderAPI.Models.Dto;

namespace ProjectSolution.Services.OrderAPI.Services.IService
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetProductDtos();
    }
}
