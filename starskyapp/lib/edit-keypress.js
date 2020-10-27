const {getBaseUrlFromSettings } = require('./get-base-url-from-settings')
const {  net } = require('electron')
const createNewEditWindow = require('./edit-windows').createNewEditWindow

const editFileDownload = require('./edit-file-download').editFileDownload;

exports.handleExitKeyPress = (fromMainWindow) => {

    console.log(getBaseUrlFromSettings());

    var latestPage = fromMainWindow.webContents.history[fromMainWindow.webContents.history.length-1];
    console.log(latestPage);
    var filePath = new URLSearchParams(new URL(latestPage).search).get("f");
    if (!filePath) return;

    doRequest(filePath, fromMainWindow.webContents.session, (data) =>  {

        if (data.pageType !== "DetailView" || data.isReadOnly) {
            createNewEditWindow(data);
            return;
        }

        var lastCollectionInList = data.fileIndexItem.collectionPaths[data.fileIndexItem.collectionPaths.length-1];

        console.log(data.fileIndexItem);
        editFileDownload(fromMainWindow,lastCollectionInList).catch((e)=>{
            createNewEditWindow({isError: true, error: e});
        });
    })

    
}

function doRequest(filePath, session, callback) {
    const request = net.request({
        useSessionCookies: true,
        url: getBaseUrlFromSettings() + "/starsky/api/index?f=" + filePath, 
        session: session,
        headers: {
            "Accept" :	"*/*"
        }
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

