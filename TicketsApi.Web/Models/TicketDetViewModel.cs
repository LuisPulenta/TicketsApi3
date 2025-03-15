using System;

namespace TicketsApi.Web.Models
{
    public class TicketDetViewModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string TicketState { get; set; }
        public DateTime StateDate { get; set; }
        public string StateUserId { get; set; }
        public string StateUserName { get; set; }
        public string Image { get; set; }
        public string ImageFullPath => string.IsNullOrEmpty(Image)
        ? $"https://keypress.serveftp.net/Tickets/images/tickets/noimage.png"
        : $"https://keypress.serveftp.net/Tickets{Image.Substring(1)}";
    }
}
