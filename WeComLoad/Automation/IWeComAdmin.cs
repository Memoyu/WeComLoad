using System.Threading.Tasks;

namespace WeComLoad.Automation
{
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
        /// <returns>bool</returns>
        Task<bool> LoginAsync(string qrCodeKey, string authCode);

        /// <summary>
        /// 获取企业应用列表
        /// </summary>
        /// <param name="isReLoad">是否需要重新获取</param>
        /// <returns>WeComCorpApp</returns>
        Task<WeComBase<WeComCorpApp>> GetCorpAppAsync(bool isReLoad = false);

        /// <summary>
        /// 获取企业部门列表
        /// </summary>
        /// <param name="isReLoad">是否需要重新获取</param>
        /// <returns>WeComCorpDept</returns>
        Task<WeComBase<WeComCorpDept>> GetCorpDeptAsync(bool isReLoad = false);

        /// <summary>
        /// 发送客户联系、通讯录Secret查看
        /// </summary>
        /// <returns>bool</returns>
        Task<bool> SendExtContactAndUserSecretAsync();

        /// <summary>
        /// 创建企业自建应用
        /// </summary>
        /// <param name="req">请求参数</param>
        /// <returns>WeComOpenapiApp</returns>
        Task<WeComOpenapiApp> AddOpenApiAppAsync(AddOpenApiAppRequest req);

        /// <summary>
        /// 配置自建应用侧边栏
        /// </summary>
        /// <param name="req">请求参数</param>
        /// <returns>bool</returns>
        Task<bool> AddChatMenuAsync(AddChatMenuRequest req);

        /// <summary>
        /// 保存应用信息（配置可信域名）
        /// </summary>
        /// <param name="req">SaveOpenApiAppRequest</param>
        /// <returns>WeComSaveOpenApiApp</returns>
        Task<WeComSaveOpenApiApp> SaveOpenApiAppAsync(SaveOpenApiAppRequest req);

        /// <summary>
        /// 配置客户联系 可调用应用
        /// </summary>
        /// <param name="req">请求参数</param>
        /// <returns>WeComSetApiAccessibleApps</returns>
        Task<bool> SetApiAccessibleApps(SetApiAccessibleAppsRequest req);

        /// <summary>
        /// 获取去可信域名校验文件
        /// </summary>
        /// <returns>文件名，文件字节数组</returns>
        Task<(string Name, byte[] File)> GetDomainVerifyFile();

        /// <summary>
        /// 校验：可作为应用OAuth2.0网页授权功能的回调域名
        /// </summary>
        /// <param name="appid">应用Id</param>
        /// <param name="domian">域名</param>
        /// <returns>bool</returns>
        Task<bool> CheckCustomAppURL(string appid, string domian);

        /// <summary>
        /// 校验：可调用JS-SDK、跳转小程序的可信域名
        /// </summary>
        /// <param name="domian">域名</param>
        /// <returns>bool</returns>
        Task<bool> CheckXcxDomainStatus(string domian);
    }
}
