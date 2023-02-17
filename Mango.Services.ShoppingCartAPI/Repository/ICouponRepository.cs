using Mango.Services.ShoppingCartAPI.Models.Dto;
using System.Threading.Tasks;

namespace Mango.Services.ShoppingCartAPI.Repository
{
    public interface ICouponRepository
    {
        Task<CouponDto> GetCoupon(string couponName);
    }
}
