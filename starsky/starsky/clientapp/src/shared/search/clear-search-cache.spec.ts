import { IConnectionDefault, newIConnectionDefault } from "../../interfaces/IConnectionDefault";
import * as FetchPost from "../fetch/fetch-post";
import { ClearSearchCache } from "./clear-search-cache";

describe("ClearSearchCache", () => {
  it("not calling when url is not search", () => {
    const mockIConnectionDefault: Promise<IConnectionDefault> =
      Promise.resolve(newIConnectionDefault());

    const fetchSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);
    ClearSearchCache("?test");

    expect(fetchSpy).toHaveBeenCalledTimes(0);

    jest.spyOn(FetchPost, "default").mockReset();
  });

  it("url is containing ?t", () => {
    const mockIConnectionDefault: Promise<IConnectionDefault> =
      Promise.resolve(newIConnectionDefault());

    const fetchSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);
    ClearSearchCache("?t=test");

    expect(fetchSpy).toHaveBeenCalledTimes(1);
  });
});
