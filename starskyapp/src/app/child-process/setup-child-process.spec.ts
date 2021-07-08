import * as spawn from "child_process";
import { app } from "electron";
import * as fs from "fs";
import * as getPort from "get-port";
import * as readline from "readline";
import logger from "../logger/logger";
import { setupChildProcess } from "./setup-child-process";

jest.mock("electron", () => {
  return {
    app: {
      getVersion: () => "99.99.99",
      getPath: () => "tmp",
      getLocale: () => "en",
      on: () => "en",
      getName: () => "test"
    },
    net: {
      request: () => {}
    }
  };
});

describe("setupChildProcess", () => {
  beforeEach(() => {});

  describe("setupChildProcess", () => {
    it("getting with null input", async () => {
      const spawnSpy = { stdout: { on: jest.fn() }, stderr: { on: jest.fn() } };
      jest.spyOn(spawn, "spawn").mockImplementationOnce(() => spawnSpy as any);
      jest
        .spyOn(fs, "existsSync")
        .mockImplementationOnce(() => false)
        .mockImplementationOnce(() => false);

      jest.spyOn(getPort, "makeRange").mockImplementationOnce(() => {
        return Promise.resolve(0) as any;
      });

      const mkdirSpy = jest
        .spyOn(fs, "mkdirSync")
        .mockImplementationOnce(() => null)
        .mockImplementationOnce(() => null);

      jest
        .spyOn(readline, "emitKeypressEvents")
        .mockImplementationOnce(() => null);

      jest.spyOn(app, "on").mockImplementationOnce((event) => {
        logger.info(event);
        return null;
      });
      await setupChildProcess();

      expect(mkdirSpy).toBeCalled();
      expect(mkdirSpy).toBeCalledTimes(2);

      expect(spawnSpy.stdout.on).toBeCalled();
      expect(spawnSpy.stdout.on).toBeCalledWith("data", expect.anything());

      expect(spawnSpy.stderr.on).toBeCalled();
      expect(spawnSpy.stderr.on).toBeCalledWith("data", expect.anything());
    });
  });
});
