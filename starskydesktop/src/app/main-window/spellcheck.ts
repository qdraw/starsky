import { BrowserWindow, Menu, MenuItem } from "electron";

export function spellCheck(newWindow: BrowserWindow) {
  // Spellcheck
  const { session } = newWindow.webContents;
  session.setSpellCheckerLanguages(["nl-NL", "en-GB"]);

  newWindow.webContents.on("context-menu", (_e, params) => {
    if (params.dictionarySuggestions && params.dictionarySuggestions.length) {
      const objMenu = new Menu();
      const objMenuHead = new MenuItem({
        label: "Corrections",
        enabled: false
      });
      objMenu.append(objMenuHead);
      const objMenuSep = new MenuItem({
        type: "separator"
      });
      objMenu.append(objMenuSep);
      // eslint-disable-next-line array-callback-return
      params.dictionarySuggestions.map((strSuggestion) => {
        const objMenuItem = new MenuItem({
          click(_this, objWindow) {
            // eslint-disable-next-line @typescript-eslint/no-floating-promises
            objWindow.webContents.insertText(strSuggestion);
            // eslint-disable-next-line @typescript-eslint/no-unsafe-argument
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
}
