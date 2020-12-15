import * as windowStateKeeper from "../window-state-keeper/window-state-keeper";
import createMainWindow from "./create-main-window";
import * as getNewFocusedWindow from "./get-new-focused-window";
import * as onHeaderReceived from "./on-headers-received";
import * as saveRememberUrl from "./save-remember-url";
import * as spellCheck from "./spellcheck";
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
        webContents: {
          userAgent: "test",
          on: (_: string, func: Function) => {
            return func();
          },
          getURL: () => "https://test.com/?f="
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

describe("create main window", () => {
  it("create a new window", async () => {
    jest
      .spyOn(windowStateKeeper, "windowStateKeeper")
      .mockImplementationOnce(() =>
        Promise.resolve({
          x: 0,
          y: 0,
          width: 1,
          height: 1,
          isMaximized: false,
          track: jest.fn()
        })
      );
    jest
      .spyOn(getNewFocusedWindow, "getNewFocusedWindow")
      .mockImplementationOnce(() => ({ x: 1, y: 1 }));
    jest
      .spyOn(onHeaderReceived, "onHeaderReceived")
      .mockImplementationOnce(() => null);
    jest.spyOn(spellCheck, "spellCheck").mockImplementationOnce(() => null);
    jest
      .spyOn(saveRememberUrl, "saveRememberUrl")
      .mockImplementationOnce(() => null);

    const result = await createMainWindow("////");
    expect(result).not.toBeUndefined();
  });
});
