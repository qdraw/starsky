/* eslint-disable @typescript-eslint/no-unused-vars */

import { BrowserWindow, dialog } from "electron";
import { fileSelectorWindow } from "./file-selector-window";

jest.mock("electron-settings", () => {
  return {
    get: () => "data",
    __esModule: true,
  };
});

jest.mock('electron', () => {
  return {
    // eslint-disable-next-line object-shorthand, func-names
    BrowserWindow: function (_:object) {
      return {
        setMenu(_2:object) {
        },
        close() {
        },
      };
    },
    dialog: {
      showOpenDialog(_4:object, _5: object) {
        return Promise.resolve({
          canceled: false,
        });
      },
    },
  };
});

describe("create main window", () => {
  it("test mock", () => {
    const result = new BrowserWindow({});
    result.setMenu(null);
    expect(result).toBeDefined();
  });

  it("test mock 2", async () => {
    const result = await dialog.showOpenDialog({} as BrowserWindow, {});
    expect(result).toBeDefined();
  });

  it("create a new window", async () => {
    const result = await fileSelectorWindow();
    expect(result).toBeDefined();
  });

  // it("canceled", async () => {
  //   jest
  //     .spyOn(dialog, "showOpenDialog")
  //     .mockImplementationOnce(() => Promise.resolve({ canceled: true, filePaths: [""] }));

  //   let error;
  //   try {
  //     await fileSelectorWindow();
  //   } catch (err) {
  //     error = err;
  //   }

  //   expect(error).toBe("canceled");
  // });

  // it("rejected by openFile", async () => {
  //   jest
  //     .spyOn(dialog, "showOpenDialog")
  //     .mockImplementationOnce(() => Promise.reject("reason_rejected"));

  //   let error;
  //   try {
  //     await fileSelectorWindow();
  //   } catch (err) {
  //     error = err;
  //   }

  //   expect(error).toBe("reason_rejected");
  // });
});
