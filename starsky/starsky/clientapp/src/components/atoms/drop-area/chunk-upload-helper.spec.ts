import { IExifStatus } from "../../../interfaces/IExifStatus";
import * as FetchPost from "../../../shared/fetch/fetch-post";
import * as CookieHelper from "../../../shared/cookie/get-cookie";
import { ChunkUploadHelper, getChunkThreshold } from "./chunk-upload-helper";

describe("ChunkUploadHelper", () => {
  beforeEach(() => {
    jest.restoreAllMocks();
    global.fetch = jest.fn();
    jest.spyOn(CookieHelper, "GetCookie").mockReturnValue("token");
  });

  afterEach(() => {
    jest.restoreAllMocks();
  });

  it("returns threshold under 100MB", () => {
    expect(getChunkThreshold()).toBe(95 * 1024 * 1024);
  });

  it("uploadFileInChunks uploads init, chunk and complete", async () => {
    const setNotificationStatus = jest.fn();
    const helper = new ChunkUploadHelper("/api/upload", "/photos", setNotificationStatus);
    const file = new File([new Uint8Array([1, 2, 3])], "test.jpg", {
      type: "image/jpeg"
    });

    jest
      .spyOn(FetchPost, "default")
      .mockResolvedValueOnce({
        statusCode: 200,
        data: {
          uploadId: "upload-1",
          expiresAt: new Date().toISOString()
        }
      } as never)
      .mockResolvedValueOnce({
        statusCode: 200,
        data: [
          {
            status: IExifStatus.Ok,
            fileIndexItem: {
              fileName: "test.jpg",
              filePath: "/photos/test.jpg",
              isDirectory: false,
              status: IExifStatus.Ok
            }
          }
        ]
      } as never);

    (global.fetch as jest.Mock).mockResolvedValue({ ok: true });

    const result = await helper.uploadFileInChunks(file, 0, 1);

    expect(FetchPost.default).toHaveBeenNthCalledWith(
      1,
      "/api/upload/chunk/init?fileName=test.jpg&totalChunks=1&totalSize=3",
      "",
      "post",
      { to: "/photos" }
    );
    expect(global.fetch).toHaveBeenCalledWith(
      "/api/upload/chunk/upload-1?chunkIndex=0",
      expect.objectContaining({
        method: "PUT",
        headers: expect.objectContaining({
          "Content-Type": "application/octet-stream",
          "X-XSRF-TOKEN": "token"
        })
      })
    );
    expect(FetchPost.default).toHaveBeenNthCalledWith(
      2,
      "/api/upload/chunk/upload-1/complete",
      "",
      "post",
      { to: "/photos" }
    );
    expect(setNotificationStatus).toHaveBeenCalledWith("Finalizing upload...");
    expect(result).toEqual([
      expect.objectContaining({
        fileName: "test.jpg",
        filePath: "/photos/test.jpg",
        status: IExifStatus.Ok
      })
    ]);
  });

  it("uploadFileInChunks retries a transient chunk failure and still completes", async () => {
    const setNotificationStatus = jest.fn();
    const helper = new ChunkUploadHelper("/api/upload", "/photos", setNotificationStatus);
    const file = new File([new Uint8Array([1, 2, 3])], "test.jpg", {
      type: "image/jpeg"
    });

    jest
      .spyOn(FetchPost, "default")
      .mockResolvedValueOnce({
        statusCode: 200,
        data: {
          uploadId: "upload-2",
          expiresAt: new Date().toISOString()
        }
      } as never)
      .mockResolvedValueOnce({
        statusCode: 200,
        data: [
          {
            status: IExifStatus.Ok,
            fileIndexItem: {
              fileName: "test.jpg",
              filePath: "/photos/test.jpg",
              isDirectory: false,
              status: IExifStatus.Ok
            }
          }
        ]
      } as never);

    (global.fetch as jest.Mock)
      .mockResolvedValueOnce({ ok: false, status: 500 })
      .mockResolvedValueOnce({ ok: true });

    const result = await helper.uploadFileInChunks(file, 0, 1);

    expect(global.fetch).toHaveBeenNthCalledWith(
      1,
      "/api/upload/chunk/upload-2?chunkIndex=0",
      expect.objectContaining({
        method: "PUT"
      })
    );
    expect(global.fetch).toHaveBeenNthCalledWith(
      2,
      "/api/upload/chunk/upload-2?chunkIndex=0",
      expect.objectContaining({
        method: "PUT"
      })
    );
    expect(setNotificationStatus).toHaveBeenCalledWith("Retrying chunk 1 (1/2)");
    expect(result).toEqual([
      expect.objectContaining({
        fileName: "test.jpg",
        filePath: "/photos/test.jpg",
        status: IExifStatus.Ok
      })
    ]);
  });

  it("uploadFileInChunks deletes session when retries are exhausted", async () => {
    const setNotificationStatus = jest.fn();
    const helper = new ChunkUploadHelper("/api/upload", "/photos", setNotificationStatus);
    const file = new File([new Uint8Array([1, 2, 3])], "test.jpg", {
      type: "image/jpeg"
    });

    jest.spyOn(FetchPost, "default").mockResolvedValueOnce({
      statusCode: 200,
      data: {
        uploadId: "upload-3",
        expiresAt: new Date().toISOString()
      }
    } as never);

    (global.fetch as jest.Mock)
      .mockResolvedValueOnce({ ok: false, status: 500 })
      .mockResolvedValueOnce({ ok: false, status: 500 })
      .mockResolvedValueOnce({ ok: false, status: 500 })
      .mockResolvedValueOnce({ ok: true });

    expect(await helper.uploadFileInChunks(file, 0, 1)).toEqual([
      expect.objectContaining({
        fileName: "test.jpg",
        filePath: "test.jpg",
        status: IExifStatus.ServerError
      })
    ]);
    expect(global.fetch).toHaveBeenNthCalledWith(
      4,
      "/api/upload/chunk/upload-3",
      expect.objectContaining({
        method: "DELETE"
      })
    );
    expect(setNotificationStatus).toHaveBeenCalledWith("Retrying chunk 1 (2/2)");
  });
});


