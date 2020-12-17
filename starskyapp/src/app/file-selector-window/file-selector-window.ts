import { BrowserWindow, dialog } from "electron";

export async function fileSelectorWindow(): Promise<string[]> {
  return new Promise(async function (resolve, reject) {
    var newOpenedWindow = new BrowserWindow({
      height: 40,
      width: 500,
      title: "Open File",
      resizable: false,
      fullscreen: false,
      backgroundColor: "#ccc",
      webPreferences: {
        devTools: false,
        contextIsolation: true
      }
    });

    // for windows
    newOpenedWindow.setMenu(null);

    var selected = dialog.showOpenDialog(newOpenedWindow, {
      properties: ["openFile"]
    });

    selected
      .then((data) => {
        if (data.canceled) {
          newOpenedWindow.close();
          reject("canceled");
          return;
        }
        resolve(data.filePaths);
        newOpenedWindow.close();
      })
      .catch((e) => {
        newOpenedWindow.close();
        reject(e);
      });
  });
}
