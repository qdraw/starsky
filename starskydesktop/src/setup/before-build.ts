import { spawnSync } from "child_process";
import * as fs from "fs";
import * as path from "path";
import { copyWithId } from "./copy-folder";

function runBuildIfNotExist(identifier: string) {
  const from = path.join(__dirname, "..", "..", "..", "starsky", identifier);
  if (!fs.existsSync(from)) {
    const rootPath = path.join(__dirname, "..", "..", "..", "starsky");

    console.log(`try to build for ${identifier} --  ${rootPath} -- with powershell core`);
    console.log(`> if this fails: there is a bash version avaiable in ${rootPath}`);

    const script = path.join(__dirname, "..", "..", "..", "starsky", "build.ps1");
    const spawn = spawnSync(
      "pwsh",
      [script, "-runtime", identifier, "-no-unit-tests", "-ready-to-run"],
      {
        cwd: rootPath,
        env: process.env,
        timeout: 3600000,
        encoding: "utf-8",
      }
    );
    console.log(spawn.stdout);
    console.log(spawn.stderr);
    console.log("--end build for");
  }
}

function removeOldRunTime(runtimeFolderName: string) {
  const p1 = path.join(__dirname, "..", "..", runtimeFolderName);
  if (fs.existsSync(p1)) {
    try {
      fs.rmSync(p1, { recursive: true });
    } catch (err) {
      console.log(err);
    }
  }
}

function removePackageJsons(runtimeFolderName: string) {
  if (fs.existsSync(path.join(runtimeFolderName, "clientapp", "package.json"))) {
    fs.rmSync(path.join(runtimeFolderName, "clientapp", "package.json"));
  }
  if (fs.existsSync(path.join(runtimeFolderName, "clientapp", "package-lock.json"))) {
    fs.rmSync(path.join(runtimeFolderName, "clientapp", "package-lock.json"));
  }
}

// eslint-disable-next-line @typescript-eslint/no-unsafe-member-access
exports.default = (context: { arch: string; platform: { buildConfigurationKey: string } }) => {
  console.log("context:");
  console.log(context);

  switch (context.platform.buildConfigurationKey) {
    case "mac":
      // for: osx-arm64, osx-x64
      if (context.arch === "x64" || context.arch === "arm64") {
        runBuildIfNotExist(`osx-${context.arch}`);
        removeOldRunTime(`runtime-starsky-mac-${context.arch}`);
        copyWithId(`osx-${context.arch}`, `runtime-starsky-mac-${context.arch}`);
      }
      break;
    case "win":
      runBuildIfNotExist("win-x64");
      removeOldRunTime("runtime-starsky-win-x64");
      copyWithId("win-x64", "runtime-starsky-win-x64");
      break;
    case "linux":
      runBuildIfNotExist("linux-x64");
      removeOldRunTime("runtime-starsky-linux-x64");
      copyWithId("linux-x64", "runtime-starsky-linux-x64");
      break;
    default:
  }

  removePackageJsons("runtime-starsky-mac-arm64");
  removePackageJsons("runtime-starsky-mac-x64");
  removePackageJsons("runtime-starsky-win-x64");
  removePackageJsons("runtime-starsky-linux-x64");

  // eslint-disable-next-line @typescript-eslint/naming-convention, no-underscore-dangle, @typescript-eslint/no-explicit-any
  const _promises: readonly any[] = [];
  return Promise.all(_promises);
};
