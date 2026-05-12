using ProjectSolution.Services.ShoppingCartAPI.Models.Dto;

namespace ProjectSolution.Services.ShoppingCartAPI.Services.IService
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetProductDtos();
    }
}
