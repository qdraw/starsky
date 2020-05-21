const { ipcMain , net } = require('electron')
const appConfig = require('electron-settings');
const mainWindows = require('./main-window').mainWindows

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

        var currentSettings = appConfig.get("settings");


        if (args && args.location && !args.location.match(urlRegex) &&  !args.location.match(ipRegex) && args.location != currentSettings.location) {
            console.log(args.location);
            
            currentSettings.locationOk = false;
            event.reply('settings', currentSettings);
            return;
        }

        if (args && args.location && ( args.location.match(urlRegex) || args.location.match(ipRegex) ) &&  args.location != currentSettings.location) {

            // to avoid errors
            var locationUrl = args.location.replace(/\/$/, "");

            const request = net.request(locationUrl + "/api/health");
            request.on('response', (response) => {
                console.log(`HEADERS: ${JSON.stringify(response.headers)}`)
                var locationOk = response.statusCode == 200 || response.statusCode == 503;
                if (locationOk) {
                    currentSettings.location = locationUrl;
                    console.log(currentSettings);
                    
                    appConfig.set("settings", currentSettings);
                }
                currentSettings.locationOk = locationOk

                // to avoid that the session is opened
                mainWindows.forEach(window => {
                    window.close()
                });
                
                event.reply('settings', currentSettings);
                
            });

            request.end()
            return;
        }

        if(args) {
            appConfig.set("settings", args);

            // to avoid that the session is opened
            mainWindows.forEach(window => {
                window.close()
            });
        }

        event.reply('settings', appConfig.get("settings"))
    });
}