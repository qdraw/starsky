const getBaseUrlFromSettings = require('./get-base-url-from-settings').getBaseUrlFromSettings
const {  net } = require('electron')

exports.handleExitKeyPress = (newWindow) => {

    console.log(getBaseUrlFromSettings());

    var latestPage = newWindow.webContents.history[newWindow.webContents.history.length-1];
    console.log(latestPage);
    var filePath = new URLSearchParams(new URL(latestPage).search).get("f");
    if (!filePath) return;

    newWindow.webContents.session.cookies.get({}, (error, cookies) => {
        console.log(error, cookies)
    });

    console.log(filePath,newWindow.webContents.session.cookies);

    request(filePath, newWindow.webContents.session,(data)=>{
        if (data.pageType !== "DetailView" || data.isReadOnly) {
            console.log("Sorry, your not allowed or able to do this");
            return;
        }
        console.log(data);
    })

    
}

function request(filePath, session, callback) {
    const request = net.request({
        useSessionCookies: true,
        url: getBaseUrlFromSettings() + "/starsky/api/index?f=" + filePath, 
        session: session
    });

    let body = '';
    request.on('response', (response) => {
        console.log(`HEADERS: ${JSON.stringify(response.headers)}`)
        if (response.statusCode !== 200) return;

        response.on('data', (chunk) => {
            body += chunk.toString()
        });
        response.on('end', () => {
            console.log(`BODY: ${body}`)
            callback(JSON.parse(body))
        })
    });

    request.end()
    return;
}