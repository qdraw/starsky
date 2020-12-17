import { dialog } from "electron";
import { fileSelectorWindow } from "./file-selector-window";

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
        setMenu: jest.fn(),
        close: jest.fn()
      };
    },
    dialog: {
      showOpenDialog: () =>
        Promise.resolve({ canceled: false, filePaths: [""] })
    }
  };
});

describe("create main window", () => {
  it("create a new window", async () => {
    const result = await fileSelectorWindow();
    expect(result).not.toBeUndefined();
  });

  it("canceled", async () => {
    jest
      .spyOn(dialog, "showOpenDialog")
      .mockImplementationOnce(() =>
        Promise.resolve({ canceled: true, filePaths: [""] })
      );

    let error = undefined;
    try {
      await fileSelectorWindow();
    } catch (err) {
      error = err;
    }

    expect(error).toBe("canceled");
  });

  it("rejected by openFile", async () => {
    jest
      .spyOn(dialog, "showOpenDialog")
      .mockImplementationOnce(() => Promise.reject("reason_rejected"));

    let error = undefined;
    try {
      await fileSelectorWindow();
    } catch (err) {
      error = err;
    }

    expect(error).toBe("reason_rejected");
  });
});
