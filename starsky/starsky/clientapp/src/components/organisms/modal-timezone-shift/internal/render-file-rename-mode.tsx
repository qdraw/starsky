import React, { useEffect } from "react";
import { ArchiveAction } from "../../../../contexts/archive-context";
import useGlobalSettings from "../../../../hooks/use-global-settings";
import { IArchiveProps } from "../../../../interfaces/IArchiveProps";
import {
  IBatchRenameOffsetRequest,
  IBatchRenameTimezoneRequest
} from "../../../../interfaces/IBatchRename";
import { IFileIndexItem } from "../../../../interfaces/IFileIndexItem";
import localization from "../../../../localization/localization.json";
import FetchPost from "../../../../shared/fetch/fetch-post";
import { FileExtensions } from "../../../../shared/file-extensions";
import { Language } from "../../../../shared/language";
import { URLPath } from "../../../../shared/url/url-path";
import { UrlQuery } from "../../../../shared/url/url-query";
import Preloader from "../../../atoms/preloader/preloader";
import { IFileRenameState } from "../hooks/use-file-rename-state";
import { loadRenamePreview } from "./load-rename-preview";

export interface IRenderFileRenameModeProps {
  select: string[];
  state: IArchiveProps;
  fileRenameState: IFileRenameState;
  handleExit: () => void;
  dispatch: React.Dispatch<ArchiveAction>;
  historyLocationSearch: string;
  undoSelection: () => void;
  collections: boolean;
  mode: "offset" | "timezone";
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
}

export const FileRenameMode: React.FC<IRenderFileRenameModeProps> = (props) => {
  const {
    select,
    state,
    fileRenameState,
    handleExit,
    dispatch,
    undoSelection,
    collections,
    mode,
    offsetData,
    timezoneData
  } = props;

  const {
    shouldRename,
    setShouldRename,
    renamePreview,
    setRenamePreview,
    isLoadingRename,
    setIsLoadingRename,
    isExecutingRename,
    setIsExecutingRename,
    renameError,
    setRenameError
  } = fileRenameState;

  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  const filePathList = new URLPath().MergeSelectFileIndexItem(select, state.fileIndexItems);

  // Load rename preview when component mounts
  useEffect(() => {
    loadRenamePreview({
      mode,
      select,
      state,
      collections,
      offsetData,
      timezoneData,
      setIsLoadingRename,
      setRenamePreview,
      setRenameError
    });
  }, []); // Empty dependency array - only load once on mount

  const handleExecuteRename = async () => {
    if (!shouldRename) {
      // If not renaming, just close the modal (shift already executed)
      undoSelection();
      handleExit();
      return;
    }

    // Execute rename
    setIsExecutingRename(true);
    setRenameError(null);

    try {
      let url: string;
      let body: IBatchRenameOffsetRequest | IBatchRenameTimezoneRequest;

      if (mode === "offset" && offsetData) {
        url = new UrlQuery().UrlBatchRenameOffsetExecute();
        body = {
          filePaths: filePathList,
          collections,
          correctionRequest: offsetData
        };
      } else if (mode === "timezone" && timezoneData) {
        url = new UrlQuery().UrlBatchRenameTimezoneExecute();
        body = {
          filePaths: filePathList,
          collections,
          correctionRequest: timezoneData
        };
      } else {
        setRenameError("Invalid mode or missing data");
        setIsExecutingRename(false);
        return;
      }

      const result = await FetchPost(url, JSON.stringify(body), "post", {
        "Content-Type": "application/json"
      });

      if (result.statusCode === 200 && result.data) {
        // Update the archive state with the new file items
        dispatch({ type: "add", add: result.data as IFileIndexItem[] });
        undoSelection();
        handleExit();
      } else {
        setRenameError("Failed to rename files");
      }
    } catch {
      setRenameError(language.key(localization.MessageErrorGenericFail));
    } finally {
      setIsExecutingRename(false);
    }
  };

  const hasErrors = renamePreview.some((item) => item.hasError);

  return (
    <>
      <div className="modal content--subheader">
        {language.key(localization.MessageRenameFiles)}
      </div>
      <div className="modal content--text">
        <div className="form-row">
          <label className="custom-checkbox">
            <input
              type="checkbox"
              checked={shouldRename}
              onChange={(e) => setShouldRename(e.target.checked)}
            />
            <span className="custom-checkbox-box" />
            {language.key(localization.MessageRenameFilesAfterShiftingTimestamps)}
          </label>
        </div>

        {isLoadingRename ? (
          <Preloader isWhite={false} isOverlay={false} />
        ) : (
          <>
            {shouldRename && renamePreview.length > 0 && (
              <>
                <p>&nbsp;</p>
                <h2>{language.key(localization.MessagePreviewOfFileChanges)}</h2>

                <div className="batch-rename-preview-list">
                  {renamePreview.map((item, index) => {
                    if (!item || !item.sourceFilePath) return null;
                    const fileName = new FileExtensions().GetFileName(item.sourceFilePath);
                    const targetFileName = new FileExtensions().GetFileName(item.targetFilePath);

                    return (
                      <div
                        key={`${item.sourceFilePath}-${index}`}
                        className={`preview-item ${item.hasError ? "preview-item--error" : ""}`}
                      >
                        <span className="preview-source">{fileName}</span>
                        {targetFileName && (
                          <>
                            <span className="preview-arrow">→</span>
                            <span className="preview-target">{targetFileName}</span>
                          </>
                        )}
                        {item.hasError && (
                          <span className="preview-error-message">
                            {item.errorMessage || "Error"}
                          </span>
                        )}
                      </div>
                    );
                  })}
                </div>
                {hasErrors && (
                  <p className="warning">
                    ⚠ {language.key(localization.MessageSomeFilesHaveErrors)}
                  </p>
                )}
                {!hasErrors && (
                  <p className="warning">
                    ⚠ {language.key(localization.MessageExistingFilenamesWillBeReplaced)}
                  </p>
                )}
              </>
            )}
          </>
        )}

        {renameError && <p className="error">{renameError}</p>}

        <div className="modal-buttons">
          {/* DO NOT Implement handleback because then you re-apply the excute-shift */}
          <button className="btn btn--info" disabled={true}>
            {language.key(localization.MessageBack)}
          </button>
          <button
            className="btn btn--default"
            onClick={handleExecuteRename}
            disabled={isExecutingRename || (shouldRename && (isLoadingRename || hasErrors))}
          >
            {isExecutingRename
              ? language.key(localization.MessageLoading)
              : shouldRename
                ? language.key(localization.MessageReplace)
                : language.key(localization.MessageFinish)}
          </button>
        </div>
      </div>
    </>
  );
};

// Legacy function export for backwards compatibility
export function renderFileRenameMode(props: IRenderFileRenameModeProps) {
  return <FileRenameMode {...props} />;
}
