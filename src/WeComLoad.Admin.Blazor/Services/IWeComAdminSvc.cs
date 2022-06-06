namespace WeComLoad.Admin.Blazor.Services;

public interface IWeComAdminSvc
{
    #region Login

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
    /// 获取登录可选企业列表
    /// </summary>
    /// <param name="qrCodeKey">二维码coede</param>
    /// <param name="authCode">授权码</param>
    /// <returns></returns>
    Task<WeComWxLoginCorps> GetWxLoginCorpsAsync(string qrCodeKey, string authCode);

    /// <summary>
    /// 开始登录（凑齐Cookie）
    /// </summary>
    /// <param name="qrCodeKey">二维码Key</param>
    /// <param name="authCode">授权登录码</param>
    /// <returns>bool</returns>
    Task<bool> LoginAsync(string qrCodeKey, string authCode);

    /// <summary>
    /// 微信扫码登录
    /// </summary>
    /// <param name="tlKey">tlKey</param>
    /// <param name="corpId">corpId</param>
    /// <returns>0：登录失败， 1：登录成功，2：需要输入验证码</returns>
    Task<int> WxLoginAsync(string tlKey, string corpId);

    /// <summary>
    /// 获取验证码发送页面信息
    /// </summary>
    /// <param name="tlKey">tlKey</param>
    /// <returns></returns>
    Task<string> WxLoginCaptchaAsync(string tlKey);

    /// <summary>
    /// 发送验证码
    /// </summary>
    /// <param name="tlKey">tlKey</param>
    /// <returns></returns>
    Task<string> WxLoginSendCaptchaAsync(string tlKey);

    #endregion

    /// <summary>
    /// 获取企业应用列表
    /// </summary>
    /// <returns>WeComCorpApp</returns>
    Task<WeComCorpApp> GetCorpAppAsync();

    /// <summary>
    /// 获取企业应用信息
    /// </summary>
    /// <param name="appOpenId">应用Id</param>
    /// <returns>WeComOpenapiApp</returns>
    Task<WeComOpenapiApp> GetCorpOpenAppAsync(string appOpenId);

    /// <summary>
    /// 获取企业部门列表
    /// </summary>
    /// <returns>WeComCorpDept</returns>
    Task<WeComCorpDept> GetCorpDeptAsync();

    /// <summary>
    /// 发送指定Agent Secret查看
    /// </summary>
    /// <param name="appId">应用id</param>
    /// <returns>bool</returns>
    Task<bool> SendSecretAsync(string appId);

    /// <summary>
    /// 发送客户联系、通讯录Secret查看
    /// </summary>
    /// <returns>bool</returns>
    Task<bool> SendExtContactAndUserSecretAsync();

    /// <summary>
    /// 创建企业自建应用
    /// </summary>
    /// <param name="req">req</param>
    /// <returns>WeComOpenapiApp</returns>
    Task<WeComOpenapiApp> AddOpenApiAppAsync(AddOpenApiAppRequest req);

    /// <summary>
    /// 配置自建应用侧边栏
    /// </summary>
    /// <param name="menus">侧边栏配置</param>
    /// <param name="agent">应用信息</param>
    /// <returns>bool</returns>
    Task<(bool flag, string msg)> AddChatMenuAsync(List<AddChatMenuRequest> menus, WeComOpenapiApp agent);

    /// <summary>
    /// 保存应用信息（配置可信域名）
    /// </summary>
    /// <param name="req">SaveOpenApiAppRequest</param>
    /// <returns>WeComSaveOpenApiApp</returns>
    Task<bool> SaveOpenApiAppAsync(List<(string Key, string Value)> req);

    /// <summary>
    /// 创建应用授权推送
    /// </summary>
    /// <param name="appId"></param>
    /// <returns></returns>
    Task<WeComCreateTwoFactorAuthOp> CreateTwoFactorAuthOpAsync(string appId);

    /// <summary>
    /// 查询确认操作状态
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<WeComQueryTwoFactorAuthOp> QueryTwoFactorAuthOpAsync(string key);


    /// <summary>
    /// 配置通讯录回调
    /// </summary>
    /// <param name="req">请求参数</param>
    /// <returns></returns>
    Task<WeComConfigCallback> ConfigContactCallbackAsync(ConfigCallbackRequest req);

    /// <summary>
    /// 配置客户联系回调
    /// </summary>
    /// <param name="req">请求参数</param>
    /// <returns></returns>
    Task<WeComConfigCallback> ConfigExtContactCallbackAsync(ConfigCallbackRequest req);

    /// <summary>
    /// 获取应用的 可调用应用
    /// </summary>
    /// <param name="businessId"></param>
    /// <returns></returns>
    Task<WeComGetApiAccessibleApps> GetApiAccessibleAppsAsync(string businessId);

    /// <summary>
    /// 配置客户联系 可调用应用
    /// </summary>
    /// <param name="businessId">目标应用id</param>
    /// <param name="accessibleApps">可调用应用id</param>
    /// <returns>WeComSetApiAccessibleApps</returns>
    Task<WeComSaveOpenApiApp> SetApiAccessibleAppsAsync(string businessId, List<string> accessibleApps);

    /// <summary>
    /// 获取去可信域名校验文件
    /// </summary>
    /// <returns>文件名，文件字节数组</returns>
    Task<(string Name, byte[] File)> GetDomainVerifyFileAsync();

    /// <summary>
    /// 校验：可作为应用OAuth2.0网页授权功能的回调域名
    /// </summary>
    /// <param name="appid">应用Id</param>
    /// <param name="domian">域名</param>
    /// <returns>bool</returns>
    Task<bool> CheckCustomAppURLAsync(string appid, string domian);

    /// <summary>
    /// 校验：可调用JS-SDK、跳转小程序的可信域名
    /// </summary>
    /// <param name="domian">域名</param>
    /// <returns>bool</returns>
    Task<bool> CheckXcxDomainStatusAsync(string domian);

    /// <summary>
    /// 配置代开发自建应用的授权信息
    /// </summary>
    /// <param name="appId">代开发自建应用AppId</param>
    /// <returns></returns>
    Task<bool> SetCustomizedAppPrivilege(string appId);
}
