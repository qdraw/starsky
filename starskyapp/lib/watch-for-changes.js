var path = require('path');
const { app, net } = require('electron')
const fs = require('fs');
const { mainWindows } = require('./main-window');
const { getBaseUrlFromSettingsSlug } = require('./get-base-url-from-settings');

watchForChanges = () => {
    var editCacheParentFolder = path.join(app.getPath("documents"), "Starsky");

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

var toDoQueue = [];

replaceToSubPath = (fullFilePath, parentCurrentFullFilePathFolder) => {
    var subPath = fullFilePath.replace(parentCurrentFullFilePathFolder, "");
    subPath = subPath.replace(/\\/ig,"/");
    return subPath;
}

watchFs = (currentSession, editCacheParentFolder) => {
    var parentCurrentFullFilePathFolder = path.join(app.getPath("documents"), "Starsky", getBaseUrlFromSettingsSlug());
    console.log("parentCurrentFullFilePathFolder " + parentCurrentFullFilePathFolder)

    // Does not work on some linux systems
    fs.watch(editCacheParentFolder, {recursive: true}, (eventType, fileName) => {
        if (!fileName.endsWith(".DS_Store")) {
            console.log('watch ~', eventType, fileName);

            var fullFilePath = path.join(editCacheParentFolder, fileName);
    
            if(	fs.existsSync(fullFilePath) && fs.lstatSync(fullFilePath).isDirectory() ) return;
    
            if (fullFilePath.indexOf(parentCurrentFullFilePathFolder) === -1 ) return;
    
            console.log('fullFilePath', fullFilePath);
            console.log('subPath', replaceToSubPath(fullFilePath,parentCurrentFullFilePathFolder));
    
            toDoQueue.push(fullFilePath);
        }
    });

    setInterval(()=> {
        let uniqueToQueue = [...new Set(toDoQueue)];
        toDoQueue = [];

        if (!uniqueToQueue || uniqueToQueue.length === 0) return;

        console.log('uniqueToQueue' , uniqueToQueue);
        console.log('currentSession', currentSession);
        
        uniqueToQueue.forEach(fullFilePath => {
            if (fs.existsSync(fullFilePath)) {
                doUploadRequest(currentSession,fullFilePath,
                    replaceToSubPath(fullFilePath,parentCurrentFullFilePathFolder));
            }
        });
    },10000)
}

doUploadRequest = (currentSession, fullFilePath, toSubPath, callback) => {
    if (!currentSession) return;

    var url = toSubPath.endsWith(".xmp") ? getBaseUrlFromSettings() + "/starsky/api/upload-sidecar" : 
        getBaseUrlFromSettings() + "/starsky/api/upload";
    console.log('> run upload ' + url);

    const request = net.request({
        useSessionCookies: true,
        url, 
        session: currentSession,
        method: 'POST',
        headers: {
            "to": toSubPath,
            "content-type": "application/octet-stream",
            "Accept" :	"*/*"
        },
    });

    // Reading response from API
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
    
    // And now Upload
	fs.readFile(fullFilePath, function (err, data) {
		if(err) console.log(fullFilePath, err);
		// skip error for now
		if (err) return;
		request.write(data);
        request.end();
        request.on('finish', () => {
            console.log('--finish doUploadRequest');
            if (callback) callback();
        });
	});
}

module.exports = {
    watchForChanges
}