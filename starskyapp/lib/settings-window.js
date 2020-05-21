const { BrowserWindow } = require('electron')
var path = require('path');
const windowStateKeeper = require('./window-state-keeper').windowStateKeeper

const settingsWindows = new Set();
exports.settingsWindows = settingsWindows;

exports.createSettingsWindow = () => {
    const mainWindowStateKeeper = windowStateKeeper('settings');

    let newWindow = new BrowserWindow({ 
        x: mainWindowStateKeeper.x,
        y: mainWindowStateKeeper.y,
        width: 800,
        height: 400,
        show: true,
        webPreferences: {
            enableRemoteModule: false,
            partition: 'persist:main',
            contextIsolation: true,
            preload: path.join(__dirname, "remote-settings-preload.js") // use a preload script
        }
    });

    mainWindowStateKeeper.track(newWindow);

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
