import * as path from "path";
import * as GetBaseUrlFromSettings from "../config/get-base-url-from-settings";
import { IlocationUrlSettings } from "../config/IlocationUrlSettings";
import * as downloadNetRequest from "../net-request/download-net-request";
import { downloadXmpFile } from "./download-xmp-file";
import * as GetParentDiskPath from "./get-parent-disk-path";

jest.mock("electron-settings", () => {
  return {
    get: () => "data",
    __esModule: true,
  };
});

describe("downloadXmpFile", () => {
  it("should download", async () => {
    jest.spyOn(GetParentDiskPath, "GetParentDiskPath").mockImplementationOnce(() => { return Promise.resolve("test"); });
    jest.spyOn(downloadNetRequest, "downloadNetRequest").mockImplementationOnce(() => { return Promise.resolve("test"); });
    jest.spyOn(GetBaseUrlFromSettings, "GetBaseUrlFromSettings").mockImplementationOnce(() => {
      return Promise.resolve({
        location: ""
      } as IlocationUrlSettings);
    });

    // eslint-disable-next-line @typescript-eslint/no-unsafe-argument
    const result = await downloadXmpFile({
      collectionPaths: ["test"],
      sidecarExtensionsList: ["test"]
    // eslint-disable-next-line @typescript-eslint/no-unsafe-argument
    } as any, {} as any);

    expect(result).toBe(`test${path.sep}undefined.test`);
  });
});
