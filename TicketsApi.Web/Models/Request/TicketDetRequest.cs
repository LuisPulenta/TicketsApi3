namespace TicketsApi.Web.Models.Request
{
    public class TicketDetRequest
    {
        public int Id { get; set; }
        public int TicketCabId { get; set; }
        public string Description { get; set; }
        public int TicketState { get; set; }
        public string StateUserId { get; set; }
        public string StateUserName { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public byte[] ImageArray { get; set; }
    }
}
