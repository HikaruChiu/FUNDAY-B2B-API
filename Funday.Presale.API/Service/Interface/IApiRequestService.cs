namespace Funday.Presale.API.Service.Interface
{
    public interface IApiRequestService
    {
        Task<(bool IsSuccess, string Message)> RequestAsync(RequsetModeEnum requsetMode, string apiUrl, string headerKeyValue);
    }

    /// <summary>
    /// 請求方式
    /// </summary>
    public enum RequsetModeEnum
    {
        Post,
        Get,
        Delete
    }
}
