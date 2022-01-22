using Funday.Presale.API.Infrastructure.ApiResultManage;
using Funday.Presale.API.Infrastructure.NLogService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace Funday.Presale.API.Filters
{
    /// <summary>
    /// 異常錯誤過濾器
    /// </summary>
    public class ApiExceptionFilter : IAsyncExceptionFilter, IOrderedFilter
    {
        private readonly INLogHelper _logHelper;
        public ApiExceptionFilter(INLogHelper logHelper)
        {
            _logHelper = logHelper;
        }

        public int Order { get; set; } = int.MaxValue - 10;

        /// <summary>
        /// 重新OnExceptionAsync方法
        /// </summary>
        /// <param name="context">異常信息</param>
        /// <returns></returns>
        public Task OnExceptionAsync(ExceptionContext context)
        {

            var exception = context.Exception;
            if (exception is MessageBox error)
            {
                context.ExceptionHandled = true;
                context.HttpContext.Response.StatusCode = 200;
                context.Result = new JsonResult(error.GetApiResult());
                return Task.CompletedTask;
            }

            if (context.HttpContext.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
            {
                context.ExceptionHandled = true;
                context.HttpContext.Response.StatusCode = 200;
                context.Result = new JsonResult(ApiResult.ResultMessage(ApiResult.ApiResultCodeEnum.UnAuth, "未授權!"));
                return Task.CompletedTask;
            }

            //nlog 寫入到 txt
            _logHelper.LogError(exception, context.HttpContext.Connection.RemoteIpAddress?.ToString());
            var message = $"伺服器端出現異常![異常訊息：{exception.Message}]";
            var apiResult = ApiResult.ResultMessage(ApiResult.ApiResultCodeEnum.Error, message);
            context.Result = new JsonResult(apiResult);
            return Task.CompletedTask;
        }
    }
}

    

