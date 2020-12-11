"use strict";
import { copyWithId } from "./copy-folder";

exports.default = (context: {
  platform: { buildConfigurationKey: string };
}) => {
  console.log(context);

  switch (context.platform.buildConfigurationKey) {
    case "mac":
      copyWithId("osx.10.12-x64", "runtime-starsky-mac");
      break;
    case "win":
      copyWithId("win7-x64", "runtime-starsky-win");
      break;
    default:
  }

  const _promises: readonly any[] = [];
  return Promise.all(_promises);
};
