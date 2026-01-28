import FetchPost from "../../../../shared/fetch/fetch-post";
import { URLPath } from "../../../../shared/url/url-path";
import { UrlQuery } from "../../../../shared/url/url-query";

export async function executeShift() {
  if (select.length === 0) return;

  setIsExecuting(true);
  setError(null);

  try {
    const collections = true;
    const collectionsParam = collections ? "true" : "false";
    const isOffset = currentStep === "offset";

    const body = isOffset
      ? JSON.stringify({
          year: offsetYears,
          month: offsetMonths,
          day: offsetDays,
          hour: offsetHours,
          minute: offsetMinutes,
          second: offsetSeconds
        })
      : JSON.stringify({
          recordedTimezone,
          correctTimezone
        });

    const url = isOffset ? new UrlQuery().UrlOffsetExecute() : new UrlQuery().UrlTimezoneExecute();

    // get MergeSelectFileIndexItem to get the full file path
    const filePathList = new URLPath().MergeSelectFileIndexItem(select, state.fileIndexItems);

    // API supports all files at once
    // f=/test/file;/test/file2;/test/file3...
    FetchPost(
      `${url}?f=${new URLPath().encodeURI(file)}&collections=${collectionsParam}`,
      body,
      "post",
      { "Content-Type": "application/json" }
    );

    if (allSucceeded) {
      // Success - close modal and refresh
      handleExit();
    } else {
      setError("Some files failed to update");
    }
  } catch (err) {
    console.error("Failed to execute shift", err);
    setError("Failed to execute shift");
  } finally {
    setIsExecuting(false);
  }
}
