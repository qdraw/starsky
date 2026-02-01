import { IExifTimezoneCorrectionResultContainer } from "../../../../interfaces/ITimezone";
import FetchPost from "../../../../shared/fetch/fetch-post";
import { URLPath } from "../../../../shared/url/url-path";
import { UrlQuery } from "../../../../shared/url/url-query";

interface IGenerateTimezonePreviewParams {
  filePathList: string[];
  recordedTimezoneId: string;
  correctTimezoneId: string;
  setIsLoadingPreview: (value: boolean) => void;
  preview: IExifTimezoneCorrectionResultContainer;
  setPreview: (value: IExifTimezoneCorrectionResultContainer) => void;
  setError: (value: string | null) => void;
  collections: boolean;
}

export async function generateTimezonePreview({
  filePathList,
  recordedTimezoneId,
  correctTimezoneId,
  setIsLoadingPreview,
  preview,
  setPreview,
  setError,
  collections
}: IGenerateTimezonePreviewParams) {
  if (filePathList.length === 0) return;

  setIsLoadingPreview(true);
  setError(null);

  try {
    // Use first file as representative sample
    const collectionsParam = collections ? "true" : "false";

    const body = JSON.stringify({
      recordedTimezoneId,
      correctTimezoneId
    });

    const response = await FetchPost(
      `${new UrlQuery().UrlTimezonePreview()}?f=${new URLPath().encodeURI(filePathList[0])}&collections=${collectionsParam}`,
      body,
      "post",
      { "Content-Type": "application/json" }
    );

    if (response.statusCode === 200 && Array.isArray(response.data)) {
      setPreview({
        ...preview,
        timezoneData: response.data
      });
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
