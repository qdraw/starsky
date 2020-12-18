import * as spawn from "child_process";
import { app } from "electron";
import * as fs from "fs";
import * as getPort from "get-port";
import * as readline from "readline";
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
      const spawnSpy = { stdout: { on: jest.fn() }, stderr: { on: jest.fn() } };
      jest.spyOn(spawn, "spawn").mockImplementationOnce(() => spawnSpy as any);
      jest
        .spyOn(fs, "existsSync")
        .mockImplementationOnce(() => false)
        .mockImplementationOnce(() => false);

      jest.spyOn(getPort, "makeRange").mockImplementationOnce(() => {
        return null;
      });

      const mkdirSpy = jest
        .spyOn(fs, "mkdirSync")
        .mockImplementationOnce(() => null)
        .mockImplementationOnce(() => null);

      jest
        .spyOn(readline, "emitKeypressEvents")
        .mockImplementationOnce(() => null);

      jest.spyOn(app, "on").mockImplementationOnce((event) => {
        console.log(event);

        return null;
      });
      setupChildProcess();

      expect(mkdirSpy).toBeCalled();
      expect(mkdirSpy).toBeCalledTimes(2);

      expect(spawnSpy.stdout.on).toBeCalled();
      expect(spawnSpy.stdout.on).toBeCalledWith("data", expect.anything());

      expect(spawnSpy.stderr.on).toBeCalled();
      expect(spawnSpy.stderr.on).toBeCalledWith("data", expect.anything());
    });
  });
});
