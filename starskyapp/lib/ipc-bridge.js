const { dialog, ipcMain , net, app, BrowserWindow } = require('electron')
const appConfig = require('electron-settings');
const mainWindows = require('./main-window').mainWindows
const editFileDownload = require('./edit-file-download').editFileDownload;
const isPackaged = require('./os-type').isPackaged
const CheckForUpdatesLocalStorageName = require('./check-for-updates').CheckForUpdatesLocalStorageName

exports.ipcBridge = () => {

    // unescaped: ^https?:\/\/((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?):?([0-9]{0,5})?\/?([a-z]+)?$
    // https://www.freeformatter.com/javascript-escape.html#ad-output
    var ipRegex = new RegExp("^https?:\\\/\\\/((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(25[0-5]|2[0-4][0-9]|[01]?" + 
    "[0-9][0-9]?):?([0-9]{0,5})?\\\/?([a-z]+)?$","ig")

    // unescaped: https://stackoverflow.com/a/9284473
    var urlRegex = new RegExp("^(?:(?:https?|ftp):\\\/\\\/)(?:\\S+(?::\\S*)?@)?(?:(?!(?:10|127)(?:\\.\\d{1,3}){3})(?!(?:169\\.254|192\\.168)(?:\\." + 
    "\\d{1,3}){2})(?!172\\.(?:1[6-9]|2\\d|3[0-1])(?:\\.\\d{1,3}){2})(?:[1-9]\\d?|1\\d\\d|2[01]\\d|22[0-3])(?:\\.(?:1?\\d{1,2}|2[0-4]\\d|25[0-5])){2}" + 
    "(?:\\.(?:[1-9]\\d?|1\\d\\d|2[0-4]\\d|25[0-4]))|(?:(?:[a-z\\u00a1-\\uffff0-9]-*)*[a-z\\u00a1-\\uffff0-9]+)(?:\\.(?:[a-z\\u00a1-\\uffff0-9]-*)*[a-z" +
    "\\u00a1-\\uffff0-9]+)*(?:\\.(?:[a-z\\u00a1-\\uffff]{2,}))\\.?)(?::\\d{2,5})?(?:[\/?#]\\S*)?$","ig");

    // {
    // location, string;
    // remote:bool   
    // locationOk: bool
    // }
    ipcMain.on("settings", (event, args) => {

        var currentSettings = appConfig.get("remote_settings_" + isPackaged());
        
        if (args && args.location && !args.location.match(urlRegex) &&  !args.location.match(ipRegex) 
            && !args.location.startsWith('http://localhost:') && args.location != currentSettings.location) {
            // console.log('28', args.location);
            
            currentSettings.locationOk = false;
            currentSettings.remote = args.remote;
            event.reply('settings', currentSettings);
            return;
        }

        if (args && args.location && ( args.location.match(urlRegex) || args.location.match(ipRegex) 
            || args.location.startsWith('http://localhost:') ) 
            &&  args.location != currentSettings.location) {

            // to avoid errors
            var locationUrl = args.location.replace(/\/$/, "");

            const request = net.request({
                url: locationUrl + "/api/health",
                headers: {
                    "Accept" :	"*/*"
                }
            });
            request.on('response', (response) => {
                console.log(`HEADERS: ${JSON.stringify(response.headers)} - ${response.statusCode} - ${locationUrl + "/api/health"}`)
                var locationOk = response.statusCode == 200 || response.statusCode == 503;
                if (locationOk) {
                    currentSettings.location = locationUrl;
                    // console.log('46', currentSettings);
                    
                    appConfig.set("remote_settings_" + isPackaged(), currentSettings);
                }
                currentSettings.locationOk = locationOk

                // to avoid that the session is opened
                mainWindows.forEach(window => {
                    window.close()
                });
                
                event.reply('settings', currentSettings);
                
            });

            request.on('error',(e)=>{
                console.log(e);
                event.reply('settings', {...currentSettings,locationOk: false });
            })

            request.end()
            return;
        }

        if(args) {
            appConfig.set("remote_settings_" + isPackaged(), args);
            // revoke url to clean session
            appConfig.set("remember-url","");
            // to avoid that the session is opened
            mainWindows.forEach(window => {
                window.close()
            });
        }

        var currentSettings = appConfig.get("remote_settings_" + isPackaged());
        if (!currentSettings) currentSettings = {};
        currentSettings.apiVersion = app.getVersion().match(new RegExp("^[0-9]+\\.[0-9]+","ig"));

        event.reply('settings', currentSettings)
    });

    // default by true
    ipcMain.on("settings_update_policy", (event, args) => {
        let currentSettings = true;

        if (appConfig.has("settings_update_policy")) {
            currentSettings = appConfig.get("settings_update_policy");
        }

        if (args === false || args === true ) {
            console.log('set arg --> ', args);
            appConfig.set("settings_update_policy", args);
            // reset check date for latest version
            appConfig.delete(CheckForUpdatesLocalStorageName);

            event.reply('settings_update_policy', args)
            return
        }

        event.reply('settings_update_policy', currentSettings)
    });

    ipcMain.on("settings_default_app", (event, args) => {
        if (args && args.reset) {
            appConfig.delete("settings_default_app");
            event.reply('settings_default_app', '')
            return;
        }

        if (args && args.showOpenDialog) {
            var newOpenedWindow = new BrowserWindow();
            var selected = dialog.showOpenDialog (
                newOpenedWindow,
                { properties: ["openFile"] }
            );
            
            selected.then((data)=> {
                if (data.canceled) {
                    newOpenedWindow.close();
                    return;
                }
                appConfig.set("settings_default_app", data.filePaths[0]);
                event.reply('settings_default_app', data.filePaths[0])
                newOpenedWindow.close();
            }).catch((e)=>{
                newOpenedWindow.close();
            })
        }

        if (appConfig.has("settings_default_app")) {
            var currentSettings = appConfig.get("settings_default_app");
            event.reply('settings_default_app', currentSettings)
        }
    });

    ipcMain.on("edit", (_, args) => {
        if (!args || !args.f || !args.to) return;
        editFileDownload(args.f, args.to);
    });

}