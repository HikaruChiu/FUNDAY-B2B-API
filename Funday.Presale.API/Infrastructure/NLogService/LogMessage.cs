namespace Funday.Presale.API.Infrastructure.NLogService
{ 
    public class LogMessage
    {
        /// <summary>
        /// IP
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        public string OperationName { get; set; }

        /// <summary>
        /// 操作時間
        /// </summary>
        public DateTime OperationTime { get; set; }

        /// <summary>
        /// 日誌信息
        /// </summary>
        public string LogInfo { get; set; }

        /// <summary>
        /// 追蹤訊息
        /// </summary>
        public string StackTrace { get; set; }
    }
}
