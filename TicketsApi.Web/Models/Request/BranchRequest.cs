namespace TicketsApi.Web.Models.Request
{
    public class BranchRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CreateUserId { get; set; }
        public string LastChangeUserId { get; set; }
        public bool Active { get; set; }
        public int CompanyId { get; set; }
    }
}