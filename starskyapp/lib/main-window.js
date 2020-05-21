const { BrowserWindow, session } = require('electron')
const windowStateKeeper = require('./window-state-keeper').windowStateKeeper
var path = require('path');
const appConfig = require('electron-settings');

const mainWindows = new Set();
exports.mainWindows = mainWindows;

exports.createMainWindow = () => {
  const mainWindowStateKeeper = windowStateKeeper('main');

  let x = mainWindowStateKeeper.x;
  let y = mainWindowStateKeeper.y;

  const currentWindow = BrowserWindow.getFocusedWindow();

  if (currentWindow) {
    const [ currentWindowX, currentWindowY ] = currentWindow.getPosition();
    x = currentWindowX + 10;
    y = currentWindowY + 10;
  }

  let newWindow = new BrowserWindow({ 
      x,
      y,
      width: mainWindowStateKeeper.width,
      height: mainWindowStateKeeper.height,
      show: false,
      webPreferences: {
      enableRemoteModule: false,
      partition: 'persist:main',
      contextIsolation: true,
      preload: path.join(__dirname, "remote-settings-preload.js") // use a preload script
    }
  });



  mainWindowStateKeeper.track(newWindow);

  newWindow.loadFile('index.html');
 
  
  newWindow.webContents.session.webRequest.onHeadersReceived((res, callback) => {

    var currentSettings = appConfig.get("settings");
    var localhost = "http://localhost:9609 "; // with space
    var whitelistDomain = !currentSettings.location ? localhost : localhost + new URL(currentSettings.location).origin;
    
    // When change also check if CSPMiddleware needs to be updated
    var csp = "default-src 'none'; img-src file://* https://*.tile.openstreetmap.org " + whitelistDomain + "; " + 
    "style-src file://* unsafe-inline "+ whitelistDomain + "; script-src 'self' file://* https://az416426.vo.msecnd.net; "+ 
    "connect-src 'self' https://dc.services.visualstudio.com "+ whitelistDomain +"; " +
    "font-src " + whitelistDomain + ";";

    if (!res.url.startsWith('devtools://')) {
      res.responseHeaders["Content-Security-Policy"] = csp;
    }

    callback({cancel: false, responseHeaders: res.responseHeaders});
  });

  newWindow.once('ready-to-show', () => {
    newWindow.show();
  });

  newWindow.on('closed', () => {
    mainWindows.delete(newWindow);
    newWindow = null;
  });

  mainWindows.add(newWindow);
  return newWindow;
};