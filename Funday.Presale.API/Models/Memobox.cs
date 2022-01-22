namespace Funday.Presale.API.Models
{
    /// <summary>
    /// 便利貼
    /// </summary>
    public class Memobox
    {
        public int articleId { get; set; }
        public List<Memo> memo { get; set; }
        public string createdDate { get; set; }
        public string modifiedDate { get; set; }
    }

    public class Memo
    {
        public string id { get; set; }
        public string text { get; set; }
        public string basicid2 { get; set; }
        public string basicid1 { get; set; }
    }


}
