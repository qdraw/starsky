import { IArchiveProps } from "../../../../interfaces/IArchiveProps";
import { ITimezoneShiftResult } from "../../../../interfaces/ITimezone";
import FetchPost from "../../../../shared/fetch/fetch-post";
import { URLPath } from "../../../../shared/url/url-path";
import { UrlQuery } from "../../../../shared/url/url-query";

export interface IOffset {
  year: number;
  month: number;
  day: number;
  hour: number;
  minute: number;
  second: number;
}

export async function generateOffsetPreview(
  select: string[],
  state: IArchiveProps,
  offset: IOffset,
  setIsLoadingPreview: React.Dispatch<React.SetStateAction<boolean>>,
  setError: React.Dispatch<React.SetStateAction<string | null>>,
  setPreview: React.Dispatch<React.SetStateAction<ITimezoneShiftResult[]>>
) {
  if (select.length === 0) return;

  setIsLoadingPreview(true);
  setError(null);

  try {
    // Use first file as representative sample
    const filePathList = new URLPath().MergeSelectFileIndexItem(select, state.fileIndexItems);
    const sampleFile = filePathList[0];

    const collectionsParam = state.collections ? "true" : "false";

    const body = JSON.stringify({
      year: offset.year,
      month: offset.month,
      day: offset.day,
      hour: offset.hour,
      minute: offset.minute,
      second: offset.second
    });

    const response = await FetchPost(
      `${new UrlQuery().UrlOffsetPreview()}?f=${new URLPath().encodeURI(sampleFile)}&collections=${collectionsParam}`,
      body,
      "post",
      { "Content-Type": "application/json" }
    );

    if (response.statusCode === 200 && Array.isArray(response.data)) {
      setPreview(response.data);
      setError(null);
    } else {
      setPreview([]);

      setError("Failed to generate preview");
    }
  } catch (err) {
    console.error("Failed to generate preview", err);
    setError("Failed to generate preview");
  } finally {
    setIsLoadingPreview(false);
  }
}
