namespace Funday.Presale.API.Infrastructure.NLogService
{
    public interface INLogHelper
    {
        void LogError(Exception ex, string? message);
    }
}
