namespace Funday.Presale.API.Service.Interface
{
    public interface ILogin
    {
        Task<IEnumerable<dynamic>> Login(string member_account, string password, string dns);

        Task LoginCountAdd(int customer_id, string member_account, int? group_id);
    }
}
