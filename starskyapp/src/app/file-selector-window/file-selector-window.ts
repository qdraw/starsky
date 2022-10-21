import { BrowserWindow, dialog } from "electron";

export async function fileSelectorWindow(): Promise<string[]> {
  return new Promise((resolve, reject) => {
    const newOpenedWindow = new BrowserWindow({
      height: 40,
      width: 500,
      title: "Open File",
      resizable: false,
      fullscreen: false,
      backgroundColor: "#ccc",
      webPreferences: {
        devTools: false,
        contextIsolation: true,
      },
    });

    // for windows
    newOpenedWindow.setMenu(null);

    const selected = dialog.showOpenDialog(newOpenedWindow, {
      properties: ["openFile"],
    });

    selected
      .then((data) => {
        console.log(data);

        if (data.canceled) {
          newOpenedWindow.close();
          // eslint-disable-next-line prefer-promise-reject-errors
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
