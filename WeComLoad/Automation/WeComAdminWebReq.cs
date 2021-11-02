using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WeComLoad.Automation
{
    public class WeComAdminWebReq
    {
        private readonly string _weWorkBaseUrl = "https://work.weixin.qq.com/";
        private CookieContainer _cookies = new CookieContainer();

        public WeComAdminWebReq()
        {
        }

        public CookieContainer Cookies => _cookies;

        public string BaseUrl => _weWorkBaseUrl;

        public async Task<HttpWebResponse> HttpWebRequestGetAsync(string url)
        {
            try
            {
                HttpWebRequest request = null;
                HttpWebResponse response = null;
                request = (HttpWebRequest)WebRequest.Create($"{_weWorkBaseUrl}{url}");
                request.Method = "GET";
                BuildRequest(request);
                response = (HttpWebResponse)await request.GetResponseAsync();
                return response;
            }
            catch (WebException ex)
            {
                var resp = (HttpWebResponse)ex.Response;
                if (resp.StatusCode == HttpStatusCode.Redirect)
                {
                    return resp;
                }

                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<HttpWebResponse> HttpWebRequestPostAsync(string url, List<(string Key, string Value)> formData)
        {
            try
            {
                Hashtable h = new Hashtable();
                HttpWebRequest request = null;
                HttpWebResponse response = null;
                request = (HttpWebRequest)WebRequest.Create($"{_weWorkBaseUrl}{url}");
                BuildRequest(request);
                request.Method = "POST";
                var datas = new List<string>();
                foreach (var item in formData)
                {
                    var encodeKey = HttpUtility.UrlEncode(item.Key);
                    var encodeVal = HttpUtility.UrlEncode(item.Value);
                    datas.Add($"{encodeKey}={encodeVal}");
                }

                var formDataStr = string.Join("&", datas);
                if (!string.IsNullOrWhiteSpace(formDataStr))
                {
                    byte[] postdatabyte = Encoding.ASCII.GetBytes(formDataStr);
                    Stream stream = await request.GetRequestStreamAsync();
                    stream.Write(postdatabyte, 0, postdatabyte.Length);
                    stream.Close();

                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = postdatabyte.Length;
                }

                response = (HttpWebResponse)await request.GetResponseAsync();
                return response;
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

        public async Task<byte[]> HttpWebRequestDownloadsync(string url)
        {
            try
            {
                HttpWebRequest request = null;
                HttpWebResponse response = null;
                request = (HttpWebRequest)WebRequest.Create($"{_weWorkBaseUrl}{url}");
                BuildRequest(request);

                // 发送请求并获取相应回应数据
                response = (HttpWebResponse)await request.GetResponseAsync();

                // 直到request.GetResponse()程序才开始向目标网页发送Post请求
                Stream responseStream = response.GetResponseStream();

                // 创建本地文件写入流
                byte[] bArr = new byte[response.ContentLength];
                int size = responseStream.Read(bArr, 0, (int)bArr.Length);
                responseStream.Close();
                return bArr;
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

        public string GetResponseStr(HttpWebResponse response)
        {
            Stream responseStream = response.GetResponseStream();
            StreamReader sr = new StreamReader(responseStream);
            var responseStr = sr.ReadToEnd();
            response.Close();
            responseStream.Close();
            return responseStr;
        }

        public bool IsResponseSucc(HttpWebResponse response) => response.StatusCode == HttpStatusCode.OK;

        public bool IsResponseRedi(HttpWebResponse response) => response.StatusCode == HttpStatusCode.Redirect;

        public string GetQueryUrl(string prefixUrl, Dictionary<string, string> query)
        {
            var queryParseStr = HttpUtility.ParseQueryString(string.Empty);
            foreach (var item in query)
            {
                queryParseStr[item.Key] = item.Value;
            }

            string queryString = queryParseStr.ToString();
            var uri = prefixUrl + (string.IsNullOrEmpty(queryString) ? "" : "?") + queryString;

            // 进行解码成中文格式，再返回
            string result = HttpUtility.UrlDecode(uri, Encoding.GetEncoding("UTF-8"));
            return result;
        }

        public string GetRedirectUrl(string data)
        {
            var index = data.IndexOf('/');
            var url = data.Substring(index + 1);
            return url;
        }

        public string GetD2st()
        {
            return $"a{new Random().Next(1000000, 9999999)}";
        }

        public string GetRandom()
        {
            Random random = new Random();
            return $"{(Math.Round((random.NextDouble() * (1 - 0)) + 0, 15) * 0.1) + 0.3}";
        }

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
