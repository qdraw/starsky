import { IUseLocation } from "../../../hooks/use-location/interfaces/IUseLocation";
import { newIArchive } from "../../../interfaces/IArchive";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import { IDetailView } from "../../../interfaces/IDetailView";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import * as FetchPost from "../../../shared/fetch/fetch-post";
import { FileListCache } from "../../../shared/filelist-cache";
import { UpdateChange } from "./update-change";

describe("Update Change", () => {
  describe("Update", () => {
    it("no content", () => {
      const fetchPostSpy = jest.spyOn(FetchPost, "default").mockImplementationOnce(() => {
        return Promise.resolve({} as IConnectionDefault);
      });

      new UpdateChange(
        {} as unknown as IFileIndexItem,
        jest.fn(),
        jest.fn(),
        {} as unknown as IUseLocation,
        {} as unknown as IDetailView
      ).Update([]);

      expect(fetchPostSpy).toHaveBeenCalledTimes(0);
    });

    it("missing name", () => {
      const fetchPostSpy = jest.spyOn(FetchPost, "default").mockImplementationOnce(() => {
        return Promise.resolve({} as IConnectionDefault);
      });

      new UpdateChange(
        {} as unknown as IFileIndexItem,
        jest.fn(),
        jest.fn(),
        {} as unknown as IUseLocation,
        {} as unknown as IDetailView
      ).Update([["", "test"]]);

      expect(fetchPostSpy).toHaveBeenCalledTimes(0);
    });

    it("no input content", () => {
      const fetchPostSpy = jest.spyOn(FetchPost, "default").mockImplementationOnce(() => {
        return Promise.resolve({} as IConnectionDefault);
      });

      new UpdateChange(
        { tag: "" } as unknown as IFileIndexItem,
        jest.fn(),
        jest.fn(),
        {} as unknown as IUseLocation,
        {} as unknown as IDetailView
      ).Update([["tag", "test"]]);

      expect(fetchPostSpy).toHaveBeenCalledTimes(1);
    });

    it("should ignore same content", () => {
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockReset()
        .mockImplementationOnce(() => {
          return Promise.resolve({} as IConnectionDefault);
        });

      new UpdateChange(
        { tag: "test" } as unknown as IFileIndexItem,
        jest.fn(),
        jest.fn(),
        {} as unknown as IUseLocation,
        {} as unknown as IDetailView
      ).Update([["tag", "test"]]);

      expect(fetchPostSpy).toHaveBeenCalledTimes(0);
    });

    it("wrong status code or missing data", async () => {
      const cacheSetSpy = jest
        .spyOn(FileListCache.prototype, "CacheSet")
        .mockImplementationOnce(() => newIArchive());

      const fetchPostSpy = jest.spyOn(FetchPost, "default").mockImplementationOnce(() => {
        return Promise.resolve({
          data: {
            fileIndexItem: [] as IFileIndexItem[]
          },
          statusCode: 500
        } as IConnectionDefault);
      });

      const result = await new UpdateChange(
        { tag: "test1" } as unknown as IFileIndexItem,
        jest.fn(),
        jest.fn(),
        {} as unknown as IUseLocation,
        {} as unknown as IDetailView
      ).Update([["tag", "test"]]);

      expect(result).toBe("wrong status code or missing data");
      expect(cacheSetSpy).toHaveBeenCalledTimes(0);

      expect(fetchPostSpy).toHaveBeenCalledTimes(1);
    });

    it("item not in result", async () => {
      const cacheSetSpy = jest
        .spyOn(FileListCache.prototype, "CacheSet")
        .mockImplementationOnce(() => newIArchive());

      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockReset()
        .mockImplementationOnce(() => {
          return Promise.resolve({
            data: [{ filePath: "/test" }] as IFileIndexItem[],
            statusCode: 200
          } as IConnectionDefault);
        });

      const result = await new UpdateChange(
        { tag: "test1" } as unknown as IFileIndexItem,
        jest.fn(),
        jest.fn(),
        { location: { search: "" } } as unknown as IUseLocation,
        {} as unknown as IDetailView
      ).Update([["tag", "test"]]);

      expect(result).toBe("item not in result");
      expect(cacheSetSpy).toHaveBeenCalledTimes(0);

      expect(fetchPostSpy).toHaveBeenCalledTimes(1);
    });

    it("contain result", async () => {
      const cacheSetSpy = jest
        .spyOn(FileListCache.prototype, "CacheSet")
        .mockImplementationOnce(() => newIArchive());

      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockReset()
        .mockImplementationOnce(() => {
          return Promise.resolve({
            data: [{ filePath: "/test.jpg" }] as IFileIndexItem[],
            statusCode: 200
          } as IConnectionDefault);
        });

      const result = await new UpdateChange(
        { tag: "test1", filePath: "/test.jpg" } as unknown as IFileIndexItem,
        jest.fn(),
        jest.fn(),
        { location: { search: "/test.jpg" } } as unknown as IUseLocation,
        {} as unknown as IDetailView
      ).Update([["tag", "test"]]);

      expect(result).toBe(true);
      expect(cacheSetSpy).toHaveBeenCalledTimes(1);

      expect(fetchPostSpy).toHaveBeenCalledTimes(1);
    });

    it("no content 2", async () => {
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockReset()
        .mockImplementationOnce(() => {
          return Promise.resolve({} as IConnectionDefault);
        });

      await new UpdateChange(
        {} as unknown as IFileIndexItem,
        jest.fn(),
        jest.fn(),
        {} as unknown as IUseLocation,
        {} as unknown as IDetailView
      ).Update([["tags", "test"]]);

      expect(fetchPostSpy).toHaveBeenCalledTimes(0);
    });
  });
});
