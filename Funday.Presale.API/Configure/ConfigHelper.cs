namespace Funday.Presale.API.Configure
{
    public static class ConfigHelper
    {
        private readonly static IConfigurationRoot Configuration;
        static ConfigHelper()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
                .Build();

        }
        public static T? GetConfig<T>(string key, T defaultValue)
        {
            try
            {
                var result = Configuration[key];
                return (T)Convert.ChangeType(result, typeof(T));
            }
            catch (Exception)
            {
                if (defaultValue != null)
                {
                    return defaultValue;
                }
                return default;
            }
        }
        public static T GetConfig<T>(string key)
        {
            try
            {
                var result = Configuration[key];
                return (T)Convert.ChangeType(result, typeof(T));
            }
            catch (Exception)
            {
                throw new Exception(string.Format("沒有在配置檔案中的appSettings中找到{0}的配置，請檢查檔案配置！", key));
            }
        }

        public static T GetAppConfig<T>(string key)
        {
            try
            {
                var result = Configuration[@"AppConfiguration:" + key];
                return (T)Convert.ChangeType(result, typeof(T));
            }
            catch (Exception)
            {
                throw new Exception(string.Format("沒有在配置檔案中的appSettings中找到{0}的配置，請檢查檔案配置！", key));
            }
        }
    }
}
