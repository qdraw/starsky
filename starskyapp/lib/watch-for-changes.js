var path = require('path');
const { session, net } = require('electron')
const fs = require('fs');
const { mainWindows } = require('./main-window');
const electronCacheLocation = require('./electron-cache-location').electronCacheLocation;

watchForChanges = () => {
    var editCacheParentFolder = path.join(electronCacheLocation(), "edit")
  
	console.log("! " + editCacheParentFolder);
    var currentSession = mainWindows.values().next().value.webContents.session;

    fs.mkdir(editCacheParentFolder , {
        recursive: true,
    }, ( ) => {
		watchFs(currentSession, editCacheParentFolder);
	});

    console.log('-watch');
    currentSession.cookies.get({}, (error, cookies) => {
        console.log(error, cookies)
    });
}

watchFs = (currentSession, editCacheParentFolder) => {
    // Does not work on some linux systems
    fs.watch(editCacheParentFolder, {recursive: true}, (eventType, fileName) => {
        console.log('watch', eventType, fileName);

        var fullFilePath = path.join(editCacheParentFolder, fileName);
        var parentCurrentFullFilePathFolder = path.join(electronCacheLocation(), "edit", getBaseUrlFromSettingsSlug());

		if(	fs.existsSync(fullFilePath) && fs.lstatSync(fullFilePath).isDirectory() ) return;

		console.log("parentCurrentFullFilePathFolder " + parentCurrentFullFilePathFolder)
        if (fullFilePath.indexOf(parentCurrentFullFilePathFolder) === -1 ) return;

        var subPath = fullFilePath.replace(parentCurrentFullFilePathFolder, "");
		subPath = subPath.replace(/\\/ig,"/");
		
		
        console.log('fullFilePath', fullFilePath);
        console.log('subPath', subPath);
        doUploadRequest(currentSession,fullFilePath,subPath);
    });
}


doUploadRequest = (currentSession, fullFilePath, toSubPath) => {
    if (!currentSession) return;
    console.log('> run upload');

    const request = net.request({
        useSessionCookies: true,
        url: getBaseUrlFromSettings() + "/starsky/api/upload", 
        session: currentSession,
        method: 'POST',
        headers: {
            "to": toSubPath,
            "content-type": "application/octet-stream",
            "Accept" :	"*/*"
        },
    });

    let body = '';
    request.on('response', (response) => {
        if (response.statusCode !== 200) console.log(`HEADERS: ${JSON.stringify(response.headers)} - ${toSubPath} -  ${response.statusCode}`)
        if (response.statusCode !== 200) return;

        response.on('data', (chunk) => {
            body += chunk.toString()
        });
        response.on('end', () => {
            console.log(`BODY: ${body}`)
        })
    });
	

	fs.readFile(fullFilePath, function (err, data) {
		if(err) console.log(fullFilePath, err);
		// skip error for now
		if (err) return;
		request.write(data);
		request.end();
	});


}

module.exports = {
    watchForChanges
}