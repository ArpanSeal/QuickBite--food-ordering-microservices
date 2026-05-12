using ProjectSolution.Web.Models;

namespace ProjectSolution.Web.Service.IService
{
    public interface IProductService
    {
        Task<ResponseDto> GetAllProductAsync(); // no need to set access modifier in interface methods by default they are public
        Task<ResponseDto> GetProductByIdAsync(int productId);
        Task<ResponseDto> GetProductByNameAsync(string productName);
        Task<ResponseDto> CreateProductAsync(ProductDto product);
        Task<ResponseDto> UpdateProductAsync(ProductDto product);
        Task<ResponseDto> DeleteProductAsync(int productId);
    }
}
