const {getBaseUrlFromSettings } = require('./get-base-url-from-settings')
const {  net, BrowserWindow } = require('electron')
const windowStateKeeper = require('./window-state-keeper').windowStateKeeper
var path = require('path');

const editWindows = new Set();
exports.editWindows = editWindows;

exports.handleExitKeyPress = (fromMainWindow) => {

    console.log(getBaseUrlFromSettings());

    var latestPage = fromMainWindow.webContents.history[fromMainWindow.webContents.history.length-1];
    console.log(latestPage);
    var filePath = new URLSearchParams(new URL(latestPage).search).get("f");
    if (!filePath) return;

    // fromMainWindow.webContents.session.cookies.get({}, (error, cookies) => {
    //     console.log(error, cookies)
    // });

    doRequest(filePath, fromMainWindow.webContents.session, (data) =>  createNewWindow(data,filePath))

    
}

function doRequest(filePath, session, callback) {
    const request = net.request({
        useSessionCookies: true,
        url: getBaseUrlFromSettings() + "/starsky/api/index?f=" + filePath, 
        session: session
    });

    let body = '';
    request.on('response', (response) => {
        if (response.statusCode !== 200) console.log(`HEADERS: ${JSON.stringify(response.headers)}`)
        if (response.statusCode !== 200) return;

        response.on('data', (chunk) => {
            body += chunk.toString()
        });
        response.on('end', () => {
            // console.log(`BODY: ${body}`)
            callback(JSON.parse(body))
        })
    });

    request.on('error',(e)=>{
        console.log(e);
        callback({isError: true})
    })

    request.end()
    return;
}

function createNewWindow(data, filePath) {

    const mainWindowStateKeeper = windowStateKeeper('main')
    let editWindow = new BrowserWindow({
        x: mainWindowStateKeeper.x,
        y: mainWindowStateKeeper.y,
        width: 400,
        height: 200,
        webPreferences: {
            enableRemoteModule: false,
            contextIsolation: true,
            preload: path.join(__dirname, "remote-settings-preload.js") // use a preload script
        }
    });

    editWindow.on('closed', () => {
        editWindows.delete(editWindow);
        editWindow = null;
        });
    editWindows.add(editWindow);

    if (data.isError) {
        editWindow.loadFile('pages/edit-connection-error.html', { query: {"f" : filePath}});
        return;
    }
    if (data.pageType !== "DetailView" || data.isReadOnly) {
        console.log("Sorry, your not allowed or able to do this");
        editWindow.loadFile('pages/edit-not-allowed.html', { query: {"f" : filePath}});
        return;
    }

    editWindow.loadFile('pages/edit-save-as.html', { query: {"f" : filePath}});

}