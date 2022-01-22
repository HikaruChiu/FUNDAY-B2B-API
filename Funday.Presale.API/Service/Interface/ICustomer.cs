using Funday.Presale.API.Models.ViewModel;
using Funday.Presale.API.Repository;

namespace Funday.Presale.API.Service.Interface
{
    public interface ICustomer
    {
        //Task<IEnumerable<Customer>> GetCustomer(Customer customer, PageBase pageBase);
        Task<IEnumerable<Customer>> GetCustomer(Customer customer);

        Task<Customer?> AddCustomer(Customer customer);

        Task<Customer?> UpdateCustomer(Customer customer);

        Task<int> DeleteCustomer(int id);

        Task<Tuple<IEnumerable<dynamic>, int>> StatisticsList(string? name, PageBase pagebase);

    }
}
