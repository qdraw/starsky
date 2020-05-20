const { BrowserWindow } = require('electron')
var path = require('path');

const settingsWindows = new Set();
exports.settingsWindows = settingsWindows;

exports.createSettingsWindow = () => {

    let newWindow = new BrowserWindow({ 
        width: 300,
        height: 400,
        show: true,
        webPreferences: {
            enableRemoteModule: false,
            partition: 'persist:main',
            contextIsolation: true,
            preload: path.join(__dirname, "remote-preload.js") // use a preload script
        }
    });

    newWindow.loadFile('settings.html');

    newWindow.once('ready-to-show', () => {
        newWindow.show();
    });

    newWindow.on('closed', () => {
        settingsWindows.delete(newWindow);
        newWindow = null;
    });

    settingsWindows.add(newWindow);
    return newWindow;
};
