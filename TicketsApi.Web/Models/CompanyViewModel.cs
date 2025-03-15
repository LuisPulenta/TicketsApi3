using System;
using System.Collections.Generic;

namespace TicketsApi.Web.Models
{
    public class CompanyViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreateDate { get; set; }
        public string? CreateUserId { get; set; }
        public string? CreateUserName { get; set; }
        public DateTime LastChangeDate { get; set; }
        public string? LastChangeUserId { get; set; }
        public string? LastChangeUserName { get; set; }
        public bool Active { get; set; }
        public string Photo { get; set; }
        public string PhotoFullPath => string.IsNullOrEmpty(Photo)
        ? $"https://keypress.serveftp.net/Tickets/images/logos/noimage.png"
        : $"https://keypress.serveftp.net/Tickets{Photo.Substring(1)}";
        public ICollection<UserViewModel> Users { get; set; }
        public int UsersNumber => Users == null ? 0 : Users.Count;
    }
}
