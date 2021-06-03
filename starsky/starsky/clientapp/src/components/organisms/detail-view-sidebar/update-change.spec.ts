import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import * as FetchPost from "../../../shared/fetch-post";
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

    it("shou11ld ignore same content", () => {
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
