import { Menu, shell } from "electron";
import * as createMainWindow from "../main-window/create-main-window";
import * as createSettingsWindow from "../settings-window/create-settings-window";
import AppMenu from "./app-menu";

let menuStorage: any;
jest.mock("electron", () => {
  return {
    app: {
      getVersion: () => "99.99.99",
      getPath: () => "tmp",
      getLocale: () => "en",
      on: () => "en"
    },
    Menu: {
      buildFromTemplate: (input: any) => {
        return input;
      },
      setApplicationMenu: (value: any) => {
        menuStorage = value;
      }
    },
    shell: {
      openExternal: jest.fn()
    }
  };
});

describe("menu", () => {
  describe("AppMenu", () => {
    it("it should inherit from template", () => {
      const buildFromTemplateSpy = jest.spyOn(Menu, "buildFromTemplate");
      const setApplicationMenuSpy = jest.spyOn(Menu, "setApplicationMenu");

      AppMenu();

      expect(buildFromTemplateSpy).toBeCalled();
      expect(setApplicationMenuSpy).toBeCalled();
    });

    it("New Main window", () => {
      const createnewWindowSpy = jest
        .spyOn(createMainWindow, "default")
        .mockImplementationOnce(() => Promise.resolve() as any);

      AppMenu();
      for (const key in menuStorage) {
        if (Object.prototype.hasOwnProperty.call(menuStorage, key)) {
          const element = menuStorage[key];
          if (element.label === "File") {
            for (const subElement of element.submenu) {
              if (
                subElement.accelerator &&
                subElement.accelerator === "CmdOrCtrl+N"
              ) {
                subElement.click();
              }
            }
          }
        }
      }
      expect(createnewWindowSpy).toBeCalled();
    });

    it("New settings window", () => {
      const createnewWindowSpy = jest
        .spyOn(createSettingsWindow, "createSettingsWindow")
        .mockImplementationOnce(() => Promise.resolve() as any);

      AppMenu();
      for (const key in menuStorage) {
        if (Object.prototype.hasOwnProperty.call(menuStorage, key)) {
          const element = menuStorage[key];
          if (element.label === "Settings") {
            for (const subElement of element.submenu) {
              if (
                subElement.accelerator &&
                subElement.accelerator === "CmdOrCtrl+,"
              ) {
                subElement.click();
              }
            }
          }
        }
      }
      expect(createnewWindowSpy).toBeCalled();
    });

    it("Help docs site", () => {
      const openExtSpy = jest
        .spyOn(shell, "openExternal")
        .mockImplementationOnce(() => Promise.resolve());

      AppMenu();
      for (const key in menuStorage) {
        if (Object.prototype.hasOwnProperty.call(menuStorage, key)) {
          const element = menuStorage[key];
          if (element.role === "help") {
            for (const subElement of element.submenu) {
              if (
                subElement.label &&
                subElement.label === "Documentation website"
              ) {
                subElement.click();
              }
            }
          }
        }
      }
      expect(openExtSpy).toBeCalled();
    });

    it("Help Release overview", () => {
      const openExtSpy = jest
        .spyOn(shell, "openExternal")
        .mockImplementationOnce(() => Promise.resolve());

      AppMenu();
      for (const key in menuStorage) {
        if (Object.prototype.hasOwnProperty.call(menuStorage, key)) {
          const element = menuStorage[key];
          if (element.role === "help") {
            for (const subElement of element.submenu) {
              if (subElement.label && subElement.label === "Release overview") {
                subElement.click();
              }
            }
          }
        }
      }
      expect(openExtSpy).toBeCalled();
    });
  });
});
