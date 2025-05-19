
namespace TicketsApi.Web.Models.Request
{
    public class SendEmailRequest
    {
        public string to { get; set; }
        public string cc { get; set; }
        public string subject { get; set; }
        public string body { get; set; }
    }
}
