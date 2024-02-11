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

export const BeforeBuild = (platform: string, arch: string, to: string) => {
  console.log("context:");
  console.log(platform);
  console.log(arch);

  switch (platform) {
    case "darwin":
      // for: osx-arm64, osx-x64
      if (arch === "x64" || arch === "arm64") {
        runBuildIfNotExist(`osx-${arch}`);
        // removeOldRunTime(`runtime-starsky-mac-${arch}`);
        // const toPath = path.join(to, `runtime-starsky-mac-${arch}`);
        console.log(`to Path: ${to}`);
        copyWithId(`osx-${arch}`, to);
      }
      break;
    case "win": // need check
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
      console.log(`not supported ${platform}`);
      break;
  }

  removePackageJsons("runtime-starsky-mac-arm64");
  removePackageJsons("runtime-starsky-mac-x64");
  removePackageJsons("runtime-starsky-win-x64");
  removePackageJsons("runtime-starsky-linux-x64");

  const packageJson = path.join(to, "package.json");
  console.log(packageJson);

  const fd = fs.openSync(packageJson, 'a');

  fs.closeSync(fd);
  
};
