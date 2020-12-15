import { SkipDisplayOfUpdate } from "./updates-warning-window";

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
        }
      };
    }
  };
});

describe("create main window", () => {
  describe("SkipDisplayOfUpdate", () => {
    it("SkipDisplayOfUpdate", async () => {
      await SkipDisplayOfUpdate();
    });
  });
});
