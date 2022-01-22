using Dapper;
using Funday.Presale.API.Repository.Interface;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;

namespace Funday.Presale.API.Repository
{
    public class DapperBase: IDapper
    {
        /// <summary>
        /// The connection string
        /// </summary>
        protected string ConnectionString;
        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryBase"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public DapperBase(string connectionString)
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Opens the connection.
        /// </summary>
        /// <returns></returns>
        public virtual IDbConnection OpenConnection()
        {
            IDbConnection connection = new SqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        //public async Task<IEnumerable<dynamic>> FindWithOffsetFetch(string sql , object parameters  , int pageIndex , int pageSize , List<SortDescriptor> sortings)
        //{
        //    using (var connection = OpenConnection())
        //    {

        //        string selectQuery;
        //        selectQuery = ";WITH _data AS (";
        //        selectQuery += $" {sql} ";
        //        selectQuery += "), ";
        //        selectQuery += "_count AS (";
        //        selectQuery += "    SELECT COUNT(1) AS TotalCount FROM _data ";
        //        selectQuery += ") ";
        //        selectQuery += "SELECT * FROM _data CROSS APPLY _count /**orderby**/ ";
        //        selectQuery += "OFFSET @PageIndex * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY";

        //        DynamicParameters dynamicParams = new DynamicParameters();
        //        dynamicParams.AddDynamicParams(parameters);
        //        dynamicParams.Add("PageIndex", pageIndex - 1);
        //        dynamicParams.Add("PageSize", pageSize);

        //        SqlBuilder builder = new();

        //        var selector = builder.AddTemplate(selectQuery, dynamicParams);

        //        foreach (var sorting in sortings)
        //        {
        //            if (string.IsNullOrWhiteSpace(sorting.Field))
        //                continue;

        //            if (sorting.Direction == SortDescriptor.SortingDirection.Ascending)
        //                builder.OrderBy(sorting.Field);
        //            else if (sorting.Direction == SortDescriptor.SortingDirection.Descending)
        //                builder.OrderBy(sorting.Field + " desc");
        //        }

        //        List<dynamic> rows = (List<dynamic>)await connection.QueryAsync<dynamic>(selector.RawSql, dynamicParams);

        //        if (rows.Count == 0)
        //        {
        //            return rows;
        //        }

        //        return rows;

        //    }
        //}

        public async Task<Tuple<IEnumerable<dynamic>, int>> FindWithOffsetFetch(string rawSql, object parameters, int pageIndex, int pageSize, List<SortDescriptor> sortings)
        {
            using (var connection = OpenConnection())
            {

                string selectQuery;
                selectQuery = ";WITH _data AS (";
                selectQuery += $" {rawSql} ";
                selectQuery += "), ";
                selectQuery += "_count AS (";
                selectQuery += "    SELECT COUNT(1) AS TotalCount FROM _data ";
                selectQuery += ") ";
                selectQuery += "SELECT * FROM _data CROSS APPLY _count /**orderby**/ ";
                selectQuery += "OFFSET @PageIndex * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY";

                DynamicParameters dynamicParams = new DynamicParameters();
                dynamicParams.AddDynamicParams(parameters);
                dynamicParams.Add("PageIndex", pageIndex - 1);
                dynamicParams.Add("PageSize", pageSize);

                SqlBuilder builder = new();

                var selector = builder.AddTemplate(selectQuery, dynamicParams);

                foreach (var sorting in sortings)
                {
                    if (string.IsNullOrWhiteSpace(sorting.Field))
                        continue;

                    if (sorting.Direction == SortDescriptor.SortingDirection.Ascending)
                        builder.OrderBy(sorting.Field);
                    else if (sorting.Direction == SortDescriptor.SortingDirection.Descending)
                        builder.OrderBy(sorting.Field + " desc");
                }

                List<dynamic> rows = (List<dynamic>)await connection.QueryAsync<dynamic>(selector.RawSql, dynamicParams);

                if (rows.Count == 0)
                {
                    return new Tuple<IEnumerable<dynamic>, int>(rows, 0);
                }

                return new Tuple<IEnumerable<dynamic>, int>(rows, rows[0].TotalCount);

            }
        }
    }
    public class SortDescriptor
    {
        public SortingDirection Direction { get; set; }
        public string Field { get; set; }

        public enum SortingDirection
        {
            Ascending,
            Descending
        }
    }

    public class ConnectionConfig
    {
        public string FundayB2B { get; set; }
        public string Presale { get; set; }
        public string Company1 { get; set; }
        public string Funday { get; set; }

    }

    /// <summary>
    /// 分頁查詢
    /// </summary>
    public class PageBase
    {
        private int pageSize;

        /// <summary>
        /// 筆數,預設10
        /// </summary>
        [Description("筆數,預設10")]
        public int PageSize
        {
            get { return pageSize <= 0 ? 10 : pageSize; }
            set { pageSize = value; }
        }

        private int pageIndex;

        /// <summary>
        /// 頁數,預設1
        /// </summary>
        [Description("頁數,預設1")]
        public int PageIndex
        {
            get { return pageIndex <= 0 ? 1 : pageIndex; }
            set { pageIndex = value; }
        }

        private string orderBy;

        /// <summary>
        /// 排序欄位
        /// </summary>
        [Description("排序欄位")]
        public string OrderBy
        {
            get { return orderBy; }
            set { orderBy = value; }
        }

        private string orderSequence;

        /// <summary>
        /// asc | desc
        /// </summary>
        [Description("asc | desc")]
        public string OrderSequence
        {
            get { return string.IsNullOrEmpty(orderSequence) ? "asc" : orderSequence; }
            set { orderSequence = value; }
        }
    }

}
