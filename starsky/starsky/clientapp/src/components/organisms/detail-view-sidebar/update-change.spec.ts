import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import * as FetchPost from "../../../shared/fetch-post";
import { FileListCache } from "../../../shared/filelist-cache";
import { UrlQuery } from "../../../shared/url-query";
import { UpdateChange } from "./update-change";

describe("Update Change", () => {
  describe("Update", () => {
    it("no content", () => {
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => {
          return Promise.resolve({} as IConnectionDefault);
        });

      new UpdateChange(
        {} as any,
        jest.fn(),
        jest.fn(),
        {} as any,
        {} as any
      ).Update([]);

      expect(fetchPostSpy).toBeCalledTimes(0);
    });

    it("missing name", () => {
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => {
          return Promise.resolve({} as IConnectionDefault);
        });

      new UpdateChange(
        {} as any,
        jest.fn(),
        jest.fn(),
        {} as any,
        {} as any
      ).Update([["", "test"]]);

      expect(fetchPostSpy).toBeCalledTimes(0);
    });

    it("no input content", () => {
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => {
          return Promise.resolve({} as IConnectionDefault);
        });

      new UpdateChange(
        { tag: "" } as any,
        jest.fn(),
        jest.fn(),
        {} as any,
        {} as any
      ).Update([["tag", "test"]]);

      expect(fetchPostSpy).toBeCalledTimes(1);
    });

    it("should ignore same content", () => {
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => {
          return Promise.resolve({} as IConnectionDefault);
        });

      new UpdateChange(
        { tag: "test" } as any,
        jest.fn(),
        jest.fn(),
        {} as any,
        {} as any
      ).Update([["tag", "test"]]);

      expect(fetchPostSpy).toBeCalledTimes(0);
    });

    it("wrong status code or missing data", async () => {
      const cacheSetSpy = jest
        .spyOn(FileListCache.prototype, "CacheSet")
        .mockImplementationOnce(() => {});

      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => {
          return Promise.resolve({
            data: {
              fileIndexItem: [] as IFileIndexItem[]
            },
            statusCode: 500
          } as IConnectionDefault);
        });

      const result = await new UpdateChange(
        { tag: "test1" } as any,
        jest.fn(),
        jest.fn(),
        {} as any,
        {} as any
      ).Update([["tag", "test"]]);

      expect(result).toBe("wrong status code or missing data");
      expect(cacheSetSpy).toBeCalledTimes(0);

      expect(fetchPostSpy).toBeCalledTimes(1);
    });

    it("item not in result", async () => {
      const cacheSetSpy = jest
        .spyOn(FileListCache.prototype, "CacheSet")
        .mockImplementationOnce(() => {});

      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => {
          return Promise.resolve({
            data: [{ filePath: "/test" }] as IFileIndexItem[],
            statusCode: 200
          } as IConnectionDefault);
        });

      const result = await new UpdateChange(
        { tag: "test1" } as any,
        jest.fn(),
        jest.fn(),
        { location: { search: "" } } as any,
        {} as any
      ).Update([["tag", "test"]]);

      expect(result).toBe("item not in result");
      expect(cacheSetSpy).toBeCalledTimes(0);

      expect(fetchPostSpy).toBeCalledTimes(1);
    });

    it("contain result", async () => {
      const cacheSetSpy = jest
        .spyOn(FileListCache.prototype, "CacheSet")
        .mockImplementationOnce(() => {});

      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => {
          return Promise.resolve({
            data: [{ filePath: "/test.jpg" }] as IFileIndexItem[],
            statusCode: 200
          } as IConnectionDefault);
        });

      const result = await new UpdateChange(
        { tag: "test1", filePath: "/test.jpg" } as any,
        jest.fn(),
        jest.fn(),
        { location: { search: "/test.jpg" } } as any,
        {} as any
      ).Update([["tag", "test"]]);

      expect(result).toBe(true);
      expect(cacheSetSpy).toBeCalledTimes(1);

      expect(fetchPostSpy).toBeCalledTimes(1);
    });

    it("no content 2", () => {
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => {
          return Promise.resolve({} as IConnectionDefault);
        });

      new UpdateChange(
        {} as any,
        jest.fn(),
        jest.fn(),
        {} as any,
        {} as any
      ).Update([["tags", "test"]]);

      expect(fetchPostSpy).toBeCalledTimes(1);
      expect(fetchPostSpy).toBeCalledWith(
        new UrlQuery().UrlUpdateApi(),
        "tags=test"
      );
    });
  });
});
