using System.ComponentModel;

namespace Funday.Presale.API.Models
{
    /// <summary>
    /// 學習紀錄
    /// </summary>
    public class LearningRecord
    {
        public List<Article> article { get; set; }
        public List<Columns> columns { get; set; }
        public List<Story> story { get; set; }
        public List<Video> video { get; set; }
        public List<MusicBox> musicbox { get; set; }
        public List<Blog> blog { get; set; }
    }

    [Description("文章")]
    public class Article
    {
        public int articleId { get; set; }
        public string chTitle { get; set; }
        public string enTitle { get; set; }
        public int articleLevel { get; set; }
        public int readMinutesCnt { get; set; }
        public int readTechingCnt { get; set; }
        public string createdDate { get; set; }
        public string modifiedDate { get; set; }
        public int recordingFlag { get; set; }
        public int memoboxFlag { get; set; }
        public int testFlag { get; set; }
    }

    //public class Magazine
    //{
    //    public int magazineId { get; set; }
    //    public string readMinutesCnt { get; set; }
    //    public string createdDate { get; set; }
    //    public string modifiedDate { get; set; }
    //}

    [Description("專欄")]
    public class Columns
    {
        public int columnsId { get; set; }
        public string chTitle { get; set; }
        public int readMinutesCnt { get; set; }
        public string createdDate { get; set; }
        public string modifiedDate { get; set; }
    }

    [Description("童話")]
    public class Story
    {
        public int storyId { get; set; }
        public string chTitle { get; set; }
        public int readMinutesCnt { get; set; }
        public string createdDate { get; set; }
        public string modifiedDate { get; set; }
    }

    [Description("影片")]
    public class Video
    {
        public int videoId { get; set; }
        public string title { get;set; }
        public int readMinutesCnt { get; set; }
        public string createdDate { get; set; }
        public string modifiedDate { get; set; }
        public int recordingFlag { get; set; }
    }

    [Description("音樂")]
    public class MusicBox
    {
        public int musicboxId { get; set; }
        public string title { get; set; }
        public int readMinutesCnt { get; set; }
        public string createdDate { get; set; }
        public string modifiedDate { get; set; }
        public int singFlag { get; set; }
        public string fileName { get; set; }
    }

    [Description("部落格")]
    public class Blog
    {
        public int blogId { get; set; }
        public string title { get; set; }
        public int readMinutesCnt { get; set; }
        public string createdDate { get; set; }
        public string modifiedDate { get; set; }
    }


}
