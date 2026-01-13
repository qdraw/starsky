import { useEffect, useState } from "react";
import { ArchiveAction } from "../../../contexts/archive-context";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { IBatchRenameItem } from "../../../interfaces/IBatchRenameItem";
import { IBatchRenameRequest } from "../../../interfaces/IBatchRenameRequest";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import localization from "../../../localization/localization.json";
import FetchPost from "../../../shared/fetch/fetch-post";
import { FileExtensions } from "../../../shared/file-extensions";
import { FileListCache } from "../../../shared/filelist-cache";
import { Language } from "../../../shared/language";
import { ClearSearchCache } from "../../../shared/search/clear-search-cache";
import { URLPath } from "../../../shared/url/url-path";
import { UrlQuery } from "../../../shared/url/url-query";
import Modal from "../../atoms/modal/modal";

export interface IModalBatchRenameProps {
  isOpen: boolean;
  handleExit: () => void;
  select: string[];
  historyLocationSearch: string;
  state: IArchiveProps;
  dispatch: React.Dispatch<ArchiveAction>;
  undoSelection: () => void;
}

const BATCH_RENAME_PATTERNS_KEY = "batch-rename-patterns";
const DEFAULT_PATTERN = "{yyyy}{MM}{dd}{HH}{mm}{ss}_{filenamebase}.{ext}";

