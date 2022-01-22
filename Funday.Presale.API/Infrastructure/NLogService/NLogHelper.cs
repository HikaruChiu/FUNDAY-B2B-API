
namespace Funday.Presale.API.Infrastructure.NLogService
{
    public class NLogHelper: INLogHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly ILogger<NLogHelper> _logger;
        public NLogHelper(IHttpContextAccessor httpContextAccessor, ILogger<NLogHelper> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public void LogError(Exception ex, string? message)
        {
            LogMessage logMessage = new LogMessage();
#pragma warning disable CS8602 // 可能 null 參考的取值 (dereference)。
            logMessage.IpAddress = _httpContextAccessor.HttpContext.Request.Host.Host;
#pragma warning restore CS8602 // 可能 null 參考的取值 (dereference)。
            if (ex.InnerException != null)
                logMessage.LogInfo = ex.InnerException.Message;
            else
                logMessage.LogInfo = ex.Message;            
            logMessage.StackTrace = ex.StackTrace;
            logMessage.OperationTime = DateTime.Now;
            logMessage.OperationName = "admin";
            _logger.LogError(LogFormat.ErrorFormat(logMessage));
        }
    }
}
