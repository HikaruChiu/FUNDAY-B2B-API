using Funday.Presale.API.Repository;
using Funday.Presale.API.Service.Interface;
using System.Data;
using Dapper;
using Funday.Presale.API.Models;
using Funday.Presale.API.Infrastructure.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Funday.Presale.API.Models.ViewModel;

namespace Funday.Presale.API.Service
{
    public class MemberService: IMember
    {
        private readonly ILogger<IMember> _logger;
        private readonly ConnectionConfig _connectionConfig;

        public string yyyymm = DateTime.Now.ToString("yyyyMM");
        public string origJson = "";
        public string classly = "";
        public int? group_id = null;

        public MemberService(ILogger<IMember> logger, ConnectionConfig connectionConfig)
        {
            _logger = logger;
            _connectionConfig = connectionConfig;        
        }

        public async Task<IEnumerable<dynamic>> GetMember(int customer_id, int member_id)
        {
            DapperBase db = new(_connectionConfig.FundayB2B);
            string strSQL = "SELECT Member.* ";
            strSQL += "FROM Member WITH (NOLOCK) ";
            strSQL += "WHERE 1=1 ";
            strSQL += " AND customer_id=@customer_id ";
            strSQL += " AND [id]=@member_id ";

            using var connection = db.OpenConnection();
            DynamicParameters parameters = new();
            parameters.Add("customer_id", customer_id);
            parameters.Add("member_id", member_id);

            var data = await connection.QueryAsync<dynamic>(strSQL, parameters);
            return data;

        }


        #region 會員學習紀錄更新

