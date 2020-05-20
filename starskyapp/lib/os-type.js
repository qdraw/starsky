const { app } = require('electron')

function isPackaged() {
    return !!app.isPackaged;
}
  
function getOsKey() {
    // osKey is used in the build script
    switch (process.platform) {
        case "darwin":
          return "mac"
        case "win32":
          return "win"
        default:
          return "";
    }
}

module.exports = {
    getOsKey,
    isPackaged
}