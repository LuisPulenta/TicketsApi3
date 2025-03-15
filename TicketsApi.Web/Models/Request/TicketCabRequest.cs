using System;


namespace TicketsApi.Web.Models.Request
{
    public class TicketCabRequest
    {
        public int Id { get; set; }
        public int TicketState { get; set; }
        public DateTime? AsignDate { get; set; }
        public DateTime? InProgressDate { get; set; }
        public DateTime? FinishDate { get; set; }
        public string UserAsign { get; set; }
        public string UserAsignName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int SubcategoryId { get; set; }
        public string SubcategoryName { get; set; }
    }
}
