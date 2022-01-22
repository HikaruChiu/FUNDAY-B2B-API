using System.ComponentModel;

namespace Funday.Presale.API.Models.ViewModel
{
    public class Customer
    {
        [DisplayName("客戶ID")]
        public int id { get; set; }

        [DisplayName("客戶名稱")]
        public string name { get; set; }

        [DisplayName("客戶類型")]
        public int type { get; set; }

        [DisplayName("B2B網址")]
        public string DNS { get; set; }

        [DisplayName("該公司承辦人")]
        public string agent { get; set; }

        [DisplayName("網站客戶起日(合約)")]
        public DateTime start_date { get; set; }

        [DisplayName("網站客戶迄日(合約)")]
        public DateTime end_date { get; set; }

        [DisplayName("使用人數")]
        public int users_number { get; set; }

        [DisplayName("接洽業務員英文名稱")]
        public string sales { get; set; }

        [DisplayName("業務員備註")]
        public string memo { get; set; }

        [DisplayName("是否為試用戶")]
        public bool is_try { get; set; }

        [DisplayName("建立時間")]
        public DateTime created_date { get; set; }

        [DisplayName("修改時間")]
        public DateTime modified_date { get; set; }
    }
}
