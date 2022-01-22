using Dapper;
using Funday.Presale.API.Repository;

namespace Funday.Presale.API.Infrastructure.Util
{
    /// <summary>
    /// 會員所用到的相關函式
    /// </summary>
    public class MemberUtil
    {
        private readonly ConnectionConfig _connectionConfig;
        public MemberUtil(ConnectionConfig connectionConfig)
        {
            _connectionConfig = connectionConfig;
        }
        public async Task<dynamic> GetArticle(int articleId)
        {
            DapperBase db = new(_connectionConfig.Funday);
            using var connection = db.OpenConnection();
            DynamicParameters parameters = new();
            string strSQL;

            //取得是否有這筆文章資料
            strSQL = "SELECT TOP 1 * FROM news WITH(NOLOCK) WHERE indx=@articleId ";
            parameters = new();
            parameters.Add("articleId", articleId);
            var result = await connection.QuerySingleOrDefaultAsync<dynamic>(strSQL, parameters);
            return result;
        }

        public async Task<dynamic> GetColumns(int columnsId)
        {
            DapperBase db = new(_connectionConfig.Funday);
            using var connection = db.OpenConnection();
            DynamicParameters parameters = new();
            string strSQL;

            //取得是否有這筆文章資料
            strSQL = "SELECT TOP 1 * FROM Columns WITH(NOLOCK) WHERE indx=@columnsId ";
            parameters = new();
            parameters.Add("columnsId", columnsId);
            var result = await connection.QuerySingleOrDefaultAsync<dynamic>(strSQL, parameters);
            return result;
        }

        public async Task<dynamic> GetStory(int storyId)
        {
            DapperBase db = new(_connectionConfig.Funday);
            using var connection = db.OpenConnection();
            DynamicParameters parameters = new();
            string strSQL;

            //取得是否有這筆文章資料
            strSQL = "SELECT TOP 1 * FROM Story WITH(NOLOCK) WHERE indx=@storyId ";
            parameters = new();
            parameters.Add("storyId", storyId);
            var result = await connection.QuerySingleOrDefaultAsync<dynamic>(strSQL, parameters);
            return result;
        }

        public async Task<dynamic> GetVideo(int videoId)
        {
            DapperBase db = new(_connectionConfig.Funday);
            using var connection = db.OpenConnection();
            DynamicParameters parameters = new();
            string strSQL;

            //取得是否有這筆文章資料
            strSQL = "SELECT TOP 1 * FROM Program WITH(NOLOCK) WHERE indx=@videoId ";
            parameters = new();
            parameters.Add("videoId", videoId);
            var result = await connection.QuerySingleOrDefaultAsync<dynamic>(strSQL, parameters);
            return result;
        }

        public async Task<dynamic> GetMusicBox(int musicboxId)
        {
            DapperBase db = new(_connectionConfig.Funday);
            using var connection = db.OpenConnection();
            DynamicParameters parameters = new();
            string strSQL;

            //取得是否有這筆文章資料
            strSQL = "SELECT TOP 1 * FROM MusicBox WITH(NOLOCK) WHERE indx=@musicboxId ";
            parameters = new();
            parameters.Add("musicboxId", musicboxId);
            var result = await connection.QuerySingleOrDefaultAsync<dynamic>(strSQL, parameters);
            return result;
        }

        public async Task<dynamic> GetBlog(int blogId)
        {
            DapperBase db = new(_connectionConfig.Funday);
            using var connection = db.OpenConnection();
            DynamicParameters parameters = new();
            string strSQL;

            //取得是否有這筆文章資料
            strSQL = "SELECT TOP 1 * FROM Blog WITH(NOLOCK) WHERE indx=@blogId ";
            parameters = new();
            parameters.Add("blogId", blogId);
            var result = await connection.QuerySingleOrDefaultAsync<dynamic>(strSQL, parameters);
            return result;
        }

        
    }
}
