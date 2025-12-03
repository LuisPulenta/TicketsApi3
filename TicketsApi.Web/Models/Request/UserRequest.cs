using TicketsApi.Common.Enums;

namespace TicketsApi.Web.Models.Request
{
    public class UserRequest
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public int IdCompany { get; set; }
        public int IdUserType { get; set; }
        public string CreateUserId { get; set; }
        public string LastChangeUserId { get; set; }
        public bool Active { get; set; }
        public int? IsResolver { get; set; }
        public int? IsBoss { get; set; }
        public int? IdBranch { get; set; }
    }
}