using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace TicketsApi.Web.Data.Entities
{
    public class Branch
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Sucursal")]
        [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Name { get; set; }

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
        public string LastChangeName { get; set; }

        [Display(Name = "Activo")]
        public bool Active { get; set; }

        public ICollection<User>? Users { get; set; }

        [Display(Name = "Empresa")]
        public Company Company { get; set; }

        [Display(Name = "Usuarios")]
        public int UsersNumber => Users == null ? 0 : Users.Count;
    }
}