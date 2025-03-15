using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TicketsApi.Common.Enums;

namespace TicketsApi.Web.Data.Entities
{
    public class TicketCab
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Fecha Creación")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public DateTime CreateDate { get; set; }

        [Display(Name = "Usuario Creación")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string UserId { get; set; }

        [Display(Name = "Usuario Creación")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string UserName { get; set; }

        [Display(Name = "Empresa")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string CompanyName { get; set; }

        [Display(Name = "Empresa")]
        public int CompanyId { get; set; }

        [Display(Name = "Categoría")]
        public int CategoryId { get; set; }

        [Display(Name = "Categoría")]
        public string CategoryName { get; set; }

        [Display(Name = "Subcategoría")]
        public int SubcategoryId { get; set; }

        [Display(Name = "Subcategoría")]
        public string SubcategoryName { get; set; }

        [Display(Name = "Asunto")]
        [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Title { get; set; }

        [Display(Name = "Estado")]
        public TicketState TicketState { get; set; }

        [Display(Name = "Fecha Asignación")]
        public DateTime? AsignDate { get; set; }

        [Display(Name = "Usuario Asignación")]
        public string UserAsign { get; set; }

        [Display(Name = "Usuario Asignación")]
        public string UserAsignName { get; set; }

        [Display(Name = "Fecha En Curso")]
        public DateTime? InProgressDate { get; set; }

        [Display(Name = "Fecha Fin")]
        public DateTime? FinishDate { get; set; }

        public ICollection<TicketDet>? TicketDets { get; set; }

        [Display(Name = "Detalles")]
        public int TicketDetsNumber => TicketDets == null ? 0 : TicketDets.Count;
}
}
