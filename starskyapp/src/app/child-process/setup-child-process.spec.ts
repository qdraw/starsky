import * as spawn from "child_process";
import { app } from "electron";
import * as fs from "fs";
import * as readline from "readline";
import * as GetPortProxy from "../get-free-port/get-free-port";
import logger from "../logger/logger";
import { setupChildProcess } from "./setup-child-process";

jest.mock('child_process', () => {
  return {
    spawn: () => {},
    __esModule: true,
  };
});

jest.mock('fs', () => {
  return {
    existsSync: () => {},
    mkdirSync: () => {},
    stat: () => {},
    __esModule: true,
  };
});

jest.mock('readline', () => {
  return {
    emitKeypressEvents: () => {},
    __esModule: true,
  };
});

jest.mock("electron", () => {
  return {
    app: {
      getVersion: () => "99.99.99",
      getPath: () => "tmp",
      getLocale: () => "en",
      on: () => "en",
      getName: () => "test",
    },
    net: {
      request: () => {},
    },
    __esModule: true,
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

      jest.spyOn(GetPortProxy, "GetFreePort").mockImplementationOnce(() => {
        return Promise.resolve(0);
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

      expect(mkdirSpy).toHaveBeenCalled();
      expect(mkdirSpy).toHaveBeenCalledTimes(2);

      expect(spawnSpy.stdout.on).toHaveBeenCalled();
      expect(spawnSpy.stdout.on).toHaveBeenCalledWith("data", expect.anything());

      expect(spawnSpy.stderr.on).toHaveBeenCalled();
      expect(spawnSpy.stderr.on).toHaveBeenCalledWith("data", expect.anything());
    });
  });
});
