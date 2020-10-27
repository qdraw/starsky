const fs = require('fs');
const { app, net, shell } = require('electron')
const { FileExtensions } = require('./file-extensions');
const { getBaseUrlFromSettingsSlug, getBaseUrlFromSettings } = require('./get-base-url-from-settings');
const path = require('path');
var childProcess = require("child_process");
const appConfig = require('electron-settings');
const osType = require('./os-type');

exports.parentFullFilePathHelper = (formSubPath) => {
    var documentsFolder = app.getPath("documents");
    return path.join(documentsFolder,
        "Starsky",
        getBaseUrlFromSettingsSlug(),
        new FileExtensions().GetParentPath(formSubPath)
        );
}

exports.editFileDownload = (fromMainWindow, formSubPath) => {
    var parentFullFilePath = exports.parentFullFilePathHelper(formSubPath);
    return new Promise(function (resolve, reject) {
        fs.promises.mkdir(parentFullFilePath, {
            recursive: true
        }).then(()=>{
            cleanAndDownloadRequest(fromMainWindow, 'download-photo', parentFullFilePath, formSubPath, undefined)
            .then((fullFilePath)=>{
                openPath(fullFilePath).then(resolve).catch(reject);
            });
        });
    });

}


const isRunning = (query, callback) => {
    let platform = process.platform;
    let cmd = '';
    switch (platform) {
        case 'win32' : cmd = `tasklist`; break;
        case 'darwin' : cmd = `ps -ax | grep ${query}`; break;
        case 'linux' : cmd = `ps -A`; break;
        default: break;
    }

    var starskyChild = childProcess.spawn(cmd, {
        shell: true
    }, (error, stdout, stderr) => { });

     var stdOutData = "";

    starskyChild.stdout.on('data', function (stdout) {
        stdOutData += stdout.toString();
    });

    starskyChild.stdout.on('end', function () {
        var queryLowercaseNoEscape = "(grep )?" + query.toLowerCase().replace(/\\ /ig," ").replace(/\//ig,".");
        var matches = (stdOutData.match(new RegExp(queryLowercaseNoEscape,"ig")) || []).filter(p => p.indexOf("grep") === -1);
        callback(matches.length >= 1)
    });

    starskyChild.stderr.on('data', function (data) {
     console.log('stderr: ' + data.toString());
    });

}


function openPath(fullFilePath) {

    return new Promise(function (resolve, reject) {
        if (appConfig.has("settings_default_app") && osType.getOsKey() === "mac" ) {

            function excuteOpenFileOnMac() {
                var openFileOnMac = `open -a "${appConfig.get("settings_default_app")}" "${fullFilePath}"`
                console.log("openFileOnMac " + openFileOnMac);
                // // need to check if fullFilePath is directory
                childProcess.exec(openFileOnMac, {
                    cwd: `${appConfig.get("settings_default_app")}`,
                    shell: true
                });
                resolve();
            }

            // There are issues oping photoshop
            if (appConfig.get("settings_default_app").indexOf("Adobe Photoshop.app")) {
                isRunning(".app/Contents/MacOS/Adobe\\ Photoshop",(isRunning)=>{
                    if (!isRunning) {
                        console.log('not running');
                        reject("Photoshop is not running, please start photoshop first and try it again")
                        return;
                    }
                    excuteOpenFileOnMac();
                });
                return;
            }

            excuteOpenFileOnMac()
        }
        else if (appConfig.has("settings_default_app") && osType.getOsKey() === "win" ) {
            // need to check if fullFilePath is file
            var openWin = `"${appConfig.get("settings_default_app")}" "${fullFilePath}"`
            console.log(openWin);
            childProcess.exec(openWin);
            resolve();
        }
        else{
            console.log('open default', fullFilePath);
            shell.openPath(fullFilePath).then((_)=>{
                resolve();
            })
        }
    });

}

cleanAndDownloadRequest = (fromMainWindow, apiName,  parentFullFilePath, formSubPath, toSubPath) => {
    if (!toSubPath) toSubPath = formSubPath;
    var fullFilePath  = path.join( parentFullFilePath, new FileExtensions().GetFileName(toSubPath)    );

    return new Promise(function (resolve, _) {
        fs.promises.access(fullFilePath, fs.constants.R_OK | fs.constants.W_OK)
            .then(() => {
                fs.promises.unlink(fullFilePath).then(()=>{
                    exports.doDownloadRequest(fromMainWindow, apiName , parentFullFilePath, formSubPath, toSubPath).then(resolve);
                })
            })
            .catch(() => {
                exports.doDownloadRequest(fromMainWindow, apiName, parentFullFilePath, formSubPath, toSubPath).then(resolve);
            });

    });
}

exports.doDownloadRequest = (fromMainWindow, apiName , parentFullFilePath, formSubPath, toSubPath) => {
    if (!toSubPath) toSubPath = formSubPath;
    return new Promise(function (resolve, reject) {

        var fullFilePath  = path.join(parentFullFilePath, new FileExtensions().GetFileName(toSubPath)    );

        var file = fs.createWriteStream(fullFilePath);

        const request = net.request({
            useSessionCookies: true,
            url: getBaseUrlFromSettings() + `/starsky/api/${apiName}?isThumbnail=false&f=${formSubPath}`,
            session: fromMainWindow.webContents.session,
            headers: {
                "Accept" :	"*/*",
            }
        });

        request.on('response', (response) => {
            console.log(`api ${apiName} statusCode ${response.statusCode} - HEADERS: ${JSON.stringify(response.headers)}`)
            
            if (response.statusCode !== 200) {
                console.log(response.statusCode);
                reject(response.statusCode);
                return;
            }
            response.pipe(file);

            file.on('error', function (err) {
                fs.unlink(dest); // Delete the file async. (But we don't check the result)
                reject(err.message);
            });
    
            file.on('finish', function () {
                fs.promises.stat(fullFilePath).then((stats) => {
                    if (response.headers['content-length'] === stats.size.toString()) {
                        resolve(fullFilePath);
                        return;
                    }
                    reject("byte size doesnt match");
                });
            })
        });

        // dont forget this one!
        request.end();
    });

}
