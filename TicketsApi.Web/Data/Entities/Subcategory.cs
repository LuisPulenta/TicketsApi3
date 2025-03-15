using System.ComponentModel.DataAnnotations;

namespace TicketsApi.Web.Data.Entities
{
    public class Subcategory
    {
        [Key]
        public int Id { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; }

        [Display(Name = "SubCategoría")]
        [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Name { get; set; }

            }
}


