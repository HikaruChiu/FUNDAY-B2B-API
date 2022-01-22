namespace Funday.Presale.API.Models
{
    /// <summary>
    /// 單字收入
    /// </summary>
    public class WordsCollect
    {
        public int articleId { get; set; }
        public List<Words> words { get; set; }
    }

    public class Words
    {
        public string enWord { get; set; }
        public string chWord { get; set; }
        public int orders { get; set; }

    }
}
