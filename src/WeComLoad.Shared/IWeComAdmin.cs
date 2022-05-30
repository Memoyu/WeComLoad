namespace WeComLoad.Shared;

public interface IWeComAdmin
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
    /// <returns>string</returns>
    Task<bool> LoginAsync(string qrCodeKey, string authCode);

    /// <summary>
    /// 获取可选企业列表
    /// </summary>
    /// <param name="qrCodeKey">二维码coede</param>
    /// <param name="authCode">授权码</param>
    /// <returns></returns>
    Task<WeComWxLoginCorps> GetWxLoginCorpsAsync(string qrCodeKey, string authCode);

    /// <summary>
    /// 微信扫码登录
    /// </summary>
    /// <param name="tlKey">tlKey</param>
    /// <param name="corpId">corpId</param>
    /// <returns></returns>
    Task<string> WxLoginAsync(string tlKey, string corpId);

    /// <summary>
    /// 微信扫码登录后完善cookie操作
    /// </summary>
    /// <returns></returns>
    Task<bool> WxLoginAfterAsync();

    /// <summary>
    /// 获取企业应用列表
    /// </summary>
    /// <returns>WeComCorpApp</returns>
    Task<string> GetCorpAppAsync();

    /// <summary>
    /// 获取企业应用信息
    /// </summary>
    /// <param name="appOpenId">应用Id</param>
    /// <returns>WeComOpenapiApp</returns>
    Task<string> GetCorpOpenAppAsync(string appOpenId);

    /// <summary>
    /// 获取企业部门列表
    /// </summary>
    /// <param name="isReLoad">是否需要重新获取</param>
    /// <returns>WeComCorpDept</returns>
    Task<string> GetCorpDeptAsync(bool isReLoad = false);

    /// <summary>
    /// 发送客户联系、通讯录Secret查看
    /// </summary>
    /// <param name="appId">应用id</param>
    /// <returns>bool</returns>
    Task<string> SendSecretAsync(string appId);

    /// <summary>
    /// 创建企业自建应用
    /// </summary>
    /// <param name="pids">pids</param>
    /// <returns>WeComOpenapiApp</returns>
    Task<string> AddOpenApiAppAsync(List<string> pids);

    /// <summary>
    /// 配置自建应用侧边栏
    /// </summary>
    /// <param name="req">请求参数</param>
    /// <param name="agent">应用信息</param>
    /// <returns>bool</returns>
    Task<bool> AddChatMenuAsync(List<AddChatMenuRequest> menus, WeComOpenapiApp agent);

    /// <summary>
    /// 保存应用信息（配置可信域名）
    /// </summary>
    /// <param name="req">SaveOpenApiAppRequest</param>
    /// <returns>WeComSaveOpenApiApp</returns>
    Task<string> SaveOpenApiAppAsync(List<(string Key, string Value)> req);

    /// <summary>
    /// 配置客户联系 可调用应用
    /// </summary>
    /// <param name="req">请求参数</param>
    /// <returns>WeComSetApiAccessibleApps</returns>
    Task<bool> SetApiAccessibleAppsAsync(SetApiAccessibleAppsRequest req);

    /// <summary>
    /// 创建应用授权推送
    /// </summary>
    /// <param name="appId"></param>
    /// <returns></returns>
    Task<string> CreateTwoFactorAuthOpAsync(string appId);

    /// <summary>
    /// 查询确认操作状态
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<int> QueryTwoFactorAuthOpAsync(string key);

    /// <summary>
    /// 配置通讯录回调
    /// </summary>
    /// <param name="req">请求参数</param>
    /// <returns></returns>
    Task<string> ConfigContactCallbackAsync(ConfigCallbackRequest req);

    /// <summary>
    /// 配置客户联系回调
    /// </summary>
    /// <param name="req">请求参数</param>
    /// <returns></returns>
    Task<string> ConfigExtContactCallbackAsync(ConfigCallbackRequest req);

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
