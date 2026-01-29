import { ArchiveAction } from "../../../../contexts/archive-context";
import { IArchiveProps } from "../../../../interfaces/IArchiveProps";
import { URLPath } from "../../../../shared/url/url-path";
import Preloader from "../../../atoms/preloader/preloader";
import SearchableDropdown from "../../../atoms/searchable-dropdown";
import { IPreviewState } from "../hooks/use-preview-state";
import { ITimezoneState } from "../hooks/use-timezone-state";
import { executeShift } from "./execute-shift";
import { fetchCityTimezones } from "./fetch-city-timezones";
import { generateTimezonePreview } from "./generate-timezone-preview";

export function renderTimezoneMode(
  select: string[],
  state: IArchiveProps,
  timezoneState: ITimezoneState,
  previewState: IPreviewState,
  handleBack: () => void,
  handleExit: () => void,
  dispatch: React.Dispatch<ArchiveAction>
) {
  const {
    preview,
    setPreview,
    isLoadingPreview,
    setIsLoadingPreview,
    error,
    setError,
    setIsExecuting,
    isExecuting
  } = previewState;

  const {
    recordedTimezoneId, // t
    setRecordedTimezoneId,
    correctTimezoneId,
    setCorrectTimezoneId,
    recordedTimezoneDisplayName,
    setRecordedTimezoneDisplayName,
    correctTimezoneDisplayName,
    setCorrectTimezoneDisplayName
  } = timezoneState;

  const filePathList = new URLPath().MergeSelectFileIndexItem(select, state.fileIndexItems);
  const firstItem = state.fileIndexItems.find((x) => x.filePath === filePathList[0]);
  const firstItemDateTime = firstItem?.dateTime ?? new Date().toISOString();

  return (
    <>
      <div className="modal content--subheader">Change Location</div>
      <div className="modal content--text">
        <div className="timezone-inputs">
          <div className="form-row">
            <label>
              Original city:
              <SearchableDropdown
                fetchResults={(city) => fetchCityTimezones(firstItemDateTime, city)}
                placeholder="Search or select..."
                defaultValue={recordedTimezoneDisplayName}
                onSelect={(id, displayName) => {
                  setRecordedTimezoneId(id);
                  setRecordedTimezoneDisplayName(displayName);
                  generateTimezonePreview(
                    select,
                    state,
                    id,
                    correctTimezoneId,
                    setIsLoadingPreview,
                    preview,
                    setPreview,
                    setError
                  );
                }}
              />
            </label>
          </div>

          <div className="form-row">
            <label>
              New city:
              <SearchableDropdown
                fetchResults={(city) => fetchCityTimezones(firstItemDateTime, city)}
                defaultValue={correctTimezoneDisplayName}
                placeholder="Search or select..."
                onSelect={(id, displayName) => {
                  setCorrectTimezoneId(id);
                  setCorrectTimezoneDisplayName(displayName);
                  generateTimezonePreview(
                    select,
                    state,
                    id,
                    recordedTimezoneId,
                    setIsLoadingPreview,
                    preview,
                    setPreview,
                    setError
                  );
                }}
              />
            </label>
          </div>
        </div>

        {isLoadingPreview ? <Preloader isWhite={false} isOverlay={false} /> : ""}

        <div className="preview-section">
          {preview.timezoneData.length > 0 && (
            <>
              <h3>Preview</h3>

              <div className="preview-result">
                <p>
                  <strong>Original:</strong> {preview.timezoneData[0]?.originalDateTime || "N/A"} (
                  {recordedTimezoneDisplayName})
                </p>
                <p>
                  <strong>New time:</strong> {preview.timezoneData[0]?.correctedDateTime || "N/A"} (
                  {correctTimezoneDisplayName})
                </p>
                <p>
                  <strong>Time shift:</strong> {preview.timezoneData[0]?.delta || "N/A"} (DST-aware)
                </p>
                {preview.timezoneData[0]?.warning && (
                  <p className="warning">⚠️ {preview.timezoneData[0].warning}</p>
                )}
                {preview.timezoneData[0]?.error && (
                  <p className="error">❌ {preview.timezoneData[0].error}</p>
                )}
              </div>
            </>
          )}

          {error && <p className="error">{error}</p>}
        </div>

        <div className="modal-buttons">
          <button className="btn btn--info" onClick={handleBack}>
            Back
          </button>
          <button
            className="btn btn--default"
            onClick={() =>
              executeShift(
                {
                  select,
                  state,
                  isOffset: false,
                  timezoneData: {
                    recordedTimezoneId,
                    correctTimezoneId
                  }
                },
                setIsExecuting,
                setError,
                handleExit,
                dispatch
              )
            }
            disabled={isExecuting || preview.timezoneData.length === 0}
          >
            {isExecuting ? "Applying..." : "Apply Shift"}
          </button>
        </div>
      </div>
    </>
  );
}
