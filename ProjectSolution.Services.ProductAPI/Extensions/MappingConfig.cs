using ProjectSolution.Services.ProductAPI.Models;
using ProjectSolution.Services.ProductAPI.Models.Dto;

namespace ProjectSolution.Services.ProductAPI.Extensions
{
    public static class MappingConfig
    {
        public static ProductDto ToProductDto(this Product product)
        {
            return new ProductDto
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Price = product.Price,
                CategoryName = product.CategoryName,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                ImageLocalPath = product.ImageLocalPath
            };
        }
        public static Product ToProduct(this ProductDto productDto)
        {
            return new Product
            {
                ProductId = productDto.ProductId,
                ProductName = productDto.ProductName,
                Price = productDto.Price,
                CategoryName = productDto.CategoryName,
                Description = productDto.Description,
                ImageUrl = productDto.ImageUrl,
                ImageLocalPath = productDto.ImageLocalPath,
            };
        }
    }
}
