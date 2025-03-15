using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TicketsApi.Web.Data.Entities
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Categoría")]
        [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Name { get; set; }

        public ICollection<Subcategory>? Subcategories { get; set; }

        [Display(Name = "Subcategorías")]
        public int SubcategoriesNumber => Subcategories == null ? 0 : Subcategories.Count;
    }
}


