import FetchPost from "./fetch/fetch-post";
import { FileExtensions } from "./file-extensions";
import { FileListCache } from "./filelist-cache";
import { UrlQuery } from "./url/url-query";

export const DRAG_MOVE_MIME = "application/starsky-move";

export interface IDragMoveData {
  filePaths: string[];
  folderPaths: string[];
}

/**
 * Moves files and folders to a target folder via the rename/move API.
 * Returns true on success, false on error.
 */
export async function moveDragAndDropFiles(
  filePaths: string[],
  folderPaths: string[],
  targetFolderPath: string
): Promise<boolean> {
  const allPaths = [...filePaths, ...folderPaths];
  if (allPaths.length === 0) return false;

  const trimTrailingSlashes = (path: string) => {
    let end = path.length;
    while (end > 0 && path[end - 1] === "/") end--;
    return path.slice(0, end);
  };

  const trimmedTarget =
    targetFolderPath === "/" ? "/" : trimTrailingSlashes(targetFolderPath);

  const toPaths = allPaths.map((selectedPath) => {
    if (!folderPaths.includes(selectedPath)) {
      return targetFolderPath;
    }
    const folderName = new FileExtensions().GetFileName(selectedPath);
    return trimmedTarget === "/" ? `/${folderName}` : `${trimmedTarget}/${folderName}`;
  });

  const bodyParams = new URLSearchParams();
  bodyParams.append("f", allPaths.join(";"));
  bodyParams.append("to", toPaths.join(";"));
  bodyParams.append("collections", "true");

  const result = await FetchPost(new UrlQuery().UrlDiskRename(), bodyParams.toString());

  if (!Array.isArray(result.data) || result.data.length === 0 || result.statusCode !== 200) {
    return false;
  }

  new FileListCache().CacheCleanEverything();
  return true;
}
