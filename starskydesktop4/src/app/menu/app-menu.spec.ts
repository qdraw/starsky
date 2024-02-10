/* eslint-disable @typescript-eslint/no-unsafe-assignment */
/* eslint-disable no-restricted-syntax */
/* eslint-disable @typescript-eslint/no-unsafe-member-access */
import { Menu, shell } from "electron";
import * as logger from "../logger/logger";
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

jest.mock("electron-settings", () => {
  return {
    get: () => "data",
    __esModule: true,
  };
});

describe("menu", () => {
  beforeAll(() => {
    jest.spyOn(logger, "default").mockImplementation(() => {
      return {
        info: jest.fn(),
        warn: jest.fn()
      };
    });
  });

  describe("AppMenu", () => {
    it("should inherit from template", () => {
      const buildFromTemplateSpy = jest.spyOn(Menu, "buildFromTemplate");
      const setApplicationMenuSpy = jest.spyOn(Menu, "setApplicationMenu");

      AppMenu();

      expect(buildFromTemplateSpy).toHaveBeenCalled();
      expect(setApplicationMenuSpy).toHaveBeenCalled();
    });

    it("New Main window", () => {
      const createnewWindowSpy = jest
        .spyOn(createMainWindow, "default")
        .mockImplementationOnce(() => Promise.resolve() as any);

      AppMenu();
      // eslint-disable-next-line no-restricted-syntax
      for (const key in menuStorage) {
        if (Object.prototype.hasOwnProperty.call(menuStorage, key)) {
          // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment, @typescript-eslint/no-unsafe-member-access
          const element = menuStorage[key];
          // eslint-disable-next-line @typescript-eslint/no-unsafe-member-access
          if (element.label === "File") {
            // eslint-disable-next-line no-restricted-syntax
            for (const subElement of element.submenu) {
              if (
                subElement.accelerator
                && subElement.accelerator === "CmdOrCtrl+N"
              ) {
                subElement.click();
              }
            }
          }
        }
      }
      expect(createnewWindowSpy).toHaveBeenCalled();
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
                subElement.accelerator
                && subElement.accelerator === "CmdOrCtrl+,"
              ) {
                subElement.click();
              }
            }
          }
        }
      }
      expect(createnewWindowSpy).toHaveBeenCalled();
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
                subElement.label
                && subElement.label === "Documentation website"
              ) {
                subElement.click();
              }
            }
          }
        }
      }
      expect(openExtSpy).toHaveBeenCalled();
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
      expect(openExtSpy).toHaveBeenCalled();
    });
  });
});
