using Newtonsoft.Json.Linq;

namespace WeComLoad.Shared;

public class WeComAdminWebReq
{
    private readonly string _weWorkBaseUrl = "https://work.weixin.qq.com/";
    private CookieContainer _cookies = new CookieContainer();
    private string _cookiesStr = string.Empty;

    public WeComAdminWebReq()
    {
    }

    public WeComAdminWebReq(string baseUrl)
    {
        _weWorkBaseUrl = baseUrl;
    }

    public string BaseUrl => _weWorkBaseUrl;

    public string CookieString
    {
        get
        {
            return _cookiesStr;
        }
    }

    public async Task<HttpWebResponse> HttpWebRequestGetAsync(string url, bool isSetCookie = false, bool isUseBaseUrl = true)
    {
        try
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            if (isUseBaseUrl)
                url = $"{_weWorkBaseUrl}{url}";
           
            request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            BuildRequest(request, isSetCookie);

            response = (HttpWebResponse)await request.GetResponseAsync();
            if (isSetCookie)
                SetCookies(request);
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

    public async Task<HttpWebResponse> HttpWebRequestPostAsync(string url, List<(string Key, string Value)> formData, bool isUseBaseUrl = true)
    {
        try
        {
            Hashtable h = new Hashtable();
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            if (isUseBaseUrl)
                url = $"{_weWorkBaseUrl}{url}";
            request = (HttpWebRequest)WebRequest.Create(url);
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

    public async Task<HttpWebResponse> HttpWebRequestPostJsonAsync(string url,string content, bool isUseBaseUrl = true)
    {
        try
        {
            Hashtable h = new Hashtable();
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            if (isUseBaseUrl)
                url = $"{_weWorkBaseUrl}{url}";
            request = (HttpWebRequest)WebRequest.Create(url);
            BuildRequest(request);
            request.Method = "POST";

            if (!string.IsNullOrWhiteSpace(content))
            {
                byte[] postdatabyte = Encoding.UTF8.GetBytes(content);
                Stream stream = await request.GetRequestStreamAsync();
                stream.Write(postdatabyte, 0, postdatabyte.Length);
                stream.Close();

                request.ContentType = "application/json";
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

    public T? GetResponseT<T>(HttpWebResponse response)
    {
        if (response.StatusCode != HttpStatusCode.OK) return default;
        Stream responseStream = response.GetResponseStream();
        StreamReader sr = new StreamReader(responseStream);
        var responseStr = sr.ReadToEnd();
        if (string.IsNullOrWhiteSpace(responseStr)) return default;
        var model = JsonConvert.DeserializeObject<T>(responseStr);
        response.Close();
        responseStream.Close();
        return model;
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

        string pattern = "(?<=Found. Redirecting to ).*";
        var match = Regex.Matches(data, pattern);
        var url = match.FirstOrDefault()?.Value;
        if (string.IsNullOrWhiteSpace(url)) throw new Exception("重定向Url截取异常");
        if (url.IndexOf('/') == 0)
        {
            url = url.Substring(1);
        }
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

    private HttpWebRequest BuildRequest(HttpWebRequest request, bool isSetCookie = false)
    {
        // 不允许重定向路由
        request.AllowAutoRedirect = false;
        request.KeepAlive = true;
        request.Referer = $"{_weWorkBaseUrl}wework_admin/loginpage_wx?from=myhome";
        request.UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36";
        // 当需要设置保存cookie时，则使用CookieContainer，因为这样更易于获取\管理\持久化请求响应的Set-Cookie
        if (isSetCookie)
        {
            request.CookieContainer = new CookieContainer();
            request.CookieContainer = _cookies;
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(_cookiesStr))
            {
                request.Headers.Add("Cookie", _cookiesStr);
            }
        }
        return request;
    }

    private void SetCookies(HttpWebRequest request)
    {
        // 获取并赋值cookies string，可将该string进行缓存，供各个服务节点使用
        _cookiesStr = request.CookieContainer.GetCookieHeader(new Uri("http://www.work.weixin.qq.com"));
    }

    private List<Cookie> GetCookies()
    {
        List<Cookie> lstCookies = new List<Cookie>();
        Hashtable table = (Hashtable)_cookies.GetType().InvokeMember("m_domainTable",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField |
            System.Reflection.BindingFlags.Instance, null, _cookies, new object[] { });

        foreach (object pathList in table.Values)
        {
            SortedList lstCookieCol = (SortedList)pathList.GetType().InvokeMember("m_list",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField
                | System.Reflection.BindingFlags.Instance, null, pathList, new object[] { });
            foreach (CookieCollection colCookies in lstCookieCol.Values)
                foreach (Cookie c in colCookies) lstCookies.Add(c);
        }
        return lstCookies;
    }

    #endregion
}
