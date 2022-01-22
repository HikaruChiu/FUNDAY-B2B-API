namespace Funday.Presale.API.Service.Interface
{
    public interface IMail
    {
        public async Task<bool> SendAsync()
        {
            //需要加上await才能執行非同步
            return false;
        }
    }
}
