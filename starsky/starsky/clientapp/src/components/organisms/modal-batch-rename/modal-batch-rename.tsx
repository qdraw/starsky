import { useEffect, useState } from "react";
import { ArchiveAction } from "../../../contexts/archive-context";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { IBatchRenameItem } from "../../../interfaces/IBatchRenameItem";
import localization from "../../../localization/localization.json";
import { FileExtensions } from "../../../shared/file-extensions";
import { Language } from "../../../shared/language";
import Modal from "../../atoms/modal/modal";
import { BATCH_RENAME_PATTERNS_KEY } from "./batch-rename-patterns-key";
import { DEFAULT_PATTERN } from "./default-pattern";
import { executeBatchRenameHelper } from "./execute-batch-rename-helper";
import { generatePreviewHelper } from "./generate-preview-helper";

export interface IModalBatchRenameProps {
  isOpen: boolean;
  handleExit: () => void;
  select: string[];
  historyLocationSearch: string;
  state: IArchiveProps;
  dispatch: React.Dispatch<ArchiveAction>;
  undoSelection: () => void;
}

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
  const MessageLoading = language.key(localization.MessageLoading);
  const MessageBatchRenameErrors = language.key(localization.MessageBatchRenameErrors);
  const MessageSelectAPattern = language.key(localization.MessageSelectAPattern);

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

    let hasMiddleErrors = false;
    for (let i = 2; i < preview.length - 1; i++) {
      if (preview[i].hasError) {
        displayItems.push(preview[i]);
        hasMiddleErrors = true;
      }
    }

    // Add ellipsis if there are more than 3 items
    if (preview.length > 3 && !hasMiddleErrors) {
      displayItems.push({
        sourceFilePath: "...",
        targetFilePath: "...",
        relatedFilePaths: [],
        sequenceNumber: 0,
        hasError: false
      });
    }

    // Add last item if not already added
    const lastItem = preview.at(-1);
    if (preview.length > 2 && lastItem) {
      displayItems.push(lastItem);
    }

    return (
      <div className="batch-rename-preview-list">
        {displayItems.map((item, index) => {
          if (item.sourceFilePath === "...") {
            return (
              <div
                key={`ellipsis-${item.sourceFilePath}`}
                className="preview-item preview-ellipsis"
              >
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

  async function generatePreview() {
    generatePreviewHelper(
      pattern,
      setIsPreviewLoading,
      setError,
      setPreview,
      setPreviewGenerated,
      props
    );
  }

  async function executeBatchRename() {
    executeBatchRenameHelper(
      setError,
      preview,
      setIsLoading,
      props,
      pattern,
      recentPatterns,
      setRecentPatterns
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
      <div className="modal content scroll" data-test="modal-batch-rename">
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
                <option value="">-- {MessageSelectAPattern} --</option>
                {recentPatterns.map((p) => (
                  <option key={`pattern-${p}`} value={p}>
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
          {previewGenerated ? (
            <>
              {/* Render preview list */}
              {renderPreviewList()}

              {/* Error messages from preview */}
              {preview.some((item) => item.hasError) && (
                <div className="warning-box batch-rename-errors">
                  <strong>
                    {preview.filter((item) => item.hasError).length} {MessageBatchRenameErrors}
                  </strong>
                </div>
              )}

              {/* Action buttons */}
              <div className="batch-rename-button-group">
                <button
                  onClick={executeBatchRename}
                  disabled={isLoading || preview.some((item) => item.hasError)}
                  className="btn btn--default"
                >
                  {isLoading ? MessageLoading : MessageBatchRenamePhotos}
                </button>
              </div>
            </>
          ) : (
            <button
              onClick={generatePreview}
              disabled={isPreviewLoading || !pattern.trim()}
              data-test="button-batch-rename-generate-preview"
              className="btn btn--default"
            >
              {isPreviewLoading ? MessageLoading : MessageBatchRenamePreview}
            </button>
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
        </div>
      </div>
    </Modal>
  );
};

export default ModalBatchRename;
