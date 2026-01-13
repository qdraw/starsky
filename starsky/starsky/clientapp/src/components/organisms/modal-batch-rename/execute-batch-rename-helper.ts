import { IBatchRenameItem } from "../../../interfaces/IBatchRenameItem";
import { IBatchRenameRequest } from "../../../interfaces/IBatchRenameRequest";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import FetchPost from "../../../shared/fetch/fetch-post";
import { FileListCache } from "../../../shared/filelist-cache";
import { ClearSearchCache } from "../../../shared/search/clear-search-cache";
import { URLPath } from "../../../shared/url/url-path";
import { UrlQuery } from "../../../shared/url/url-query";
import { BATCH_RENAME_PATTERNS_KEY } from "./batch-rename-patterns-key";
import { IModalBatchRenameProps } from "./modal-batch-rename";

/**
 * Execute batch rename
 */
export async function executeBatchRenameHelper(
  previewGenerated: boolean,
  setError: React.Dispatch<React.SetStateAction<string | null>>,
  preview: IBatchRenameItem[],
  setIsLoading: React.Dispatch<React.SetStateAction<boolean>>,
  props: IModalBatchRenameProps,
  pattern: string,
  recentPatterns: string[],
  setRecentPatterns: React.Dispatch<React.SetStateAction<string[]>>
) {
  if (!previewGenerated || preview.length === 0) {
    setError("Please generate a preview first");
    return;
  }

  // Check for errors in preview
  const hasErrors = preview.some((item) => item.hasError);
  if (hasErrors) {
    setError("Cannot rename: there are errors in the preview");
    return;
  }

  setIsLoading(true);
  setError(null);

  try {
    const filePathList = new URLPath().MergeSelectFileIndexItem(
      props.select,
      props.state.fileIndexItems
    );
    const request: IBatchRenameRequest = {
      filePaths: filePathList,
      pattern: pattern,
      collections: true
    };

    const response = await FetchPost(
      new UrlQuery().UrlBatchRenameExecute(),
      JSON.stringify(request),
      "post",
      { "Content-Type": "application/json" }
    );

    if (response.statusCode !== 200) {
      setError("Failed to execute batch rename");
      setIsLoading(false);
      return;
    }

    // Save pattern to recent patterns
    const currentPatterns = recentPatterns.filter((p) => p !== pattern);
    currentPatterns.unshift(pattern);
    const newPatterns = currentPatterns.slice(0, 10); // Keep only 10 most recent
    setRecentPatterns(newPatterns);
    localStorage.setItem(BATCH_RENAME_PATTERNS_KEY, JSON.stringify(newPatterns));

    const updatedFileIndexItems = response.data as IFileIndexItem[];

    // Clean cache and close modal
    new FileListCache().CacheCleanEverything();
    props.handleExit();
    props.undoSelection();
    props.dispatch({ type: "add", add: updatedFileIndexItems });
    ClearSearchCache(props.historyLocationSearch);
  } catch (err) {
    console.error("Error executing batch rename:", err);
    setError("Error executing batch rename");
  } finally {
    setIsLoading(false);
  }
}
