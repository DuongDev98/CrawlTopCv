using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CrawlTopCv
{
    class TopCv
    {
        private void LayDuLieuTopCv(ref DataTable dt, ref string error, string username, string password, DateTime fromDate, DateTime toDate)
        {
            try
            {
                string url = "https://tuyendung-api.topcv.vn/api/v1/auth/login";
                HttpClient client = new HttpClient();
                using (MultipartFormDataContent content = new MultipartFormDataContent())
                {
                    content.Add(new StringContent(username), "email");
                    content.Add(new StringContent(password), "password");
                    var response = client.PostAsync(url, content).Result;
                    var resString = response.Content.ReadAsStringAsync().Result;
                    JObject obj = JObject.Parse(resString);
                    if (obj["status"].ToString() == "success")
                    {
                        string accessToken = obj["access_token"].ToString();

                        //&filter_by=not-viewed
                        string urlCv = "https://tuyendung-api.topcv.vn/api/v1/cv-management/cvs?get_newest_cv=true&filter_by=all";
                        client = new HttpClient();
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                        response = client.GetAsync(urlCv).Result;
                        resString = response.Content.ReadAsStringAsync().Result;
                        obj = JObject.Parse(resString);
                        if (obj["status"].ToString() == "success")
                        {
                            JObject cvs = JObject.Parse(obj["cvs"].ToString());
                            List<Cv_topcv> lst = JsonConvert.DeserializeObject<List<Cv_topcv>>(cvs["data"].ToString());
                            if (lst == null || lst.Count == 0) return;
                            foreach (Cv_topcv item in lst)
                            {
                                //kiểm tra ngày phù hợp thì lấy
                                if (item.created_at >= fromDate && item.created_at <= toDate)
                                {
                                    DataRow rNew = dt.NewRow();
                                    rNew["NGAY"] = item.created_at.ToString("dd/MM/yyyy");
                                    rNew["JOB"] = item.campaign.job.title;
                                    rNew["NAME"] = item.fullname;
                                    rNew["EMAIL"] = item.email;
                                    rNew["DIENTHOAI"] = item.phone;
                                    rNew["DIACHI"] = item.address;
                                    rNew["NGUON"] = "TopCv";
                                    rNew["NGUONSTR"] = "www.topcv.vn";
                                    dt.Rows.Add(rNew);
                                }
                            }
                        }
                        else
                        {
                            error += "<br>www.topcv.vn error:" + obj["message"].ToString();
                        }
                    }
                    else
                    {
                        error += "<br>www.topcv.vn error:" + obj["message"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                error += "<br>www.topcv.vn error:" + ex.Message;
            }
        }

        string downloadString(string url, string nameOfCookie, string valueOfCookie)
        {
            //ServicePointManager.Expect100Continue = true;
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                   | SecurityProtocolType.Tls11
                   | SecurityProtocolType.Tls12
                   | SecurityProtocolType.Ssl3;

            WebClient wc = new WebClient();
            wc.Encoding = Encoding.UTF8;
            wc.Headers.Add(HttpRequestHeader.Cookie, nameOfCookie + "=" + valueOfCookie);
            return wc.DownloadString(url);
        }
    }

    class Cv_topcv
    {
        public int id { set; get; }
        public string title { set; get; }
        public int user_id { set; get; }
        public string iframe_access_url { set; get; }
        public string download_url { set; get; }
        public string fullname { set; get; }
        public string email { set; get; }
        public string phone { set; get; }
        public DateTime created_at { set; get; }
        public DateTime? year_of_birth { set; get; }
        public string gender { set; get; }
        public string address { set; get; }
        public string city_name { set; get; }
        public string last_company { set; get; }
        public string year_of_experience_str { set; get; }
        public Campaign_topcv campaign { set; get; }
    }

    class Campaign_topcv
    {
        public int id { set; get; }
        public string title { set; get; }
        public DateTime created_at { set; get; }
        public int status { set; get; }
        public bool has_job { set; get; }
        public Job_topcv job { set; get; }
    }

    class Job_topcv
    {
        public int id { set; get; }
        public string title { set; get; }
    }
}
