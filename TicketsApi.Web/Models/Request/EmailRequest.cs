using System.ComponentModel.DataAnnotations;

namespace TicketsApi.Web.Models.Request
{
    public class EmailRequest
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Email { get; set; }
    }
}
