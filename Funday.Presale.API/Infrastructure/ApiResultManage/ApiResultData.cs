using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funday.Presale.API.Infrastructure.ApiResultManage
{

    /// <summary>
    /// Api 回傳資料結果類別
    /// </summary>
    public class ApiResultData
    {

        /// <summary>
        /// 回傳Result
        /// </summary>
        /// <param name="apiResultCodeEnum"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ApiResult Result(ApiResult.ApiResultCodeEnum apiResultCodeEnum, string message, object data)
        {
            return ApiResult.Result(apiResultCodeEnum, message, data);
        }

        /// <summary>
        /// 回傳Result
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ApiResult Result(int code, string message, object data)
        {
            return ApiResult.Result(code, message, data);
        }

        #region Ok

        /// <summary>
        /// 回傳Result OK
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public ApiResult ResultOk(string message)
        {
            return ApiResult.OkMessage(message);
        }

        /// <summary>
        /// 回傳Result OK
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ApiResult ResultOk(object data)
        {
            return ApiResult.OkData(data);
        }

        /// <summary>
        /// 回傳Result OK 和訊息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ApiResult ResultOk(string message, object data)
        {
            return ApiResult.Ok(message, data);
        }

        #endregion

        #region 警告

        /// <summary>
        /// 回傳訊息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public ApiResult ResultWarn(string message)
        {
            return ApiResult.OkMessage(message);
        }

        /// <summary>
        /// 回傳資料
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ApiResult ResultWarn(object data)
        {
            return ApiResult.OkData(data);
        }

        /// <summary>
        /// 回傳訊息和資料
        /// </summary>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ApiResult ResultWarn(string message, object data)
        {
            return ApiResult.Ok(message, data);
        }

        #endregion

    }
}
