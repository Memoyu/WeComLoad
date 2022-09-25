export function reload() {
    window.location.reload();
}

export async function checkLoginState(httpPorts, httpsPorts) {
    let data;
    // 发起http请求
    httpPorts.map(async p => {
        let res = await fetchLocal(false, p)
        if (res !== undefined) data = res;
    })
    // 发起https请求
    httpsPorts.map(async p => {
        let res = await fetchLocal(true, p)
        if (res !== undefined) data = res;
    })
    console.log(data)
    return JSON.stringify(data);
}

async function fetchLocal(isSSL, port){
    const data = await fetch(`${isSSL ? 'https://localhost.work.weixin.qq.com' : 'http://127.0.0.1'}:${port}/checkLoginState`, {
        headers: {
            'content-type': 'text/plain;charset=UTF-8',
            'accept-language': 'zh-CN,zh;q=0.9',
            'sec-fetch-dest': 'empty',
            'sec-fetch-mode': 'cors',
            'sec-fetch-site': 'same-site',
            'xxxxx': 'yyyyy'
        },
        // referrerPolicy: "strict-origin-when-cross-origin",
        body: "{\"scene\":1,\"redirect_uri\":\"https://open.work.weixin.qq.com\"}",
        method: "POST",
        mode: 'no-cors',
        referrer: ""
    }).then(response => response.json())
        .catch(error => console.log('Error:', error));
    return data;
}

export async function confirmLogin(key) {
    const data = {
        scene: 4,
        window_action: 4,
        web_key: key
    }
    const respo = await fetch("http://127.0.0.1:50000/confirmLogin", {
        headers: {
            'content-type': 'text/plain;charset=UTF-8',
            'accept': '*/*',
            'accept-language': 'zh-CN,zh;q=0.9',
            //'pragma': 'no-cache',
            'sec-ch-ua': '\" Not A;Brand\";v=\'99\", \"Chromium\";v=\'99\", \"Microsoft Edge\";v=\"99\"',
            'sec-ch-ua-mobile': '?0',
            'sec-ch-ua-platform': 'Windows',
            'sec-fetch-dest': 'empty',
            'sec-fetch-mode': 'cors',
            'sec-fetch-site': 'cross-site',
            'Host': '127.0.0.1:50000',
            'Origin': 'https://open.work.weixin.qq.com',
            'referer': 'https://open.work.weixin.qq.com/', //*/
        },
        // referrerPolicy: "strict-origin-when-cross-origin",
        body: JSON.stringify(data),
        method: "POST"
    }).then(response => response.ok)
        .catch(error => {
            console.log('Error:', error)
            return false
        });

    console.log(respo)
    return respo;
}

