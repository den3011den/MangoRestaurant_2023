using Mango.Web.Models;
using Mango.Web.Services.IServices;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mango.Web.Services
{
    public class CartService : BaseService, ICartService
    {

        private readonly IHttpClientFactory _clientFactory;

        public CartService(IHttpClientFactory clientFactory) : base(clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<T> AddToCartAsync<T>(CartDto cartDto, string token = null)
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = cartDto,
                ApiUrl = SD.ShoppingCartAPIBase + "/api/cart/AddCart/",
                AccessToken = token
            });
        }

        public async Task<T> ApplyCoupon<T>(CartDto cartDto, string token = null)
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = cartDto,
                ApiUrl = SD.ShoppingCartAPIBase + "/api/cart/ApplyCoupon/",
                AccessToken = token
            });
        }

        public async Task<T> Checkout<T>(CartHeaderDto cartHeaderDto, string token = null)
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = cartHeaderDto,
                ApiUrl = SD.ShoppingCartAPIBase + "/api/cart/checkout",
                AccessToken = token
            });
        }

        public async Task<T> GetCartByUserIdAsync<T>(string userId, string token = null)
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.GET,
                ApiUrl = SD.ShoppingCartAPIBase + "/api/cart/GetCart/" + userId,
                AccessToken = token
            });
        }

        public async Task<T> RemoveCoupon<T>(string userId, string token = null)
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = userId,
                ApiUrl = SD.ShoppingCartAPIBase + "/api/cart/RemoveCoupon/",
                AccessToken = token
            });
        }

        public async Task<T> RemoveFromCartAsync<T>(int cartId, string token = null)
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = cartId,
                ApiUrl = SD.ShoppingCartAPIBase + "/api/cart/RemoveCart/",
                AccessToken = token
            });
        }

        public async Task<T> UpdateToCartAsync<T>(CartDto cartDto, string token = null)
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = cartDto,
                ApiUrl = SD.ShoppingCartAPIBase + "/api/cart/UpdateCart/",
                AccessToken = token
            });
        }
    }
}
