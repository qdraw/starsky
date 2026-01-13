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
      previewGenerated,
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
              data-test="button-batch-rename-generate-preview"
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
