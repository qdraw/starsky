import * as fs from "fs";

export async function CheckFileExist(filePathOnDisk: string): Promise<boolean> {
  try {
    await fs.promises.access(filePathOnDisk);
    return true;
  } catch (error) {
    return false;
  }
}
