using Funday.Presale.API.Repository;
using Funday.Presale.API.Service.Interface;
using Funday.Presale.API.Infrastructure.Util;
using System.Data;
using Dapper;
using Newtonsoft.Json;
using Funday.Presale.API.Models;

namespace Funday.Presale.API.Service
{
    public class LoginService : ILogin
    {
        private readonly ILogger<ILogin> _logger;
        private readonly ConnectionConfig _connectionConfig;
        public LoginService(ILogger<ILogin> logger, ConnectionConfig connectionConfig)
        {
            _logger = logger;
            _connectionConfig = connectionConfig;
        }

        public async Task<IEnumerable<dynamic>> Login(string member_account, string password, string dns)
        {
            if (StringExtensions.IsUrl(dns))
            {
                Uri uri = new(dns);
                dns = uri.Host;
            }

            DapperBase db = new(_connectionConfig.FundayB2B);

            string yyyyMM = DateTime.Now.ToString("yyyyMM");

            using var connection = db.OpenConnection();
            DynamicParameters parameters = new();
            string strSQL;
            strSQL = "SELECT Member.*, Member_Levels.* ";
            strSQL += "FROM Member WITH (NOLOCK) ";
            strSQL += "INNER JOIN Customer WITH (NOLOCK) ";
            strSQL += " ON Member.customer_id = Customer.[id] ";
            strSQL += "LEFT JOIN Member_Levels WITH (NOLOCK) ";
            strSQL += " ON Member.customer_id = Member_Levels.customer_id AND Member.id = Member_Levels.member_id ";
            strSQL += "WHERE 1=1 ";
            strSQL += " AND member_account=@member_account ";
            strSQL += " AND password=@password ";
            strSQL += " AND DNS LIKE @dns ";
            parameters.Add("member_account", member_account);
            parameters.Add("password", password);
            parameters.Add("dns", $"%{dns}");
            var data = await connection.QueryAsync<dynamic>(strSQL, parameters);
            if (data.Any())
            {
                foreach (var d in data)
                {
                    //如果登入成功，就+1
                    await LoginCountAdd(d.customer_id, member_account, d.group_id);

                    //紀錄會員登入次數 json
                    string loginCntJson = "";
                    List<MemberLoginCount> memberLoginCountList = new();
                    //檢查是否有會員登入紀錄 json
                    if (string.IsNullOrWhiteSpace(d.login_cnt))
                    {
                        memberLoginCountList.Add(new MemberLoginCount() { yyyymm = yyyyMM, loginCnt = 1 });

                        loginCntJson = JsonConvert.SerializeObject(memberLoginCountList);
                    }
                    else
                    {
                        memberLoginCountList = JsonConvert.DeserializeObject<List<MemberLoginCount>>(d.login_cnt);
                        var rec = memberLoginCountList.Find(x => x.yyyymm.Equals(yyyyMM));
                        if (rec == null)
                        {
                            memberLoginCountList.Add(new MemberLoginCount() { yyyymm = yyyyMM, loginCnt = 1 });
                        }
                        else
                        {
                            rec.loginCnt++; //累計+1 by 年月
                        }
                        memberLoginCountList = memberLoginCountList.OrderBy(x => x.yyyymm).ToList();

                        loginCntJson = JsonConvert.SerializeObject(memberLoginCountList);

                    }
                    //Member login_cnt
                    strSQL = "UPDATE Member SET login_cnt=@login_cnt, last_login_date=GETDATE() WHERE customer_id=@customer_id AND member_account=@member_account";
                    parameters = new();
                    parameters.Add("customer_id", d.customer_id);
                    parameters.Add("member_account", member_account);
                    parameters.Add("login_cnt", loginCntJson);
                    await connection.ExecuteAsync(strSQL, parameters);
                    //重新更新結果
                    d.login_cnt = loginCntJson;
                }

            }

            IEnumerable<dynamic> result = data.Select(x => new
            {
                x.id,
                x.customer_id,
                x.member_account,
                x.nick_name,
                x.real_name,
                x.start_date,
                x.end_date,
                x.curtor,
                x.group_id,
                x.file_name,
                x.birthday,
                x.sex,
                x.last_login_date,
                x.is_pay,
                levels = string.IsNullOrEmpty(x.levels) ? "" : x.levels,
                levels_step = string.IsNullOrEmpty(x.levels_step) ? "" : x.levels_step,
                x.words_collect,
                x.sentences_collect,
                x.bookes_collect,
                x.musicbox_collect,
                x.login_cnt,
                x.create_date,
                x.modified_date               

            });

            return result;

        }

        /// <summary>
        /// 登入次數記錄
        /// </summary>
        /// <param name="customer_id"></param>
        /// <param name="member_account"></param>
        /// <param name="group_id"></param>
        /// <returns></returns>
        public async Task LoginCountAdd(int customer_id, string member_account, int? group_id)
        {
            DapperBase db = new(_connectionConfig.FundayB2B);
            
            using (var connection = db.OpenConnection())
            {
                DynamicParameters parameters = new();
                string strSQL;
                int rec;
                //Login_Count
                strSQL = "UPDATE Login_Count SET login_cnt = login_cnt + 1 ";
                strSQL += "WHERE customer_id=@customer_id AND year_month=@yyyyMM";
                parameters.Add("customer_id", customer_id);                
                parameters.Add("yyyyMM", DateTime.Now.ToString("yyyyMM"));
                if (group_id != null)
                {
                    strSQL += " AND group_id=@group_id";
                    parameters.Add("group_id", group_id);
                }
                rec = await connection.ExecuteAsync(strSQL, parameters);
                if (rec == 0)
                {
                    strSQL = "INSERT INTO Login_Count (year_month, login_cnt, customer_id, group_id) ";
                    strSQL += "VALUES(@yyyyMM, 1, @customer_id, @group_id)";
                    parameters = new();
                    parameters.Add("customer_id", customer_id);
                    parameters.Add("group_id", group_id);
                    parameters.Add("yyyyMM", DateTime.Now.ToString("yyyyMM"));
                    await connection.ExecuteAsync(strSQL, parameters);
                }

                //Login_Log
                strSQL = "UPDATE Login_Log SET login_cnt = login_cnt + 1 ";
                strSQL += "WHERE customer_id=@customer_id AND year=@yyyy AND login_hour=@hour";
                parameters = new();
                parameters.Add("customer_id", customer_id);                
                parameters.Add("yyyy", DateTime.Now.ToString("yyyy"));
                parameters.Add("hour", DateTime.Now.Hour);
                if (group_id != null)
                {
                    strSQL += " AND group_id=@group_id";
                    parameters.Add("group_id", group_id);
                }
                rec = await connection.ExecuteAsync(strSQL, parameters);
                if (rec == 0)
                {
                    strSQL = "INSERT INTO Login_Log (year, login_hour, login_cnt, customer_id, group_id) ";
                    strSQL += "VALUES(@yyyy, @hour, 1, @customer_id, @group_id)";
                    parameters = new();
                    parameters.Add("customer_id", customer_id);
                    parameters.Add("group_id", group_id);
                    parameters.Add("yyyy", DateTime.Now.ToString("yyyy"));
                    parameters.Add("hour", DateTime.Now.Hour);
                    await connection.ExecuteAsync(strSQL, parameters);
                }

            }
        }
    }
}
