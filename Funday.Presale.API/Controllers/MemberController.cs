using EnumsNET;
using Funday.Presale.API.Infrastructure.ApiResultManage;
using Funday.Presale.API.Service;
using Funday.Presale.API.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Funday.Presale.API.Controllers
{
    /// <summary>
    /// 會員控制器
    /// </summary>
    public class MemberController : ApiBaseController<IMember>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        public MemberController(IMember service) : base(service)
        {

        }

        /// <summary>
        /// 取得會員
        /// </summary>
        /// <param name="customer_id"></param>
        /// <param name="member_id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> GetMember(int customer_id, int member_id)
        {
            var result = await _service.GetMember(customer_id, member_id);
            string description;
            if (result.Any())
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
        /// 更新學習紀錄-文章
        /// </summary>
        /// <param name="customer_id">客戶ID</param>
        /// <param name="member_id">會員ID</param>
        /// <param name="articleId">文章ID</param>
        /// <param name="readType">閱讀模式(文章 = 1 ， 老師講解 = 2)</param>
        /// <remarks>每次呼叫這支API都會累積學習分鐘數 + 1</remarks>
        /// <returns></returns>
        [HttpPost("UpdateArticleLR")]
        public async Task<ApiResult> UpdateArticleLR(int customer_id, int member_id, int articleId, int readType)
        {
            var result = await _service.UpdateArticleLearningRecord(customer_id, member_id, articleId, readType);
            string description = result.Item2;
            if (description == "OK")
            {
                description = ApiResult.ApiResultCodeEnum.Ok.AsString(EnumFormat.Description);
                return this.ResultOk(description);
            }
            else
            {
                return this.ResultWarn(description);
            }
        }

        /// <summary>
        /// 更新學習紀錄-專欄
        /// </summary>
        /// <param name="customer_id">客戶ID</param>
        /// <param name="member_id">會員ID</param>
        /// <param name="columnsId">專欄ID</param>
        /// <remarks>每次呼叫這支API都會累積學習分鐘數 + 1</remarks>
        /// <returns></returns>
        [HttpPost("UpdateColumnsLR")]
        public async Task<ApiResult> UpdateColumnsLR(int customer_id, int member_id, int columnsId)
        {
            var result = await _service.UpdateColumnsLearningRecord(customer_id, member_id, columnsId);
            string description = result.Item2;
            if (description == "OK")
            {
                description = ApiResult.ApiResultCodeEnum.Ok.AsString(EnumFormat.Description);
                return this.ResultOk(description);
            }
            else
            {
                return this.ResultWarn(description);
            }
        }

        /// <summary>
        /// 更新學習紀錄-童話故事
        /// </summary>
        /// <param name="customer_id">客戶ID</param>
        /// <param name="member_id">會員ID</param>
        /// <param name="storyId">故事ID</param>
        /// <remarks>每次呼叫這支API都會累積學習分鐘數 + 1</remarks>
        /// <returns></returns>
        [HttpPost("UpdateStoryLR")]
        public async Task<ApiResult> UpdateStoryLR(int customer_id, int member_id, int storyId)
        {
            var result = await _service.UpdateStoryLearningRecord(customer_id, member_id, storyId);
            string description = result.Item2;
            if (description == "OK")
            {
                description = ApiResult.ApiResultCodeEnum.Ok.AsString(EnumFormat.Description);
                return this.ResultOk(description);
            }
            else
            {
                return this.ResultWarn(description);
            }
        }

        /// <summary>
        /// 更新學習紀錄-影片
        /// </summary>
        /// <param name="customer_id">客戶ID</param>
        /// <param name="member_id">會員ID</param>
        /// <param name="videoId">影片ID</param>
        /// <remarks>每次呼叫這支API都會累積學習分鐘數 + 1</remarks>
        /// <returns></returns>
        [HttpPost("UpdateVideoLR")]
        public async Task<ApiResult> UpdateVideoLR(int customer_id, int member_id, int videoId)
        {
            var result = await _service.UpdateVideoLearningRecord(customer_id, member_id, videoId);
            string description = result.Item2;
            if (description == "OK")
            {
                description = ApiResult.ApiResultCodeEnum.Ok.AsString(EnumFormat.Description);
                return this.ResultOk(description);
            }
            else
            {
                return this.ResultWarn(description);
            }
        }

        /// <summary>
        /// 更新學習紀錄-音樂盒
        /// </summary>
        /// <param name="customer_id">客戶ID</param>
        /// <param name="member_id">會員ID</param>
        /// <param name="musicboxId">音樂盒ID</param>
        /// <remarks>每次呼叫這支API都會累積學習分鐘數 + 1</remarks>
        /// <returns></returns>
        [HttpPost("UpdateMusicBoxLR")]
        public async Task<ApiResult> UpdateMusicBoxLR(int customer_id, int member_id, int musicboxId)
        {
            var result = await _service.UpdateMusicBoxLearningRecord(customer_id, member_id, musicboxId);
            string description = result.Item2;
            if (description == "OK")
            {
                description = ApiResult.ApiResultCodeEnum.Ok.AsString(EnumFormat.Description);
                return this.ResultOk(description);
            }
            else
            {
                return this.ResultWarn(description);
            }
        }

        /// <summary>
        /// 更新學習紀錄-部落格
        /// </summary>
        /// <param name="customer_id">客戶ID</param>
        /// <param name="member_id">會員ID</param>
        /// <param name="blogId">部落格ID</param>
        /// <remarks>每次呼叫這支API都會累積學習分鐘數 + 1</remarks>
        /// <returns></returns>
        [HttpPost("UpdateBlogLR")]
        public async Task<ApiResult> UpdateBlogLR(int customer_id, int member_id, int blogId)
        {
            var result = await _service.UpdateBlogLearningRecord(customer_id, member_id, blogId);
            string description = result.Item2;
            if (description == "OK")
            {
                description = ApiResult.ApiResultCodeEnum.Ok.AsString(EnumFormat.Description);
                return this.ResultOk(description);
            }
            else
            {
                return this.ResultWarn(description);
            }
        }

        /// <summary>
        /// 取得會員所有學習紀錄給首頁Review Bar使用
        /// </summary>
        /// <param name="customer_id">客戶ID</param>
        /// <param name="member_id">會員ID</param>
        /// <param name="pg">帶入頁數(每一頁皆為一個月份的紀錄)</param>
        /// <returns></returns>
        [HttpPost("GetReviewLR")]
        public async Task<ApiResult> GetReviewLR(int customer_id, int member_id, int pg = 1)
        {
            var result = await _service.GetLearningRecordForReview(customer_id, member_id, pg);
            string description;
            if (result.Item1 != null)
            {
                description = ApiResult.ApiResultCodeEnum.Ok.AsString(EnumFormat.Description);
                object pageInfo = new { PageSize = pg, TotalPage = result.Item2 };
                return this.ResultOk(description, result.Item1, pageInfo);
            }
            else
            {
                description = ApiResult.ApiResultCodeEnum.NoContent.AsString(EnumFormat.Description);
                return this.ResultWarn(description);
            }
        }

        /// <summary>
        /// 複製學習紀錄假資料
        /// </summary>
        /// <param name="customer_id">測試客戶ID = 308</param>
        /// <param name="member_id">測試會員ID = 359</param>
        /// <param name="fromYear">複製來源哪年</param>
        /// <param name="fromMonth">複製來源哪月</param>
        /// <param name="setYear">複製後設為哪年</param>
        /// <param name="setMonth">複製後設為哪月</param>
        /// <remarks>僅限測試客戶用，customer_id=308, member_id=359</remarks>
        /// <returns></returns>
        [HttpPost("複製學習紀錄假資料")]
        public async Task<ApiResult> 複製學習紀錄假資料(int customer_id, int member_id, int fromYear, int fromMonth, int setYear, int setMonth)
        {
            var result = await _service.CopyLearningRecord(customer_id, member_id, fromYear, fromMonth, setYear, setMonth);
            string description;
            if (result > 0)
            {
                description = ApiResult.ApiResultCodeEnum.Ok.AsString(EnumFormat.Description);
                return this.ResultOk(description);
            }
            else
            {
                description = ApiResult.ApiResultCodeEnum.NoContent.AsString(EnumFormat.Description);
                return this.ResultWarn(description);
            }

        }

    }
}
