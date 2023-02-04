using System.ComponentModel.DataAnnotations;

namespace Mango.Web.Models
{
    public class ProductDto
    {

        public ProductDto()
        {
            Count = 1;
        }

        public int ProductId { get; set; }
        [Display(Name = "Наименование")]
        public string Name { get; set; }
        [Display(Name = "Цена")]
        public double Price { get; set; }
        [Display(Name = "Описание")]
        public string Description { get; set; }
        [Display(Name = "Категория")]
        public string CategoryName { get; set; }
        [Display(Name = "Ссылка на изображение")]
        public string ImageUrl { get; set; }
        

        [Range(1, 100)]
        [Display(Name = "Количество")]
        public int Count { get; set; }
    }
}
