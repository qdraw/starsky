import fs from "fs";
import * as path from "path";
import * as process from "process";
import { ExecuteCodesignCommand, ExecuteXattrCommand, SpawnCleanMacOs } from "./spawn-clean-mac-os";

describe("SpawnCleanMacOs function", () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it("should resolve true when processPlatform is not darwin", async () => {
    const result = await SpawnCleanMacOs("appStarskyPath", "notDarwinPlatform");

    expect(result).toBe(true);
  });

  it("should call executeXattrCommand and executeCodesignCommand when processPlatform is darwin", async () => {
    if (process.platform !== "darwin") {
      return;
    }
    const exampleApp = path.join(process.cwd(), "src", "shared", "__test", "starsky");

    console.log(exampleApp);

    const result = await SpawnCleanMacOs(exampleApp, "darwin");

    expect(result).toBeTruthy();
  });

  it("mock out ExecuteCodesignCommand happy flow", async () => {
    if (process.platform !== "darwin" && process.platform !== "linux") {
      return;
    }

    const beforeCwd = process.cwd();
    const testDir = path.join(process.cwd(), "src", "shared", "__test");
    const starskyMockApp = path.join(process.cwd(), "src", "shared", "__test", "starsky");

    process.chdir(testDir);

    fs.chmodSync(starskyMockApp, "755");

    const result = await ExecuteCodesignCommand("./starsky", "./starsky");

    expect(result).toBeUndefined();

    process.chdir(beforeCwd);
  });

  it("mock out ExecuteXattrCommand happy flow", async () => {
    if (process.platform !== "darwin" && process.platform !== "linux") {
      return;
    }

    const beforeCwd = process.cwd();
    const testDir = path.join(process.cwd(), "src", "shared", "__test");
    const starskyMockApp = path.join(process.cwd(), "src", "shared", "__test", "starsky");

    process.chdir(testDir);

    fs.chmodSync(starskyMockApp, "755");

    const result = await ExecuteXattrCommand("./starsky", "./starsky");

    expect(result).toBeUndefined();

    process.chdir(beforeCwd);
  });

  it("mock out ExecuteXattrCommand error", async () => {
    if (process.platform !== "darwin" && process.platform !== "linux") {
      return;
    }

    const beforeCwd = process.cwd();
    const testDir = path.join(process.cwd(), "src", "shared", "__test");
    const starskyMockApp = path.join(process.cwd(), "src", "shared", "__test", "mock-error");

    process.chdir(testDir);

    fs.chmodSync(starskyMockApp, "755");

    await expect(ExecuteXattrCommand("./starsky", "./mock-error")).rejects.toThrow(
      new Error("xattr command exited with code 1 and signal null")
    );

    process.chdir(beforeCwd);
  });
});
