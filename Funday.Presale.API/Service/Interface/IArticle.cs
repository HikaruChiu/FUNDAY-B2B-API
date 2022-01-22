namespace Funday.Presale.API.Service.Interface
{
    public interface IArticle
    {
        Task<dynamic> GetArticle(int articleId);

        Task<dynamic> UpdateMemberRecording(int customer_id, int member_id, string fromRecordingJson);

        Task<dynamic> UpdateMemberMemobox(int customer_id, int member_id, int articleId, string fromMemoboxJson);

        Task<dynamic> UpdateSentencesCollect(int customer_id, int member_id, int articleId, string xmlFileName, string ch_Sentences, string en_Sentences, string clock, string note, int orders);

        Task<dynamic> UpdateWordsCollect(int customer_id, int member_id, int articleId, string enWord, string chWord);

    }
}
