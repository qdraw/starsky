const { ipcMain } = require('electron')

exports.ipcBridge = () => {
    ipcMain.on("toMain", (event, args) => {
        console.log("toMain");

        console.log(event);
        
    });
}