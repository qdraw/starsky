import { ArchiveAction } from "../../../../contexts/archive-context";
import { IArchiveProps } from "../../../../interfaces/IArchiveProps";
import { IExifTimezoneCorrectionResult } from "../../../../interfaces/ITimezone";
import FetchPost from "../../../../shared/fetch/fetch-post";
import { URLPath } from "../../../../shared/url/url-path";
import { UrlQuery } from "../../../../shared/url/url-query";

interface ExecuteShiftParams {
  select: string[];
  state: IArchiveProps;
  isOffset: boolean;
  offsetData?: {
    year: number;
    month: number;
    day: number;
    hour: number;
    minute: number;
    second: number;
  };
  timezoneData?: {
    recordedTimezone: string;
    correctTimezone: string;
  };
}

export async function executeShift(
  params: ExecuteShiftParams,
  setIsExecuting: (value: boolean) => void,
  setError: (value: string | null) => void,
  handleExit: () => void,
  dispatch: React.Dispatch<ArchiveAction>
) {
  const { select, state, isOffset, offsetData, timezoneData } = params;

  if (select.length === 0) return;

  setIsExecuting(true);
  setError(null);

  try {
    const collections = true;
    const collectionsParam = collections ? "true" : "false";

    const body = isOffset
      ? JSON.stringify({
          year: offsetData?.year || 0,
          month: offsetData?.month || 0,
          day: offsetData?.day || 0,
          hour: offsetData?.hour || 0,
          minute: offsetData?.minute || 0,
          second: offsetData?.second || 0
        })
      : JSON.stringify({
          recordedTimezone: timezoneData?.recordedTimezone || "",
          correctTimezone: timezoneData?.correctTimezone || ""
        });

    const url = isOffset ? new UrlQuery().UrlOffsetExecute() : new UrlQuery().UrlTimezoneExecute();

    // get MergeSelectFileIndexItem to get the full file path
    const filePathList = new URLPath().MergeSelectFileIndexItem(select, state.fileIndexItems);

    // API supports all files at once
    // f=/test/file;/test/file2;/test/file3...
    const anyData = await FetchPost(
      `${url}?f=${new URLPath().encodeURI(filePathList.join(";"))}&collections=${collectionsParam}`,
      body,
      "post",
      { "Content-Type": "application/json" }
    );
    const result = cast(anyData.data);

    console.log("Execute shift result", result);

    if (anyData.statusCode !== 200) {
      throw new Error("Failed to execute shift " + anyData.statusCode);
    }

    const updatedFileIndexItems = [];
    for (const item of result) {
      updatedFileIndexItems.push(item.fileIndexItem);
    }
    dispatch({ type: "add", add: updatedFileIndexItems });

    handleExit();
  } catch (err) {
    console.error("Failed to execute shift", err);
    setError("Failed to execute shift");
  } finally {
    setIsExecuting(false);
  }
}

function cast(data: unknown): IExifTimezoneCorrectionResult[] {
  const castedData = data as IExifTimezoneCorrectionResult[];
  if (!castedData || !Array.isArray(castedData)) {
    throw new Error("Invalid data format");
  }
  return castedData;
}
