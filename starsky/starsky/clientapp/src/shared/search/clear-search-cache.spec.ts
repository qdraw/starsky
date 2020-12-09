import {
  IConnectionDefault,
  newIConnectionDefault
} from "../../interfaces/IConnectionDefault";
import * as FetchPost from "../fetch-post";
import { ClearSearchCache } from "./clear-search-cache";

describe("ClearSearchCache", () => {
  it("not calling when url is not search", () => {
    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
      newIConnectionDefault()
    );

    var fetchSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);
    ClearSearchCache("?test");

    expect(fetchSpy).toBeCalledTimes(0);

    jest.spyOn(FetchPost, "default").mockReset();
  });

  it("url is containing ?t", () => {
    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
      newIConnectionDefault()
    );

    var fetchSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);
    ClearSearchCache("?t=test");

    expect(fetchSpy).toBeCalledTimes(1);
  });
});
