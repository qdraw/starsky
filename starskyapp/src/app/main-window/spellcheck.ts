import { BrowserWindow, Menu, MenuItem } from "electron";

export function spellCheck(newWindow: BrowserWindow) {
  // Spellcheck
  const session = newWindow.webContents.session;
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
      params.dictionarySuggestions.map((strSuggestion) => {
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
}
