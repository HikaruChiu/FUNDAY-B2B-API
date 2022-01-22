using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Reflection;
using System.ComponentModel;
using System.IO.Compression;

namespace Funday.Presale.API.Infrastructure.Util
{
    /// <summary>
    /// 一些字元處裡的擴充類別
    /// </summary>
    public static partial class StringExtensions
    {
        #region 正則表達式
        /// <summary>
        /// 使用RegularExpression在指定的字串是否有符合的項目
        /// </summary>
        /// <param name="value">來源字串</param>
        /// <param name="pattern">正則運算式</param>
        /// <param name="isContains">是否只是要包含而已，否則就是要全部準確符合</param>
        /// <returns>如果有符合就回傳 true</returns>
        public static bool IsMatch(this string value, string pattern, bool isContains = true)
        {
            if (value == null)
            {
                return false;
            }
            return isContains
                ? Regex.IsMatch(value, pattern)
                : Regex.Match(value, pattern).Success;
        }
        /// <summary>
        /// 回傳在正則運算式符合的第一個項目字串
        /// </summary>
        /// <param name="value">來源字串</param>
        /// <param name="pattern">正則運算式</param>
        /// <returns>回傳第一個有符合的項目</returns>
        public static string Match(this string value, string pattern)
        {
            if (value == null)
            {
                return null;
            }
            return Regex.Match(value, pattern).Value;
        }
        /// <summary>
        /// 在指定的輸入字元串中搜索指定的正則表達式的所有符合的字元集合
        /// </summary>
        /// <param name="value">來源字串</param>
        /// <param name="pattern">正則運算式</param>
        /// <returns>回傳符合的字串集合</returns>
        public static IEnumerable<string> Matches(this string value, string pattern)
        {
            if (value == null)
            {
                return new string[] { };
            }
            MatchCollection matches = Regex.Matches(value, pattern);
            return from Match match in matches select match.Value;
        }
        /// <summary>
        /// 判斷字串裡面是否有數字
        /// </summary>
        /// <param name="value">來源字串</param>
        /// <returns></returns>
        public static bool IsMatchNumber(this string value)
        {
            return IsMatch(value, @"\d");
        }
        /// <summary>
        /// 判斷字串裡面是否有數字，並且長度為指定的長度
        /// </summary>
        /// <param name="value">來源字串</param>
        /// <param name="length">指定長度</param>
        /// <returns></returns>
        public static bool IsMatchNumber(this string value, int length)
        {
            Regex regex = new Regex(@"^\d{" + length + "}$");
            return regex.IsMatch(value);
        }
        /// <summary>
        /// 是否為電子郵件
        /// </summary>
        /// <param name="value">來源字串</param>
        /// <returns></returns>
        public static bool IsEmail(this string value)
        {
            const string pattern = @"^[\w-]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)+$";
            return value.IsMatch(pattern);
        }
        /// <summary>
        /// 是否為IP Address
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsIpAddress(this string value)
        {
            const string pattern = @"^((?:(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))\.){3}(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d))))$";
            return value.IsMatch(pattern);
        }
        /// <summary>
        /// 是否為整數
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNumeric(this string value)
        {
            const string pattern = @"^\-?[0-9]+$";
            return value.IsMatch(pattern);
        }

        /// <summary>
        /// 是否是Unicode字串
        /// </summary>
        public static bool IsUnicode(this string value)
        {
            const string pattern = @"^[\u4E00-\u9FA5\uE815-\uFA29]+$";
            return value.IsMatch(pattern);
        }

        /// <summary>
        /// 是否為Url字串
        /// </summary>
        public static bool IsUrl(this string value)
        {
            const string pattern = @"^(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#!]*[\w\-\@?^=%&amp;/~\+#!])?$";
            return value.IsMatch(pattern);
        }
        #endregion

