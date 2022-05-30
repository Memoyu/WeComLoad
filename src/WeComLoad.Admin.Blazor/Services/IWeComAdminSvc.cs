namespace WeComLoad.Admin.Blazor.Services;

public interface IWeComAdminSvc
{
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
}
