const appConfig = require('electron-settings');

exports.getBaseUrlFromSettings = () => {
    var defaultUrl = "http://localhost:9609";
    var currentSettings = appConfig.get("settings");
    if (!currentSettings || !currentSettings.remote) return defaultUrl;
    return currentSettings.location;
}