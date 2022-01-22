using Flurl.Http;
using Funday.Presale.API.Service.Interface;

namespace Funday.Presale.API.Service
{
    public class ApiRequestService: IApiRequestService
    {
        private readonly ILogger<IApiRequestService> _logger;

        public ApiRequestService(ILogger<IApiRequestService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 請求資料
        /// </summary>
        /// <param name="requsetMode"></param>
        /// <param name="apiUrl"></param>
        /// <param name="headerKeyValue"></param>
        /// <returns></returns>
        public async Task<(bool IsSuccess, string Message)> RequestAsync(RequsetModeEnum requsetMode, string apiUrl, string headerKeyValue = null)
        {
            try
            {
                string headerKey = "Funday.Presale.API.Request";
                string headerValue = "Success";

                if (!string.IsNullOrWhiteSpace(headerKeyValue) && headerKeyValue.Contains('='))
                {
                    headerKey = headerKeyValue.Split('=')[0];
                    headerValue = headerKeyValue.Split('=')[1];
                }

                IFlurlRequest flurlRequest = apiUrl.WithHeader(headerKey, headerValue);
                if (flurlRequest == null)
                {
                    return (false, "flurlRequest Null！");
                }

                IFlurlResponse? flurResponse = default;

                if (requsetMode == RequsetModeEnum.Delete)
                {
                    flurResponse = await flurlRequest.DeleteAsync();
                }

                if (requsetMode == RequsetModeEnum.Post)
                {
                    flurResponse = await flurlRequest.PostAsync();
                }

                if (requsetMode == RequsetModeEnum.Get)
                {
                    flurResponse = await flurlRequest.GetAsync();
                }

                if (flurResponse == null)
                {
                    return (false, "flurResponse Null！");
                }

                var result = await flurResponse.GetStringAsync();

                if (string.IsNullOrWhiteSpace(result))
                {
                    return (false, "result Null！");
                }

                return (true, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"接口呼叫異常【ApiRequestService 》RequestAsync】：{ex.Message}");
                return (false, ex.Message);
            }

        }
    }


}
