import { ProcessingState } from "../../../../shared/export/processing-state";
import { CacheControl } from "../../../../shared/fetch/cache-control";
import FetchGet from "../../../../shared/fetch/fetch-get";
import FetchPost from "../../../../shared/fetch/fetch-post";
import { UrlQuery } from "../../../../shared/url/url-query";

export async function publishToRemote(
  publishProfileName: string,
  itemName: string,
  setIsProcessing: React.Dispatch<React.SetStateAction<ProcessingState>>
) {
  const statusResult = await FetchGet(
    new UrlQuery().UrlPublishRemoteStatus(publishProfileName),
    CacheControl
  );
  if (statusResult.statusCode !== 200) {
    setIsProcessing(ProcessingState.fail);
    return;
  }

  const isRemotePublishEnabled = statusResult.data as boolean;
  if (!isRemotePublishEnabled) {
    return;
  }

  const bodyParams = new URLSearchParams();
  bodyParams.set("itemName", itemName);
  bodyParams.set("publishProfileName", publishProfileName);

  const ftpResult = await FetchPost(new UrlQuery().UrlPublishRemoteCreate(), bodyParams.toString());
  if (ftpResult.statusCode !== 200) {
    setIsProcessing(ProcessingState.fail);
  }
}
