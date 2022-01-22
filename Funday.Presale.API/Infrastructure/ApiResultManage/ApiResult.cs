using System.ComponentModel;

namespace Funday.Presale.API.Infrastructure.ApiResultManage
{

    /// <summary>
    /// Api 回傳Result類別
    /// </summary>
    public class ApiResult
    {
        public ApiResult(int code, string message, object data, object pageInfo = null)
        {
            Code = code;
            Message = message;
            Data = data;
            PageInfo = pageInfo;            
        }

        public int Code { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public object PageInfo { get; set; }


        /// <summary>
        /// 訊息回傳碼
        /// </summary>
        public enum ApiResultCodeEnum
        {
            /// <summary>
            /// 接口不存在
            /// </summary>
            [Description("接口不存在")]
            NotFount = -3,

            /// <summary>
            /// 程式錯誤
            /// </summary>
            [Description("程式錯誤")]
            Error,

            /// <summary>
            /// 未授權
            /// </summary>
            [Description("未授權")]
            UnAuth,

            /// <summary>
            /// 警告
            /// </summary>
            [Description("警告")]
            Warn,

            /// <summary>
            /// 成功
            /// </summary>
            [Description("成功")]
            Ok,

            /// <summary>
            /// 失敗
            /// </summary>
            [Description("失敗")]
            Fail,

            /// <summary>
            /// 找不到資料
            /// </summary>
            [Description("找不到資料")]
            NoContent,

            /// <summary>
            /// 登入成功
            /// </summary>
            [Description("登入成功")]
            LoginOK,

            /// <summary>
            /// 登入失敗
            /// </summary>
            [Description("登入失敗")]
            LoginFail,
        }

        #region result

        /// <summary>
        /// 回傳訊息
        /// </summary>
        /// <param name="apiResultCodeEnum"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ApiResult ResultMessage(ApiResultCodeEnum apiResultCodeEnum, string message)
        {
            return new ApiResult((int)apiResultCodeEnum, message, null);
        }

        /// <summary>
        /// 回傳資料
        /// </summary>
        /// <param name="apiResultCodeEnum"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ApiResult ResultData(ApiResultCodeEnum apiResultCodeEnum, object data)
        {
            return new ApiResult((int)apiResultCodeEnum, null, data);
        }

        /// <summary>
        /// 回傳訊息和資料
        /// </summary>
        /// <param name="apiResultCodeEnum"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ApiResult Result(ApiResultCodeEnum apiResultCodeEnum, string message, object data, object pageInfo = null)
        {
            return new ApiResult((int)apiResultCodeEnum, message, data, pageInfo);
        }

        #endregion

        #region result code 可傳入 int

        /// <summary>
        /// 回傳訊息
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ApiResult ResultMessage(int code, string message)
        {
            return new ApiResult(code, message, null);
        }

        /// <summary>
        /// 回傳資料
        /// </summary>
        /// <param name="code"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ApiResult ResultData(int code, object data)
        {
            return new ApiResult(code, null, data);
        }

        /// <summary>
        /// 回傳訊息和資料
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ApiResult Result(int code, string message, object data)
        {
            return new ApiResult(code, message, data);
        }

        #endregion

        #region Ok

        /// <summary>
        /// 回傳成功訊息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ApiResult OkMessage(string message)
        {
            return ResultMessage(ApiResultCodeEnum.Ok, message);
        }

        /// <summary>
        /// 回傳成功資料
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ApiResult OkData(object data)
        {
            return ResultData(ApiResultCodeEnum.Ok, data);
        }

        /// <summary>
        /// 回傳成功訊息和資料
        /// </summary>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ApiResult Ok(string message, object data, object pageInfo = null)
        {
            return Result(ApiResultCodeEnum.Ok, message, data, pageInfo);
        }

        #endregion

        #region warn

        /// <summary>
        /// 回傳警告訊息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ApiResult WarnMessage(string message)
        {
            return ResultMessage(ApiResultCodeEnum.Warn, message);
        }

        /// <summary>
        /// 回傳警告資料
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ApiResult WarnData(object data)
        {
            return ResultData(ApiResultCodeEnum.Warn, data);
        }

        /// <summary>
        /// 回傳警告訊息和資料
        /// </summary>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ApiResult Warn(string message, object data)
        {
            return Result(ApiResultCodeEnum.Warn, message, data);
        }

        #endregion

    }

    
}
