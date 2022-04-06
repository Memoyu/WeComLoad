export function reload() {
    window.location.reload();
}

export async function checkLoginState() {
    const data = await fetch("http://127.0.0.1:50000/checkLoginState", {
        headers: {
            'content-type': 'text/plain;charset=UTF-8',
            'mode': 'no-cors',
            'accept': '*/*',
            'accept-language': 'zh-CN,zh;q=0.9',
            'cache-control': 'no-cache',
            'pragma': 'no-cache',
            /*'sec-ch-ua': '\" Not A;Brand\";v=\'99\", \"Chromium\";v=\'99\", \"Microsoft Edge\";v=\"99\"',
            'sec-ch-ua-mobile': '?0',
            'sec-ch-ua-platform': 'Windows',
            'sec-fetch-dest': 'empty',
            'sec-fetch-mode': 'cors',
            'sec-fetch-site': 'cross-site',*/
            'Host': '127.0.0.1:50000',
            'Origin': 'https://open.work.weixin.qq.com',
            'referer': 'https://open.work.weixin.qq.com/',
        },
        // referrerPolicy: "strict-origin-when-cross-origin",
        body: "{\"scene\":1,\"redirect_uri\":\"https://open.work.weixin.qq.com\"}",
        method: "POST"
    }).then(response => response.json())
        .catch(error => console.log('Error:', error));

    console.log(data)
    return JSON.stringify(data);
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
            'mode': 'no-cors',
            'accept': '*/*',
            'accept-language': 'zh-CN,zh;q=0.9',
            'cache-control': 'no-cache',
            'pragma': 'no-cache',
            /*'sec-ch-ua': '\" Not A;Brand\";v=\'99\", \"Chromium\";v=\'99\", \"Microsoft Edge\";v=\"99\"',
            'sec-ch-ua-mobile': '?0',
            'sec-ch-ua-platform': 'Windows',
            'sec-fetch-dest': 'empty',
            'sec-fetch-mode': 'cors',
            'sec-fetch-site': 'cross-site',*/
            'Host': '127.0.0.1:50000',
            'Origin': 'https://open.work.weixin.qq.com',
            'referer': 'https://open.work.weixin.qq.com/',
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