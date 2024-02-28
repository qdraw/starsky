/* eslint-disable @typescript-eslint/ban-types */
/* eslint-disable @typescript-eslint/no-unsafe-return */
import * as windowStateKeeper from "../window-state-keeper/window-state-keeper";
import createMainWindow from "./create-main-window";
import * as getNewFocusedWindow from "./get-new-focused-window";
import * as saveRememberUrl from "./save-remember-url";
import * as spellCheck from "./spellcheck";

jest.mock("electron", () => {
  return {
    app: {
      getVersion: () => "99.99.99",
      getPath: () => "tmp",
      getLocale: () => "en",
      on: () => "en",
    },
    // eslint-disable-next-line object-shorthand, func-names, @typescript-eslint/no-unused-vars
    BrowserWindow: function (
      // eslint-disable-next-line @typescript-eslint/no-unused-vars
      _x: object,
      // eslint-disable-next-line @typescript-eslint/no-unused-vars
      _y: number,
      // eslint-disable-next-line @typescript-eslint/no-unused-vars
      _w: number,
      // eslint-disable-next-line @typescript-eslint/no-unused-vars
      _h: number,
      // eslint-disable-next-line @typescript-eslint/no-unused-vars
      _s: boolean,
      // eslint-disable-next-line @typescript-eslint/no-unused-vars
      _w2: object
    ) {
      return {
        id: 99,
        loadFile: jest.fn(),
        webContents: {
          userAgent: "test",
          on: (_: string, func: Function) => {
            return func();
          },
          getURL: () => "https://test.com/?f=",
          setWindowOpenHandler: jest.fn(),
        },
        once: (_: string, func: Function) => {
          return func();
        },
        show: jest.fn(),
        on: (_: string, func: Function) => {
          return func();
        },
      };
    },
    __esModule: true,
  };
});

jest.mock("electron-settings", () => {
  return {
    get: () => "data",
    __esModule: true,
  };
});

describe("create main window", () => {
  it("create a new window (main)", async () => {
    jest.spyOn(windowStateKeeper, "windowStateKeeper").mockImplementationOnce(() => Promise.resolve({
      x: 0,
      y: 0,
      width: 1,
      height: 1,
      isMaximized: false,
      track: jest.fn(),
    }));
    jest
      .spyOn(getNewFocusedWindow, "getNewFocusedWindow")
      .mockImplementationOnce(() => ({ x: 1, y: 1 }));

    jest.spyOn(spellCheck, "spellCheck").mockImplementationOnce(() => null);

    jest.spyOn(saveRememberUrl, "removeRememberUrl").mockImplementationOnce(() => null);

    jest
      .spyOn(saveRememberUrl, "saveRememberUrl")
      .mockImplementationOnce(() => null)
      .mockImplementationOnce(() => null);

    const result = await createMainWindow("////");
    expect(result).toBeDefined();
  });
});
