var path = require('path');
const { session, net } = require('electron')
const fs = require('fs');
const { mainWindows } = require('./main-window');
const electronCacheLocation = require('./electron-cache-location').electronCacheLocation;

watchForChanges = () => {
    var editCacheParentFolder = path.join(electronCacheLocation(), "edit")
  
    fs.promises.mkdir(editCacheParentFolder , {
        recursive: true,
    }).then(()=>{
        watchFs(editCacheParentFolder);
    });

    console.log('-watch');
    var ses = mainWindows.values().next().value.webContents.session;
    ses.cookies.get({}, (error, cookies) => {
        console.log(error, cookies)
    });
}

watchFs = (editCacheParentFolder) => {
    // Does not work on some linux systems
    fs.watch(editCacheParentFolder, {recursive: true}, (eventType, fileName) => {
        console.log('watch', eventType, fileName);

        var fullFilePath = path.join(editCacheParentFolder, fileName);
        var parentCurrentFullFilePathFolder = path.join(electronCacheLocation(), "edit", getBaseUrlFromSettingsSlug());


        if (fullFilePath.indexOf(parentCurrentFullFilePathFolder) === -1 ) return;

        var subPath = fullFilePath.replace(parentCurrentFullFilePathFolder, "");
        console.log('fullFilePath', fullFilePath);
        console.log('subPath', subPath);
        doUploadRequest(ses,fullFilePath,subPath);
    });
}


doUploadRequest = (ses, fullFilePath, toSubPath) => {
    if (!ses) return;
    console.log('> run upload');

    const request = net.request({
        useSessionCookies: true,
        url: getBaseUrlFromSettings() + "/starsky/api/upload", 
        session: ses,
        method: 'POST',
        headers: {
            "to": toSubPath,
            "content-type": "application/octet-stream",
            "Accept" :	"*/*"
        },
    });

    let body = '';
    request.on('response', (response) => {
        if (response.statusCode !== 200) console.log(`HEADERS: ${JSON.stringify(response.headers)} ${response.statusCode}`)
        if (response.statusCode !== 200) return;

        response.on('data', (chunk) => {
            body += chunk.toString()
        });
        response.on('end', () => {
            console.log(`BODY: ${body}`)
        })
    });

    fs.readFile(fullFilePath, function (err, data) {
        if (err) throw err;
        request.write(data);
        request.end();
    });
}

module.exports = {
    watchForChanges
}