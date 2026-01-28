import React from "react";
import { ITimezone, ITimezoneShiftResult } from "../../../../interfaces/ITimezone";
import { executeShift } from "./execute-shift";
import { generateTimezonePreview } from "./generate-timezone-preview";

export function renderTimezoneMode(
  select: string[],
  recordedTimezone: string,
  correctTimezone: string,
  timezones: ITimezone[],
  setRecordedTimezone: React.Dispatch<React.SetStateAction<string>>,
  setCorrectTimezone: React.Dispatch<React.SetStateAction<string>>,
  setPreview: React.Dispatch<React.SetStateAction<ITimezoneShiftResult[]>>,
  preview: ITimezoneShiftResult[],
  isLoadingPreview: boolean,
  setError: React.Dispatch<React.SetStateAction<string | null>>,
  error: string | null,
  handleBack: () => void,
  isExecuting: boolean,
  setIsLoadingPreview: React.Dispatch<React.SetStateAction<boolean>>
) {
  return (
    <div className="modal-timezone-shift">
      <h2>Change Location</h2>

      <div className="timezone-inputs">
        <div className="form-row">
          <label>
            Original location:
            <select value={recordedTimezone} onChange={(e) => setRecordedTimezone(e.target.value)}>
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
            <select value={correctTimezone} onChange={(e) => setCorrectTimezone(e.target.value)}>
              {timezones.map((tz) => (
                <option key={tz.id} value={tz.id}>
                  {tz.displayName}
                </option>
              ))}
            </select>
          </label>
        </div>
      </div>

      <div className="preview-section">
        <h3>Preview</h3>

        {!preview.length && (
          <button
            className="btn btn--default"
            onClick={() =>
              generateTimezonePreview(
                select,
                setIsLoadingPreview,
                recordedTimezone,
                setPreview,
                setIsLoadingPreview,
                setError
              )
            }
            disabled={isLoadingPreview || !recordedTimezone || !correctTimezone}
          >
            {isLoadingPreview ? "Loading..." : "Generate Preview"}
          </button>
        )}

        {preview.length > 0 && (
          <div className="preview-result">
            <p>
              <strong>Original:</strong> {preview[0]?.originalDateTime || "N/A"} ({recordedTimezone}
              )
            </p>
            <p>
              <strong>New time:</strong> {preview[0]?.correctedDateTime || "N/A"} ({correctTimezone}
              )
            </p>
            <p>
              <strong>Time shift:</strong> {preview[0]?.delta || "N/A"} (DST-aware)
            </p>
            {preview[0]?.warning && <p className="warning">⚠️ {preview[0].warning}</p>}
            {preview[0]?.error && <p className="error">❌ {preview[0].error}</p>}
          </div>
        )}

        {error && <p className="error">{error}</p>}
      </div>

      <div className="modal-buttons">
        <button className="btn btn--info" onClick={handleBack}>
          Back
        </button>
        <button
          className="btn btn--default"
          onClick={() => executeShift()}
          disabled={isExecuting || preview.length === 0}
        >
          {isExecuting ? "Applying..." : "Apply Shift"}
        </button>
      </div>
    </div>
  );
}
