namespace Funday.Presale.API.Models.ViewModel
{
    public class Member
    {
        public int id { get; set; }

        public int customer_id { get; set; }

        public string member_account { get; set; }

        public string password { get; set; }

        public string email { get; set; }

        public string nick_name { get; set; }

        public string real_name { get; set; }

        public DateTime start_date { get; set; }

        public DateTime end_date { get; set; }

        public int curator { get; set; }

        public int? group_id { get; set; }

        public string file_name { get; set; }

        public string birthday { get; set; }

        public string sex { get; set; }

        public DateTime? last_login_date { get; set; }

        public int? is_pay { get; set; }

        public string words_collect { get; set; }

        public string sentences_collect { get; set; }

        public string bookes_collect { get; set; }

        public string musicbox_collect { get; set; }

        public string login_cnt { get; set; }

        public DateTime created_date { get; set; }

        public DateTime modified_date { get; set; }

    }
}
