const { BrowserWindow } = require('electron')
const windowStateKeeper = require('./window-state-keeper').windowStateKeeper

const mainWindows = new Set();
exports.mainWindows = mainWindows;

exports.createWindow = () => {
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
    }
  });

  mainWindowStateKeeper.track(newWindow);

  newWindow.loadFile('index.html');

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