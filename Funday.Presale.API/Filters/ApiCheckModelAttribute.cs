using Funday.Presale.API.Infrastructure.ApiResultManage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Funday.Presale.API.Filters
{
    /// <summary>
    /// 對接口模型作驗證
    /// </summary>
    public class ApiCheckModelAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 每次Action發生前
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            if (context.ModelState.IsValid) return;
            var messages = new List<string>();
            var keys = context.ModelState.Keys;
            var values = context.ModelState.Values;
            foreach (var item in keys)
            {
                var value = context.ModelState.FirstOrDefault(w => w.Key == item).Value;
                foreach (var err in value.Errors)
                {
                    messages.Add($"{err.ErrorMessage}");
                }
            }

            var apiResult = ApiResult.Warn(string.Join("<br /><br />", messages), null);
            context.Result = new JsonResult(apiResult);
        }
    }
}
