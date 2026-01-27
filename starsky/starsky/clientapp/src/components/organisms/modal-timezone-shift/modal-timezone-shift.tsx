import { useEffect, useState } from "react";
import { ArchiveAction } from "../../../contexts/archive-context";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { ITimezone, ITimezoneShiftResult } from "../../../interfaces/ITimezone";
import { parseDate, parseTime } from "../../../shared/date";
import FetchGet from "../../../shared/fetch/fetch-get";
import FetchPost from "../../../shared/fetch/fetch-post";
import { SupportedLanguages } from "../../../shared/language";
import { URLPath } from "../../../shared/url/url-path";
import { UrlQuery } from "../../../shared/url/url-query";
import Modal from "../../atoms/modal/modal";
import { generateOffsetPreview } from "./generate-offset-preview";

export interface IModalTimezoneShiftProps {
  isOpen: boolean;
  handleExit: () => void;
  select: string[];
  historyLocationSearch: string;
  state: IArchiveProps;
  dispatch: React.Dispatch<ArchiveAction>;
  undoSelection: () => void;
}

type ShiftMode = "mode-selection" | "offset" | "timezone";

const ModalTimezoneShift: React.FunctionComponent<IModalTimezoneShiftProps> = ({
  isOpen,
  handleExit,
  select,
  state
}) => {
  const urlQuery = new UrlQuery();

  // Mode and step tracking
  const [currentStep, setCurrentStep] = useState<ShiftMode>("mode-selection");

  // Offset mode state
  const [offsetYears, setOffsetYears] = useState(0);
  const [offsetMonths, setOffsetMonths] = useState(0);
  const [offsetDays, setOffsetDays] = useState(0);
  const [offsetHours, setOffsetHours] = useState(0);
  const [offsetMinutes, setOffsetMinutes] = useState(0);
  const [offsetSeconds, setOffsetSeconds] = useState(0);

  // Timezone mode state
  const [timezones, setTimezones] = useState<ITimezone[]>([]);
  const [recordedTimezone, setRecordedTimezone] = useState("");
  const [correctTimezone, setCorrectTimezone] = useState("");

  // Preview and execution state
  const [preview, setPreview] = useState<ITimezoneShiftResult[]>([]);
  const [isLoadingPreview, setIsLoadingPreview] = useState(false);
  const [isExecuting, setIsExecuting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Load timezones when entering timezone mode
  useEffect(() => {
    if (currentStep === "timezone" && timezones.length === 0) {
      loadTimezones();
    }
  }, [currentStep]);

  async function loadTimezones() {
    try {
      const response = await FetchGet(urlQuery.UrlTimezones());
      if (response.statusCode === 200 && Array.isArray(response.data)) {
        setTimezones(response.data);
        // Set default values
        if (response.data.length > 0) {
          const europeLondon = response.data.find((tz: ITimezone) => tz.id === "Europe/London");
          const europeAmsterdam = response.data.find(
            (tz: ITimezone) => tz.id === "Europe/Amsterdam"
          );
          setRecordedTimezone(europeLondon?.id || response.data[0].id);
          setCorrectTimezone(europeAmsterdam?.id || response.data[0].id);
        }
      }
    } catch (err) {
      console.error("Failed to load timezones", err);
      setError("Failed to load timezones");
    }
  }

  async function generateTimezonePreview() {
    if (select.length === 0) return;

    const collections = true;

    setIsLoadingPreview(true);
    setError(null);

    try {
      // Use first file as representative sample
      const sampleFile = select[0];
      const collectionsParam = collections ? "true" : "false";

      const body = JSON.stringify({
        recordedTimezone,
        correctTimezone
      });

      const response = await FetchPost(
        `${urlQuery.UrlTimezonePreview()}?f=${new URLPath().encodeURI(sampleFile)}&collections=${collectionsParam}`,
        body,
        "post",
        { "Content-Type": "application/json" }
      );

      if (response.statusCode === 200 && Array.isArray(response.data)) {
        setPreview(response.data);
      } else {
        setError("Failed to generate preview");
      }
    } catch (err) {
      console.error("Failed to generate preview", err);
      setError("Failed to generate preview");
    } finally {
      setIsLoadingPreview(false);
    }
  }

  async function executeShift() {
    if (select.length === 0) return;

    setIsExecuting(true);
    setError(null);

    try {
      const collections = true;
      const collectionsParam = collections ? "true" : "false";
      const isOffset = currentStep === "offset";

      const body = isOffset
        ? JSON.stringify({
            year: offsetYears,
            month: offsetMonths,
            day: offsetDays,
            hour: offsetHours,
            minute: offsetMinutes,
            second: offsetSeconds
          })
        : JSON.stringify({
            recordedTimezone,
            correctTimezone
          });

      const url = isOffset ? urlQuery.UrlOffsetExecute() : urlQuery.UrlTimezoneExecute();

      // Execute for all selected files
      const promises = select.map((file) =>
        FetchPost(
          `${url}?f=${new URLPath().encodeURI(file)}&collections=${collectionsParam}`,
          body,
          "post",
          { "Content-Type": "application/json" }
        )
      );

      const results = await Promise.all(promises);
      const allSucceeded = results.every((r) => r.statusCode === 200);

      if (allSucceeded) {
        // Success - close modal and refresh
        handleExit();
      } else {
        setError("Some files failed to update");
      }
    } catch (err) {
      console.error("Failed to execute shift", err);
      setError("Failed to execute shift");
    } finally {
      setIsExecuting(false);
    }
  }

  function handleBack() {
    if (currentStep === "offset" || currentStep === "timezone") {
      setCurrentStep("mode-selection");
      setPreview([]);
      setError(null);
    }
  }

  function handleModeSelect(mode: "offset" | "timezone") {
    setCurrentStep(mode);
    setPreview([]);
    setError(null);
  }

  function renderModeSelection() {
    return (
      <>
        <div className="modal content--subheader">Shift Photo Timestamps</div>
        <div className="modal content--text">
          <p>
            You have selected {select.length} image{select.length !== 1 ? "s" : ""}
          </p>

          <div className="mode-selection">
            <p>What do you want to do?</p>

            <label className="radio-option">
              <input
                type="radio"
                name="shift-mode"
                value="offset"
                onChange={() => handleModeSelect("offset")}
              />
              <div>
                <strong>Correct incorrect camera timezone</strong>
                <p>The camera clock was set to the wrong timezone.</p>
              </div>
            </label>

            <label className="radio-option">
              <input
                type="radio"
                name="shift-mode"
                value="timezone"
                onChange={() => handleModeSelect("timezone")}
              />
              <div>
                <strong>I moved to a different place</strong>
                <p>I traveled and took photos in another location.</p>
              </div>
            </label>
          </div>

          <div className="modal-buttons">
            <button className="btn btn--default" onClick={handleExit}>
              Cancel
            </button>
          </div>
        </div>
      </>
    );
  }

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

  function renderOffsetMode() {
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

  function renderTimezoneMode() {
    return (
      <div className="modal-timezone-shift">
        <h2>Change Location</h2>

        <div className="timezone-inputs">
          <div className="form-row">
            <label>
              Original location:
              <select
                value={recordedTimezone}
                onChange={(e) => setRecordedTimezone(e.target.value)}
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
              onClick={generateTimezonePreview}
              disabled={isLoadingPreview || !recordedTimezone || !correctTimezone}
            >
              {isLoadingPreview ? "Loading..." : "Generate Preview"}
            </button>
          )}

          {preview.length > 0 && (
            <div className="preview-result">
              <p>
                <strong>Original:</strong> {preview[0]?.originalDateTime || "N/A"} (
                {recordedTimezone})
              </p>
              <p>
                <strong>New time:</strong> {preview[0]?.correctedDateTime || "N/A"} (
                {correctTimezone})
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
            onClick={executeShift}
            disabled={isExecuting || preview.length === 0}
          >
            {isExecuting ? "Applying..." : "Apply Shift"}
          </button>
        </div>
      </div>
    );
  }

  // Reset state when modal is closed
  useEffect(() => {
    if (!isOpen) {
      setCurrentStep("mode-selection");
      setPreview([]);
      setError(null);
      setOffsetYears(0);
      setOffsetMonths(0);
      setOffsetDays(0);
      setOffsetHours(0);
      setOffsetMinutes(0);
      setOffsetSeconds(0);
    }
  }, [isOpen]);

  return (
    <Modal isOpen={isOpen} handleExit={handleExit} dataTest="modal-timezone-shift">
      <div className="modal content scroll modal-timezone-shift">
        {currentStep === "mode-selection" && renderModeSelection()}
        {currentStep === "offset" && renderOffsetMode()}
        {currentStep === "timezone" && renderTimezoneMode()}
      </div>
    </Modal>
  );
};

export default ModalTimezoneShift;
