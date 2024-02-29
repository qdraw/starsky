import { IConnectionDefault } from "../../interfaces/IConnectionDefault";
import * as FetchGet from "../fetch/fetch-get";
import { UrlQuery } from "../url/url-query";
import { ExportIntervalUpdate } from "./export-interval-update";
import { ProcessingState } from "./processing-state";

describe("ExportIntervalUpdate", () => {
  it("ready", async () => {
    const setProcessingSpy = jest.fn();

    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 200,
      data: true
    } as IConnectionDefault);

    const fetchGetSpy = jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    await ExportIntervalUpdate("test", setProcessingSpy);

    expect(fetchGetSpy).toHaveBeenCalled();
    expect(fetchGetSpy).toHaveBeenCalledWith(new UrlQuery().UrlExportZipApi("test", true));
    expect(setProcessingSpy).toHaveBeenCalled();
    expect(setProcessingSpy).toHaveBeenCalledWith(ProcessingState.ready);
  });

  it("fail", async () => {
    const setProcessingSpy = jest.fn();

    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 500,
      data: true
    } as IConnectionDefault);

    const fetchGetSpy = jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    await ExportIntervalUpdate("test", setProcessingSpy);

    expect(fetchGetSpy).toHaveBeenCalled();
    expect(fetchGetSpy).toHaveBeenCalledWith(new UrlQuery().UrlExportZipApi("test", true));
    expect(setProcessingSpy).toHaveBeenCalled();
    expect(setProcessingSpy).toHaveBeenCalledWith(ProcessingState.fail);
  });

  it("wait 206", async () => {
    const setProcessingSpy = jest.fn();

    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 206,
      data: true
    } as IConnectionDefault);

    const fetchGetSpy = jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    await ExportIntervalUpdate("test", setProcessingSpy);

    expect(fetchGetSpy).toHaveBeenCalled();
    expect(fetchGetSpy).toHaveBeenCalledWith(new UrlQuery().UrlExportZipApi("test", true));
    expect(setProcessingSpy).toHaveBeenCalledTimes(0);
  });

  it("wait 404", async () => {
    const setProcessingSpy = jest.fn();

    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 404,
      data: true
    } as IConnectionDefault);

    const fetchGetSpy = jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    await ExportIntervalUpdate("test", setProcessingSpy);

    expect(fetchGetSpy).toHaveBeenCalled();
    expect(fetchGetSpy).toHaveBeenCalledWith(new UrlQuery().UrlExportZipApi("test", true));
    expect(setProcessingSpy).toHaveBeenCalledTimes(0);
  });
});
