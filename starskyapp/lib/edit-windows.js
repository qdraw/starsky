
const {  BrowserWindow } = require('electron')

const windowStateKeeper = require('./window-state-keeper').windowStateKeeper
var path = require('path');

const editWindows = new Set();
exports.editWindows = editWindows;

function createNewEditWindow(data, filePath) {

    const mainWindowStateKeeper = windowStateKeeper('main')
    let editWindow = new BrowserWindow({
        x: mainWindowStateKeeper.x,
        y: mainWindowStateKeeper.y,
        width: 400,
        height: 200,
        webPreferences: {
            enableRemoteModule: false,
            contextIsolation: true,
            preload: path.join(__dirname, "remote-settings-preload.js") // use a preload script
        }
    });

    editWindow.on('closed', () => {
        editWindows.delete(editWindow);
        editWindow = null;
        });
    editWindows.add(editWindow);

    if (data.isError && data.error) {
        editWindow.loadFile('pages/edit-error.html', { query: {"f" : filePath, "error": data.error}});
        return;
    }
    if (data.pageType !== "DetailView" || data.isReadOnly) {
        console.log("Sorry, your not allowed or able to do this");
        editWindow.loadFile('pages/edit-not-allowed.html', { query: {"f" : filePath}});
        return;
    }

    editWindow.loadFile('pages/edit-save-as.html', { query: {"f" : filePath}});
}

exports.createNewEditWindow = createNewEditWindow;