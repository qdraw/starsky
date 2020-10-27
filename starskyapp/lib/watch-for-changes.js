var path = require('path');
const { app, net } = require('electron')
const fs = require('fs');
const { mainWindows } = require('./main-window');

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

replaceToSubPath = (fullFilePath,parentCurrentFullFilePathFolder) => {
    var subPath = fullFilePath.replace(parentCurrentFullFilePathFolder, "");
    subPath = subPath.replace(/\\/ig,"/");
    return subPath;
}

watchFs = (currentSession, editCacheParentFolder) => {
    // Does not work on some linux systems
    fs.watch(editCacheParentFolder, {recursive: true}, (eventType, fileName) => {
        console.log('watch', eventType, fileName);

        var fullFilePath = path.join(editCacheParentFolder, fileName);
        var parentCurrentFullFilePathFolder = path.join(app.getPath("documents"), "Starsky", getBaseUrlFromSettingsSlug());

		if(	fs.existsSync(fullFilePath) && fs.lstatSync(fullFilePath).isDirectory() ) return;

		console.log("parentCurrentFullFilePathFolder " + parentCurrentFullFilePathFolder)
        if (fullFilePath.indexOf(parentCurrentFullFilePathFolder) === -1 ) return;


		
        console.log('fullFilePath', fullFilePath);
        console.log('subPath', replaceToSubPath(fullFilePath,parentCurrentFullFilePathFolder));


        toDoQueue.push(fullFilePath);
    });

    setInterval(()=> {
        let uniqueToQueue = [...new Set(toDoQueue)];


        // console.log('uniqueToQueue' , uniqueToQueue);
        // todo remove from list
        
        // doUploadRequest(currentSession,fullFilePath,subPath);

    },10000)
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