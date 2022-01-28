namespace WeComLoad.Shared;

public interface IWeComOpen
{
    WeComAdminWebReq GetWeCombReq();
    /// <summary>
    /// 获取企微后台登录二维码
    /// </summary>
    /// <returns>string</returns>
    Task<(string Url, string Key)> GetLoginQrCodeUrlAsync();

    /// <summary>
    /// 获取企微后台登录二维码扫描情况
    /// </summary>
    /// <param name="qrCodeKey">二维码Key</param>
    /// <returns>WeComQrCodeScanStatus</returns>
    Task<WeComQrCodeScanStatus> GetQrCodeScanStatusAsync(string qrCodeKey);

    /// <summary>
    /// 开始登录（凑齐Cookie）
    /// </summary>
    /// <param name="qrCodeKey">二维码Key</param>
    /// <param name="authCode">授权登录码</param>
    /// <returns>bool</returns>
    Task<bool> LoginAsync(string qrCodeKey, string authCode);

    /// <summary>
    /// 获取企业应用列表
    /// </summary>
    /// <param name="isReLoad">是否需要重新获取</param>
    /// <returns>WeComCorpApp</returns>
    Task<WeComBase<WeComSuiteApp>> GetCustomAppsAsync();

    /// <summary>
    /// 获取代开发应用模板授权的客户信息列表
    /// </summary>
    /// <param name="suitId">模板Id</param>
    /// <param name="offset">第几页</param>
    /// <param name="limit">每页大小</param>
    /// <returns></returns>
    Task<WeComBase<WeComSuiteAppAuth>> GetCustomAppAuthsAsync(string suitId, int offset = 0, int limit = 10);

    /// <summary>
    /// 获取授权企业代开发自建应用详情
    /// </summary>
    /// <param name="suitId"></param>
    /// <returns></returns>
    Task<WeComBase<WeComSuiteAppAuthDetail>> GetCustomAppAuthDetailAsync(string suitId);

    /// <summary>
    /// 审核授权代开发自建应用
    /// </summary>
    /// <param name="req">请参</param>
    /// <returns></returns>
    Task<WeComAuthAppResult?> AuthCorpAppAsync(AuthCorpAppRequest req);

    /// <summary>
    /// 获取可信域名校验文件
    /// </summary>
    /// <param name="corpAppId">企业AppOd</param>
    /// <param name="suiteId">代开发应用ID</param>
    /// <returns></returns>
    Task<(string Name, byte[] File)> GetDomainVerifyFile(string corpAppId, string suiteId);

    /// <summary>
    /// 提交审核代开发自建应用
    /// </summary>
    /// <param name="req">请参</param>
    /// <returns></returns>
    Task<SubmitAuditCorpAppResult?> SubmitAuditCorpAppAsync(SubmitAuditCorpAppRequest req);

    /// <summary>
    /// 上线代开发自建应用
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    Task<OnlineCorpAppResult?> OnlineAuditCorpAppAsync(OnlineCorpAppRequest req);
}
