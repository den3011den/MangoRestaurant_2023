using System.ComponentModel.DataAnnotations.Schema;

namespace Mango.Services.OrderAPI.Messaging
{
    public class CartDetailsDto
    {
        public int CartDetailsId { get; set; }
        public int CartHeaderId { get; set;}
        
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual ProductDto Product { get; set; }
        public int Count { get; set; }
    }
}
