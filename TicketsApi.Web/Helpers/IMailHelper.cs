using TicketsApi.Common.Models;

namespace TicketsApi.Web.Helpers
{
    public interface IMailHelper
    {
        Response SendMail(string to, string subject, string body);
    }
}
