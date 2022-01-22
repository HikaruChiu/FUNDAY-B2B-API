using EnumsNET;
using Funday.Presale.API.Infrastructure.ApiResultManage;
using Funday.Presale.API.Models.ViewModel;
using Funday.Presale.API.Repository;
using Funday.Presale.API.Service;
using Funday.Presale.API.Service.Interface;
using Microsoft.AspNetCore.Mvc;


namespace Funday.Presale.API.Controllers
{
    /// <summary>
    /// 客戶相關的API
    /// </summary>
    public class CustomerController : ApiBaseController<ICustomer>
    {
        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="service"></param>
        public CustomerController(ICustomer service) : base(service)
        {

        }

        /// <summary>
        /// 取得客戶資料
        /// </summary>
        /// <param name="customer"></param>        
        /// <remarks>預設取得全部，查詢時帶入編號 or 名稱 or 業務<br/>查詢條件範例：{ "id": 600, "name": "newb2b2", "sales": "kai" }</remarks>
        /// <returns></returns>
        [HttpPost("Get")]
        public async Task<ApiResult> GetCustomer(Customer customer)
        {
            var result = await _service.GetCustomer(customer);
            string description;
            if (result.Any())
            {
                description = ApiResult.ApiResultCodeEnum.Ok.AsString(EnumFormat.Description);                
                return this.ResultOk(description, result);
            }
            else
            {
                description = ApiResult.ApiResultCodeEnum.NoContent.AsString(EnumFormat.Description);
                return this.ResultWarn(description, null);
            }
            //return this.ResultOk(description, result);
        }

        /// <summary>
        /// 新增客戶
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        [HttpPost("Add")]
        public async Task<ApiResult> AddCustomer(Customer customer)
        {
            var result = await _service.AddCustomer(customer);
            string description;
            if (result != null)
            {
                description = ApiResult.ApiResultCodeEnum.Ok.AsString(EnumFormat.Description);
                return this.ResultOk(description, result);
            }
            else
            {
                description = ApiResult.ApiResultCodeEnum.Fail.AsString(EnumFormat.Description);
                return this.ResultWarn(description, null);
            }
        }

        /// <summary>
        /// 更新客戶資料
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        [HttpPost("Update")]
        public async Task<ApiResult> UpdateCustomer(Customer customer)
        {
            var result = await _service.UpdateCustomer(customer);
            string description;
            if (result != null)
            {
                description = ApiResult.ApiResultCodeEnum.Ok.AsString(EnumFormat.Description);
                return this.ResultOk(description, result);
            }
            else
            {
                description = ApiResult.ApiResultCodeEnum.Fail.AsString(EnumFormat.Description);
                return this.ResultWarn(description, null);
            }
        }

        /// <summary>
        /// 刪除客戶
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("Delete/{id}")]
        public async Task<ApiResult> DeleteCustomer(int id)
        {
            var result = await _service.DeleteCustomer(id);
            string description;
            if (result > 0)
            {
                description = ApiResult.ApiResultCodeEnum.Ok.AsString(EnumFormat.Description);
                return this.ResultOk(description);
            }
            else
            {
                description = ApiResult.ApiResultCodeEnum.Fail.AsString(EnumFormat.Description);
                return this.ResultWarn(description);
            }
        }

        /// <summary>
        /// 客戶統計資料
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost("StatisticsList")]
        public async Task<ApiResult> StatisticsList(string name, [FromQuery] PageBase pagebase)
        {
            var result = await _service.StatisticsList(name, pagebase);
            string description;
            if (result.Item1.Any())
            {
                description = ApiResult.ApiResultCodeEnum.Ok.AsString(EnumFormat.Description);
                int totalCount = result.Item2;
                int totalPage;
                
                totalPage = Math.Max((totalCount + pagebase.PageSize - 1) / pagebase.PageSize, 1);
                object pageInfo = new { PageSize = pagebase.PageSize, PageIndex = pagebase.PageIndex, TotalPage = totalPage, TotalCount = totalCount };
                return this.ResultOk(description, result.Item1, pageInfo);
            }
            else
            {
                description = ApiResult.ApiResultCodeEnum.NoContent.AsString(EnumFormat.Description);
                return this.ResultWarn(description);
            }
        }
    }
}
