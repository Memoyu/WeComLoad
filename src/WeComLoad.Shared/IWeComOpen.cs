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
}
