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
    BrowserWindow: () => {
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

    const result = await createSettingsWindow();
    expect(result).not.toBeUndefined();
  });
});
