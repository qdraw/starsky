// import { dialog } from "electron";

import { BrowserWindow } from "electron";

jest.mock("electron-settings", () => {
  return {
    get: () => "data",
    __esModule: true,
  };
});

jest.mock('electron', () => {
  return {
    BrowserWindow: jest.fn(() => jest.fn().mockImplementation((_1) => {
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
        close: jest.fn(),
        __esModule: true,
      };
    })),
  };
}, { virtual: true });

describe("create main window", () => {
  it("test mock", () => {
    const result = new BrowserWindow({});
    console.log(result);

    result.setMenu(null);
    expect(result).toBeDefined();
  });

  // it("create a new window", async () => {
  //   const result = await fileSelectorWindow();
  //   expect(result).toBeDefined();
  // });

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
