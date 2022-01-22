using Funday.Presale.API.Filters;
using Funday.Presale.API.Infrastructure.ApiResultManage;
using Microsoft.AspNetCore.Mvc;

namespace Funday.Presale.API.Controllers
{
    /// <summary>
    /// 基礎控制器
    /// </summary>
    /// <typeparam name="TDefaultService"></typeparam>
    public class ApiBaseController<TDefaultService> : ApiBaseController
        where TDefaultService : class
    {
        protected readonly TDefaultService _service;

        public ApiBaseController(TDefaultService service)
        {
            _service = service;
        }

    }

    /// <summary>
    /// 基礎控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ApiBaseController : ControllerBase
    {

        /// <summary>
        /// 回傳資料
        /// </summary>
        /// <param name="apiResultCodeEnum"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [NonAction]
        public ApiResult Result(ApiResult.ApiResultCodeEnum apiResultCodeEnum, string message, object data)
        {
            return ApiResult.Result(apiResultCodeEnum, message, data);
        }

        /// <summary>
        /// 回傳資料
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [NonAction]
        public ApiResult Result(int code, string message, object data)
        {
            return ApiResult.Result(code, message, data);
        }

        #region Ok

        /// <summary>
        /// 回傳訊息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [NonAction]
        public ApiResult ResultOk(string message)
        {
            return ApiResult.OkMessage(message);
        }

        /// <summary>
        /// 回傳資料
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [NonAction]
        public ApiResult ResultOk(object data)
        {
            return ApiResult.OkData(data);
        }

        /// <summary>
        /// 回傳訊息和資料
        /// </summary>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [NonAction]
        public ApiResult ResultOk(string message, object data, object pageInfo = null)
        {
            return ApiResult.Ok(message, data, pageInfo);
        }

        #endregion

        #region 警告

        /// <summary>
        /// 回傳訊息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [NonAction]
        public ApiResult ResultWarn(string message)
        {
            return ApiResult.WarnMessage(message);
        }

        /// <summary>
        /// 回傳資料
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [NonAction]
        public ApiResult ResultWarn(object data)
        {
            return ApiResult.WarnData(data);
        }

        /// <summary>
        /// 回傳訊息和資料
        /// </summary>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [NonAction]
        public ApiResult ResultWarn(string message, object data)
        {
            return ApiResult.Warn(message, data);
        }

        #endregion

    }
}