        #region 其他方法
        /// <summary>
        /// 指定的字符串是 null 還是 System.String.Empty 字串
        /// </summary>
        [DebuggerStepThrough]
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// 判斷字串是否為Null或是空白
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static bool IsNull(this string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }
        /// <summary>
        /// 判斷字串是否不為Null或空白
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static bool NotNull(this string s)
        {
            return !string.IsNullOrWhiteSpace(s);
        }
        /// <summary>
        /// 比較兩個字串是否相等，忽略大小寫
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public static bool EqualsIgnoreCase(this string s1, string s2)
        {
            return s1.Equals(s2, StringComparison.OrdinalIgnoreCase);
        }
        /// <summary>
        /// 判斷檔案是否為圖片
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static bool IsImageFile(this string filename)
        {
            if (!File.Exists(filename))
            {
                return false;
            }
            byte[] filedata = File.ReadAllBytes(filename);
            if (filedata.Length == 0)
            {
                return false;
            }
            ushort code = BitConverter.ToUInt16(filedata, 0);
            switch (code)
            {
                case 0x4D42: //bmp
                case 0xD8FF: //jpg
                case 0x4947: //gif
                case 0x5089: //png
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 支援中文長度或全形，中文長度是2
        /// </summary>
        /// <param name="value">來源字串</param>
        /// <returns>回傳字串長度，中文或全形長度是2</returns>
        public static int TextLength(this string value)
        {
            ASCIIEncoding ascii = new ASCIIEncoding();
            int tempLen = 0;
            byte[] bytes = ascii.GetBytes(value);
            foreach (byte b in bytes)
            {
                if (b == 63)
                {
                    tempLen += 2;
                }
                else
                {
                    tempLen += 1;
                }
            }
            return tempLen;
        }

        /// <summary>
        /// 給URL增加查詢參數
        /// </summary>
        /// <param name="url">URL字串</param>
        /// <param name="queries">要添加的參數如："id=1,cid=2"</param>
        /// <returns></returns>
        public static string AddUrlQuery(this string url, params string[] queries)
        {
            foreach (string query in queries)
            {
                if (!url.Contains("?"))
                {
                    url += "?";
                }
                else if (!url.EndsWith("&"))
                {
                    url += "&";
                }

                url += query;
            }
            return url;
        }

        /// <summary>
        /// 取得URL的參數值，沒有的話就回傳空字串
        /// </summary>
        public static string GetUrlQuery(this string url, string key)
        {
            Uri uri = new Uri(url);
            string query = uri.Query;
            if (query.IsNullOrEmpty())
            {
                return string.Empty;
            }
            query = query.TrimStart('?');
            var dict = (from m in query.Split('&', StringSplitOptions.RemoveEmptyEntries)
                        let strs = m.Split("=")
                        select new KeyValuePair<string, string>(strs[0], strs[1]))
                .ToDictionary(m => m.Key, m => m.Value);
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }
            return string.Empty;
        }

        /// <summary>
        /// 將字符串轉換byte[]，預設編碼Encoding.UTF8
        /// </summary>
        public static byte[] ToBytes(this string value, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            return encoding.GetBytes(value);
        }

        /// <summary>
        /// 將byte[]轉換為字串，預設編碼Encoding.UTF8
        /// </summary>
        public static string ToString(this byte[] bytes, Encoding encoding)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            return encoding.GetString(bytes);
        }

        /// <summary>
        /// 將字串轉換Long型態
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static long ToLong(this string str)
        {
            long result;
            long.TryParse(str, out result);
            return result;
        }

