using System;

namespace TicketsApi.Web.Models.Request
{
    public class TicketResueltosRequest
    {
        public int UserType { get; set; }
        public string UserId { get; set; }
        public int CompanyId { get; set; }
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
    }
}
