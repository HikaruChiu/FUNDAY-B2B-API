using Dapper;
using Funday.Presale.API.Infrastructure.Util;
using Funday.Presale.API.Models;
using Funday.Presale.API.Repository;
using Funday.Presale.API.Service.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Funday.Presale.API.Service
{
    public class ArticleService: IArticle
    {
        private readonly ILogger<IArticle> _logger;
        private readonly ConnectionConfig _connectionConfig;

        public string yyyymm = DateTime.Now.ToString("yyyyMM");
        public string origJson = "";
        public string classly = "";
        public int? group_id = null;

        public ArticleService(ILogger<IArticle> logger, ConnectionConfig connectionConfig)
        {
            _logger = logger;
            _connectionConfig = connectionConfig;
        }

        /// <summary>
        /// 取得文章
        /// </summary>
        /// <param name="articleId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 更新文章錄音紀錄
        /// </summary>
        /// <param name="customer_id"></param>
        /// <param name="member_id"></param>
        /// <param name="fromRecordingJson"></param>
        /// <returns></returns>
        public async Task<dynamic> UpdateMemberRecording(int customer_id, int member_id, string fromRecordingJson)
        {
            DapperBase db = new(_connectionConfig.FundayB2B);
            using var connection = db.OpenConnection();
            DynamicParameters parameters = new();
            
            RecordingRecord fromRecordingRecord = new();
            RecordingRecord filterRecordingRecord = new();
            Recording filterRecording = new();

            List<RecordingRecord> recordingRecordsList = new();

            List<Recording> fromRecordingList = new();
            List<Recording> filterRecordingList = new();

            fromRecordingJson = fromRecordingJson.TrimStart(new char[] { '[' }).TrimEnd(new char[] { ']' });

            string strSQL;

            strSQL = "SELECT * FROM Member_Record WITH (NOLOCK) ";
            strSQL += "WHERE 1 = 1 ";
            strSQL += " AND customer_id=@customer_id ";
            strSQL += " AND member_id=@member_id ";
            parameters.Add("customer_id", customer_id);
            parameters.Add("member_id", member_id);
            var recordInfo = await connection.QuerySingleOrDefaultAsync(strSQL, parameters);
            if (recordInfo != null)
            {
                //如果有紀錄，就更新Json
                //更新比較麻煩，要把現有的JSON取出來轉換為物件
                //再去比對前端丟過來的文章錄音紀錄是否有存在再去做修改或新增
                
                fromRecordingJson = JObject.Parse(fromRecordingJson).ToString();
                fromRecordingRecord = JsonConvert.DeserializeObject<RecordingRecord>(fromRecordingJson);

                //origJson = JObject.Parse(recordInfo.recording_record).ToString();
                origJson = recordInfo.recording_record;
                

                if (string.IsNullOrEmpty(origJson)){
                    //如果是沒有錄音紀錄欄位是空白就直接塞
                    origJson = origJson.TrimStart(new char[] { '[' }).TrimEnd(new char[] { ']' });
                    origJson = JsonConvert.SerializeObject(fromRecordingRecord);
                }
                else
                {
                    //如果錄音紀錄欄位有資料，就要比對文章存不存在
                    //如果有存在就是修改，否則新增
                    //origJson = $@"[{origJson}]";
                    recordingRecordsList = JsonConvert.DeserializeObject<List<RecordingRecord>>(origJson);
                    fromRecordingList = fromRecordingRecord.recording;

                    filterRecordingRecord = recordingRecordsList.Find(x => x.articleId == fromRecordingRecord.articleId);
                    if (filterRecordingRecord == null)
                    {
                        filterRecordingRecord = new();
                        //新增
                        foreach (var rec in fromRecordingList)
                        {
                            filterRecordingList.Add(new Recording { filename = rec.filename, row = rec.row });
                        }
                        filterRecordingRecord.articleId = fromRecordingRecord.articleId;
                        filterRecordingRecord.recording = filterRecordingList;
                        filterRecordingRecord.createdDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                        filterRecordingRecord.modifiedDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                        recordingRecordsList.Add(filterRecordingRecord);
                    }
                    else
                    {
                        //修改
                        //先刪除原本的一筆，再新增
                        filterRecordingRecord.recording.Clear();
                        foreach (var rec in fromRecordingList)
                        {
                            filterRecordingRecord.recording.Add(new Recording { filename = rec.filename, row = rec.row });
                        }
                        
                        filterRecordingRecord.modifiedDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                        
                    }

                    recordInfo.recording_record = recordingRecordsList;
                    origJson = JsonConvert.SerializeObject(recordInfo.recording_record);
                }

                //origJson = origJson.TrimStart(new char[] { '[' }).TrimEnd(new char[] { ']' });
                //origJson = $@"[{origJson}]";
                strSQL = "UPDATE Member_Record SET ";
                strSQL += "recording_record=@recording_record ";
                strSQL += "WHERE customer_id=@customer_id AND member_id=@member_id; ";
                strSQL += "SELECT [id], customer_id, member_id, recording_record FROM Member_Record WITH (NOLOCK) ";
                strSQL += "WHERE 1 = 1 ";
                strSQL += " AND customer_id=@customer_id ";
                strSQL += " AND member_id=@member_id ";
                parameters = new();
                parameters.Add("customer_id", customer_id);
                parameters.Add("member_id", member_id);
                parameters.Add("recording_record", origJson);
                recordInfo = await connection.QuerySingleOrDefaultAsync(strSQL, parameters);

            }
            else
            {
                //如果沒有紀錄，就新增一筆
                fromRecordingRecord = JsonConvert.DeserializeObject<RecordingRecord>(fromRecordingJson);
                origJson = JsonConvert.SerializeObject(fromRecordingRecord);
                origJson = $@"[{origJson}]";
                strSQL = "INSERT INTO Member_Record (customer_id, member_id, recording_record) ";
                strSQL += "VALUES(@customer_id, @member_id, @recording_record); ";
                strSQL += "SELECT [id], customer_id, member_id, recording_record FROM Member_Record WITH (NOLOCK) ";
                strSQL += "WHERE 1 = 1 ";
                strSQL += " AND customer_id=@customer_id ";
                strSQL += " AND member_id=@member_id ";
                parameters = new();
                parameters.Add("customer_id", customer_id);
                parameters.Add("member_id", member_id);
                parameters.Add("recording_record", origJson);
                recordInfo = await connection.QuerySingleOrDefaultAsync(strSQL, parameters);


            }

            return recordInfo;

        }

        /// <summary>
        /// 更新便利貼
        /// </summary>
        /// <param name="customer_id"></param>
        /// <param name="member_id"></param>
        /// <param name="articleId"></param>
        /// <param name="fromMemoboxJson"></param>
        /// <returns></returns>
        public async Task<dynamic> UpdateMemberMemobox(int customer_id, int member_id, int articleId, string fromMemoboxJson)
        {
            DapperBase db = new(_connectionConfig.FundayB2B);
            using var connection = db.OpenConnection();
            DynamicParameters parameters = new();

            Memobox fromMemobox = new();
            Memobox filterMemobox = new();
            Memo filterMemo = new();

            List<Memobox> MemoboxList = new();

            List<Memo> fromMemoList = new();
            List<Memo> filterMemoList = new();

            string strSQL;
            strSQL = "SELECT * FROM Member_Record WITH (NOLOCK) ";
            strSQL += "WHERE 1 = 1 ";
            strSQL += " AND customer_id=@customer_id ";
            strSQL += " AND member_id=@member_id ";
            parameters.Add("customer_id", customer_id);
            parameters.Add("member_id", member_id);
            var memoboxInfo = await connection.QuerySingleOrDefaultAsync(strSQL, parameters);
            if (memoboxInfo != null)
            {
                //如果有紀錄，就更新Json
                //更新比較麻煩，要把現有的JSON取出來轉換為物件
                //再去比對前端丟過來的文章的便利貼是否有存在再去做修改或新增
                fromMemoboxJson = JArray.Parse(fromMemoboxJson).ToString();
                fromMemoList = JsonConvert.DeserializeObject<List<Memo>>(fromMemoboxJson);

                origJson = memoboxInfo.memobox_record;

                if (string.IsNullOrEmpty(origJson))
                {
                    MemoboxList.Add(new Memobox
                    {
                        articleId = articleId,
                        memo = fromMemoList == null ? new() : fromMemoList,
                        createdDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                        modifiedDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")
                    });

                    origJson = JsonConvert.SerializeObject(MemoboxList);
                }
                else
                {
                    //如果錄音紀錄欄位有資料，就要比對文章存不存在
                    //如果有存在就是修改，否則新增
                                        
                    MemoboxList = JsonConvert.DeserializeObject<List<Memobox>>(memoboxInfo.memobox_record);
                    filterMemobox = MemoboxList.Find(x => x.articleId == articleId);
                    if (filterMemobox == null)
                    {
                        filterMemobox = new();
                        filterMemobox.articleId = articleId;
                        filterMemobox.memo = fromMemoList;
                        filterMemobox.createdDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                        filterMemobox.modifiedDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                        MemoboxList.Add(filterMemobox);
                    }
                    else
                    {
                        filterMemobox.memo.Clear();                        
                        filterMemobox.memo = fromMemoList;
                        filterMemobox.modifiedDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                        
                    }

                    origJson = JsonConvert.SerializeObject(MemoboxList);

                }

                strSQL = "UPDATE Member_Record SET ";
                strSQL += "memobox_record=@memobox_record ";
                strSQL += "WHERE customer_id=@customer_id AND member_id=@member_id; ";
                strSQL += "SELECT [id], customer_id, member_id, memobox_record FROM Member_Record WITH (NOLOCK) ";
                strSQL += "WHERE 1 = 1 ";
                strSQL += " AND customer_id=@customer_id ";
                strSQL += " AND member_id=@member_id ";
                parameters = new();
                parameters.Add("customer_id", customer_id);
                parameters.Add("member_id", member_id);
                parameters.Add("memobox_record", origJson);
                memoboxInfo = await connection.QuerySingleOrDefaultAsync(strSQL, parameters);


            }
            else
            {
                //如果沒有紀錄，就新增一筆
                fromMemoboxJson = JArray.Parse(fromMemoboxJson).ToString();
                fromMemoList = JsonConvert.DeserializeObject<List<Memo>>(fromMemoboxJson);

                MemoboxList.Add(new Memobox
                {
                    articleId = articleId,
                    memo = fromMemoList == null ? new() : fromMemoList,
                    createdDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                    modifiedDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")
                });

                origJson = JsonConvert.SerializeObject(MemoboxList);

                strSQL = "INSERT INTO Member_Record (customer_id, member_id, memobox_record) ";
                strSQL += "VALUES(@customer_id, @member_id, @memobox_record); ";
                strSQL += "SELECT [id], customer_id, member_id, memobox_record FROM Member_Record WITH (NOLOCK) ";
                strSQL += "WHERE 1 = 1 ";
                strSQL += " AND customer_id=@customer_id ";
                strSQL += " AND member_id=@member_id ";
                parameters = new();
                parameters.Add("customer_id", customer_id);
                parameters.Add("member_id", member_id);
                parameters.Add("memobox_record", origJson);
                memoboxInfo = await connection.QuerySingleOrDefaultAsync(strSQL, parameters);


            }

            return memoboxInfo;
        }

        /// <summary>
        /// 更新佳句收錄
        /// </summary>
        /// <param name="customer_id"></param>
        /// <param name="member_id"></param>
        /// <param name="articleId"></param>
        /// <param name="xmlFileName"></param>
        /// <param name="ch_Sentences"></param>
        /// <param name="en_Sentences"></param>
        /// <param name="clock"></param>
        /// <param name="note"></param>
        /// <param name="orders"></param>
        /// <returns></returns>
        public async Task<dynamic> UpdateSentencesCollect(int customer_id, int member_id, int articleId, string xmlFileName, string ch_Sentences, string en_Sentences, string clock, string note, int orders)
        {
            return null;
        }


        /// <summary>
        /// 更新會員單字收錄
        /// </summary>
        /// <param name="customer_id"></param>
        /// <param name="member_id"></param>
        /// <param name="articleId"></param>
        /// <param name="enWord"></param>
        /// <param name="chWord"></param>        
        /// <returns></returns>
        public async Task<dynamic> UpdateWordsCollect(int customer_id, int member_id, int articleId, string enWord, string chWord)
        {
            DapperBase db = new(_connectionConfig.FundayB2B);
            using var connection = db.OpenConnection();
            DynamicParameters parameters = new();

            WordsCollect wordsCollect = new();
            WordsCollect filterWordsCollect = new();
            
            List<WordsCollect> wordsCollectList = new();
            Words words = new();
            
            string origJson;
            string strSQL;
            int orders = 0;

            strSQL = "SELECT words_collect FROM Member WITH (NOLOCK) ";
            strSQL += "WHERE customer_id=@customer_id AND [id]=@member_id";
            parameters.Add("customer_id", customer_id);
            parameters.Add("member_id", member_id);

            var result = await connection.QueryFirstOrDefaultAsync<string>(strSQL, parameters);
            if (!string.IsNullOrEmpty(result))
            {
                wordsCollectList = JsonConvert.DeserializeObject<List<WordsCollect>>(result);
            }
            

            if (!wordsCollectList.Any())
            {
                //無收錄過單字，直接新增一筆JSON
                wordsCollect.articleId = articleId;
                wordsCollect.words = new List<Words> { new Words { enWord = enWord, chWord = chWord, orders = 0 } };
                wordsCollectList.Add(wordsCollect);
                origJson = JsonConvert.SerializeObject(wordsCollectList);
                strSQL = "UPDATE Member SET words_collect=@words_collect ";
                strSQL += "WHERE customer_id=@customer_id AND [id]=@member_id";
                parameters = new();
                parameters.Add("customer_id", customer_id);
                parameters.Add("member_id", member_id);
                parameters.Add("words_collect", origJson);
                await connection.ExecuteAsync(strSQL, parameters);
                
                return wordsCollectList;
            }
            else
            {
               
                //有收錄過單字，要判斷文章ID來更新
                filterWordsCollect = wordsCollectList.Find(x => x.articleId == articleId);
                if (filterWordsCollect == null)
                {
                    filterWordsCollect = new();
                    filterWordsCollect.articleId = articleId;
                    filterWordsCollect.words = (new List<Words> { new Words { enWord = enWord, chWord = chWord, orders = 0 } });
                    wordsCollectList.Add(filterWordsCollect);

                    origJson = JsonConvert.SerializeObject(wordsCollectList);
                    strSQL = "UPDATE Member SET words_collect=@words_collect ";
                    strSQL += "WHERE customer_id=@customer_id AND [id]=@member_id";
                    parameters = new();
                    parameters.Add("customer_id", customer_id);
                    parameters.Add("member_id", member_id);
                    parameters.Add("words_collect", origJson);
                    await connection.ExecuteAsync(strSQL, parameters);
                    
                    return filterWordsCollect;
                }
                else
                {
                    //增加此諞文章的單字
                    //取得目前該篇文章的orders
                    words = filterWordsCollect.words.MaxBy(x => x.orders);
                    if (words == null) { orders = 0; }
                    else { orders = words.orders + 1; } //預設單字排序累加
                    filterWordsCollect.words.Add(new Words { enWord = enWord, chWord = chWord, orders = orders});
                    
                    origJson = JsonConvert.SerializeObject(wordsCollectList);
                    strSQL = "UPDATE Member SET words_collect=@words_collect ";
                    strSQL += "WHERE customer_id=@customer_id AND [id]=@member_id";
                    parameters = new();
                    parameters.Add("customer_id", customer_id);
                    parameters.Add("member_id", member_id);
                    parameters.Add("words_collect", origJson);
                    await connection.ExecuteAsync(strSQL, parameters);

                    return filterWordsCollect;
                }
            }

        }
    }
}
