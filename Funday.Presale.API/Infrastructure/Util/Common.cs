using System.ComponentModel;

namespace Funday.Presale.API.Infrastructure.Util
{
    /// <summary>
    /// 寫一些共用參數的地方
    /// </summary>
    public static class Common
    {

        /// <summary>
        /// 取得物件的[Description]
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetDescription(Type type)
        {
            var descriptions = (DescriptionAttribute[])
                type.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (descriptions.Length == 0)
            {
                return null;
            }
            return descriptions[0].Description;
        }
    }

    /// <summary>
    /// 學習等級(score)
    /// </summary>
    public enum LevelEnum
    {
        [Description("pre-A1")]
        preA1 = 0,

        [Description("A1")]
        A1 = 1,

        [Description("A2")]
        A2 = 2,

        [Description("B1")]
        B1 = 3,

        [Description("B2")]
        B2 = 4,

        [Description("C1")]
        C1 = 5
    }
}
