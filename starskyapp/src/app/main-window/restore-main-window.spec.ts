import * as appConfig from "electron-settings";
import * as createMainWindow from "../main-window/create-main-window";
import { restoreMainWindow } from "./restore-main-window";

jest.mock("electron", () => {
  return {
    app: {
      getVersion: () => "99.99.99",
      getPath: () => "tmp",
      getLocale: () => "en",
      on: () => "en"
    },
    BrowserWindow: () => {
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
    }
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

    await restoreMainWindow();

    expect(createMainWindowSpy).toBeCalled();
    expect(createMainWindowSpy).toBeCalledWith("?f=/", 0);
  });

  it("restore one window", async () => {
    jest.spyOn(appConfig, "has").mockImplementationOnce(() => {
      return Promise.resolve(true);
    });

    jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
      return Promise.resolve({ 0: "url_get" });
    });

    const createMainWindowSpy = jest
      .spyOn(createMainWindow, "default")
      .mockImplementationOnce(() => Promise.resolve() as any);

    await restoreMainWindow();

    expect(createMainWindowSpy).toBeCalled();
    expect(createMainWindowSpy).toBeCalledWith("url_get", 0);
  });

  it("restore multiple windows", async () => {
    jest.spyOn(appConfig, "has").mockImplementationOnce(() => {
      return Promise.resolve(true);
    });

    jest.spyOn(appConfig, "get").mockImplementationOnce(() => {
      return Promise.resolve({ 0: "url_get0", 1: "url_get1" });
    });

    const createMainWindowSpy = jest
      .spyOn(createMainWindow, "default")
      .mockReset()
      .mockImplementationOnce(() => Promise.resolve() as any)
      .mockImplementationOnce(() => Promise.resolve() as any);

    await restoreMainWindow();

    expect(createMainWindowSpy).toBeCalled();
    expect(createMainWindowSpy).toHaveBeenNthCalledWith(1, "url_get0", 0);
    expect(createMainWindowSpy).toHaveBeenNthCalledWith(2, "url_get1", 20);
  });
});
