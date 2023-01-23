using Mango.Web.Models;
using Mango.Web.Services.IServices;
using System.Collections;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mango.Web.Services
{
    public class ProductService : BaseService, IProductService
    {

        private readonly IHttpClientFactory _clientFactory;

        public ProductService(IHttpClientFactory clientFactory) : base(clientFactory) 
        {
            _clientFactory = clientFactory;
        }

        public async Task<T> CreateProductAsync<T>(ProductDTO productDTO)
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.POST,
                Data= productDTO,
                ApiUrl = SD.ProductAPIBase + "/api/products",
                AccessToken = ""
            });
        }

        public async Task<T> DeleteProductAsync<T>(int id)
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.DELETE,                
                ApiUrl = SD.ProductAPIBase + "/api/products/"+id,
                AccessToken = ""
            });
        }

        public async Task<T> GetProductByIdAsync<T>(int id)
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.GET,
                ApiUrl = SD.ProductAPIBase + "/api/products/"+id,
                AccessToken = ""
            });
        }

        public async Task<T> GetAllProductsAsync<T>()
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.GET,
                ApiUrl = SD.ProductAPIBase + "/api/products/",
                AccessToken = ""
            });
        }

        public async Task<T> UpdateProductAsync<T>(ProductDTO productDTO)
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.PUT,
                Data = productDTO,
                ApiUrl = SD.ProductAPIBase + "/api/products",
                AccessToken = ""
            });
        }
    }
}
