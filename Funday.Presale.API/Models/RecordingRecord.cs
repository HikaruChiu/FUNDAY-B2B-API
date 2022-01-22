namespace Funday.Presale.API.Models
{
    /// <summary>
    /// 文章錄音紀錄
    /// </summary>
    public class RecordingRecord
    {
        public int articleId { get; set; }
        public List<Recording> recording { get; set; }
        public string createdDate { get; set; }
        public string modifiedDate { get; set; }
    }

    public class Recording
    {
        public string filename { get; set; }
        public string row { get; set; }
    }


}
