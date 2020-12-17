import * as fs from "fs";
import { GetParentDiskPath } from "./get-parent-disk-path";

export async function createParentFolders(parentSubDir: string = null) {
  const parentFullFilePath = await GetParentDiskPath(parentSubDir);
  console.log(parentFullFilePath);

  await fs.promises.mkdir(parentFullFilePath, {
    recursive: true
  });
}
