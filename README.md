## WeComLoad Demo

基于.NET 6 企业微信后台管理操作Demo，解决方案中包含如下三个个项目：

**Admin**：(WPF)初衷在于简化甚至于免去第三方企微Scrm系统中的部分用户配置操作，使得用户在初次使用系统时体验会更好；

**Open**：(WPF)主要解决服务商对授权企业的代开发自建应用的审核、上线流程一条龙服务，提高审核效率以及配置正确性，无脑一键自动化；

**Open.Blazor**：(Blazor)与Open 功能一致，采用了Blazor + Ant Design Blazor 实现；**适配企微服务商后台快速登录功能，并集成到工具中**；

本项目主要提供一个思路，并实现了部分功能；

希望能帮到你，觉得有用的话请给个start哈🙌，万分感谢！

## What can I do?

**WeComLoad.Admin：**

* 获取Login QrCode
* 登录获得Cookies
* 获取应用列表
* 发送Secret查看
* 创建自建应用、应用配置（侧边栏、可信域名等）
* More.......

**WeComLoad.Open**

* 适配企微服务商后台快速登录

* 代开自建应用审核、上线

* More.......

## How to achieve?

**Admin**

登录在改版后发生了改变，只需要一步即可获取完整带授权cookies信息

![mind.png](https://github.com/Memoyu/WeComLoad/raw/master/doc/mind.png)

**Open**

![mind.png](https://github.com/Memoyu/WeComLoad/raw/master/doc/open.flow.png)

企微客户端4.0.3以上版本在启动后会开启50000 或 50001 或 50002端口的本地接口服务；

1. 调用127.0.0.1:50000/checkLoginState，获取企微客户端信息及校验能否快速登陆；

2. 调用企微服务wwopen/wwLogin/wwQuickLogin，获取登录凭证参数（web_key、client_key）

3. 调用127.0.0.1:50000/checkLoginState，进行确认登录

4. 调用企微服务wwopen/monoApi/wwQuickLogin/login/confirmQuickLoginByKey，获取最终登录授权码

5. 调用企微服务wwopen/login，进行登录cookie获取（该接口即可完整获取到授权的cookies）

## Effect

**Admin Demo**

![Effect.gif](https://github.com/Memoyu/WeComLoad/raw/master/doc/Effect.gif)

**Open Demo**

![sp20220405_001606.png](https://github.com/Memoyu/WeComLoad/raw/master/doc/open.png)

## Run

1、.NET 6

2、Visual Studio -> F5

**PS：在使用Open.Blazor时，保证自己的企微客户端版本为4.0.3以上，请确认企微账户为服务商下的管理员，并且本地客户端已登录该账号，否则默认只会使用扫码登陆**

## License

[MIT](LICENSE).
