using Mango.Services.ProductAPI.Models.DTO;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mango.Services.ProductAPI.Repository
{
    public interface IProductRepository
    {
        Task<IEnumerable<ProductDTO>> GetProducts();
        Task<ProductDTO> GetProductById(int productId);
        Task<ProductDTO> CreateUpdateProduct(ProductDTO productDto);
        Task<bool> DeleteProduct(int productId);
    }
}
