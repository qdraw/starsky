import { ArchiveAction } from "../../../../contexts/archive-context";
import { IArchiveProps } from "../../../../interfaces/IArchiveProps";
import Preloader from "../../../atoms/preloader/preloader";
import { IPreviewState } from "../hooks/use-preview-state";
import { ITimezoneState } from "../hooks/use-timezone-state";
import { executeShift } from "./execute-shift";
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
    timezones,
    recordedTimezone, // t
    setRecordedTimezone,
    correctTimezone,
    setCorrectTimezone
  } = timezoneState;

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

  return (
    <>
      <div className="modal content--subheader">Change Location</div>
      <div className="modal content--text">
        <div className="timezone-inputs">
          <div className="form-row">
            <label>
              Original location:
              <select
                value={recordedTimezone}
                onChange={(e) => {
                  setRecordedTimezone(e.target.value);
                  generateTimezonePreview(
                    select,
                    state,
                    e.target.value,
                    correctTimezone,
                    setIsLoadingPreview,
                    preview,
                    setPreview,
                    setError
                  );
                }}
              >
                {timezones.map((tz) => (
                  <option key={tz.id} value={tz.id}>
                    {tz.displayName}
                  </option>
                ))}
              </select>
            </label>
          </div>

          <div className="form-row">
            <label>
              New location:
              <select
                value={correctTimezone}
                onChange={(e) => {
                  setCorrectTimezone(e.target.value);
                  generateTimezonePreview(
                    select,
                    state,
                    recordedTimezone,
                    e.target.value,
                    setIsLoadingPreview,
                    preview,
                    setPreview,
                    setError
                  );
                }}
              >
                {timezones.map((tz) => (
                  <option key={tz.id} value={tz.id}>
                    {tz.displayName}
                  </option>
                ))}
              </select>
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
