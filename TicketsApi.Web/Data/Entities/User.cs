using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TicketsApi.Common.Enums;

namespace TicketsApi.Web.Data.Entities
{
    public class User : IdentityUser
    {
        [Display(Name = "Nombre")]
        [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} carácteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string FirstName { get; set; }

        [Display(Name = "Apellido")]
        [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} carácteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string LastName { get; set; }

        [Display(Name = "Tipo de usuario")]
        public UserType UserType { get; set; }

        [Display(Name = "Empresa")]
        public Company? Company { get; set; }

        [Display(Name = "Empresa")]
        public int? CompanyId { get; set; }

        [Display(Name = "Fecha Creación")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public DateTime CreateDate { get; set; }

        [Display(Name = "Usuario Creación")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string CreateUserId { get; set; }

        [Display(Name = "Usuario Creación")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string CreateUserName { get; set; }

        [Display(Name = "Fecha Ultima Modificación")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public DateTime LastChangeDate { get; set; }

        [Display(Name = "Usuario Ultima Modificación")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string LastChangeUserId { get; set; }

        [Display(Name = "Usuario Ultima Modificación")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string LastChangeUserName { get; set; }

        [Display(Name = "Activo")]
        public bool Active { get; set; }

        [Display(Name = "Usuario")]
        public string FullName => $"{FirstName} {LastName}";

        public ICollection<TicketCab>? Tickets { get; set; }

        [Display(Name = "Tickets")]
        public int UsersNumber => Tickets == null ? 0 : Tickets.Count;
    }
}
