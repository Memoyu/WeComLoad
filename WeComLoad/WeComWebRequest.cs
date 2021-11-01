using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WeComLoad.Model;

namespace WeComLoad
{
    public class WeComWebRequest
    {
        private readonly CookieContainer _cookies = new CookieContainer();
        private readonly string _weWorkBaseUrl = "https://work.weixin.qq.com/";
        private Base<CorpApp> _corpApps = null;
        private Base<CorpDept> _corpDepts = null;
        private HttpWebRequest _request = null;
        private HttpWebResponse _response = null;

        public CookieContainer Cookies => _cookies;

        public string BaseUrl => _weWorkBaseUrl;

        public WeComWebRequest()
        {

        }

        public string GetQueryUrl(string prefixUrl, Dictionary<string, string> query)
        {
            var queryParseStr = HttpUtility.ParseQueryString(string.Empty);
            foreach (var item in query)
            {
                queryParseStr[item.Key] = item.Value;
            }
            string queryString = queryParseStr.ToString();
            var uri = prefixUrl + (string.IsNullOrEmpty(queryString) ? "" : "?") + queryString;
            //进行解码成中文格式，再返回
            string result = HttpUtility.UrlDecode(uri, Encoding.GetEncoding("UTF-8"));
            return result;
        }

        public async Task<string> HttpWebRequestGetAsync(string url)
        {
            try
            {
                _request = (HttpWebRequest)WebRequest.Create($"{_weWorkBaseUrl}{url}");
                _request.Method = "GET";
                BuildRequest(_request);
                _response = (HttpWebResponse)await _request.GetResponseAsync();
                _cookies.Add(_response.Cookies);
                Stream responseStream = _response.GetResponseStream();
                StreamReader sr = new StreamReader(responseStream);
                var responseStr = sr.ReadToEnd();
                _response.Close();
                responseStream.Close();
                return responseStr;
            }
            catch (WebException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> HttpWebRequestPostAsync(string url, Dictionary<string, string> formData)
        {
            try
            {
                _request = (HttpWebRequest)WebRequest.Create($"{_weWorkBaseUrl}{url}");
                BuildRequest(_request);
                _request.Method = "POST";
                var datas = new List<string>();
                foreach (var item in formData)
                {
                    var encodeVal = HttpUtility.UrlEncode(item.Value);
                    datas.Add($"{item.Key}={encodeVal}");
                }

                var formDataStr = string.Join("&", datas);
                if (!string.IsNullOrWhiteSpace(formDataStr))
                {
                    byte[] postdatabyte = Encoding.ASCII.GetBytes(formDataStr);
                    Stream stream = await _request.GetRequestStreamAsync();
                    stream.Write(postdatabyte, 0, postdatabyte.Length);
                    stream.Close();

                    _request.ContentType = "application/x-www-form-urlencoded";
                    _request.ContentLength = postdatabyte.Length;
                }

                _response = (HttpWebResponse)await _request.GetResponseAsync();
                _cookies.Add(_response.Cookies);
                Stream responseStream = _response.GetResponseStream();
                StreamReader sr = new StreamReader(responseStream);
                var responseStr = sr.ReadToEnd();
                _response.Close();
                responseStream.Close();
                return responseStr;
            }
            catch (WebException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetD2st()
        {
            return $"a{new Random().Next(1000000, 9999999)}";
        }

        public string GetRandom()
        {
            Random random = new Random();
            return $"{ (Math.Round(random.NextDouble() * (1 - 0) + 0, 15) * 0.1) + 0.3}";
        }

        #region WeCom

        /// <summary>
        /// 获取企业应用列表
        /// </summary>
        /// <param name="isReLoad">是否需要重新获取</param>
        /// <returns></returns>
        public async Task<Base<CorpApp>> GetCorpAppAsync(bool isReLoad = false)
        {
            if (_corpApps == null || isReLoad)
            {
                var dic = new Dictionary<string, string>
                {
                    {"_d2st",GetD2st() }, {"app_type","0" }
                };
                var url = GetQueryUrl("wework_admin/getCorpApplication", new Dictionary<string, string>
                {
                    {"lang","zh_CN" }, {"f", "json"}, {"ajax", "1"}, {"timeZoneInfo%5Bzone_offset%5D", "-8"}, {"random", GetRandom()}
                });
                var data = await HttpWebRequestPostAsync(url, dic);
                var model = JsonConvert.DeserializeObject<Base<CorpApp>>(data);
                _corpApps = model;
                return model;
            }
            return _corpApps;
        }

        /// <summary>
        /// 获取企业部门列表
        /// </summary>
        /// <param name="isReLoad">是否需要重新获取</param>
        /// <returns></returns>
        public async Task<Base<CorpDept>> GetCorpDeptAsync(bool isReLoad = false)
        {
            if (_corpDepts == null || isReLoad)
            {
                var dic = new Dictionary<string, string>
                {
                    {"_d2st",GetD2st() }
                };
                var url = GetQueryUrl("wework_admin/contacts/party/cache", new Dictionary<string, string>
                {
                    {"lang","zh_CN" }, {"f", "json"}, {"timeZoneInfo%5Bzone_offset%5D", "-8"}, {"random",  GetRandom()}
                });
                var data = await HttpWebRequestPostAsync(url, dic);
                var model = JsonConvert.DeserializeObject<Base<CorpDept>>(data);
                _corpDepts = model;
                return model;
            }
            return _corpDepts;
        }

        #endregion

        #region Private

        private HttpWebRequest BuildRequest(HttpWebRequest request)
        {
            // 不允许重定向路由
            request.AllowAutoRedirect = false;
            request.KeepAlive = true;
            request.Referer = $"{_weWorkBaseUrl}wework_admin/loginpage_wx?from=myhome";
            request.UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36";
            request.CookieContainer = _cookies;
            return request;
        }

        #endregion
    }
}
