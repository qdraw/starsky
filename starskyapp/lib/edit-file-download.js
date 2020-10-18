const fs = require('fs');
const { mainWindows } = require('./main-window');
const {  net, shell } = require('electron')
const { FileExtensions } = require('./file-extensions');
const { getBaseUrlFromSettingsSlug, getBaseUrlFromSettings } = require('./get-base-url-from-settings');
const path = require('path');
const electronCacheLocation = require('./electron-cache-location').electronCacheLocation;
var childProcess = require("child_process");

exports.editFileDownload = (formSubPath, toSubPath) => {
    if (!mainWindows.values().next().value) return;

    var parentFullFilePath = path.join(electronCacheLocation(), "edit", getBaseUrlFromSettingsSlug(), new FileExtensions().GetParentPath(formSubPath) ) ;
    fs.promises.mkdir(parentFullFilePath, {
        recursive: true
    }).then(()=>{
        doDownloadRequest(parentFullFilePath, formSubPath, toSubPath).then((fullFilePath)=>{
            openPath(fullFilePath)
            // watchForChanges(fullFilePath,toSubPath)
        });
    })
}

function openPath(fullFilePath) {
    console.log('-> ', fullFilePath);

    // TODO: ONLY 2020 EDITION
    var openMac = `osascript -e 'tell application "Adobe Photoshop 2020"\n  activate\n  set thisFile to "${fullFilePath}" as string\n open alias thisFile\n end tell'`;
    console.log(openMac);
    childProcess.exec(openMac);
}




doDownloadRequest = (parentFullFilePath, formSubPath, toSubPath) => {
    return new Promise(function (resolve, _) {

        var fullFilePath  = path.join( parentFullFilePath,       new FileExtensions().GetFileName(toSubPath)    )
        let writeStream = fs.createWriteStream(fullFilePath, { 
            'flags': 'a', 
            'encoding': null, 
            'mode': 0666
        });

        var firstWindow = mainWindows.values().next().value;
        const request = net.request({
            useSessionCookies: true,
            url: getBaseUrlFromSettings() + "/starsky/api/download-photo?isThumbnail=false&f=" + formSubPath, 
            session: firstWindow.webContents.session
        });

        request.on('response', (response) => {
            console.log(`HEADERS: ${JSON.stringify(response.headers)}`)
            if (response.statusCode !== 200) return;

            response.on('data', (chunk) => {
                writeStream.write(chunk);
            });
            response.on('end', () => {
                writeStream.close();
                resolve(fullFilePath)
            })
        });

        // dont forget this one!
        request.end();
    });

}
