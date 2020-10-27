const {getBaseUrlFromSettings } = require('./get-base-url-from-settings')
const {  net } = require('electron')
const createNewEditWindow = require('./edit-windows').createNewEditWindow

const editFileDownload = require('./edit-file-download').editFileDownload;
const doDownloadRequest = require('./edit-file-download').doDownloadRequest;
const parentFullFilePathHelper = require('./edit-file-download').parentFullFilePathHelper;

exports.handleExitKeyPress = (fromMainWindow) => {

    console.log(getBaseUrlFromSettings());

    var latestPage = fromMainWindow.webContents.history[fromMainWindow.webContents.history.length-1];
    console.log(latestPage);
    var filePath = new URLSearchParams(new URL(latestPage).search).get("f");
    if (!filePath) return;

    doRequest(filePath, fromMainWindow.webContents.session, (data) =>  {

        if (!data || !data[0] || data[0].status !== "Ok") {
            createNewEditWindow(data);
            return;
        }

        // when selecting a tiff image, the jpg will be picked 
        // the last one is always picked
        var lastCollectionInList = data[0].collectionPaths[data[0].collectionPaths.length-1];

        console.log(data[data[0].collectionPaths.length-1].sidecarPathsList.length );

        // get info of raw file and get xmp
        if (   data[data[0].collectionPaths.length-1] 
            && data[data[0].collectionPaths.length-1].status === "Ok" 
            && data[data[0].collectionPaths.length-1].sidecarPathsList 
            && data[data[0].collectionPaths.length-1].sidecarPathsList.length >= 1) {

                var sidecarFile = data[data[0].collectionPaths.length-1].sidecarPathsList[0];
                if (sidecarFile) {
                    doDownloadRequest(fromMainWindow, 'download-sidecar', parentFullFilePathHelper(sidecarFile), sidecarFile)
                }
        }

        editFileDownload(fromMainWindow,lastCollectionInList).catch((e)=>{
            createNewEditWindow({isError: true, error: e});
        });


    })

    
}

function doRequest(filePath, session, callback) {
    const request = net.request({
        useSessionCookies: true,
        url: getBaseUrlFromSettings() + "/starsky/api/info?f=" + filePath, 
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

