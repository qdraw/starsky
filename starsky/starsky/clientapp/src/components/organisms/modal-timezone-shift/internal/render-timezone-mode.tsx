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
    recordedTimezone, // t
    setRecordedTimezone,
    correctTimezone,
    setCorrectTimezone
  } = timezoneState;

      const filePathList = new URLPath().MergeSelectFileIndexItem(select, state.fileIndexItems);
      const firstItem = state.fileIndexItems.find(x => x.filePath === filePathList[0]);
      const firstItemDateTime = firstItem?.dateTime ?? new Date().toISOString()
  

  return (
    <>
      <div className="modal content--subheader">Change Location</div>
      <div className="modal content--text">
        <div className="timezone-inputs">
          <div className="form-row">
                        <label>
              Original city:
            <SearchableDropdown
              fetchResults={(city) => 
                fetchCityTimezones(firstItemDateTime, city)}
              placeholder="Search or select..."
              onSelect={(value) => {
                                  setCorrectTimezone(value);
                  generateTimezonePreview(
                    select,
                    state,
                    correctTimezone,
                    value,
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
  placeholder="Search or select..."
  onSelect={(value) => {
                                  setRecordedTimezone(value);
                  generateTimezonePreview(
                    select,
                    state,
                    value,
                    correctTimezone,
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
                  {recordedTimezone})
                </p>
                <p>
                  <strong>New time:</strong> {preview.timezoneData[0]?.correctedDateTime || "N/A"} (
                  {correctTimezone})
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
                    recordedTimezone,
                    correctTimezone
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
