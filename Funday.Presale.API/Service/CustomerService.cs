using Funday.Presale.API.Repository;
using Funday.Presale.API.Service.Interface;
using System.Data;
using Dapper;
using Funday.Presale.API.Models.ViewModel;
using Funday.Presale.API.Configure;

namespace Funday.Presale.API.Service
{
    public class CustomerService : ICustomer
    {
        private readonly ILogger<ICustomer> _logger;
        private readonly ConnectionConfig _connectionConfig;
        public CustomerService(ILogger<ICustomer> logger, ConnectionConfig connectionConfig)
        {
            _logger = logger;
            _connectionConfig = connectionConfig;
        }

        public async Task<IEnumerable<Customer>> GetCustomer(Customer customer)
        {
            DapperBase db = new(_connectionConfig.FundayB2B);
            string strSQL;
            using (var connection = db.OpenConnection())
            {
                strSQL = "SELECT * FROM Customer WITH (NOLOCK) WHERE 1=1 ";
                if (!string.IsNullOrEmpty(customer.id.ToString()))
                {
                    if (customer.id != 0)
                        strSQL += " AND [id] = @id ";
                }
                if (!string.IsNullOrEmpty(customer.name))
                {
                    customer.name = $@"%{customer.name}%";
                    strSQL += " AND [name] LIKE @name ";
                }
                if (!string.IsNullOrEmpty(customer.sales))
                {
                    strSQL += " AND [sales] = @sales ";
                }
                var data = await connection.QueryAsync<Customer>(strSQL, customer);
                return data;
            }
        }

