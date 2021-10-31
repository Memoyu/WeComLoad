using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WeComLoad
{
    public class WeWorkWebRequest
    {
        private static readonly CookieContainer _cookies = new CookieContainer();
        private static readonly string _weWorkBaseUrl = "https://work.weixin.qq.com/";
        private static HttpWebRequest _request = null;
        private static HttpWebResponse _response = null;

        public static CookieContainer GetCookies => _cookies;

        public static string GetBaseUrl => _weWorkBaseUrl;

        static WeWorkWebRequest()
        {

        }

        public static string GetQueryUrl(string prefixUrl, Dictionary<string, string> query)
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

        public static async Task<string> HttpWebRequestGetAsync(string url)
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

        public static async Task<string> HttpWebRequestPostAsync(string url, Dictionary<string, string> formData)
        {
            try
            {
                _request = (HttpWebRequest)WebRequest.Create($"{_weWorkBaseUrl}{url}");
                BuildRequest(_request);
                _request.Method = "POST";
                var datas = new List<string>();
                foreach (var item in formData)
                {
                    datas.Add($"{item.Key}={item.Value}");
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

        #region Private

        private static HttpWebRequest BuildRequest(HttpWebRequest request)
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
