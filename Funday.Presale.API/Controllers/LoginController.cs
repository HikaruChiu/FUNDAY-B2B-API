using EnumsNET;
using Funday.Presale.API.Infrastructure.ApiResultManage;
using Funday.Presale.API.Service;
using Funday.Presale.API.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Funday.Presale.API.Controllers
{
    public class LoginController : ApiBaseController<ILogin>
    {
        public LoginController(ILogin service) : base(service)
        {

        }

        [HttpPost]
        public async Task<ApiResult> Login(string member_account, string password, string dns)
        {
            var result = await _service.Login(member_account, password, dns);
            string description;
            //string description = ApiResult.ApiResultCodeEnum.Ok.AsString(EnumFormat.Description);
            if (result.Any())
            {
                description = ApiResult.ApiResultCodeEnum.LoginOK.AsString(EnumFormat.Description);
                return this.ResultOk(description, result);
            }
            else
            {
                description = ApiResult.ApiResultCodeEnum.LoginFail.AsString(EnumFormat.Description);
                return this.ResultWarn(description, result);
            }

        }
    }
}
