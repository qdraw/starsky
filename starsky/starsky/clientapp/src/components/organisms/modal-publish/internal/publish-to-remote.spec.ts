import { ProcessingState } from "../../../../shared/export/processing-state";
import { CacheControl } from "../../../../shared/fetch/cache-control";
import * as FetchGet from "../../../../shared/fetch/fetch-get";
import * as FetchPost from "../../../../shared/fetch/fetch-post";
import { UrlQuery } from "../../../../shared/url/url-query";
import { publishToRemote } from "./publish-to-remote";

describe("publishToRemote", () => {
  let setIsProcessing: jest.Mock;

  beforeEach(() => {
    setIsProcessing = jest.fn();
  });

  it("sets fail when status check returns non-200", async () => {
    jest.spyOn(FetchGet, "default").mockResolvedValueOnce({ statusCode: 500, data: null });
    const fetchPostSpy = jest.spyOn(FetchPost, "default");

    await publishToRemote("_default", "album-a", setIsProcessing);

    expect(FetchGet.default).toHaveBeenCalledWith(
      new UrlQuery().UrlPublishRemoteStatus("_default"),
      CacheControl
    );
    expect(setIsProcessing).toHaveBeenCalledWith(ProcessingState.fail);
    expect(fetchPostSpy).toHaveBeenCalledTimes(0);
  });

  it("does not post when status check returns false", async () => {
    jest.spyOn(FetchGet, "default").mockResolvedValueOnce({ statusCode: 200, data: false });
    const fetchPostSpy = jest.spyOn(FetchPost, "default");

    await publishToRemote("_default", "album-a", setIsProcessing);

    expect(FetchGet.default).toHaveBeenCalledWith(
      new UrlQuery().UrlPublishRemoteStatus("_default"),
      CacheControl
    );
    expect(fetchPostSpy).toHaveBeenCalledTimes(0);
    expect(setIsProcessing).toHaveBeenCalledTimes(0);
  });

  it("sets fail when remote create returns non-200", async () => {
    jest.spyOn(FetchGet, "default").mockResolvedValueOnce({ statusCode: 200, data: true });
    const fetchPostSpy = jest.spyOn(FetchPost, "default").mockResolvedValueOnce({
      statusCode: 500,
      data: null
    });

    await publishToRemote("_default", "album-a", setIsProcessing);

    expect(fetchPostSpy).toHaveBeenCalledWith(
      new UrlQuery().UrlPublishRemoteCreate(),
      expect.stringContaining("itemName=album-a")
    );
    const bodyParams = fetchPostSpy.mock.calls[0][1] as string;
    expect(bodyParams).toContain("publishProfileName=_default");
    expect(setIsProcessing).toHaveBeenCalledWith(ProcessingState.fail);
  });

  it("posts remote create when status is true and keeps processing state unchanged on 200", async () => {
    jest.spyOn(FetchGet, "default").mockResolvedValueOnce({ statusCode: 200, data: true });
    const fetchPostSpy = jest.spyOn(FetchPost, "default").mockResolvedValueOnce({
      statusCode: 200,
      data: null
    });

    await publishToRemote("_default", "album-a", setIsProcessing);

    expect(fetchPostSpy).toHaveBeenCalledWith(
      new UrlQuery().UrlPublishRemoteCreate(),
      expect.stringContaining("itemName=album-a")
    );
    expect(setIsProcessing).toHaveBeenCalledTimes(0);
  });
});
