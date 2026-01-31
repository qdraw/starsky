import { ArchiveAction } from "../../../../contexts/archive-context";
import useGlobalSettings from "../../../../hooks/use-global-settings";
import { IArchiveProps } from "../../../../interfaces/IArchiveProps";
import localization from "../../../../localization/localization.json";
import { parseDate, parseTime } from "../../../../shared/date";
import { Language } from "../../../../shared/language";
import Preloader from "../../../atoms/preloader/preloader";
import { useOffsetState } from "../hooks/use-offset-state";
import { IPreviewState } from "../hooks/use-preview-state";
import { executeShift } from "./execute-shift";
import { formatOffsetLabel } from "./format-offset-label";
import { generateOffsetPreview } from "./generate-offset-preview";

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

  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  return (
    <>
      <div className="modal content--subheader">
        {language.key(localization.MessageCorrectCameraTime)}
      </div>
      <div className="modal content--text">
        <div className="offset-inputs">
          <p>{language.key(localization.MessageTimeOffsetsRelativeToOriginalTimestamp)}</p>

          <div className="form-row">
            <label>
              {language.key(localization.MessageYear)}
              <br />
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
              {language.key(localization.MessageMonth)}
              <br />
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
              {language.key(localization.MessageDay)}
              <br />
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
              {language.key(localization.MessageHour)}
              <br />
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
              {language.key(localization.MessageMinute)}
              <br />
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
              {language.key(localization.MessageSecond)}
              <br />
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
              <h3>{language.key(localization.MessagePreview)}</h3>
              <div className="preview-result">
                <p>
                  <strong>{language.key(localization.MessageOriginal)}:</strong>{" "}
                  {parseDate(preview.offsetData[0]?.originalDateTime, settings.language, false)}{" "}
                  {parseTime(preview.offsetData[0]?.originalDateTime)}
                </p>
                <p>
                  <strong>{language.key(localization.MessageNewTimeResult)}:</strong>{" "}
                  {parseDate(preview.offsetData[0]?.correctedDateTime, settings.language, false)}{" "}
                  {parseTime(preview.offsetData[0]?.correctedDateTime)}
                </p>
                <p>
                  <strong>{language.key(localization.MessageAppliedShift)}:</strong>{" "}
                  {formatOffsetLabel(
                    {
                      label: language.key(localization.MessageYears).toLowerCase(),
                      value: offsetYears
                    },
                    {
                      label: language.key(localization.MessageMonths).toLowerCase(),
                      value: offsetMonths
                    },
                    {
                      label: language.key(localization.MessageDays).toLowerCase(),
                      value: offsetDays
                    },
                    {
                      label: language.key(localization.MessageHours).toLowerCase(),
                      value: offsetHours
                    },
                    {
                      label: language.key(localization.MessageMinutes).toLowerCase(),
                      value: offsetMinutes
                    },
                    {
                      label: language.key(localization.MessageSeconds).toLowerCase(),
                      value: offsetSeconds
                    }
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
            {language.key(localization.MessageBack)}
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
            {isExecuting
              ? language.key(localization.MessageLoading)
              : language.key(localization.MessageApplyShift)}
          </button>
        </div>
      </div>
    </>
  );
}
