namespace WeComLoad.Shared;

public interface IWeComOpen
{
    WeComAdminWebReq GetWeComReq();

    void ClearReqCookie();

    /// <summary>
    /// 获取企微后台登录二维码
    /// </summary>
    /// <returns>string</returns>
    Task<string> GetLoginQrCodeUrlAsync();

    /// <summary>
    /// 获取企微后台登录二维码扫描情况
    /// </summary>
    /// <param name="qrCodeKey">二维码Key</param>
    /// <returns>string</returns>
    Task<string> GetQrCodeScanStatusAsync(string qrCodeKey);

    /// <summary>
    /// 开始登录（凑齐Cookie）
    /// </summary>
    /// <param name="qrCodeKey">二维码Key</param>
    /// <param name="authCode">授权登录码</param>
    /// <param name="authSource">授权途径</param>
    /// <returns>-1 登录失败，0 需要输入验证码， 1 登录成功</returns>
    Task<(int flag, string msg, string url)> LoginAsync(string qrCodeKey, string authCode, string authSource);

    /// <summary>
    /// 快速登录
    /// </summary>
    /// <param name="authCode">授权码</param>
    /// <param name="authCode">授权来源</param>
    /// <returns></returns>
    Task<bool> LoginAfterAsync(string authCode, string authSource);

    /// <summary>
    /// 跳转验证码输入页面
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    Task<string> LoginCaptchaAsync(string url);

    /// <summary>
    /// 重发验证码
    /// </summary>
    /// <param name="tlKey"></param>
    /// <returns></returns>
    Task<string> LoginSendCaptchaAsync(string tlKey);

    /// <summary>
    /// 确认验证码
    /// </summary>
    /// <param name="tlKey"></param>
    /// <param name="captcha"></param>
    /// <returns></returns>
    Task<string> LoginConfirmCaptchaAsync(string tlKey, string captcha);

    /// <summary>
    /// 获取可信域名校验文件
    /// </summary>
    /// <param name="corpAppId">企业AppOd</param>
    /// <param name="suiteId">代开发应用ID</param>
    /// <returns></returns>
    Task<(string Name, byte[] File)> GetDomainVerifyFileAsync(string corpAppId, string suiteId);

    /// <summary>
    /// 获取企业应用模板列表 WeComBase<WeComSuiteApp>
    /// </summary>
    /// <returns>string<returns>
    Task<string> GetCustomAppTplsAsync();

    /// <summary>
    /// 获取代开发应用模板授权的客户信息列表 WeComBase<WeComSuiteAppAuth>
    /// </summary>
    /// <param name="suitId">模板Id</param>
    /// <param name="offset">第几页</param>
    /// <param name="limit">每页大小</param>
    /// <returns></returns>
    Task<string> GetCustomAppAuthsAsync(string suitId, int offset = 0, int limit = 10);

    /// <summary>
    /// 获取授权企业代开发自建应用详情 WeComBase<WeComSuiteAppAuthDetail>
    /// </summary>
    /// <param name="suitId"></param>
    /// <returns></returns>
    Task<string> GetCustomAppAuthDetailAsync(string suitId);

    /// <summary>
    /// 审核授权代开发自建应用 WeComAuthAppResult
    /// </summary>
    /// <param name="req">请参</param>
    /// <returns></returns>
    Task<string> AuthCorpAppAsync(AuthCorpAppRequest req);

    /// <summary>
    /// 提交审核代开发自建应用 SubmitAuditCorpAppResult
    /// </summary>
    /// <param name="req">请参</param>
    /// <returns></returns>
    Task<string> SubmitAuditCorpAppAsync(SubmitAuditCorpAppRequest req);

    /// <summary>
    /// 上线代开发自建应用 OnlineCorpAppResult
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    Task<string> OnlineCorpAppAsync(OnlineCorpAppRequest req);

    #region 快速登录

    /// <summary>
    /// 获取快速登录参数
    /// </summary>
    /// <returns></returns>
    Task<string> GetQuickLoginParameAsync();

    /// <summary>
    /// 获取企业绑定服务商关系
    /// </summary>
    /// <param name="webKey"></param>
    /// <returns></returns>
    Task<string> GetCorpBindDeveloperInfoAsync(string webKey);

    /// <summary>
    /// 获取快速登录授权企业
    /// </summary>
    /// <param name="webKey"></param>
    /// <returns></returns>
    Task<string> GetQuickLoginCorpInfoAsync(string webKey);

    /// <summary>
    /// 确认快速登录获取授权信息
    /// </summary>
    /// <param name="webKey"></param>
    /// <returns></returns>
    Task<string> ConfirmQuickLoginAsync(string webKey);

    #endregion
}
