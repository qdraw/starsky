import { IConnectionDefault } from '../../interfaces/IConnectionDefault';
import * as FetchGet from '../fetch-get';
import { UrlQuery } from '../url-query';
import { ExportIntervalUpdate } from './export-interval-update';
import { ProcessingState } from './processing-state';

describe("ExportIntervalUpdate", () => {

  it("ready", async () => {
    var setProcessingSpy = jest.fn();

    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 200, data: true
    } as IConnectionDefault);

    var fetchGetSpy = jest.spyOn(FetchGet, 'default')
      .mockImplementationOnce(() => mockGetIConnectionDefault)

    await ExportIntervalUpdate('test', setProcessingSpy);

    expect(fetchGetSpy).toBeCalled();
    expect(fetchGetSpy).toBeCalledWith(new UrlQuery().UrlExportZipApi('test', true));
    expect(setProcessingSpy).toBeCalled();
    expect(setProcessingSpy).toBeCalledWith(ProcessingState.ready);
  });

  it("fail", async () => {
    var setProcessingSpy = jest.fn();

    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 500, data: true
    } as IConnectionDefault);

    var fetchGetSpy = jest.spyOn(FetchGet, 'default')
      .mockImplementationOnce(() => mockGetIConnectionDefault)

    await ExportIntervalUpdate('test', setProcessingSpy);

    expect(fetchGetSpy).toBeCalled();
    expect(fetchGetSpy).toBeCalledWith(new UrlQuery().UrlExportZipApi('test', true));
    expect(setProcessingSpy).toBeCalled();
    expect(setProcessingSpy).toBeCalledWith(ProcessingState.fail);
  });

  it("wait 206", async () => {
    var setProcessingSpy = jest.fn();

    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 206, data: true
    } as IConnectionDefault);

    var fetchGetSpy = jest.spyOn(FetchGet, 'default')
      .mockImplementationOnce(() => mockGetIConnectionDefault)

    await ExportIntervalUpdate('test', setProcessingSpy);

    expect(fetchGetSpy).toBeCalled();
    expect(fetchGetSpy).toBeCalledWith(new UrlQuery().UrlExportZipApi('test', true));
    expect(setProcessingSpy).toBeCalledTimes(0);
  });

  it("wait 404", async () => {
    var setProcessingSpy = jest.fn();

    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 404, data: true
    } as IConnectionDefault);

    var fetchGetSpy = jest.spyOn(FetchGet, 'default')
      .mockImplementationOnce(() => mockGetIConnectionDefault)

    await ExportIntervalUpdate('test', setProcessingSpy);

    expect(fetchGetSpy).toBeCalled();
    expect(fetchGetSpy).toBeCalledWith(new UrlQuery().UrlExportZipApi('test', true));
    expect(setProcessingSpy).toBeCalledTimes(0);
  });
});