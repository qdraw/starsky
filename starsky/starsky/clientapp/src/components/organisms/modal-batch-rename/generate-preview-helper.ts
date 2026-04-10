import { IBatchRenameItem } from "../../../interfaces/IBatchRenameItem";
import { IBatchRenameRequest } from "../../../interfaces/IBatchRenameRequest";
import FetchPost from "../../../shared/fetch/fetch-post";
import { URLPath } from "../../../shared/url/url-path";
import { UrlQuery } from "../../../shared/url/url-query";
import { IModalBatchRenameProps } from "./modal-batch-rename";

/**
 * Generate preview of batch rename
 */
export async function generatePreviewHelper(
  pattern: string,
  setIsPreviewLoading: React.Dispatch<React.SetStateAction<boolean>>,
  setError: React.Dispatch<React.SetStateAction<string | null>>,
  setPreview: React.Dispatch<React.SetStateAction<IBatchRenameItem[]>>,
  setPreviewGenerated: React.Dispatch<React.SetStateAction<boolean>>,
  props: IModalBatchRenameProps
) {
  if (!pattern.trim()) {
    setError("Pattern cannot be empty");
    return;
  }

  setIsPreviewLoading(true);
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
      new UrlQuery().UrlBatchRenamePreview(),
      JSON.stringify(request),
      "post",
      { "Content-Type": "application/json" }
    );

    if (response.statusCode !== 200) {
      setError("Failed to generate preview");
      setIsPreviewLoading(false);
      return;
    }

    const previewItems = (response.data as IBatchRenameItem[]) || [];
    setPreview(previewItems);
    setPreviewGenerated(true);
  } catch (err) {
    console.error("Error generating preview:", err);
    setError("Error generating preview");
  } finally {
    setIsPreviewLoading(false);
  }
}