        /// <summary>
        /// 文章學習紀錄更新
        /// </summary>
        /// <param name="customer_id"></param>
        /// <param name="member_id"></param>
        /// <param name="articleId"></param>
        /// <param name="readType"></param>
        /// <param name="recordingFlag"></param>
        /// <param name="memoboxFlag"></param>
        /// <param name="testFlag"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<dynamic>, string>> UpdateArticleLearningRecord(int customer_id, int member_id, int articleId, int readType)
        {
            readType = readType == 0 ? 1 : readType;

            DapperBase db = new(_connectionConfig.FundayB2B);
            DapperBase db2 = new(_connectionConfig.Funday);
            CustomerUtil customerUtil = new(_connectionConfig);

            LearningRecord learningRecord = new();

            List<Article> articleList = new();
            Article objArticle = new();

            //using var connection = db.OpenConnection();
            DynamicParameters parameters = new();

            //首先判斷有沒有這個會員
            var mb = await GetMember(customer_id, member_id);
            if (!mb.Any())
            {
                //沒有這個會員
                return new Tuple<IEnumerable<dynamic>, string>(null, "找不到此會員");
            }
            foreach (var g in mb)
            {
                group_id = g.group_id;
            }

            //取得是否有這筆文章資料
            MemberUtil memberUtil = new(_connectionConfig);
            var newsInfo = await memberUtil.GetArticle(articleId);
            if (newsInfo == null)
            {
                //沒有這個文章
                return new Tuple<IEnumerable<dynamic>, string>(null, "找不到此文章");
            }


            //初始化一筆本月紀錄，如果已經存在就忽略
            var learningTimeData = await InitLearningTime(customer_id, member_id, DateTime.Now.Year, DateTime.Now.Month);
            if (learningTimeData.Any())
            {

                foreach (var ltrec in learningTimeData)
                {
                    //檢查learning_record如果是空的就新增json
                    if (string.IsNullOrWhiteSpace(ltrec.learning_record))
                    {
                        learningRecord.article = new List<Article>
                        {
                            new Article
                            {
                                articleId = newsInfo.indx,
                                chTitle = newsInfo.ch_subject,
                                enTitle = newsInfo.en_subject,
                                articleLevel = newsInfo.score,
                                readMinutesCnt = readType == 1 ? 1 : 0,
                                readTechingCnt = readType == 2 ? 1 : 0,
                                createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                modifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                
                            }
                        };

                        //更新學習紀錄到 learning_record 欄位
                        origJson = JsonConvert.SerializeObject(learningRecord);
                        await UpdateLearningRecord(ltrec.customer_id, ltrec.member_id, ltrec.year, ltrec.month, origJson, 3);

                        //更新客戶閱讀統計
                        classly = readType == 2 ? "teaching" : typeof(Article).Name.ToLower();
                        await UpdateCustomerRead(ltrec.customer_id, group_id, yyyymm, classly, 3);


                        return new Tuple<IEnumerable<dynamic>, string>(null, "OK");

                    }
                    else
                    {
                        //如果非空值，則取得是否有存在該筆文章，否則Append一筆Json
                        int readCountType;
                        learningRecord = JsonConvert.DeserializeObject<LearningRecord>(ltrec.learning_record);
                        articleList = learningRecord.article;
                        if (articleList != null)
                        {
                            objArticle = learningRecord.article.Find(x => x.articleId == articleId);
                            if (objArticle != null)
                            {
                                //更新次數
                                readCountType = 2;
                                //這邏輯是為了記錄文章裡的閱讀老師講解篇數
                                if (readType == 2 && objArticle.readTechingCnt == 0)
                                {
                                    readCountType = 3;
                                }

                                if (readType == 1) { objArticle.readMinutesCnt++; }
                                if (readType == 2) { objArticle.readTechingCnt++; }
                                objArticle.modifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            else
                            {

                                //append 一筆新的
                                readCountType = 3;
                                learningRecord.article.Add(new Article
                                {
                                    articleId = newsInfo.indx,
                                    chTitle = newsInfo.ch_subject,
                                    enTitle = newsInfo.en_subject,
                                    articleLevel = newsInfo.score,
                                    readMinutesCnt = readType == 1 ? 1 : 0,
                                    readTechingCnt = readType == 2 ? 1 : 0,
                                    createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                    modifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                });

                            }
                        }
                        else
                        {
                            //append 一筆新的
                            readCountType = 3;
                            learningRecord.article = new();
                            learningRecord.article.Add(new Article
                            {
                                articleId = newsInfo.indx,
                                chTitle = newsInfo.ch_subject,
                                enTitle = newsInfo.en_subject,
                                articleLevel = newsInfo.score,
                                readMinutesCnt = readType == 1 ? 1 : 0,
                                readTechingCnt = readType == 2 ? 1 : 0,
                                createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                modifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            });
                        }

                        //更新到 learning_record 欄位
                        origJson = JsonConvert.SerializeObject(learningRecord);
                        await UpdateLearningRecord(ltrec.customer_id, ltrec.member_id, ltrec.year, ltrec.month, origJson, readCountType);

                        //更新客戶閱讀統計
                        classly = readType == 2 ? "teaching" : typeof(Article).Name.ToLower();
                        await UpdateCustomerRead(ltrec.customer_id, group_id, yyyymm, classly, readCountType);

                        return new Tuple<IEnumerable<dynamic>, string>(null, "OK");
                    }
                }

            }

            //如果程式跑到這裡，表示沒有更新成功
            return new Tuple<IEnumerable<dynamic>, string>(null, "更新失敗");

        }

        /// <summary>
        /// 專欄學習紀錄更新
        /// </summary>
        /// <param name="customer_id"></param>
        /// <param name="member_id"></param>
        /// <param name="columnsId"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<dynamic>, string>> UpdateColumnsLearningRecord(int customer_id, int member_id, int columnsId)
        {
            DapperBase db = new(_connectionConfig.FundayB2B);
            DapperBase db2 = new(_connectionConfig.Funday);
            LearningRecord learningRecord = new();
            List<Columns> columnsList = new();
            Columns objColumns = new();

            using var connection = db.OpenConnection();
            DynamicParameters parameters = new();

            //首先判斷有沒有這個會員
            var mb = await GetMember(customer_id, member_id);
            if (!mb.Any())
            {
                //沒有這個會員
                return new Tuple<IEnumerable<dynamic>, string>(null, "找不到此會員");
            }

            //取得是否有這筆專欄資料
            MemberUtil memberUtil = new(_connectionConfig);
            var columnsInfo = await memberUtil.GetColumns(columnsId);
            if (columnsInfo == null)
            {
                //沒有這個專欄
                return new Tuple<IEnumerable<dynamic>, string>(null, "找不到此專欄");
            }

            //初始化一筆本月紀錄，如果已經存在就忽略
            var learningTimeData = await InitLearningTime(customer_id, member_id, DateTime.Now.Year, DateTime.Now.Month);
            if (learningTimeData.Any())
            {
                foreach (var ltrec in learningTimeData)
                {
                    //檢查learning_record如果是空的就新增json
                    if (string.IsNullOrWhiteSpace(ltrec.learning_record))
                    {
                        learningRecord.columns = new List<Columns>
                        {
                            new Columns
                            {
                                columnsId = columnsInfo.indx,
                                chTitle = columnsInfo.ch_subject,
                                readMinutesCnt = 1,
                                createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                modifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                            }
                        };

                        //更新到 learning_record 欄位
                        origJson = JsonConvert.SerializeObject(learningRecord);

                        await UpdateLearningRecord(ltrec.customer_id, ltrec.member_id, ltrec.year, ltrec.month, origJson, 3);
                        return new Tuple<IEnumerable<dynamic>, string>(null, "OK");

                    }
                    else
                    {
                        //如果非空值，則取得是否有存在該筆文章，否則Append一筆Json
                        int readCountType;
                        learningRecord = JsonConvert.DeserializeObject<LearningRecord>(ltrec.learning_record);
                        columnsList = learningRecord.columns;
                        if (columnsList != null)
                        {
                            objColumns = learningRecord.columns.Find(x => x.columnsId == columnsId);
                            if (objColumns != null)
                            {
                                //更新次數
                                readCountType = 2;
                                objColumns.readMinutesCnt++;
                                objColumns.modifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            else
                            {
                                //append 一筆新的
                                readCountType = 3;
                                learningRecord.columns.Add(new Columns
                                {
                                    columnsId = columnsInfo.indx,
                                    chTitle = columnsInfo.ch_subject,
                                    readMinutesCnt = 1,
                                    createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                    modifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                });

                            }
                        }
                        else
                        {
                            //append 一筆新的
                            readCountType = 3;
                            learningRecord.columns = new();
                            learningRecord.columns.Add(new Columns
                            {
                                columnsId = columnsInfo.indx,
                                chTitle = columnsInfo.ch_subject,
                                readMinutesCnt = 1,
                                createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                modifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                            });
                        }


                        //更新到 learning_record 欄位
                        origJson = JsonConvert.SerializeObject(learningRecord);
                        await UpdateLearningRecord(ltrec.customer_id, ltrec.member_id, ltrec.year, ltrec.month, origJson, readCountType);

                        //更新客戶閱讀統計
                        classly = typeof(Columns).Name.ToLower();
                        await UpdateCustomerRead(ltrec.customer_id, group_id, yyyymm, classly, readCountType);

                        return new Tuple<IEnumerable<dynamic>, string>(null, "OK");
                    }
                }
            }

            //如果程式跑到這裡，表示沒有更新成功
            return new Tuple<IEnumerable<dynamic>, string>(null, "更新失敗");

        }

        /// <summary>
        /// 童話故事學習紀錄更新
        /// </summary>
        /// <param name="customer_id"></param>
        /// <param name="member_id"></param>
        /// <param name="storyId"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<dynamic>, string>> UpdateStoryLearningRecord(int customer_id, int member_id, int storyId)
        {
            DapperBase db = new(_connectionConfig.FundayB2B);
            DapperBase db2 = new(_connectionConfig.Funday);
            LearningRecord learningRecord = new();
            List<Story> storyList = new();
            Story objStory = new();

            using var connection = db.OpenConnection();
            DynamicParameters parameters = new();

            //首先判斷有沒有這個會員
            var mb = await GetMember(customer_id, member_id);
            if (!mb.Any())
            {
                //沒有這個會員
                return new Tuple<IEnumerable<dynamic>, string>(null, "找不到此會員");
            }

            //取得是否有這筆專欄資料
            MemberUtil memberUtil = new(_connectionConfig);
            var storyInfo = await memberUtil.GetStory(storyId);
            if (storyInfo == null)
            {
                //沒有這個專欄
                return new Tuple<IEnumerable<dynamic>, string>(null, "找不到這筆童話故事");
            }

            //初始化一筆本月紀錄，如果已經存在就忽略
            var learningTimeData = await InitLearningTime(customer_id, member_id, DateTime.Now.Year, DateTime.Now.Month);
            if (learningTimeData.Any())
            {
                foreach (var ltrec in learningTimeData)
                {
                    //檢查learning_record如果是空的就新增json
                    if (string.IsNullOrWhiteSpace(ltrec.learning_record))
                    {
                        learningRecord.story = new List<Story>
                        {
                            new Story
                            {
                                storyId = storyInfo.indx,
                                chTitle = storyInfo.cTitle,
                                readMinutesCnt = 1,
                                createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                modifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                            }
                        };

                        //更新到 learning_record 欄位
                        origJson = JsonConvert.SerializeObject(learningRecord);

                        await UpdateLearningRecord(ltrec.customer_id, ltrec.member_id, ltrec.year, ltrec.month, origJson, 3);
                        return new Tuple<IEnumerable<dynamic>, string>(null, "OK");

                    }
                    else
                    {
                        //如果非空值，則取得是否有存在該筆文章，否則Append一筆Json
                        int readCountType;
                        learningRecord = JsonConvert.DeserializeObject<LearningRecord>(ltrec.learning_record);
                        storyList = learningRecord.story;
                        if (storyList != null)
                        {
                            objStory = learningRecord.story.Find(x => x.storyId == storyId);
                            if (objStory != null)
                            {
                                //更新次數
                                readCountType = 2;
                                objStory.readMinutesCnt++;
                                objStory.modifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            else
                            {
                                //append 一筆新的
                                readCountType = 3;
                                learningRecord.story.Add(new Story
                                {
                                    storyId = storyInfo.indx,
                                    chTitle = storyInfo.cTitle,
                                    readMinutesCnt = 1,
                                    createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                    modifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                });

                            }
                        }
                        else
                        {
                            //append 一筆新的
                            readCountType = 3;
                            learningRecord.story = new();
                            learningRecord.story.Add(new Story
                            {
                                storyId = storyInfo.indx,
                                chTitle = storyInfo.cTitle,
                                readMinutesCnt = 1,
                                createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                modifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                            });
                        }


                        //更新到 learning_record 欄位
                        origJson = JsonConvert.SerializeObject(learningRecord);
                        await UpdateLearningRecord(ltrec.customer_id, ltrec.member_id, ltrec.year, ltrec.month, origJson, readCountType);

                        //更新客戶閱讀統計
                        classly = typeof(Story).Name.ToLower();
                        await UpdateCustomerRead(ltrec.customer_id, group_id, yyyymm, classly, readCountType);

                        return new Tuple<IEnumerable<dynamic>, string>(null, "OK");
                    }
                }
            }

            //如果程式跑到這裡，表示沒有更新成功
            return new Tuple<IEnumerable<dynamic>, string>(null, "更新失敗");

        }

        /// <summary>
        /// 影音學習紀錄更新
        /// </summary>
        /// <param name="customer_id"></param>
        /// <param name="member_id"></param>
        /// <param name="videoId"></param>
        /// <param name="recordingFlag"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<dynamic>, string>> UpdateVideoLearningRecord(int customer_id, int member_id, int videoId)
        {
            DapperBase db = new(_connectionConfig.FundayB2B);
            DapperBase db2 = new(_connectionConfig.Funday);
            LearningRecord learningRecord = new();
            List<Video> videoList = new();
            Video objVideo = new();

            using var connection = db.OpenConnection();
            DynamicParameters parameters = new();

            //首先判斷有沒有這個會員
            var mb = await GetMember(customer_id, member_id);
            if (!mb.Any())
            {
                //沒有這個會員
                return new Tuple<IEnumerable<dynamic>, string>(null, "找不到此會員");
            }

            //取得是否有這筆專欄資料
            MemberUtil memberUtil = new(_connectionConfig);
            var videoInfo = await memberUtil.GetVideo(videoId);
            if (videoInfo == null)
            {
                //沒有這個專欄
                return new Tuple<IEnumerable<dynamic>, string>(null, "找不到此影片");
            }

            //初始化一筆本月紀錄，如果已經存在就忽略
            var learningTimeData = await InitLearningTime(customer_id, member_id, DateTime.Now.Year, DateTime.Now.Month);
            if (learningTimeData.Any())
            {
                foreach (var ltrec in learningTimeData)
                {
                    //檢查learning_record如果是空的就新增json
                    if (string.IsNullOrWhiteSpace(ltrec.learning_record))
                    {
                        learningRecord.video = new List<Video>
                        {
                            new Video
                            {
                                videoId = videoInfo.indx,
                                title = videoInfo.Title,
                                readMinutesCnt = 1,
                                createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                modifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                            }
                        };

                        //更新到 learning_record 欄位
                        origJson = JsonConvert.SerializeObject(learningRecord);

                        await UpdateLearningRecord(ltrec.customer_id, ltrec.member_id, ltrec.year, ltrec.month, origJson, 3);
                        return new Tuple<IEnumerable<dynamic>, string>(null, "OK");

                    }
                    else
                    {
                        //如果非空值，則取得是否有存在該筆文章，否則Append一筆Json
                        int readCountType;
                        learningRecord = JsonConvert.DeserializeObject<LearningRecord>(ltrec.learning_record);
                        videoList = learningRecord.video;
                        if (videoList != null)
                        {
                            objVideo = learningRecord.video.Find(x => x.videoId == videoId);
                            if (objVideo != null)
                            {
                                //更新次數
                                readCountType = 2;
                                objVideo.readMinutesCnt++;
                                objVideo.modifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            else
                            {
                                //append 一筆新的
                                readCountType = 3;
                                learningRecord.video.Add(new Video
                                {
                                    videoId = videoInfo.indx,
                                    title = videoInfo.Title,
                                    readMinutesCnt = 1,
                                    createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                    modifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                });

                            }
                        }
                        else
                        {
                            //append 一筆新的
                            readCountType = 3;
                            learningRecord.video = new();
                            learningRecord.video.Add(new Video
                            {
                                videoId = videoInfo.indx,
                                title = videoInfo.Title,
                                readMinutesCnt = 1,
                                createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                modifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                            });
                        }


                        //更新到 learning_record 欄位
                        origJson = JsonConvert.SerializeObject(learningRecord);
                        await UpdateLearningRecord(ltrec.customer_id, ltrec.member_id, ltrec.year, ltrec.month, origJson, readCountType);

                        //更新客戶閱讀統計
                        classly = typeof(Video).Name.ToLower();
                        await UpdateCustomerRead(ltrec.customer_id, group_id, yyyymm, classly, readCountType);

                        return new Tuple<IEnumerable<dynamic>, string>(null, "OK");
                    }
                }
            }

            //如果程式跑到這裡，表示沒有更新成功
            return new Tuple<IEnumerable<dynamic>, string>(null, "更新失敗");
        }

        /// <summary>
        /// 音樂盒學習紀錄更新
        /// </summary>
        /// <param name="customer_id"></param>
        /// <param name="member_id"></param>
        /// <param name="musicboxId"></param>
        /// <param name="singFlag"></param>
        /// <param name="singFileName"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<dynamic>, string>> UpdateMusicBoxLearningRecord(int customer_id, int member_id, int musicboxId)
        {
            DapperBase db = new(_connectionConfig.FundayB2B);
            DapperBase db2 = new(_connectionConfig.Funday);
            LearningRecord learningRecord = new();
            List<MusicBox> musicboxList = new();
            MusicBox objMusicBox = new();

            using var connection = db.OpenConnection();
            DynamicParameters parameters = new();

            //首先判斷有沒有這個會員
            var mb = await GetMember(customer_id, member_id);
            if (!mb.Any())
            {
                //沒有這個會員
                return new Tuple<IEnumerable<dynamic>, string>(null, "找不到此會員");
            }

            //取得是否有這筆專欄資料
            MemberUtil memberUtil = new(_connectionConfig);
            var musicboxInfo = await memberUtil.GetMusicBox(musicboxId);
            if (musicboxInfo == null)
            {
                //沒有這個專欄
                return new Tuple<IEnumerable<dynamic>, string>(null, "找不到此音樂盒");
            }

            //初始化一筆本月紀錄，如果已經存在就忽略
            var learningTimeData = await InitLearningTime(customer_id, member_id, DateTime.Now.Year, DateTime.Now.Month);
            if (learningTimeData.Any())
            {
                foreach (var ltrec in learningTimeData)
                {
                    //檢查learning_record如果是空的就新增json
                    if (string.IsNullOrWhiteSpace(ltrec.learning_record))
                    {
                        learningRecord.musicbox = new List<MusicBox>
                        {
                            new MusicBox
                            {
                                musicboxId = musicboxInfo.indx,
                                title = musicboxInfo.Title,
                                readMinutesCnt = 1,
                                createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                modifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                            }
                        };

                        //更新到 learning_record 欄位
                        origJson = JsonConvert.SerializeObject(learningRecord);

                        await UpdateLearningRecord(ltrec.customer_id, ltrec.member_id, ltrec.year, ltrec.month, origJson, 3);
                        return new Tuple<IEnumerable<dynamic>, string>(null, "OK");

                    }
                    else
                    {
                        //如果非空值，則取得是否有存在該筆文章，否則Append一筆Json
                        int readCountType;
                        learningRecord = JsonConvert.DeserializeObject<LearningRecord>(ltrec.learning_record);
                        musicboxList = learningRecord.musicbox;
                        if (musicboxList != null)
                        {
                            objMusicBox = learningRecord.musicbox.Find(x => x.musicboxId == musicboxId);
                            if (objMusicBox != null)
                            {
                                //更新次數
                                readCountType = 2;
                                objMusicBox.readMinutesCnt++;
                                objMusicBox.modifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            else
                            {
                                //append 一筆新的
                                readCountType = 3;
                                learningRecord.musicbox.Add(new MusicBox
                                {
                                    musicboxId = musicboxInfo.indx,
                                    title = musicboxInfo.Title,
                                    readMinutesCnt = 1,
                                    createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                    modifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                });

                            }
                        }
                        else
                        {
                            //append 一筆新的
                            readCountType = 3;
                            learningRecord.musicbox = new();
                            learningRecord.musicbox.Add(new MusicBox
                            {
                                musicboxId = musicboxInfo.indx,
                                title = musicboxInfo.Title,
                                readMinutesCnt = 1,
                                createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                modifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                            });
                        }


                        //更新到 learning_record 欄位
                        origJson = JsonConvert.SerializeObject(learningRecord);
                        await UpdateLearningRecord(ltrec.customer_id, ltrec.member_id, ltrec.year, ltrec.month, origJson, readCountType);

                        //更新客戶閱讀統計
                        classly = typeof(MusicBox).Name.ToLower();
                        await UpdateCustomerRead(ltrec.customer_id, group_id, yyyymm, classly, readCountType);

                        return new Tuple<IEnumerable<dynamic>, string>(null, "OK");
                    }
                }
            }

            //如果程式跑到這裡，表示沒有更新成功
            return new Tuple<IEnumerable<dynamic>, string>(null, "更新失敗");
        }

        /// <summary>
        /// 部落格學習紀錄更新
        /// </summary>
        /// <param name="customer_id"></param>
        /// <param name="member_id"></param>
        /// <param name="blogId"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<dynamic>, string>> UpdateBlogLearningRecord(int customer_id, int member_id, int blogId)
        {
            DapperBase db = new(_connectionConfig.FundayB2B);
            DapperBase db2 = new(_connectionConfig.Funday);
            LearningRecord learningRecord = new();
            List<Blog> blogList = new();
            Blog objBlog = new();

            using var connection = db.OpenConnection();
            DynamicParameters parameters = new();

            //首先判斷有沒有這個會員
            var mb = await GetMember(customer_id, member_id);
            if (!mb.Any())
            {
                //沒有這個會員
                return new Tuple<IEnumerable<dynamic>, string>(null, "找不到此會員");
            }

            //取得是否有這筆專欄資料
            MemberUtil memberUtil = new(_connectionConfig);
            var blogInfo = await memberUtil.GetBlog(blogId);
            if (blogInfo == null)
            {
                //沒有這個專欄
                return new Tuple<IEnumerable<dynamic>, string>(null, "找不到此部落格");
            }

            //初始化一筆本月紀錄，如果已經存在就忽略
            var learningTimeData = await InitLearningTime(customer_id, member_id, DateTime.Now.Year, DateTime.Now.Month);
            if (learningTimeData.Any())
            {
                foreach (var ltrec in learningTimeData)
                {
                    //檢查learning_record如果是空的就新增json
                    if (string.IsNullOrWhiteSpace(ltrec.learning_record))
                    {
                        learningRecord.blog = new List<Blog>
                        {
                            new Blog
                            {
                                blogId = blogInfo.indx,
                                title = blogInfo.title,
                                readMinutesCnt = 1,
                                createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                modifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                            }
                        };

                        //更新到 learning_record 欄位
                        origJson = JsonConvert.SerializeObject(learningRecord);

                        await UpdateLearningRecord(ltrec.customer_id, ltrec.member_id, ltrec.year, ltrec.month, origJson, 3);
                        return new Tuple<IEnumerable<dynamic>, string>(null, "OK");

                    }
                    else
                    {
                        //如果非空值，則取得是否有存在該筆文章，否則Append一筆Json
                        int readCountType;
                        learningRecord = JsonConvert.DeserializeObject<LearningRecord>(ltrec.learning_record);
                        blogList = learningRecord.blog;
                        if (blogList != null)
                        {
                            objBlog = learningRecord.blog.Find(x => x.blogId == blogId);
                            if (objBlog != null)
                            {
                                //更新次數
                                readCountType = 2;
                                objBlog.readMinutesCnt++;
                                objBlog.modifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            else
                            {
                                //append 一筆新的
                                readCountType = 3;
                                learningRecord.blog.Add(new Blog
                                {
                                    blogId = blogInfo.indx,
                                    title = blogInfo.title,
                                    readMinutesCnt = 1,
                                    createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                    modifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                });

                            }
                        }
                        else
                        {
                            //append 一筆新的
                            readCountType = 3;
                            learningRecord.blog = new();
                            learningRecord.blog.Add(new Blog
                            {
                                blogId = blogInfo.indx,
                                title = blogInfo.title,
                                readMinutesCnt = 1,
                                createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                modifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                            });
                        }


                        //更新到 learning_record 欄位
                        origJson = JsonConvert.SerializeObject(learningRecord);
                        await UpdateLearningRecord(ltrec.customer_id, ltrec.member_id, ltrec.year, ltrec.month, origJson, readCountType);

                        //更新客戶閱讀統計
                        classly = typeof(Blog).Name.ToLower();
                        await UpdateCustomerRead(ltrec.customer_id, group_id, yyyymm, classly, readCountType);

                        return new Tuple<IEnumerable<dynamic>, string>(null, "OK");
                    }
                }
            }


            //如果程式跑到這裡，表示沒有更新成功
            return new Tuple<IEnumerable<dynamic>, string>(null, "更新失敗");
        }



        /// <summary>
        /// 初始化一筆本月學習紀錄，如果已經存在就回傳資料
        /// </summary>
        /// <param name="customer_id"></param>
        /// <param name="member_id"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public async Task<IEnumerable<dynamic>> InitLearningTime(int customer_id, int member_id, int year, int month)
        {
            DapperBase db = new(_connectionConfig.FundayB2B);
            using var connection = db.OpenConnection();
            DynamicParameters parameters = new();
            string strSQL;

            //取得這個會員這個月是否有學習紀錄
            strSQL = "SELECT TOP 1 * FROM Learning_Time WITH (NOLOCK) ";
            strSQL += "WHERE customer_id=@customer_id AND member_id=@member_id ";
            strSQL += " AND [year]=@year AND [month]=@month ";
            parameters = new();
            parameters.Add("customer_id", customer_id);
            parameters.Add("member_id", member_id);
            parameters.Add("year", DateTime.Now.Year);
            parameters.Add("month", DateTime.Now.Month);
            var learningRec = await connection.QueryAsync(strSQL, parameters);
            if (!learningRec.Any())
            {
                //沒有學習紀錄，則新增一筆初始值
                strSQL = "INSERT INTO Learning_Time (customer_id, member_id, [year], [month]) ";
                strSQL += "VALUES(@customer_id, @member_id, @year, @month); ";
                strSQL += "SELECT TOP 1 * FROM Learning_Time WITH (NOLOCK) ";
                strSQL += "WHERE customer_id=@customer_id AND member_id=@member_id AND [year]=@year AND [month]=@month";
                return await connection.QueryAsync(strSQL, parameters);
            }
            else
            {
                return learningRec;
            }

        }

        /// <summary>
        /// 更新整份學習紀錄
        /// </summary>
        /// <param name="customer_id"></param>
        /// <param name="member_id"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="origJson"></param>
        /// <param name="readCountType">(1=更新閱讀篇數+1, 2=更新閱讀分鐘數+1, 3=兩個都各+1)</param>
        /// <returns></returns>
        public async Task<int> UpdateLearningRecord(int customer_id, int member_id, int year, int month, string origJson, int readCountType = 1)
        {
            DapperBase db = new(_connectionConfig.FundayB2B);
            using var connection = db.OpenConnection();
            DynamicParameters parameters = new();
            string strSQL;
            strSQL = "UPDATE Learning_Time SET learning_record=@learning_record ";
            if (readCountType == 1) //按照kay哥的邏輯，應該不會只更新閱讀篇數，但還是先保留條件
            {
                strSQL += ",reading_record_cnt=ISNULL(reading_record_cnt,0)+1 ";
            }
            else if (readCountType == 2)
            {
                strSQL += ",reading_minutes_cnt=ISNULL(reading_minutes_cnt,0)+1 ";
            }
            else if (readCountType == 3)
            {
                strSQL += ",reading_record_cnt=ISNULL(reading_record_cnt,0)+1, reading_minutes_cnt=ISNULL(reading_minutes_cnt,0)+1 ";
            }
            strSQL += "WHERE customer_id=@customer_id AND member_id=@member_id ";
            strSQL += " AND [year]=@year AND [month]=@month ";
            parameters = new();
            parameters.Add("customer_id", customer_id);
            parameters.Add("member_id", member_id);
            parameters.Add("year", year);
            parameters.Add("month", month);
            parameters.Add("learning_record", origJson);
            return await connection.ExecuteAsync(strSQL, parameters);
        }

        /// <summary>
        /// 更新客戶相關統計數(閱讀篇數、分鐘數、閱讀紀錄、閱讀分鐘紀錄)
        /// </summary>
        /// <param name="customer_id"></param>
        /// <param name="group_id"></param>
        /// <param name="year_month"></param>
        /// <param name="classly"></param>
        /// <param name="readCountType">更新模式(1=閱讀篇數, 2=閱讀分鐘數, 3=兩個都更新)</param>
        /// <returns></returns>
        public async Task UpdateCustomerRead(int customer_id, int? group_id, string year_month, string classly, int readCountType = 1)
        {
            CustomerUtil customerUtil = new(_connectionConfig);
            LearningRecord learningRecord = new();

            dynamic readRecord;
            dynamic readMinutesRecord;
            string readJson;

            //更新客戶閱讀篇數
            if (readCountType == 1 || readCountType == 3)
            {
                await customerUtil.UpdateReadCount(customer_id, group_id, year_month);
                //初始化客戶閱讀數紀錄
                readRecord = await customerUtil.InitReadRecord(customer_id, group_id, year_month);
                if (readRecord != null)
                {
                    //更新客戶閱讀紀錄                    
                    List<ReadRecord> rdList = JsonConvert.DeserializeObject<List<ReadRecord>>(readRecord.read_record);
                    readRecord = rdList.Find(x => x.classly == classly);
                    if (readRecord != null)
                    {
                        readRecord.cnt++;
                        readJson = JsonConvert.SerializeObject(rdList);
                        await customerUtil.UpdateReadRecord(customer_id, group_id, year_month, readJson);
                    }
                }
            }

            //更新客戶閱讀分鐘數
            if (readCountType == 2 || readCountType == 3)
            {
                await customerUtil.UpdateReadMinutesCount(customer_id, group_id, year_month);

                //初始化客戶閱讀分鐘數紀錄
                readMinutesRecord = await customerUtil.InitReadMinutesRecord(customer_id, group_id, year_month);
                if (readMinutesRecord != null)
                {
                    //更新客戶閱讀分鐘數紀錄
                    List<ReadMinutesRecord> rdmList = JsonConvert.DeserializeObject<List<ReadMinutesRecord>>(readMinutesRecord.read_minutes_record);
                    readMinutesRecord = rdmList.Find(x => x.classly == classly);
                    if (readMinutesRecord != null)
                    {
                        readMinutesRecord.cnt++;
                        readJson = JsonConvert.SerializeObject(rdmList);
                        await customerUtil.UpdateReadMinutesRecord(customer_id, group_id, year_month, readJson);
                    }
                }
            }

        }

        #endregion

        #region 取得會員學習紀錄

        /// <summary>
        /// 取得會員所有學習紀錄給首頁Review Bar使用
        /// </summary>
        /// <param name="customer_id"></param>
        /// <param name="member_id"></param>
        /// <param name="pg">頁數</param>
        /// <returns></returns>
        public async Task<Tuple<dynamic, int>> GetLearningRecordForReview(int customer_id, int member_id, int pg = 1)
        {
            
            DapperBase db = new(_connectionConfig.FundayB2B);
            using var connection = db.OpenConnection();
            DynamicParameters parameters = new();
            List<Review> reviewList = new();
            int totalPage = 0;

            //把頁數當作是月份來看
            //pg=1為當月, pg=2為上個月, pg=3上上個月....
            pg = pg == 0 ? 1 : pg;
            string yyyymm = DateTime.Now.AddMonths(-(pg - 1)).ToString("yyyyMM");
            string strSQL;
            //先取得總頁數
            strSQL = "SELECT COUNT(1) FROM Learning_Time WITH (NOLOCK) ";
            strSQL += "WHERE 1=1 ";
            strSQL += " AND customer_id=@customer_id ";
            strSQL += " AND member_id=@member_id ";
            parameters.Add("customer_id", customer_id);
            parameters.Add("member_id", member_id);
            totalPage = await connection.QueryFirstAsync<int>(strSQL, parameters);

            strSQL = "SELECT learning_record FROM Learning_Time WITH (NOLOCK) ";
            strSQL += "WHERE 1=1 ";
            strSQL += " AND customer_id=@customer_id ";
            strSQL += " AND member_id=@member_id ";
            //預設先取2個月前?
            strSQL += $" AND (([year] * 100) + [month]) = {yyyymm} ";
            strSQL += "ORDER BY [year] desc, [month] desc";
            parameters = new();
            parameters.Add("customer_id", customer_id);
            parameters.Add("member_id", member_id);
            var data = await connection.QueryAsync(strSQL, parameters);
            if (data.Any())
            {
                
                foreach (var rec in data)
                {
                    //需要傳換成凱哥要的格式
                    
                    /*
                     [
                      {
                        "date": "2021/5/6",
                        "sort": "音樂",
                        "subject": "Look What You Made Me Do",
                        "level": "",
                        "id": "759"
                      },
                      {
                        "date": "2021/5/6",
                        "sort": "影片",
                        "subject": "FUNDAY 教師專訪 | Tommy",
                        "level": "",
                        "id": "267"
                      },
                      {
                        "date": "2021/5/5",
                        "sort": "文章",
                        "subject": "請假買蛋糕",
                        "level": "A1",
                        "id": "-19929"
                      },
                    ]

                     */
                    LearningRecord JsonObj = JsonConvert.DeserializeObject<LearningRecord>(rec.learning_record);

                    foreach(var a in JsonObj.article)
                    {
                        reviewList.Add(new Review { 
                            date = Convert.ToDateTime(a.createdDate).ToString("yyyy/MM/dd HH:mm:ss"),
                            sort = Common.GetDescription(typeof(Article)),
                            subject = a.chTitle,
                            level = Enum.GetName(typeof(LevelEnum), a.articleLevel),
                            id = a.articleId.ToString()
                        });
                    }
                    foreach (var a in JsonObj.columns)
                    {
                        reviewList.Add(new Review
                        {
                            date = Convert.ToDateTime(a.createdDate).ToString("yyyy/MM/dd HH:mm:ss"),
                            sort = Common.GetDescription(typeof(Columns)),
                            subject = a.chTitle,
                            level = "",
                            id = a.columnsId.ToString()
                        });
                    }
                    foreach (var a in JsonObj.story)
                    {
                        reviewList.Add(new Review
                        {
                            date = Convert.ToDateTime(a.createdDate).ToString("yyyy/MM/dd HH:mm:ss"),
                            sort = Common.GetDescription(typeof(Story)),
                            subject = a.chTitle,
                            level = "",
                            id = a.storyId.ToString()
                        });
                    }
                    foreach (var a in JsonObj.musicbox)
                    {
                        reviewList.Add(new Review
                        {
                            date = Convert.ToDateTime(a.createdDate).ToString("yyyy/MM/dd HH:mm:ss"),
                            sort = Common.GetDescription(typeof(MusicBox)),
                            subject = a.title,
                            level = "",
                            id = a.musicboxId.ToString()
                        });
                    }
                    foreach (var a in JsonObj.video)
                    {
                        reviewList.Add(new Review
                        {
                            date = Convert.ToDateTime(a.createdDate).ToString("yyyy/MM/dd HH:mm:ss"),
                            sort = Common.GetDescription(typeof(Video)),
                            subject = a.title,
                            level = "",
                            id = a.videoId.ToString()
                        });
                    }
                    foreach (var a in JsonObj.blog)
                    {
                        reviewList.Add(new Review
                        {
                            date = Convert.ToDateTime(a.createdDate).ToString("yyyy/MM/dd HH:mm:ss"),
                            sort = Common.GetDescription(typeof(Blog)),
                            subject = a.title,
                            level = "",
                            id = a.blogId.ToString()
                        });
                    }
                                        
                }
            }

            if (reviewList.Any())
            {
                //依照日期最新排序
                reviewList = reviewList.OrderByDescending(a => a.date).ToList();

                //var resultListJSON = JsonConvert.SerializeObject(resultList);

            }
            else
            {
                totalPage = 0;
            }


            return new Tuple<dynamic, int>(reviewList, totalPage);

        }

        /// <summary>
        /// 產生假資料(測試用)
        /// </summary>
        /// <param name="customer_id"></param>
        /// <param name="member_id"></param>
        /// <param name="fromYear"></param>
        /// <param name="fromMonth"></param>
        /// <param name="setYear"></param>
        /// <param name="setMonth"></param>
        /// <returns></returns>
        public async Task<int> CopyLearningRecord(int customer_id, int member_id, int fromYear, int fromMonth, int setYear, int setMonth)
        {
            DapperBase db = new(_connectionConfig.FundayB2B);
            using var connection = db.OpenConnection();
            DynamicParameters parameters = new();
            string strSQL;
            

            strSQL = "SELECT * FROM Learning_Time WITH (NOLOCK) ";
            strSQL += "WHERE 1=1 ";
            strSQL += " AND customer_id=@customer_id ";
            strSQL += " AND member_id=@member_id ";
            strSQL += " AND [year]=@fromYear AND [month]=@fromMonth ";
            
            parameters.Add("customer_id", customer_id);
            parameters.Add("member_id", member_id);
            parameters.Add("fromYear", fromYear);
            parameters.Add("fromMonth", fromMonth);

            var data = await connection.QueryAsync(strSQL, parameters);
            if (data.Any())
            {
                foreach (var rec in data)
                {
                    LearningRecord JsonObj = JsonConvert.DeserializeObject<LearningRecord>(rec.learning_record);
                    foreach (var a in JsonObj.article)
                    {
                        a.createdDate = Convert.ToDateTime(a.createdDate).ToString($"{setYear}-{setMonth}-dd HH:mm:ss");
                    }
                    foreach (var a in JsonObj.columns)
                    {
                        a.createdDate = Convert.ToDateTime(a.createdDate).ToString($"{setYear}-{setMonth}-dd HH:mm:ss");
                    }
                    foreach (var a in JsonObj.story)
                    {
                        a.createdDate = Convert.ToDateTime(a.createdDate).ToString($"{setYear}-{setMonth}-dd HH:mm:ss");
                    }
                    foreach (var a in JsonObj.musicbox)
                    {
                        a.createdDate = Convert.ToDateTime(a.createdDate).ToString($"{setYear}-{setMonth}-dd HH:mm:ss");
                    }
                    foreach (var a in JsonObj.video)
                    {
                        a.createdDate = Convert.ToDateTime(a.createdDate).ToString($"{setYear}-{setMonth}-dd HH:mm:ss");
                    }
                    foreach (var a in JsonObj.blog)
                    {
                        a.createdDate = Convert.ToDateTime(a.createdDate).ToString($"{setYear}-{setMonth}-dd HH:mm:ss");
                    }

                    string JsonText = JsonConvert.SerializeObject(JsonObj);
                    strSQL = "INSERT INTO Learning_Time (customer_id, member_id, [year], [month], learning_record, reading_record_cnt, reading_minutes_cnt) ";
                    strSQL += "VALUES(@customer_id, @member_id, @setYear, @setMonth, @JsonText, @reading_record_cnt, @reading_minutes_cnt) ";
                    parameters.Add("customer_id", customer_id);
                    parameters.Add("member_id", member_id);
                    parameters.Add("setYear", setYear);
                    parameters.Add("setMonth", setMonth);
                    parameters.Add("JsonText", JsonText);
                    parameters.Add("reading_record_cnt", rec.reading_record_cnt);
                    parameters.Add("reading_minutes_cnt", rec.reading_minutes_cnt);
                    return await connection.ExecuteAsync(strSQL, parameters);
                }
            }
            
            //如果沒複製成功，就會跑到這裡
            return 0;

        }

        #endregion

    }
}
