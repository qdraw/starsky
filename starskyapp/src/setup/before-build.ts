import * as fs from "fs";
import * as path from "path";
import { copyWithId } from "./copy-folder";

// eslint-disable-next-line @typescript-eslint/no-unsafe-member-access
exports.default = (context: {
  platform: { buildConfigurationKey: string };
}) => {
  console.log(context);

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
