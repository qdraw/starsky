const { app, net, BrowserWindow } = require('electron')
var path = require('path');
const windowStateKeeper = require('./window-state-keeper').windowStateKeeper
const appConfig = require('electron-settings');
const {getBaseUrlFromSettings } = require('./get-base-url-from-settings')

const checkForUpdatesWindows = new Set();
exports.checkForUpdatesWindows = checkForUpdatesWindows;

const CheckForUpdatesLocalStorageName = "HealthCheckForUpdates";
exports.CheckForUpdatesLocalStorageName = CheckForUpdatesLocalStorageName;

/**
 * Return difference in Minutes
 * @param date a Javascript Datetime stamp (unix*1000)
 * @param now Javascript now
 */
const DifferenceInDate = (date, now = new Date().valueOf()) => {
    return (now - date) / 60000;
};

/**
 * Skip display of message
 */
function SkipDisplayOfUpdate() {
    const localStorageItem = appConfig.get(CheckForUpdatesLocalStorageName);
    if (!localStorageItem) return false;
  
    var getItem = parseInt(localStorageItem);
    if (isNaN(getItem)) return false;
    return DifferenceInDate(getItem) < 5760; // 4 days
}

exports.createCheckForUpdatesWindow = () => {
    const mainWindowStateKeeper = windowStateKeeper('settings');

    // skip if settings are disabled
    if (appConfig.has("settings_update_policy")) {
        settings_update_policy = appConfig.get("settings_update_policy");
        if (!settings_update_policy) return;
    }

    if (SkipDisplayOfUpdate()) {
        console.log('-> skipped for 4 days');
        return;
    }

    setTimeout(()=>{
        doRequest(()=>{
            let newWindow = new BrowserWindow({ 
                x: mainWindowStateKeeper.x,
                y: mainWindowStateKeeper.y,
                width: 350,
                height: 300,
                show: true,
                resizable: true,
                webPreferences: {
                    enableRemoteModule: false,
                    partition: 'persist:main',
                    contextIsolation: true
                }
            });
            
            // hides the menu for windows
            newWindow.setMenu(null);
        
            mainWindowStateKeeper.track(newWindow);
        
            newWindow.loadFile('pages/check-for-updates-new-version.html');
        
            newWindow.once('ready-to-show', () => {
                newWindow.show();
            });

            newWindow.on('closed', () => {
                checkForUpdatesWindows.delete(newWindow);
                appConfig.set(CheckForUpdatesLocalStorageName, Date.now().toString())
                newWindow = null;
            });
        
            checkForUpdatesWindows.add(newWindow);
        })
        
    },5000)
};


function doRequest(callback) {
    var electronVersion = app.getVersion()
    const request = net.request({
        url: getBaseUrlFromSettings() +
         "/api/health/check-for-updates?currentVersion=" + electronVersion, 
        headers: {
            "Accept" :	"*/*"
        }
    });

    let body = '';
    request.on('response', (response) => {
        if (response.statusCode !== 202) console.log(`HEADERS: ${JSON.stringify(response.headers)}`)
        if (response.statusCode !== 202) return;

        response.on('data', (chunk) => {
            body += chunk.toString()
        });
        response.on('end', () => {
            // console.log(`BODY: ${body}`)
            try {
                callback(JSON.parse(body))
            } catch (error) {
                callback()
            }
        })
    });

    request.on('error',(e)=>{
        console.log(e);
    })

    request.end()
    return;
}