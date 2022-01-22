namespace Funday.Presale.API.Service.Interface
{
    public interface IMember
    {
        Task<IEnumerable<dynamic>> GetMember(int customer_id, int member_id);

        Task<Tuple<IEnumerable<dynamic>, string>> UpdateArticleLearningRecord(int customer_id, int member_id, int articleId, int readType0);

        Task<Tuple<IEnumerable<dynamic>, string>> UpdateColumnsLearningRecord(int customer_id, int member_id, int columnsId);

        Task<Tuple<IEnumerable<dynamic>, string>> UpdateStoryLearningRecord(int customer_id, int member_id, int storyId);

        Task<Tuple<IEnumerable<dynamic>, string>> UpdateVideoLearningRecord(int customer_id, int member_id, int videoId);

        Task<Tuple<IEnumerable<dynamic>, string>> UpdateMusicBoxLearningRecord(int customer_id, int member_id, int musicboxId);

        Task<Tuple<IEnumerable<dynamic>, string>> UpdateBlogLearningRecord(int customer_id, int member_id, int blogId);

        Task<IEnumerable<dynamic>> InitLearningTime(int customer_id, int member_id, int year, int month);

        Task<Tuple<dynamic, int>> GetLearningRecordForReview(int customer_id, int member_id, int pg);

        Task<int> CopyLearningRecord(int customer_id, int member_id, int fromYear, int fromMonth, int setYear, int setMonth);

    }
}
