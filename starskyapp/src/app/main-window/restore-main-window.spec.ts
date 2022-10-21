/* eslint-disable @typescript-eslint/ban-types */
import * as appConfig from "electron-settings";
import * as createMainWindow from "./create-main-window";
import { restoreMainWindow } from "./restore-main-window";

jest.mock("electron-settings", () => {
  return {
    get: () => "http://localhost:9609",
    set: () => "data",
    has: () => true,
    unset: () => {},
    __esModule: true,
  };
});

jest.mock("electron", () => {
  return {
    app: {
      getVersion: () => "99.99.99",
      getPath: () => "tmp",
      getLocale: () => "en",
      on: () => "en",
      __esModule: true,
    },
    // eslint-disable-next-line object-shorthand, func-names
    BrowserWindow: function (_x:object, _y: number, _w: number, _h: number, _s: boolean, _w2: object) {
      return {
        loadFile: jest.fn(),
        id: 103,
        webContents: {
          userAgent: "test",
          on: (_: string, func: Function) => {
            return func();
          }
        },
        once: (_: string, func: Function) => {
          return func();
        },
        show: jest.fn(),
        on: (_: string, func: Function) => {
          return func();
        }
      };
    },
    __esModule: true,
  };
});

describe("restore main window", () => {
  it("failed restore window", async () => {
    jest.spyOn(appConfig, "has").mockImplementationOnce(() => {
      return Promise.resolve(false);
    });

    const createMainWindowSpy = jest
      .spyOn(createMainWindow, "default")
      .mockImplementationOnce(() => Promise.resolve() as any);

    jest.spyOn(appConfig, "set").mockImplementationOnce(() => {
      return Promise.resolve();
    });

    await restoreMainWindow();

    expect(createMainWindowSpy).toHaveBeenCalled();
    expect(createMainWindowSpy).toHaveBeenCalledWith("?f=/", 0);
  });

  it("restore one window", async () => {
    jest.spyOn(appConfig, "has").mockImplementationOnce(() => {
      return Promise.resolve(true);
    });

    jest.spyOn(appConfig, "set").mockImplementationOnce(() => {
      return Promise.resolve();
    });

    jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
      return Promise.resolve({ 0: "url_get" });
    });

    const createMainWindowSpy = jest
      .spyOn(createMainWindow, "default")
      .mockImplementationOnce(() => Promise.resolve() as any);

    console.log("---!");

    await restoreMainWindow();

    expect(createMainWindowSpy).toHaveBeenCalled();
    expect(createMainWindowSpy).toHaveBeenCalledWith("url_get", 0);
  });

  it("restore multiple windows", async () => {
    jest.spyOn(appConfig, "has").mockImplementationOnce(() => {
      return Promise.resolve(true);
    });

    jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
      return Promise.resolve({ 0: "url_get0", 1: "url_get1" });
    });

    jest.spyOn(appConfig, "set").mockImplementationOnce(() => {
      return Promise.resolve();
    });

    const createMainWindowSpy = jest
      .spyOn(createMainWindow, "default")
      .mockReset()
      .mockImplementationOnce(() => Promise.resolve() as any)
      .mockImplementationOnce(() => Promise.resolve() as any);

    await restoreMainWindow();

    expect(createMainWindowSpy).toHaveBeenCalled();
    expect(createMainWindowSpy).toHaveBeenNthCalledWith(1, "url_get0", 0);
    expect(createMainWindowSpy).toHaveBeenNthCalledWith(2, "url_get1", 20);
  });
});
