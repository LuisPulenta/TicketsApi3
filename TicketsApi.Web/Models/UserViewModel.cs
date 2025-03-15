using System;
using System.Collections.Generic;


namespace TicketsApi.Web.Models
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int UserTypeId { get; set; }
        public string UserTypeName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirm { get; set; }
        public string PhoneNumber { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public DateTime CreateDate { get; set; }
        public string? CreateUserId { get; set; }
        public string? CreateUserName { get; set; }
        public DateTime LastChangeDate { get; set; }
        public string? LastChangeUserId { get; set; }
        public string? LastChangeUserName { get; set; }
        public bool Active { get; set; }
        public ICollection<TicketCabViewModel> Tickets { get; set; }
        public string FullName => $"{FirstName} {LastName}";
    }
}
