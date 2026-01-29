import { ArchiveAction } from "../../../../contexts/archive-context";
import { IArchiveProps } from "../../../../interfaces/IArchiveProps";
import { parseDate, parseTime } from "../../../../shared/date";
import { SupportedLanguages } from "../../../../shared/language";
import Preloader from "../../../atoms/preloader/preloader";
import { useOffsetState } from "../hooks/use-offset-state";
import { IPreviewState } from "../hooks/use-preview-state";
import { executeShift } from "./execute-shift";
import { generateOffsetPreview } from "./generate-offset-preview";

function formatOffsetLabel(
  years: number,
  months: number,
  days: number,
  hours: number,
  minutes: number,
  seconds: number
) {
  const parts: string[] = [];
  if (years !== 0) parts.push(`${years > 0 ? "+" : ""}${years} years`);
  if (months !== 0) parts.push(`${months > 0 ? "+" : ""}${months} months`);
  if (days !== 0) parts.push(`${days > 0 ? "+" : ""}${days} days`);
  if (hours !== 0) parts.push(`${hours > 0 ? "+" : ""}${hours} hours`);
  if (minutes !== 0) parts.push(`${minutes > 0 ? "+" : ""}${minutes} minutes`);
  if (seconds !== 0) parts.push(`${seconds > 0 ? "+" : ""}${seconds} seconds`);

  if (parts.length === 0) return "No shift";
  return parts.join(", ");
}

export function renderOffsetMode(
  offsetState: ReturnType<typeof useOffsetState>,
  previewState: IPreviewState,
  select: string[],
  state: IArchiveProps,
  handleBack: () => void,
  handleExit: () => void,
  dispatch: React.Dispatch<ArchiveAction>
) {
  const {
    offsetYears,
    setOffsetYears,
    offsetMonths,
    setOffsetMonths,
    offsetDays,
    setOffsetDays,
    offsetHours,
    setOffsetHours,
    offsetMinutes,
    setOffsetMinutes,
    offsetSeconds,
    setOffsetSeconds
  } = offsetState;

  const {
    preview,
    setPreview,
    isLoadingPreview,
    setIsLoadingPreview,
    isExecuting,
    error,
    setError
  } = previewState;

  const handleOffsetChange = (
    field: "year" | "month" | "day" | "hour" | "minute" | "second",
    value: number
  ) => {
    const offsetData = {
      year: field === "year" ? value : offsetYears,
      month: field === "month" ? value : offsetMonths,
      day: field === "day" ? value : offsetDays,
      hour: field === "hour" ? value : offsetHours,
      minute: field === "minute" ? value : offsetMinutes,
      second: field === "second" ? value : offsetSeconds
    };

    generateOffsetPreview(
      select,
      state,
      offsetData,
      setIsLoadingPreview,
      setError,
      preview,
      setPreview
    );
  };
  return (
    <>
      <div className="modal content--subheader">Correct Camera Time</div>
      <div className="modal content--text">
        <div className="offset-inputs">
          <p>Time offsets (relative to original timestamp)</p>

          <div className="form-row">
            <label>
              Years
              <input
                className="form-control"
                type="number"
                value={offsetYears}
                onChange={(e) => {
                  const value = Number.parseInt(e.target.value) || 0;
                  setOffsetYears(value);
                  handleOffsetChange("year", value);
                }}
              />
            </label>
          </div>

          <div className="form-row">
            <label>
              Months
              <input
                className="form-control"
                type="number"
                value={offsetMonths}
                onChange={(e) => {
                  const value = Number.parseInt(e.target.value) || 0;
                  setOffsetMonths(value);
                  handleOffsetChange("month", value);
                }}
              />
            </label>
          </div>

          <div className="form-row">
            <label>
              Days
              <input
                className="form-control"
                type="number"
                value={offsetDays}
                onChange={(e) => {
                  const value = Number.parseInt(e.target.value) || 0;
                  setOffsetDays(value);
                  handleOffsetChange("day", value);
                }}
              />
            </label>
          </div>

          <div className="form-row">
            <label>
              Hours
              <input
                type="number"
                className="form-control"
                value={offsetHours}
                onChange={(e) => {
                  const value = Number.parseInt(e.target.value) || 0;
                  setOffsetHours(value);
                  handleOffsetChange("hour", value);
                }}
              />
            </label>
          </div>

          <div className="form-row">
            <label>
              Minutes
              <input
                type="number"
                className="form-control"
                value={offsetMinutes}
                onChange={(e) => {
                  const value = Number.parseInt(e.target.value) || 0;
                  setOffsetMinutes(value);
                  handleOffsetChange("minute", value);
                }}
              />
            </label>
          </div>

          <div className="form-row">
            <label>
              Seconds
              <input
                type="number"
                value={offsetSeconds}
                className="form-control"
                onChange={(e) => {
                  const value = Number.parseInt(e.target.value) || 0;
                  setOffsetSeconds(value);
                  handleOffsetChange("second", value);
                }}
              />
            </label>
          </div>
        </div>

        {isLoadingPreview ? <Preloader isWhite={false} isOverlay={false} /> : ""}

        <div className="preview-section">
          {preview.offsetData.length > 0 && (
            <>
              <h3>Preview</h3>
              <div className="preview-result">
                <p>
                  <strong>Original:</strong>{" "}
                  {parseDate(preview.offsetData[0]?.originalDateTime, SupportedLanguages.nl, false)}{" "}
                  {parseTime(preview.offsetData[0]?.originalDateTime)}
                </p>
                <p>
                  <strong>Result:</strong>{" "}
                  {parseDate(
                    preview.offsetData[0]?.correctedDateTime,
                    SupportedLanguages.nl,
                    false
                  )}{" "}
                  {parseTime(preview.offsetData[0]?.correctedDateTime)}
                </p>
                <p>
                  <strong>Applied shift:</strong>{" "}
                  {formatOffsetLabel(
                    offsetYears,
                    offsetMonths,
                    offsetDays,
                    offsetHours,
                    offsetMinutes,
                    offsetSeconds
                  )}
                </p>
                {preview.offsetData[0]?.warning && (
                  <p className="warning">⚠️ {preview.offsetData[0].warning}</p>
                )}
                {preview.offsetData[0]?.error && (
                  <p className="error">❌ {preview.offsetData[0].error}</p>
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
                  isOffset: true,
                  offsetData: {
                    year: offsetYears,
                    month: offsetMonths,
                    day: offsetDays,
                    hour: offsetHours,
                    minute: offsetMinutes,
                    second: offsetSeconds
                  }
                },
                setIsLoadingPreview,
                setError,
                handleExit,
                dispatch
              )
            }
            disabled={isExecuting || preview.offsetData.length === 0}
          >
            {isExecuting ? "Applying..." : "Apply Shift"}
          </button>
        </div>
      </div>
    </>
  );
}