const ModalBatchRename: React.FunctionComponent<IModalBatchRenameProps> = (props) => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  const MessageBatchRenamePhotos = language.key(localization.MessageBatchRenamePhotos);
  const MessageBatchRenameEnterPattern = language.key(localization.MessageBatchRenameEnterPattern);
  const MessageBatchRenameRecentPatterns = language.key(
    localization.MessageBatchRenameRecentPatterns
  );
  const MessageBatchRenamePhotosCount = language.key(localization.MessageBatchRenamePhotosCount);
  const MessageBatchRenamePreview = language.key(localization.MessageBatchRenamePreview);
  const MessageBatchRenameLoadingPreview = language.key(
    localization.MessageBatchRenameLoadingPreview
  );
  const MessageBatchRenameError = language.key(localization.MessageBatchRenameError);
  const MessageCancel = language.key(localization.MessageCancel);

  // State management
  const [pattern, setPattern] = useState(DEFAULT_PATTERN);
  const [recentPatterns, setRecentPatterns] = useState<string[]>([]);
  const [preview, setPreview] = useState<IBatchRenameItem[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [isPreviewLoading, setIsPreviewLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [previewGenerated, setPreviewGenerated] = useState(false);

  // Load recent patterns from localStorage
  useEffect(() => {
    const stored = localStorage.getItem(BATCH_RENAME_PATTERNS_KEY);
    if (stored) {
      try {
        setRecentPatterns(JSON.parse(stored));
      } catch (e) {
        console.error("Failed to parse recent patterns", e);
      }
    }
  }, []);

  /**
   * Handle pattern selection from dropdown
   */
  function handlePatternSelect(selectedPattern: string) {
    setPattern(selectedPattern);
    setPreviewGenerated(false);
    setPreview([]);
    setError(null);
  }

  /**
   * Generate preview of batch rename
   */
  async function generatePreview() {
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

  /**
   * Execute batch rename
   */
  async function executeBatchRename() {
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

  /**
   * Render preview list (first 2 and last one)
   */
  function renderPreviewList() {
    if (preview.length === 0) return null;

    const displayItems: IBatchRenameItem[] = [];

    // Add first item
    if (preview[0]) {
      displayItems.push(preview[0]);
    }

    // Add second item if exists
    if (preview.length > 1 && preview[1]) {
      displayItems.push(preview[1]);
    }

    // Add ellipsis if there are more than 3 items
    if (preview.length > 3) {
      displayItems.push({
        sourceFilePath: "...",
        targetFilePath: "...",
        relatedFilePaths: [],
        sequenceNumber: 0,
        hasError: false
      });
    }

    // Add last item if not already added
    if (preview.length > 2 && preview[preview.length - 1]) {
      displayItems.push(preview[preview.length - 1]);
    }

    return (
      <div className="batch-rename-preview-list">
        {displayItems.map((item, index) => {
          if (item.sourceFilePath === "...") {
            return (
              <div key={`ellipsis-${index}`} className="preview-item preview-ellipsis">
                ...
              </div>
            );
          }

          const fileName = new FileExtensions().GetFileName(item.sourceFilePath);
          const targetFileName = new FileExtensions().GetFileName(item.targetFilePath);

          return (
            <div
              key={`${item.sourceFilePath}-${index}`}
              className={`preview-item ${item.hasError ? "preview-item--error" : ""}`}
            >
              <span className="preview-source">{fileName}</span>
              <span className="preview-arrow">→</span>
              <span className="preview-target">{targetFileName}</span>
              {item.hasError && (
                <span className="preview-error-message">{item.errorMessage || "Error"}</span>
              )}
            </div>
          );
        })}
      </div>
    );
  }

  return (
    <Modal
      id="batch-rename-modal"
      isOpen={props.isOpen}
      handleExit={() => {
        props.handleExit();
      }}
    >
      <div className="content" data-test="modal-batch-rename">
        <div className="modal content--subheader">{MessageBatchRenamePhotos}</div>
        <div className="modal content--text">
          {/* Pattern input */}
          <label className="label-batch-rename">{MessageBatchRenameEnterPattern}</label>
          <input
            type="text"
            value={pattern}
            onChange={(e) => {
              setPattern(e.target.value);
              setPreviewGenerated(false);
              setPreview([]);
              setError(null);
            }}
            placeholder={DEFAULT_PATTERN}
            data-test="input-batch-rename-pattern"
            className="input-batch-rename-pattern"
            disabled={isLoading || isPreviewLoading}
          />

          {/* Recent patterns dropdown */}
          {recentPatterns.length > 0 && (
            <div className="batch-rename-recent-patterns">
              <label>{MessageBatchRenameRecentPatterns}</label>
              <select
                onChange={(e) => handlePatternSelect(e.target.value)}
                defaultValue=""
                className="select-batch-rename-patterns"
                disabled={isLoading || isPreviewLoading}
              >
                <option value="">-- Select a pattern --</option>
                {recentPatterns.map((p, index) => (
                  <option key={`pattern-${index}`} value={p}>
                    {p}
                  </option>
                ))}
              </select>
            </div>
          )}

          {/* Count of photos */}
          <div className="batch-rename-count">
            <strong>
              {props.select.length} {MessageBatchRenamePhotosCount}
            </strong>
          </div>

          {/* Preview section */}
          {!previewGenerated ? (
            <button
              onClick={generatePreview}
              disabled={isPreviewLoading || !pattern.trim()}
              className="btn btn--default btn-preview"
            >
              {isPreviewLoading ? MessageBatchRenameLoadingPreview : MessageBatchRenamePreview}
            </button>
          ) : (
            <>
              {/* Render preview list */}
              {renderPreviewList()}

              {/* Error messages from preview */}
              {preview.some((item) => item.hasError) && (
                <div className="warning-box batch-rename-errors">
                  <strong>Errors found:</strong>
                  {preview
                    .filter((item) => item.hasError)
                    .map((item, index) => (
                      <div key={`error-${index}`} className="error-item">
                        {new FileExtensions().GetFileName(item.sourceFilePath)}: {item.errorMessage}
                      </div>
                    ))}
                </div>
              )}

              {/* Action buttons */}
              <div className="batch-rename-button-group">
                <button
                  onClick={() => {
                    setPreviewGenerated(false);
                    setPreview([]);
                  }}
                  className="btn btn--secondary"
                  disabled={isLoading}
                >
                  {MessageBatchRenamePreview}
                </button>
                <button
                  onClick={executeBatchRename}
                  disabled={isLoading || preview.some((item) => item.hasError)}
                  className="btn btn--default"
                >
                  {isLoading ? "Loading..." : MessageBatchRenameError}
                </button>
              </div>
            </>
          )}

          {/* Error display */}
          {error && (
            <div
              data-test="modal-batch-rename-error-box"
              className="warning-box--under-form warning-box"
            >
              {error}
            </div>
          )}

          {/* Cancel button */}
          <button
            onClick={() => props.handleExit()}
            className="btn btn--cancel"
            disabled={isLoading || isPreviewLoading}
          >
            {MessageCancel}
          </button>
        </div>
      </div>
    </Modal>
  );
};

export default ModalBatchRename;
