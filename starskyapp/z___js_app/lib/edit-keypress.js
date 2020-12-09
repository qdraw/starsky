const {getBaseUrlFromSettings } = require('./get-base-url-from-settings')
const {  net } = require('electron')
const createNewEditWindow = require('./edit-windows').createNewEditWindow
var path = require('path');
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
        if (!data && !data.fileIndexItem && data.fileIndexItem.status !== "Ok" 
              && data.fileIndexItem.status !== "Default") {
            console.log('-!Default/o',data);
            createNewEditWindow(data);
            return;
        }

        // when selecting a tiff image, the jpg will be picked 
        // the last one is always picked

        var subPathLastColInList = ""
        if (data.fileIndexItem && data.fileIndexItem.collectionPaths) {
            subPathLastColInList = data.fileIndexItem.collectionPaths[data.fileIndexItem.collectionPaths.length-1];
        }

        // get info of raw file and get xmp
        // needed app version 0.4 or newer
        if (   data 
            && data.fileIndexItem
            && data.fileIndexItem.fileCollectionName
            && data.fileIndexItem.sidecarExtensionsList
            && data.fileIndexItem.sidecarExtensionsList[0]) {

                var ext = data.fileIndexItem.sidecarExtensionsList[0];
                var sidecarFile = path.join(data.fileIndexItem.parentDirectory,    
                                  data.fileIndexItem.fileCollectionName + "." + ext);
                // download xmp file
                doDownloadRequest(fromMainWindow, 'download-sidecar', 
                     parentFullFilePathHelper(sidecarFile), sidecarFile)
        }

        // download is included
        editFileDownload(fromMainWindow,subPathLastColInList).catch((e)=>{
            console.log(e);
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

