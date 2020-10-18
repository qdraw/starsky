const os = require('os');
const path = require('path');

function electronCacheLocation() {
    switch (process.platform) {
        case "darwin":
            // ~/Library/Application\ Support/starsky/Cache
            return path.join(os.homedir(), "Library", "Application Support", "starsky");
        case "win32":
            // C:\Users\<user>\AppData\Roaming\starsky\Cache
            return path.join(os.homedir(),  "AppData", "Roaming", "starsky");
        default:
            return path.join(os.homedir(), '.config','starsky');
        }
}

module.exports = {
    electronCacheLocation
};