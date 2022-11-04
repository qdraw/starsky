/* eslint-disable @typescript-eslint/ban-types */
import * as windowStateKeeper from "../window-state-keeper/window-state-keeper";
import { createSettingsWindow } from "./create-settings-window";

jest.mock("electron", () => {
  return {
    app: {
      getVersion: () => "99.99.99",
      getPath: () => "tmp",
      getLocale: () => "en",
      on: () => "en"
    },
    // eslint-disable-next-line object-shorthand, func-names, @typescript-eslint/no-unused-vars
    BrowserWindow: function (_x:object, _y: number, _w: number, _h: number, _s: boolean, _w2: object) {
      return {
        loadFile: jest.fn(),
        once: (_: string, func: Function) => {
          return func();
        },
        show: jest.fn(),
        on: (_: string, func: Function) => {
          return func();
        },
        setMenu: jest.fn()
      };
    }
  };
});

describe("create settings window", () => {
  it("create a new window (settings)", async () => {
    jest
      .spyOn(windowStateKeeper, "windowStateKeeper")
      .mockImplementationOnce(() => Promise.resolve({
        x: 0,
        y: 0,
        width: 1,
        height: 1,
        isMaximized: false,
        track: jest.fn()
      }));

    const result = await createSettingsWindow();
    expect(result).toBeDefined();
  });
});
