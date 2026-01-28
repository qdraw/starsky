function formatOffsetLabel() {
  const parts: string[] = [];
  if (offsetYears !== 0) parts.push(`${offsetYears > 0 ? "+" : ""}${offsetYears} years`);
  if (offsetMonths !== 0) parts.push(`${offsetMonths > 0 ? "+" : ""}${offsetMonths} months`);
  if (offsetDays !== 0) parts.push(`${offsetDays > 0 ? "+" : ""}${offsetDays} days`);
  if (offsetHours !== 0) parts.push(`${offsetHours > 0 ? "+" : ""}${offsetHours} hours`);
  if (offsetMinutes !== 0) parts.push(`${offsetMinutes > 0 ? "+" : ""}${offsetMinutes} minutes`);
  if (offsetSeconds !== 0) parts.push(`${offsetSeconds > 0 ? "+" : ""}${offsetSeconds} seconds`);

  if (parts.length === 0) return "No shift";
  return parts.join(", ");
}

export function renderOffsetMode() {
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
                  setOffsetYears(parseInt(e.target.value) || 0);
                  generateOffsetPreview(
                    select,
                    state,
                    {
                      year: parseInt(e.target.value) || 0,
                      month: offsetMonths,
                      day: offsetDays,
                      hour: offsetHours,
                      minute: offsetMinutes,
                      second: offsetSeconds
                    },
                    setIsLoadingPreview,
                    setError,
                    setPreview
                  );
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
                  setOffsetMonths(parseInt(e.target.value) || 0);
                  generateOffsetPreview(
                    select,
                    state,
                    {
                      year: offsetYears,
                      month: parseInt(e.target.value) || 0,
                      day: offsetDays,
                      hour: offsetHours,
                      minute: offsetMinutes,
                      second: offsetSeconds
                    },
                    setIsLoadingPreview,
                    setError,
                    setPreview
                  );
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
                  setOffsetDays(parseInt(e.target.value) || 0);
                  generateOffsetPreview(
                    select,
                    state,
                    {
                      year: offsetYears,
                      month: offsetMonths,
                      day: parseInt(e.target.value) || 0,
                      hour: offsetHours,
                      minute: offsetMinutes,
                      second: offsetSeconds
                    },
                    setIsLoadingPreview,
                    setError,
                    setPreview
                  );
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
                  setOffsetHours(parseInt(e.target.value) || 0);
                  generateOffsetPreview(
                    select,
                    state,
                    {
                      year: offsetYears,
                      month: offsetMonths,
                      day: offsetDays,
                      hour: parseInt(e.target.value) || 0,
                      minute: offsetMinutes,
                      second: offsetSeconds
                    },
                    setIsLoadingPreview,
                    setError,
                    setPreview
                  );
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
                  setOffsetMinutes(parseInt(e.target.value) || 0);
                  generateOffsetPreview(
                    select,
                    state,
                    {
                      year: offsetYears,
                      month: offsetMonths,
                      day: offsetDays,
                      hour: offsetHours,
                      minute: parseInt(e.target.value) || 0,
                      second: offsetSeconds
                    },
                    setIsLoadingPreview,
                    setError,
                    setPreview
                  );
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
                  setOffsetSeconds(parseInt(e.target.value) || 0);
                  generateOffsetPreview(
                    select,
                    state,
                    {
                      year: offsetYears,
                      month: offsetMonths,
                      day: offsetDays,
                      hour: offsetHours,
                      minute: offsetMinutes,
                      second: parseInt(e.target.value) || 0
                    },
                    setIsLoadingPreview,
                    setError,
                    setPreview
                  );
                }}
              />
            </label>
          </div>
        </div>

        <hr />

        <div className="preview-section">
          <h3>Preview</h3>

          {isLoadingPreview ? "Loading..." : ""}

          {preview.length > 0 && (
            <div className="preview-result">
              <p>
                <strong>Original:</strong>{" "}
                {parseDate(preview[0]?.originalDateTime, SupportedLanguages.nl, false)}{" "}
                {parseTime(preview[0]?.originalDateTime)}
              </p>
              <p>
                <strong>Result:</strong>{" "}
                {parseDate(preview[0]?.correctedDateTime, SupportedLanguages.nl, false)}{" "}
                {parseTime(preview[0]?.correctedDateTime)}
              </p>
              <p>
                <strong>Applied shift:</strong> {formatOffsetLabel()}
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
            onClick={executeShift}
            disabled={isExecuting || preview.length === 0}
          >
            {isExecuting ? "Applying..." : "Apply Shift"}
          </button>
        </div>
      </div>
    </>
  );
}
