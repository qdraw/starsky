import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import * as FetchGet from "../../../shared/fetch-get";
import * as FetchPost from "../../../shared/fetch-post";
import { ILatLong } from "./modal-geo";
import { updateGeoLocation } from "./update-geo-location";

describe("updateGeoLocation", () => {
  it("no location null result", async () => {
    const setErrorSpy = jest.fn();
    const result = await updateGeoLocation(
      "",
      "/",
      null,
      setErrorSpy,
      jest.fn(),
      true
    );
    expect(result).toBe(null);
  });

  it("update failed", async () => {
    const setErrorSpy = jest.fn();

    const mockGetIConnectionDefault: Promise<IConnectionDefault> =
      Promise.resolve({
        statusCode: 403,
        data: null
      } as IConnectionDefault);
    jest.spyOn(FetchGet, "default").mockReset();
    const fetchGetSpy = jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    jest.spyOn(FetchPost, "default").mockImplementationOnce(() => {
      throw new Error("test");
    });

    await updateGeoLocation(
      "",
      "/",
      { latitude: 1, longitude: 1 } as ILatLong,
      setErrorSpy,
      jest.fn(),
      true
    );

    expect(fetchGetSpy).toBeCalled();
    expect(fetchGetSpy).toBeCalledTimes(1);

    expect(setErrorSpy).toBeCalled();
    expect(setErrorSpy).toBeCalledTimes(1);
  });

  it("update failed 1", async () => {
    const setErrorSpy = jest.fn();

    const mockGetIConnectionDefault: Promise<IConnectionDefault> =
      Promise.resolve({
        statusCode: 403,
        data: null
      } as IConnectionDefault);
    jest.spyOn(FetchGet, "default").mockReset();
    const fetchGetSpy = jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);
    const fetchPostSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    await updateGeoLocation(
      "",
      "/",
      { latitude: 1, longitude: 1 } as ILatLong,
      setErrorSpy,
      jest.fn(),
      true
    );

    expect(fetchGetSpy).toBeCalled();
    expect(fetchGetSpy).toBeCalledTimes(1);

    expect(fetchPostSpy).toBeCalled();
    expect(fetchPostSpy).toBeCalledTimes(1);

    expect(setErrorSpy).toBeCalled();
    expect(setErrorSpy).toBeCalledTimes(1);
  });

  it("update succeed", async () => {
    const setErrorSpy = jest.fn();

    const mockGetIConnectionDefault: Promise<IConnectionDefault> =
      Promise.resolve({
        statusCode: 200,
        data: {
          locationCity: "a",
          locationCountry: "b",
          locationCountryCode: "c",
          locationState: "d"
        }
      } as IConnectionDefault);
    jest.spyOn(FetchGet, "default").mockReset();
    const fetchGetSpy = jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);
    const fetchPostSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    const result = await updateGeoLocation(
      "",
      "/",
      { latitude: 1, longitude: 1 } as ILatLong,
      setErrorSpy,
      jest.fn(),
      true
    );

    expect(fetchGetSpy).toBeCalled();
    expect(fetchGetSpy).toBeCalledTimes(1);

    expect(fetchPostSpy).toBeCalled();
    expect(fetchPostSpy).toBeCalledTimes(1);

    expect(setErrorSpy).toBeCalledTimes(0);
    expect(result).toStrictEqual({
      locationCity: "a",
      locationCountry: "b",
      locationCountryCode: "c",
      locationState: "d"
    });
  });
});
