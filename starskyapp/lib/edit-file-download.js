const fs = require('fs');
const {  net, shell } = require('electron')
const { FileExtensions } = require('./file-extensions');
const { getBaseUrlFromSettingsSlug, getBaseUrlFromSettings } = require('./get-base-url-from-settings');
const path = require('path');
const electronCacheLocation = require('./electron-cache-location').electronCacheLocation;
var childProcess = require("child_process");
const appConfig = require('electron-settings');
const osType = require('./os-type');

exports.editFileDownload = (fromMainWindow, formSubPath) => {

    var parentFullFilePath = path.join(electronCacheLocation(), "edit", getBaseUrlFromSettingsSlug(), new FileExtensions().GetParentPath(formSubPath) ) ;
    fs.promises.mkdir(parentFullFilePath, {
        recursive: true
    }).then(()=>{
        cleanAndDownloadRequest(fromMainWindow, parentFullFilePath, formSubPath, undefined).then((fullFilePath)=>{
            openPath(fullFilePath)
        });
    })
}

function openPath(fullFilePath) {
    console.log('-> ', fullFilePath);

    if (appConfig.has("settings_default_app") && osType.getOsKey() === "mac" ) {
        var openMac = `open -a "${appConfig.get("settings_default_app")}" "${fullFilePath}"`
        console.log(openMac);
		// need to check if fullFilePath is directory
        childProcess.exec(openMac);
    }
	else if (appConfig.has("settings_default_app") && osType.getOsKey() === "win" ) {
		// need to check if fullFilePath is file
		var openWin = `"${appConfig.get("settings_default_app")}" "${fullFilePath}"`
		console.log(openWin);
        childProcess.exec(openWin);
	}
    else{
        shell.openPath(fullFilePath)
    }


}

cleanAndDownloadRequest = (fromMainWindow, parentFullFilePath, formSubPath, toSubPath) => {
    if (!toSubPath) toSubPath = formSubPath;
    var fullFilePath  = path.join( parentFullFilePath, new FileExtensions().GetFileName(toSubPath)    );

    return new Promise(function (resolve, _) {
        fs.promises.access(fullFilePath, fs.constants.R_OK | fs.constants.W_OK)
            .then(() => {
                fs.promises.unlink(fullFilePath).then(()=>{
                    doDownloadRequest(fromMainWindow, parentFullFilePath, formSubPath, toSubPath).then(resolve);
                })
            })
            .catch(() => {
                doDownloadRequest(fromMainWindow, parentFullFilePath, formSubPath, toSubPath).then(resolve);
            });

    });
}


doDownloadRequest = (fromMainWindow,parentFullFilePath, formSubPath, toSubPath) => {
    if (!toSubPath) toSubPath = formSubPath;
    return new Promise(function (resolve, _) {

        var fullFilePath  = path.join( parentFullFilePath, new FileExtensions().GetFileName(toSubPath)    );

        // ONLY append!
        let writeStream = fs.createWriteStream(fullFilePath, { 
            'flags': 'a', 
            'encoding': null, 
            'mode': 0666
        });

        const request = net.request({
            useSessionCookies: true,
            url: getBaseUrlFromSettings() + "/starsky/api/download-photo?isThumbnail=false&f=" + formSubPath, 
            session: fromMainWindow.webContents.session,
            headers: {
                "Accept" :	"*/*"
            }
        });

        request.on('response', (response) => {
            console.log(`HEADERS: ${JSON.stringify(response.headers)}`)
            if (response.statusCode !== 200) return;

            response.on('data', (chunk) => {
                writeStream.write(chunk);
            });
            response.on('end', () => {
                writeStream.close();
                setTimeout(()=>{
                    resolve(fullFilePath)
                },100);
            })
        });

        // dont forget this one!
        request.end();
    });

}
