const { BrowserWindow, Menu, MenuItem } = require('electron')
const windowStateKeeper = require('./window-state-keeper').windowStateKeeper
var path = require('path');
const appConfig = require('electron-settings');
const handleExitKeyPress = require('./handle-edit-keypress').handleExitKeyPress

const mainWindows = new Set();
exports.mainWindows = mainWindows;

exports.createMainWindow = () => {
  const mainWindowStateKeeper = windowStateKeeper('main');

  let x = mainWindowStateKeeper.x;
  let y = mainWindowStateKeeper.y;

  const currentWindow = BrowserWindow.getFocusedWindow();

  if (currentWindow) {
    const [currentWindowX, currentWindowY] = currentWindow.getPosition();
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

  // set Remember url
  var rememberUrl = "";
  if (appConfig && appConfig.has("remember-url")) {
    rememberUrl = appConfig.get("remember-url");
  }
  console.log('rememberUrl', rememberUrl);
  newWindow.loadFile('index.html', { query: {"remember-url" : rememberUrl}});
  newWindow.on('close',()=>{
    var url = new URL(newWindow.webContents.getURL()).search
    console.log(url);
    appConfig.set("remember-url",encodeURI(url));
  })
  // end remember url

  newWindow.webContents.session.webRequest.onHeadersReceived((res, callback) => {

    var currentSettings = appConfig.get("settings");
    var localhost = "http://localhost:9609 "; // with space on end

    whitelistDomain = localhost;
    if (currentSettings && currentSettings.location) {
      whitelistDomain = !currentSettings.location ? localhost : localhost + new URL(currentSettings.location).origin;
    }

    // When change also check if CSPMiddleware needs to be updated
    var csp = "default-src 'none'; img-src 'self' file://* https://www.openstreetmap.org https://tile.openstreetmap.org https://*.tile.openstreetmap.org "
     + whitelistDomain + "; " +      "style-src file://* unsafe-inline https://www.openstreetmap.org " + whitelistDomain
      + "; script-src 'self' file://* https://az416426.vo.msecnd.net; " +
      "connect-src 'self' https://dc.services.visualstudio.com " + whitelistDomain + "; " +
      "font-src " + whitelistDomain + "; media-src " + whitelistDomain + ";";

    if (!res.url.startsWith('devtools://')) {
      res.responseHeaders["Content-Security-Policy"] = csp;
    }

    callback({ cancel: false, responseHeaders: res.responseHeaders });
  });

  newWindow.webContents.on("before-input-event", (_, input) => { 
    if (input.type === 'keyDown' && input.key === 'e') handleExitKeyPress(newWindow);
  });

  // Spellcheck
  const session = newWindow.webContents.session;
  session.setSpellCheckerLanguages(['nl-NL', 'en-GB']);

  newWindow.webContents.on('context-menu', (_e, params) => {

    if (params.dictionarySuggestions && params.dictionarySuggestions.length) {

      const objMenu = new Menu();
      const objMenuHead = new MenuItem({
        label: 'Corrections',
        enabled: false
      });
      objMenu.append(objMenuHead);
      const objMenuSep = new MenuItem({
        type: 'separator'
      });
      objMenu.append(objMenuSep);
      params.dictionarySuggestions.map(strSuggestion => {

        const objMenuItem = new MenuItem({
          click(_this, objWindow) {

            objWindow.webContents.insertText(strSuggestion);
            objMenu.closePopup(this);

          },
          label: strSuggestion
        });
        objMenu.append(objMenuItem);

      });
      objMenu.popup({
        window: newWindow,
        x: params.x,
        y: params.y
      });

    }

  });
  // end spell check

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