const path = require('path')
const { spawn } = require('child_process');
const extract = require('extract-zip');
const fs = require('fs');
const getOsKey = require('./os-type').getOsKey
const isPackaged = require('./os-type').isPackaged
const readline = require('readline');
const { app } = require('electron')
const isLegacyMacOS = require('./os-type').isLegacyMacOS
const os = require('os');

function getStarskyPath() {

    if (!isPackaged()) { // dev
        switch (process.platform) {
        case "darwin":
            return Promise.resolve(path.join(__dirname, "../", "../", "starsky", "osx.10.12-x64", "starsky"));
        case "win32":
            return Promise.resolve(path.join(__dirname, "../", "../", "starsky", "win7-x86", "starsky"));
        default:
            return Promise.resolve("");
        }
    }

    // prod
    var includedZipPath = path.join(process.resourcesPath, `include-starsky-${getOsKey()}.zip`);
    var targetFilePath = path.join(process.resourcesPath, "include");

    var exeFilePath = path.join(targetFilePath, "starsky")
    if (process.platform === "win32") exeFilePath = path.join(targetFilePath, "starsky.exe");

    return new Promise(function (resolve, reject) {
        fs.promises.access(exeFilePath).then((status) => {
            resolve(exeFilePath);
        }).catch(() => {
            extract(includedZipPath, { dir: targetFilePath }).then(() => {

                // make chmod +x
                if (process.platform !== "win32") fs.chmodSync(exeFilePath, 0o755);
                resolve(exeFilePath);
            }).catch((error) => {
                console.log('catch', error);
            });
        });
    });
}

function electronCacheLocation() {
    switch (process.platform) {
        case "darwin":
            // ~/Library/Application\ Support/starsky/Cache
            return path.join(os.homedir(), "Library", "Application Support", "starsky");
        case "win32":
            // C:\Users\<user>\AppData\Roaming\starsky\Cache
            return path.join(os.homedir(),  "AppData", "Roaming", "starsky");
        default:
            return path.join(os.homedir(), '.config','starsky');
        }
}

function setupChildProcess() {

    if ( isLegacyMacOS() ) {
        console.log("OS Not supported, only in remote mode");
        return;
    }

    var thumbnailTempFolder = path.join(electronCacheLocation(),"thumbnailTempFolder");
    if (!fs.existsSync(thumbnailTempFolder)) {
        fs.mkdirSync(thumbnailTempFolder)
    }

    var tempFolder = path.join(electronCacheLocation(),"tempFolder");
    if (!fs.existsSync(tempFolder)) {
        fs.mkdirSync(tempFolder)
    }

    var appSettingsPath = path.join(electronCacheLocation(),"appsettings.json");
    var databaseConnection = "Data Source="+ path.join(electronCacheLocation(),"starsky.db") ;

    var starskyChild;
    getStarskyPath().then((starskyPath) => {
      starskyChild = spawn(starskyPath, {
        cwd: path.dirname(starskyPath),
        detached: true,
        env: {
          "ASPNETCORE_URLS": "http://localhost:9609",
          "app__thumbnailTempFolder": thumbnailTempFolder,
          "app__tempFolder": tempFolder,
          "app__appSettingsPath" : appSettingsPath,
          "app__databaseConnection": databaseConnection
        }
      }, (error, stdout, stderr) => { });
    
      starskyChild.stdout.on('data', function (data) {
        console.log(data.toString());
      });
    });

    readline.emitKeypressEvents(process.stdin);

    /**
     * Needed for terminals
     * @param {bool} modes true or false
     */
    function setRawMode(modes) {
        if (!process.stdin.setRawMode) return;
        process.stdin.setRawMode(modes)
    }

    function kill() {
    setRawMode(false);
        if (!starskyChild) return;
        starskyChild.stdin.pause();
        starskyChild.kill();
    }

    setRawMode(true);

    process.stdin.on('keypress', (str, key) => {
        if (key.ctrl && key.name === 'c') {
            kill();
            console.log('===> end of starsky');
            setTimeout(() => { process.exit(0); }, 400);
        }
    });

    app.on("before-quit", function (event) {
        event.preventDefault();
        console.log('----> end');
        kill();
        setTimeout(() => { process.exit(0); }, 400);
    });
}

module.exports = {
    setupChildProcess
}
  
