import { IArchiveProps } from "../../../../interfaces/IArchiveProps";
import {
  IBatchRenameOffsetRequest,
  IBatchRenameResult,
  IBatchRenameTimezoneRequest
} from "../../../../interfaces/IBatchRename";
import FetchPost from "../../../../shared/fetch/fetch-post";
import { URLPath } from "../../../../shared/url/url-path";
import { UrlQuery } from "../../../../shared/url/url-query";

export interface LoadRenamePreviewParams {
  mode: "offset" | "timezone";
  select: string[];
  state: IArchiveProps;
  collections: boolean;
  offsetData?: {
    year: number;
    month: number;
    day: number;
    hour: number;
    minute: number;
    second: number;
  };
  timezoneData?: {
    recordedTimezoneId: string;
    correctTimezoneId: string;
  };
  setIsLoadingRename: (value: boolean) => void;
  setRenamePreview: (value: IBatchRenameResult[]) => void;
  setRenameError: (value: string | null) => void;
}

export async function loadRenamePreview(params: LoadRenamePreviewParams): Promise<void> {
  const {
    mode,
    select,
    state,
    collections,
    offsetData,
    timezoneData,
    setIsLoadingRename,
    setRenamePreview,
    setRenameError
  } = params;

  const filePathList = new URLPath().MergeSelectFileIndexItem(select, state.fileIndexItems);

  setIsLoadingRename(true);
  setRenameError(null);

  try {
    let url: string;
    let body: IBatchRenameOffsetRequest | IBatchRenameTimezoneRequest;

    if (mode === "offset" && offsetData) {
      url = new UrlQuery().UrlBatchRenameOffsetPreview();
      body = {
        filePaths: filePathList,
        collections,
        correctionRequest: offsetData
      };
    } else if (mode === "timezone" && timezoneData) {
      url = new UrlQuery().UrlBatchRenameTimezonePreview();
      body = {
        filePaths: filePathList,
        collections,
        correctionRequest: timezoneData
      };
    } else {
      setRenameError("Invalid mode or missing data");
      setIsLoadingRename(false);
      return;
    }

    const result = await FetchPost(url, JSON.stringify(body), "post", {
      "Content-Type": "application/json"
    });

    if (result.statusCode === 200 && result.data) {
      setRenamePreview(result.data as IBatchRenameResult[]);
    } else {
      setRenameError("Failed to load preview");
    }
  } catch {
    setRenameError("Failed to load preview");
  } finally {
    setIsLoadingRename(false);
  }
}
