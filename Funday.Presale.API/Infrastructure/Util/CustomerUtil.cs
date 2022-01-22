using Dapper;
using Funday.Presale.API.Models;
using Funday.Presale.API.Repository;
using Newtonsoft.Json;

namespace Funday.Presale.API.Infrastructure.Util
{
    /// <summary>
    /// 客戶會用到的相關函式
    /// </summary>
    public class CustomerUtil
    {
        private readonly ConnectionConfig _connectionConfig;
        public CustomerUtil(ConnectionConfig connectionConfig)
        {
            _connectionConfig = connectionConfig;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="customer_id"></param>
        /// <param name="group_id"></param>
        /// <param name="year_month"></param>
        /// <returns></returns>
        public async Task<IEnumerable<dynamic>> GetCustomerRecord(int customer_id, int? group_id, string year_month = "")
        {
            DapperBase db = new(_connectionConfig.FundayB2B);
            using var connection = db.OpenConnection();
            DynamicParameters parameters = new();
            parameters.Add("customer_id", customer_id);
            string strSQL;
            strSQL = "SELECT * FROM Customer_Record WITH(NOLOCK) ";
            strSQL += "WHERE customer_id=@customer_id ";
            if (group_id != null)
            {
                parameters.Add("group_id", group_id);
                strSQL += " AND group_id=@group_id ";
            }
            if (!string.IsNullOrEmpty(year_month))
            {
                parameters.Add("year_month", year_month);
                strSQL += " AND yyyymm=@year_month ";
            }

            var result = await connection.QueryAsync(strSQL, parameters);
            return result.ToList();
        }

        /// <summary>
        /// 初始化客戶閱讀篇數紀錄Json
        /// </summary>
        /// <param name="customer_id"></param>
        /// <param name="group_id"></param>
        /// <param name="year_month"></param>
        /// <returns></returns>
        public async Task<dynamic> InitReadRecord(int customer_id, int? group_id, string year_month = "")
        {
            dynamic readRecord;
            List<ReadRecord> readRecordList = new();
            var customerRecord = await GetCustomerRecord(customer_id, group_id, year_month);
            if (customerRecord.Any())
            {
                foreach (var cr in customerRecord)
                {
                    if (!string.IsNullOrEmpty(cr.read_record)) { break; }
                    readRecordList.Add(new ReadRecord { classly = typeof(Article).Name.ToLower(), cnt = 0 });
                    readRecordList.Add(new ReadRecord { classly = "teaching", cnt = 0 });
                    readRecordList.Add(new ReadRecord { classly = typeof(MusicBox).Name.ToLower(), cnt = 0 });
                    readRecordList.Add(new ReadRecord { classly = typeof(Story).Name.ToLower(), cnt = 0 });
                    readRecordList.Add(new ReadRecord { classly = typeof(Blog).Name.ToLower(), cnt = 0 });
                    readRecordList.Add(new ReadRecord { classly = typeof(Columns).Name.ToLower(), cnt = 0 });
                    readRecordList.Add(new ReadRecord { classly = typeof(Video).Name.ToLower(), cnt = 0 });

                    string readJson = JsonConvert.SerializeObject(readRecordList);
                    await UpdateReadRecord(customer_id, group_id, year_month, readJson);
                }
            }
            readRecord = await GetReadRecord(customer_id, group_id, year_month);
            return readRecord;
        }

        /// <summary>
        /// 初始化客戶閱讀分鐘數紀錄Json
        /// </summary>
        /// <param name="customer_id"></param>
        /// <param name="group_id"></param>
        /// <param name="year_month"></param>
        /// <returns></returns>
        public async Task<dynamic> InitReadMinutesRecord(int customer_id, int? group_id, string year_month = "")
        {
            dynamic readMinutesRecord;
            List<ReadMinutesRecord> readMinutesRecordList = new();
            var customerRecord = await GetCustomerRecord(customer_id, group_id, year_month);
            if (customerRecord.Any())
            {
                foreach (var cr in customerRecord)
                {
                    if (!string.IsNullOrEmpty(cr.read_minutes_record)) { break; }
                    readMinutesRecordList.Add(new ReadMinutesRecord { classly = typeof(Article).Name.ToLower(), cnt = 0 });
                    readMinutesRecordList.Add(new ReadMinutesRecord { classly = "teaching", cnt = 0 });
                    readMinutesRecordList.Add(new ReadMinutesRecord { classly = typeof(MusicBox).Name.ToLower(), cnt = 0 });
                    readMinutesRecordList.Add(new ReadMinutesRecord { classly = typeof(Story).Name.ToLower(), cnt = 0 });
                    readMinutesRecordList.Add(new ReadMinutesRecord { classly = typeof(Blog).Name.ToLower(), cnt = 0 });
                    readMinutesRecordList.Add(new ReadMinutesRecord { classly = typeof(Columns).Name.ToLower(), cnt = 0 });
                    readMinutesRecordList.Add(new ReadMinutesRecord { classly = typeof(Video).Name.ToLower(), cnt = 0 });

                    string readJson = JsonConvert.SerializeObject(readMinutesRecordList);
                    await UpdateReadMinutesRecord(customer_id, group_id, year_month, readJson);
                }
                
            }
            readMinutesRecord = await GetReadMinutesRecord(customer_id, group_id, year_month);
            return readMinutesRecord;
        }

        /// <summary>
        /// 更新閱讀篇數/月
        /// </summary>
        /// <param name="customer_id"></param>
        /// <param name="group_id"></param>
        /// <param name="year_month"></param>
        /// <returns></returns>
        public async Task<int> UpdateReadCount(int customer_id, int? group_id, string year_month)
        {
            year_month = string.IsNullOrEmpty(year_month) ? DateTime.Now.ToString("yyyyMM") : year_month;
            DapperBase db = new(_connectionConfig.FundayB2B);
            using var connection = db.OpenConnection();
            DynamicParameters parameters = new();
            parameters.Add("customer_id", customer_id);
            parameters.Add("year_month", year_month);
            string strSQL;

            var result = await GetCustomerRecord(customer_id, group_id, year_month);
            if (result.Any())
            {
                strSQL = "UPDATE Customer_Record SET read_cnt=ISNULL(read_cnt,0)+1 "; 
                strSQL += "WHERE customer_id=@customer_id ";
                strSQL += " AND yyyymm=@year_month ";
                if (group_id != null)
                {
                    parameters.Add("group_id", group_id);
                    strSQL += " AND group_id=@group_id ";
                }
            }
            else
            {
                parameters.Add("group_id", group_id);
                strSQL = "INSERT INTO Customer_Record (yyyymm, customer_id, group_id, read_cnt) ";
                strSQL += "VALUES(@year_month, @customer_id, @group_id, 1) ";
            }

            return await connection.ExecuteAsync(strSQL, parameters);

        }

        /// <summary>
        /// 更新閱讀分鐘數/月
        /// </summary>
        /// <param name="customer_id"></param>
        /// <param name="group_id"></param>
        /// <param name="year_month"></param>
        /// <returns></returns>
        public async Task<int> UpdateReadMinutesCount(int customer_id, int? group_id, string year_month)
        {
            year_month = string.IsNullOrEmpty(year_month) ? DateTime.Now.ToString("yyyyMM") : year_month;
            DapperBase db = new(_connectionConfig.FundayB2B);
            using var connection = db.OpenConnection();
            DynamicParameters parameters = new();
            parameters.Add("customer_id", customer_id);
            parameters.Add("year_month", year_month);
            string strSQL;

            var result = await GetCustomerRecord(customer_id, group_id, year_month);
            if (result.Any())
            {
                strSQL = "UPDATE Customer_Record SET read_minutes_cnt=ISNULL(read_minutes_cnt,0)+1 ";
                strSQL += "WHERE customer_id=@customer_id ";
                strSQL += " AND yyyymm=@year_month ";
                if (group_id != null)
                {
                    parameters.Add("group_id", group_id);
                    strSQL += " AND group_id=@group_id ";
                }
            }
            else
            {
                parameters.Add("group_id", group_id);
                strSQL = "INSERT INTO Customer_Record (yyyymm, customer_id, group_id, read_minutes_cnt) ";
                strSQL += "VALUES(@year_month, @customer_id, @group_id, 1) ";
            }

            return await connection.ExecuteAsync(strSQL, parameters);

        }
        /// <summary>
        /// 取得閱讀紀錄/月
        /// </summary>
        /// <param name="customer_id"></param>
        /// <param name="group_id"></param>
        /// <param name="year_month">年月(空值則取全部)</param>
        /// <returns></returns>

        public async Task<dynamic> GetReadRecord(int customer_id, int? group_id, string year_month = "")
        {
            year_month = string.IsNullOrEmpty(year_month) ? DateTime.Now.ToString("yyyyMM") : year_month;
            DapperBase db = new(_connectionConfig.FundayB2B);
            using var connection = db.OpenConnection();
            DynamicParameters parameters = new();
            parameters.Add("customer_id", customer_id);
            string strSQL;
            strSQL = "SELECT read_record FROM Customer_Record WITH(NOLOCK) ";
            strSQL += "WHERE customer_id=@customer_id ";
            if (group_id != null)
            {
                parameters.Add("group_id", group_id);
                strSQL += " AND group_id=@group_id ";
            }
            if (!string.IsNullOrEmpty(year_month))
            {
                parameters.Add("year_month", year_month);
                strSQL += " AND yyyymm=@year_month ";
            }

            var result = await connection.QueryFirstOrDefaultAsync(strSQL, parameters);
            return result;
        }
        /// <summary>
        /// 更新閱讀紀錄/月
        /// </summary>
        /// <param name="customer_id"></param>
        /// <param name="group_id"></param>
        /// <param name="year_month"></param>
        /// <param name="readJson"></param>
        /// <returns></returns>
        public async Task<int> UpdateReadRecord(int customer_id, int? group_id, string year_month, string readJson)
        {
            year_month = string.IsNullOrEmpty(year_month) ? DateTime.Now.ToString("yyyyMM") : year_month;
            DapperBase db = new(_connectionConfig.FundayB2B);
            using var connection = db.OpenConnection();
            DynamicParameters parameters = new();
            parameters.Add("customer_id", customer_id);
            parameters.Add("year_month", year_month);
            parameters.Add("readJson", readJson);
            string strSQL;

            var result = await GetCustomerRecord(customer_id, group_id, year_month);
            if (result.Any())
            {
                strSQL = $"UPDATE Customer_Record SET read_record=@readJson ";
                strSQL += "WHERE customer_id=@customer_id ";
                strSQL += " AND yyyymm=@year_month ";
                if (group_id != null)
                {
                    parameters.Add("group_id", group_id);
                    strSQL += " AND group_id=@group_id ";
                }
            }
            else
            {
                parameters.Add("group_id", group_id);
                strSQL = "INSERT INTO Customer_Record (yyyymm, customer_id, group_id, read_record) ";                
                strSQL += "VALUES(@year_month, @customer_id, @group_id, @readJson) ";
            }

            return await connection.ExecuteAsync(strSQL, parameters);

        }

        /// <summary>
        /// 取得閱讀分鐘數紀錄/月
        /// </summary>
        /// <param name="customer_id"></param>
        /// <param name="group_id"></param>
        /// <param name="year_month">年月(空值則取全部)</param>
        /// <returns></returns>
        public async Task<dynamic> GetReadMinutesRecord(int customer_id, int? group_id, string year_month = "")
        {
            year_month = string.IsNullOrEmpty(year_month) ? DateTime.Now.ToString("yyyyMM") : year_month;
            DapperBase db = new(_connectionConfig.FundayB2B);
            using var connection = db.OpenConnection();
            DynamicParameters parameters = new();
            parameters.Add("customer_id", customer_id);
            string strSQL;
            strSQL = "SELECT read_minutes_record FROM Customer_Record WITH(NOLOCK) ";
            strSQL += $"WHERE customer_id=@customer_id ";
            if (group_id != null)
            {
                parameters.Add("group_id", group_id);
                strSQL += " AND group_id=@group_id ";
            }
            if (!string.IsNullOrEmpty(year_month))
            {
                parameters.Add("year_month", year_month);
                strSQL += " AND yyyymm=@year_month ";
            }

            var result = await connection.QueryFirstOrDefaultAsync(strSQL, parameters);
            return result;
        }
        /// <summary>
        /// 更新閱讀分鐘數紀錄/月
        /// </summary>
        /// <param name="customer_id"></param>
        /// <param name="group_id"></param>
        /// <param name="year_month"></param>
        /// <param name="readJson"></param>
        /// <returns></returns>
        public async Task<int> UpdateReadMinutesRecord(int customer_id, int? group_id, string year_month, string readJson)
        {
            year_month = string.IsNullOrEmpty(year_month) ? DateTime.Now.ToString("yyyyMM") : year_month;
            DapperBase db = new(_connectionConfig.FundayB2B);
            using var connection = db.OpenConnection();
            DynamicParameters parameters = new();
            parameters.Add("customer_id", customer_id);
            parameters.Add("year_month", year_month);
            parameters.Add("readJson", readJson);
            string strSQL;

            var result = await GetCustomerRecord(customer_id, group_id, year_month);
            if (result.Any())
            {
                strSQL = $"UPDATE Customer_Record SET read_minutes_record=@readJson ";
                strSQL += "WHERE customer_id=@customer_id ";
                strSQL += " AND yyyymm=@year_month ";
                if (group_id != null)
                {
                    parameters.Add("group_id", group_id);
                    strSQL += " AND group_id=@group_id ";
                }
            }
            else
            {
                parameters.Add("group_id", group_id);
                strSQL = "INSERT INTO Customer_Record (yyyymm, customer_id, group_id, read_minutes_record) ";
                strSQL += "VALUES(@year_month, @customer_id, @group_id, @readJson) ";
            }

            return await connection.ExecuteAsync(strSQL, parameters);

        }
    }
}
