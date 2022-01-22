using Funday.Presale.API.Infrastructure.ApiResultManage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Funday.Presale.API.Filters
{
    /// <summary>
    /// Api 回傳結果封裝
    /// </summary>
    public class ApiResultFilterAttribute : Attribute, IResultFilter
    {
        public bool Ignore { get; set; }

        public ApiResultFilterAttribute(bool ignore = false)
        {
            Ignore = ignore;
        }

        /// <summary>
        /// 回傳結果前
        /// </summary>
        /// <param name="context"></param>
        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (Ignore) return;

            if (context.Result == null)
            {
                return;
            }

            var apiResultData = new ApiResultData();

            if (context.Result is ObjectResult)
            {
                var result = context.Result as ObjectResult;
                context.Result = new JsonResult(apiResultData.ResultOk("success", result.Value));
                return;
            }
            if (context.Result is EmptyResult)
            {
                context.Result = new JsonResult(apiResultData.ResultOk("success", null));
                return;
            }
            if (context.Result is ContentResult)
            {
                var result = context.Result as ContentResult;
                if (result.ContentType.Contains("text/html")) return;

                context.Result = new JsonResult(apiResultData.ResultOk("success", result.Content));
                return;
            }
        }

        /// <summary>
        /// 回傳結果後
        /// </summary>
        /// <param name="context"></param>
        public void OnResultExecuted(ResultExecutedContext context)
        {
            //throw new NotImplementedException();
        }



    }
}
