namespace Funday.Presale.API.Models
{
    /// <summary>
    /// 週測紀錄
    /// </summary>
    public class WeeklyTestRecord
    {
        public int memberLevels { get; set; }
        public int levelsStep { get; set; }
        public int paperId { get; set; }
        public string answer { get; set; }
        public int score { get; set; }
        public string createdDate { get; set; }
        public string modified_date { get; set; }
    }
}
