// import * as windowStateKeeper from "../window-state-keeper/window-state-keeper";
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

describe("main", () => {
  it("create a new window", async () => {
    require("./main");
  });
});
