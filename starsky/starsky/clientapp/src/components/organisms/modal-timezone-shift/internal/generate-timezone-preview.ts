import FetchPost from "../../../../shared/fetch/fetch-post";
import { URLPath } from "../../../../shared/url/url-path";
import { UrlQuery } from "../../../../shared/url/url-query";

export async function generateTimezonePreview(
  select: string[],
  setIsLoadingPreview: any,
  recordedTimezone: string,
  setPreview: any,
  setIsLoadingPreview: any,
  setError: any
) {
  if (select.length === 0) return;

  const collections = true;

  setIsLoadingPreview(true);
  setError(null);

  try {
    // Use first file as representative sample
    const sampleFile = select[0];
    const collectionsParam = collections ? "true" : "false";

    const body = JSON.stringify({
      recordedTimezone,
      correctTimezone
    });

    // get MergeSelectFileIndexItem to get the full file path
    const filePathList = new URLPath().MergeSelectFileIndexItem(select, state.fileIndexItems);

    const response = await FetchPost(
      `${new UrlQuery().UrlTimezonePreview()}?f=${new URLPath().encodeURI(sampleFile)}&collections=${collectionsParam}`,
      body,
      "post",
      { "Content-Type": "application/json" }
    );

    if (response.statusCode === 200 && Array.isArray(response.data)) {
      setPreview(response.data);
    } else {
      setError("Failed to generate preview");
    }
  } catch (err) {
    console.error("Failed to generate preview", err);
    setError("Failed to generate preview");
  } finally {
    setIsLoadingPreview(false);
  }
}
