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

function isLegacyMacOS() {
  return process.platform === "darwin" && (process.getSystemVersion().startsWith("10.10") || process.getSystemVersion().startsWith("10.11") ) 
}

module.exports = {
    getOsKey,
    isPackaged,
    isLegacyMacOS
}