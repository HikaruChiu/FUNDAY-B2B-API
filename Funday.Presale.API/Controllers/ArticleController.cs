using EnumsNET;
using Funday.Presale.API.Infrastructure.ApiResultManage;
using Funday.Presale.API.Service.Interface;
using Microsoft.AspNetCore.Mvc;
namespace Funday.Presale.API.Controllers
{
    /// <summary>
    /// 文章控制器
    /// </summary>
    public class ArticleController : ApiBaseController<IArticle>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        public ArticleController(IArticle service) : base(service)
        {

        }

        /// <summary>
        /// 取得 News
        /// </summary>
        /// <param name="articleId">文章ID</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> GetArticle(int articleId)
        {
            var result = await _service.GetArticle(articleId);
            string description;
            if (result != null)
            {
                description = ApiResult.ApiResultCodeEnum.Ok.AsString(EnumFormat.Description);
                return this.ResultOk(description, result);
            }
            else
            {
                description = ApiResult.ApiResultCodeEnum.NoContent.AsString(EnumFormat.Description);
                return this.ResultWarn(description);
            }

        }

        /// <summary>
        /// 更新文章錄音紀錄
        /// </summary>
        /// <param name="customer_id">客戶ID</param>
        /// <param name="member_id">會員ID</param>
        /// <param name="fromRecordingJson">錄音紀錄JSON</param>
        /// <remarks>
        /// 錄音紀錄JSON 範例 :
        /// <pre>
        /// {
        ///     "articleId":20739,
        ///     "recording":[
        ///         {
        ///             "filename":"411-488125-20739-1.mp3",
        ///             "row":"1"
        ///         },
        ///         {
        ///             "filename":"411-488125-20739-2.mp3",
        ///             "row":"2"
        ///         },
        ///         {
        ///             "filename":"411-488125-20739-3.mp3",
        ///             "row":"3"
        ///         },
        ///         "createdDate":"2021/12/09 17:25:45",
        ///         "modifiedDate":"2021/12/09 17:25:45"
        ///     ]
        /// }
        /// </pre>
        /// </remarks>
        /// <returns></returns>
        [HttpPost("UpdateMemberRecording")]
        public async Task<ApiResult> UpdateMemberRecording(int customer_id, int member_id, string fromRecordingJson)
        {
            var result = await _service.UpdateMemberRecording(customer_id, member_id, fromRecordingJson);
            string description;
            if (result != null)
            {
                description = ApiResult.ApiResultCodeEnum.Ok.AsString(EnumFormat.Description);
                return this.ResultOk(description, result);
            }
            else
            {
                description = ApiResult.ApiResultCodeEnum.NoContent.AsString(EnumFormat.Description);
                return this.ResultWarn(description);
            }
        }

        /// <summary>
        /// 更新便利貼
        /// </summary>
        /// <param name="customer_id">客戶ID</param>
        /// <param name="member_id">會員ID</param>
        /// <param name="articleId">文章ID</param>
        /// <param name="fromMemoboxJson">便利貼Json</param>
        /// <remarks>
        /// <pre>
        /// 便利貼JSON 範例 :
        /// [
        ///     {
        ///         "id":"memo1",,
        ///         "text":"XXXX",
        ///         "basicid2":"Nct2-80",
        ///         "basicid1":"ct2-80"
        ///     },
        ///     {
        ///         "id":"memo2",
        ///         "text":"GOD",
        ///         "basicid2":"Nt2-14",
        ///         "basicid1":"t2-14"
        ///     }
        /// ]
        /// </pre>
        /// </remarks>
        /// <returns></returns>
        [HttpPost("UpdateMemberMemobox")]
        public async Task<ApiResult> UpdateMemberMemobox(int customer_id, int member_id, int articleId, string fromMemoboxJson)
        {
            var result = await _service.UpdateMemberMemobox(customer_id, member_id, articleId, fromMemoboxJson);
            string description;
            if (result != null)
            {
                description = ApiResult.ApiResultCodeEnum.Ok.AsString(EnumFormat.Description);
                return this.ResultOk(description, result);
            }
            else
            {
                description = ApiResult.ApiResultCodeEnum.NoContent.AsString(EnumFormat.Description);
                return this.ResultWarn(description);
            }
        }

        /// <summary>
        /// 更新會員單字收錄
        /// </summary>
        /// <param name="customer_id">客戶ID</param>
        /// <param name="member_id"></param>
        /// <param name="articleId">文章ID</param>
        /// <param name="enWord">英文單字</param>
        /// <param name="chWord">中文說明</param>
        /// <returns></returns>
        [HttpPost("UpdateWordsCollect")]
        public async Task<ApiResult> UpdateWordsCollect(int customer_id, int member_id, int articleId, string enWord, string chWord)
        {
            var result = await _service.UpdateWordsCollect(customer_id, member_id, articleId, enWord, chWord);
            string description;
            if (result != null)
            {
                description = ApiResult.ApiResultCodeEnum.Ok.AsString(EnumFormat.Description);
                return this.ResultOk(description, result);
            }
            else
            {
                description = ApiResult.ApiResultCodeEnum.NoContent.AsString(EnumFormat.Description);
                return this.ResultWarn(description);
            }
        }
    }
}
