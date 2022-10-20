import * as fs from "fs";
import * as path from "path";
import { copyWithId } from "./copy-folder";

// eslint-disable-next-line @typescript-eslint/no-unsafe-member-access
exports.default = (context: {
  platform: { buildConfigurationKey: string };
}) => {
  console.log("context:");
  console.log(context);

  const p1 = path.join(__dirname, "..", "..", "runtime-starsky-mac");
  if (fs.existsSync(p1)) {
    try {
      fs.rmSync(p1, { recursive: true });
    } catch (err) {
      console.log(err);
    }
  }

  const p2 = path.join(__dirname, "..", "..", "runtime-starsky-win");
  if (fs.existsSync(p2)) {
    try {
      fs.rmSync(p2, { recursive: true });
    } catch (err) {
      console.log(err);
    }
  }

  switch (context.platform.buildConfigurationKey) {
    case "mac":
      copyWithId("osx-x64", "runtime-starsky-mac");
      break;
    case "win":
      copyWithId("win-x64", "runtime-starsky-win");
      break;
    default:
  }

  if (fs.existsSync(path.join("runtime-starsky-mac", "clientapp", "package.json"))) {
    fs.rmSync(path.join("runtime-starsky-mac", "clientapp", "package.json"));
  }
  if (fs.existsSync(path.join("runtime-starsky-mac", "clientapp", "package-lock.json"))) {
    fs.rmSync(path.join("runtime-starsky-mac", "clientapp", "package-lock.json"));
  }

  if (fs.existsSync(path.join("runtime-starsky-win", "clientapp", "package.json"))) {
    fs.rmSync(path.join("runtime-starsky-win", "clientapp", "package.json"));
  }

  if (fs.existsSync(path.join("runtime-starsky-win", "clientapp", "package-lock.json"))) {
    fs.rmSync(path.join("runtime-starsky-win", "clientapp", "package-lock.json"));
  }

  // eslint-disable-next-line @typescript-eslint/naming-convention, no-underscore-dangle, @typescript-eslint/no-explicit-any
  const _promises: readonly any[] = [];
  return Promise.all(_promises);
};
