using System;
using System.Collections.Generic;
using TicketsApi.Common.Enums;


namespace TicketsApi.Web.Models
{
    public class TicketCabViewModel
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreateUserId { get; set; }
        public string CreateUserName { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int SubcategoryId { get; set; }
        public string SubcategoryName { get; set; }
        public string Title { get; set; }
        public TicketState TicketState { get; set; }
        public DateTime? AsignDate { get; set; }
        public string UserAsign { get; set; }
        public string UserAsignName { get; set; }
        public DateTime? InProgressDate { get; set; }
        public DateTime? FinishDate { get; set; }
        public ICollection<TicketDetViewModel>? TicketDets { get; set; }
        public int TicketDetsNumber => TicketDets == null ? 0 : TicketDets.Count;
    }
}
