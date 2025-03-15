using System.Collections.Generic;
using System.Threading.Tasks;
using TicketsApi.Web.Data.Entities;
using TicketsApi.Web.Models;

namespace TicketsApi.Web.Helpers
{
    public interface IConverterHelper
    {
        Task<User> ToUserAsync(UserViewModel model);

        UserViewModel ToUserViewModel(User user);

        List<UserViewModel> ToUserResponse(List<User> users);

        Task<Company> ToCompanyAsync(CompanyViewModel model);

        CompanyViewModel ToCompanyViewModel(Company company);
    }
}
