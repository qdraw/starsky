/* eslint-disable @typescript-eslint/no-unsafe-argument */
/* eslint-disable @typescript-eslint/no-unsafe-assignment */
/* eslint-disable no-restricted-syntax */
/* eslint-disable @typescript-eslint/no-unsafe-member-access */
import * as downloadNetRequest from "../net-request/download-net-request";
import { downloadBinary } from "./download-binary";
import * as GetParentDiskPath from "./get-parent-disk-path";

jest.mock("electron", () => {
  return {
    app: {
      getVersion: () => "99.99.99",
      getPath: () => "tmp",
      getLocale: () => "en",
      on: () => "en",
    },
    Menu: {},
    shell: {
      openExternal: jest.fn(),
    },
  };
});

jest.mock("electron-settings", () => {
  return {
    get: () => "http://localhost:9609",
    set: () => "data",
    has: () => true,
    unset: () => {},
    configure: () => {},
    file: () => {},
    __esModule: true,
  };
});

describe("downloadBinary", () => {
  it("should download and fail", async () => {
    jest.spyOn(GetParentDiskPath, "GetParentDiskPath").mockImplementationOnce(() => {
      return Promise.resolve("test");
    });
    jest.spyOn(downloadNetRequest, "downloadNetRequest").mockImplementationOnce(() => {
      return Promise.resolve("test");
    });

    const result = await downloadBinary(
      {
        collectionPaths: ["test"],
      } as any,
      {} as any
    );

    expect(result).toBeNull();
  });

  it("should download and fail 2", async () => {
    jest.spyOn(GetParentDiskPath, "GetParentDiskPath").mockImplementationOnce(() => {
      return Promise.resolve("test");
    });
    const downloadSpy = jest
      .spyOn(downloadNetRequest, "downloadNetRequest")
      .mockClear()
      .mockImplementationOnce(() => {
        return Promise.reject(new Error("test"));
      })
      .mockImplementationOnce(() => {
        return Promise.resolve(new Error("test"));
      });

    // eslint-disable-next-line @typescript-eslint/no-unsafe-argument
    const result = await downloadBinary(
      {
        collectionPaths: ["test"],
        // eslint-disable-next-line @typescript-eslint/no-unsafe-argument
      } as any,
      {} as any
    );

    expect(result).toBeNull();
    expect(downloadSpy).toHaveBeenCalledTimes(2);
  });
});