        /// <summary>
        /// 將字串轉換為MD5加密（Intranet 一樣方式）
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToMD5String(this string str)
        {
            byte[] b = Encoding.UTF8.GetBytes(str);
            b = new MD5CryptoServiceProvider().ComputeHash(b);
            //for (int i = 0; i < b.Length; i++) {
            //    ret += b[i].ToString("x").PadLeft(2, '0');
            //}

            //2021-09-27 Hikaru MD5密碼改為與Intranet Web 一樣方式
            string ret = Convert.ToBase64String(b);
            return ret;
        }

        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="toEncrypt"></param>
        /// <returns></returns>
        public static string AesEncrypt(string toEncrypt)
        {
            if (string.IsNullOrEmpty(toEncrypt))
            {
                return string.Empty;
            }
            try
            {
                string KeySecret = "gDp4wLdCnCIhbJ12tR2sqH6GTWbURf58"; //與intranet 一樣的Key
                string KeyIv = "Tvcphq2F2LWeuYhX";
                byte[] keyArray = Encoding.UTF8.GetBytes(KeySecret);
                byte[] ivArray = Encoding.UTF8.GetBytes(KeyIv);
                byte[] toEncryptArray = Encoding.UTF8.GetBytes(toEncrypt);

                byte[] useKeyBytes = new byte[16];
                byte[] useIvBytes = new byte[16];

                if (keyArray.Length > useKeyBytes.Length)
                {
                    Array.Copy(keyArray, useKeyBytes, useKeyBytes.Length);
                }
                else
                {
                    Array.Copy(keyArray, useKeyBytes, keyArray.Length);
                }
                if (ivArray.Length > useIvBytes.Length)
                {
                    Array.Copy(ivArray, useIvBytes, useIvBytes.Length);
                }
                else
                {
                    Array.Copy(ivArray, useIvBytes, ivArray.Length);
                }

                Aes aes = System.Security.Cryptography.Aes.Create();
                aes.KeySize = 256;//金鑰的大小，位元128,256等
                aes.BlockSize = 128;//支持的區塊大小
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.CBC;
                aes.Key = useKeyBytes;
                aes.IV = useIvBytes;//初始化向量，如果没有設定，預設16個0

                ICryptoTransform cTransform = aes.CreateEncryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="toDecrypt"></param>
        /// <returns></returns>
        public static string AesDecrypt(string toDecrypt)
        {
            if (string.IsNullOrEmpty(toDecrypt))
            {
                return string.Empty;
            }
            try
            {
                string KeySecret = "gDp4wLdCnCIhbJ12tR2sqH6GTWbURf58"; //與intranet 一樣的Key
                string KeyIv = "Tvcphq2F2LWeuYhX";
                byte[] keyArray = Encoding.UTF8.GetBytes(KeySecret);
                byte[] ivArray = Encoding.UTF8.GetBytes(KeyIv);
                byte[] toEncryptArray = Convert.FromBase64String(toDecrypt);

                byte[] useKeyBytes = new byte[16];
                byte[] useIvBytes = new byte[16];

                if (keyArray.Length > useKeyBytes.Length)
                {
                    Array.Copy(keyArray, useKeyBytes, useKeyBytes.Length);
                }
                else
                {
                    Array.Copy(keyArray, useKeyBytes, keyArray.Length);
                }
                if (ivArray.Length > useIvBytes.Length)
                {
                    Array.Copy(ivArray, useIvBytes, useIvBytes.Length);
                }
                else
                {
                    Array.Copy(ivArray, useIvBytes, ivArray.Length);
                }

                Aes aes = System.Security.Cryptography.Aes.Create();
                aes.KeySize = 256;//金鑰的大小，位元128,256等
                aes.BlockSize = 128;//支持的區塊大小
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.CBC;
                aes.Key = useKeyBytes;
                aes.IV = useIvBytes;//初始化向量，如果没有設定，預設16個0


                ICryptoTransform cTransform = aes.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                return UTF8Encoding.UTF8.GetString(resultArray);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// string轉int
        /// </summary>
        /// <param name="str">字串</param>
        /// <returns></returns>
        public static int ToInt(this string str)
        {
            str = str.Replace("\0", "");
            if (string.IsNullOrEmpty(str))
                return 0;
            return Convert.ToInt32(str);
        }

        /// <summary>
        /// 將Json字串反序列化為物件
        /// </summary>
        /// <typeparam name="T">物件類型</typeparam>
        /// <param name="jsonStr">Json字串</param>
        /// <returns></returns>
        public static T ToObject<T>(this string jsonStr)
        {
            return JsonConvert.DeserializeObject<T>(jsonStr);
        }

        /// <summary>
        /// 將Json字串反序列化為物件
        /// </summary>
        /// <param name="jsonStr">Json字串</param>
        /// <param name="type">物件類型</param>
        /// <returns></returns>
        public static object ToObject(this string jsonStr, Type type)
        {
            return JsonConvert.DeserializeObject(jsonStr, type);
        }

        /// <summary>
        /// 將XML字串反序列化為物件
        /// </summary>
        /// <typeparam name="T">物件類型</typeparam>
        /// <param name="xmlStr">XML字串</param>
        /// <returns></returns>
        public static T XmlStrToObject<T>(this string xmlStr)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlStr);
            string jsonJsonStr = JsonConvert.SerializeXmlNode(doc);

            return JsonConvert.DeserializeObject<T>(jsonJsonStr);
        }

        /// <summary>
        /// 將XML字串反序列化為JObject
        /// </summary>
        /// <param name="xmlStr">XML字符串</param>
        /// <returns></returns>
        public static JObject XmlStrToJObject(this string xmlStr)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlStr);
            string jsonJsonStr = JsonConvert.SerializeXmlNode(doc);

            return JsonConvert.DeserializeObject<JObject>(jsonJsonStr);
        }

        /// <summary>
        /// 將Json字串轉換為 List'T'
        /// </summary>
        /// <typeparam name="T">物件類型</typeparam>
        /// <param name="jsonStr">Json字串</param>
        /// <returns></returns>
        public static List<T> ToList<T>(this string jsonStr)
        {
            return string.IsNullOrEmpty(jsonStr) ? null : JsonConvert.DeserializeObject<List<T>>(jsonStr);
        }
        /// <summary>
        /// 將Json字串轉換DataTable
        /// </summary>
        /// <param name="jsonStr">Json字串</param>
        /// <returns></returns>
        public static DataTable ToDataTable(this string jsonStr)
        {
            return jsonStr == null ? null : JsonConvert.DeserializeObject<DataTable>(jsonStr);
        }

        /// <summary>
        /// 將将Json字串轉為JObject
        /// </summary>
        /// <param name="jsonStr">Json字串</param>
        /// <returns></returns>
        public static JObject ToJObject(this string jsonStr)
        {
            return jsonStr == null ? JObject.Parse("{}") : JObject.Parse(jsonStr.Replace("&nbsp;", ""));
        }

        /// <summary>
        /// 將Json字串轉為JArray
        /// </summary>
        /// <param name="jsonStr">Json字串</param>
        /// <returns></returns>
        public static JArray ToJArray(this string jsonStr)
        {
            return jsonStr == null ? JArray.Parse("[]") : JArray.Parse(jsonStr.Replace("&nbsp;", ""));
        }


        #endregion

    }
}
