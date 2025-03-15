using System;
using System.ComponentModel.DataAnnotations;
using TicketsApi.Common.Enums;

namespace TicketsApi.Web.Data.Entities
{
    public class TicketDet
    {
        [Key]
        public int Id { get; set; }
        public TicketCab TicketCab { get; set; }

        [Display(Name = "Descripción")]
        [MaxLength(1000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Description { get; set; }

        [Display(Name = "Estado")]
        public TicketState TicketState { get; set; }

        [Display(Name = "Fecha Estado")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public DateTime StateDate { get; set; }

        [Display(Name = "Usuario Estado")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string StateUserId { get; set; }

        [Display(Name = "Usuario Estado")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string StateUserName { get; set; }

        [Display(Name = "Imagen")]
        public string Image { get; set; }
        public string ImageFullPath => string.IsNullOrEmpty(Image)
        ? $"https://keypress.serveftp.net/Tickets/images/tickets/noimage.png"
        : $"https://keypress.serveftp.net/Tickets{Image.Substring(1)}";
    }
}