        public async Task<Customer?> AddCustomer(Customer customer)
        {
            //Presale 和 FundayB2B 各增加一筆
            DapperBase db = new(_connectionConfig.Presale);
            DapperBase db2 = new(_connectionConfig.FundayB2B);
            string strSQL;
            int effect;
            using var connection = db.OpenConnection();
            //先取得已存在的customer_id最大號
            strSQL = "select max(indx) + 1 from Customer with (nolock)";
            customer.id = await connection.QueryFirstAsync<int>(strSQL);


            string defaultAccount = ConfigHelper.GetConfig<string>("B2B_Admin:DefaultAccount");
            string defaultPwd = ConfigHelper.GetConfig<string>("B2B_Admin:DefaultPwd");
            string defaultName = ConfigHelper.GetConfig<string>("B2B_Admin:DefaultName");

            strSQL = "INSERT INTO Customer ([indx], [name], [type], [DNS], [agent], [number], [Sales], [memo], is_try, startdates, enddates, updates) ";
            strSQL += "VALUES(@id, @name, @type, @dns, @agent, @users_number, @sales, @memo, @is_try, @start_date, @end_date, GETDATE()) ";
            effect = await connection.ExecuteAsync(strSQL, customer);
            if (effect > 0)
            {                
                using var connection2 = db2.OpenConnection();
                try
                {
                    
                    //每增加一筆客戶，要先新增一筆Member帳號: admin
                    //string defaultAccount = ConfigHelper.GetConfig<string>("B2B_Admin:DefaultAccount");
                    //string defaultPwd = ConfigHelper.GetConfig<string>("B2B_Admin:DefaultPwd");
                    //string defaultName = ConfigHelper.GetConfig<string>("B2B_Admin:DefaultName");
                    strSQL = "INSERT INTO Member (customer_id, member_account, [password], nick_name, real_name, start_date, end_date, curator, is_pay, created_date, modified_date)";
                    strSQL += "VALUES(@customer_id, @member_account, @password, @nickname, @realname, @start_date, @end_date, 1, 1, GETDATE(), GETDATE()) ";
                    DynamicParameters parameters = new();
                    parameters.Add("customer_id", customer.id);
                    parameters.Add("member_account", defaultAccount);
                    parameters.Add("password", defaultPwd);
                    parameters.Add("nickname", defaultName);
                    parameters.Add("realname", defaultName);
                    parameters.Add("start_date", DateTime.Now.ToString("yyyy-MM-dd 00:00:00"));
                    parameters.Add("end_date", DateTime.Now.ToString("2099-04-30 00:00:00"));
                    await connection2.ExecuteAsync(strSQL, parameters);

                    //新增客戶
                    strSQL = "INSERT INTO Customer ([id], [name], [type], [DNS], [agent], users_number, [sales], [memo], is_try, start_date, end_date, created_date, modified_date) ";
                    strSQL += "VALUES(@id, @name, @type, @dns, @agent, @users_number, @sales, @memo, @is_try, @start_date, @end_date, GETDATE(), GETDATE()) ";
                    strSQL += $";SELECT * FROM Customer WITH (NOLOCK) WHERE [id]={customer.id}";

                    var data = await connection2.QuerySingleOrDefaultAsync<Customer>(strSQL, customer);

                    return data;
                }
                catch
                {
                    //如果新的Customer新增失敗，新舊的要先刪除
                    await connection.ExecuteAsync($"DELETE Customer WHERE indx={customer.id}");
                    await connection2.ExecuteAsync($"DELETE Customer WHERE [id]={customer.id}");
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public async Task<Customer?> UpdateCustomer(Customer customer)
        {
            DapperBase db = new(_connectionConfig.Presale);
            DapperBase db2 = new(_connectionConfig.FundayB2B);

            string strSQL;
            using var connection = db.OpenConnection();
            strSQL = "UPDATE Customer SET ";
            strSQL += "[name]=@name, [type]=@type, [DNS]=@dns, [agent]=@agent, [number]=@users_number, [Sales]=@sales, [memo]=@memo, is_try=@is_try, startdates=@start_date, enddates=@end_date ";
            strSQL += $"WHERE indx={customer.id}";
            await connection.ExecuteAsync(strSQL, customer);
                        
            using var connection2 = db2.OpenConnection();
            strSQL = "UPDATE Customer SET ";
            strSQL += "[name]=@name, [type]=@type, [DNS]=@dns, [agent]=@agent, users_number=@users_number, [sales]=@sales, [memo]=@memo, is_try=@is_try, start_date=@start_date, end_date=@end_date ";
            strSQL += $"WHERE [id]={customer.id}";
            strSQL += $";SELECT * FROM Customer WITH (NOLOCK) WHERE [id]={customer.id}";
            var data = await connection2.QuerySingleOrDefaultAsync<Customer>(strSQL, customer);

            return data;
        }

        public async Task<int> DeleteCustomer(int id)
        {
            DapperBase db = new(_connectionConfig.Presale);
            DapperBase db2 = new(_connectionConfig.FundayB2B);
            string strSQL;
            int effect;
            DynamicParameters parameters = new();
            parameters.Add("id", id);

            using var connection = db.OpenConnection();
            strSQL = "DELETE Customer WHERE indx=@id";            
            effect = await connection.ExecuteAsync(strSQL, parameters);
                        
            using var connection2 = db2.OpenConnection();
            strSQL = "DELETE Customer WHERE [id]=@id";
            effect = await connection2.ExecuteAsync(strSQL, parameters);

            return effect;
        }

        public async Task<Tuple<IEnumerable<dynamic>, int>> StatisticsList(string? name, PageBase pagebase)
        {
            DapperBase db = new(_connectionConfig.FundayB2B);
            string strSQL;
            //建立SQL語句
            SqlBuilder builder = new();

            strSQL = "SELECT ";
            strSQL += "A.[id], A.[name], A.is_try, A.[start_date], A.[end_date], ";
            strSQL += "ISNULL(A.[agent],'') AS agent, ISNULL(A.[sales],'') AS sales, ";
            strSQL += "ISNULL(A.created_date, '') AS created_date, ";
            strSQL += "ISNULL(B.total_login_cnt, 0) AS total_login_cnt, ISNULL(C.total_read_minutes_cnt, 0) AS total_read_minutes_cnt ";
            strSQL += "FROM Customer A WITH (NOLOCK) ";
            strSQL += "LEFT JOIN ( ";
            strSQL += " SELECT customer_id, SUM(ISNULL(login_cnt,0)) AS total_login_cnt ";
            strSQL += " FROM Login_Count WITH (NOLOCK) ";
            strSQL += " GROUP BY customer_id ";
            strSQL += ") B	ON A.id = B.customer_id ";
            strSQL += "LEFT JOIN ( ";
            strSQL += " SELECT customer_id, SUM(ISNULL(read_minutes_cnt,0)) AS total_read_minutes_cnt ";
            strSQL += " FROM Customer_Record WITH (NOLOCK) ";
            strSQL += " GROUP BY customer_id ";
            strSQL += ") C	ON A.id = C.customer_id ";
            strSQL += "/**where**/ ";
            var selector = builder.AddTemplate(strSQL);
            if (!string.IsNullOrWhiteSpace(name))
            {
                //strSQL += " AND A.[name]=@name ";
                name = $@"%{name}%";
                builder.Where("A.[name] LIKE @name", new { name });
            }
            //設定排序
            List<SortDescriptor> sortings = new();
            pagebase.OrderBy = string.IsNullOrWhiteSpace(pagebase.OrderBy) ? "[id]" : pagebase.OrderBy;
            if (!string.IsNullOrEmpty(pagebase.OrderBy))
            {
                sortings.Add(new SortDescriptor
                {
                    Direction = !string.IsNullOrEmpty(pagebase.OrderSequence) & pagebase.OrderSequence.ToLower() == "desc" ? SortDescriptor.SortingDirection.Descending : SortDescriptor.SortingDirection.Ascending,
                    Field = pagebase.OrderBy
                });
            }
            var data = await db.FindWithOffsetFetch(selector.RawSql, selector.Parameters, pagebase.PageIndex, pagebase.PageSize, sortings);

            return data;
        }
    }
}
