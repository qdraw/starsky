import * as spawn from "child_process";
import { app } from "electron";
import { setupChildProcess } from "./setup-child-process";

jest.mock("electron", () => {
  return {
    app: {
      getVersion: () => "99.99.99",
      getPath: () => "tmp",
      getLocale: () => "en",
      on: () => "en"
    },
    net: {
      request: () => {}
    }
  };
});

describe("setupChildProcess", () => {
  describe("setupChildProcess", () => {
    it("getting with null input", () => {
      jest
        .spyOn(spawn, "spawn")
        .mockImplementationOnce(
          () =>
            ({ stdout: { on: jest.fn() }, stderr: { on: jest.fn() } } as any)
        );

      jest.spyOn(app, "on").mockImplementationOnce((event) => {
        console.log(event);

        return null;
      });
      setupChildProcess();
      //
    });
  });
});
