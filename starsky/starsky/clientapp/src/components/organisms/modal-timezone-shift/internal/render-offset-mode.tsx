import { ArchiveAction } from "../../../../contexts/archive-context";
import useGlobalSettings from "../../../../hooks/use-global-settings";
import { IArchiveProps } from "../../../../interfaces/IArchiveProps";
import localization from "../../../../localization/localization.json";
import { parseDate, parseTime } from "../../../../shared/date";
import { Language } from "../../../../shared/language";
import Preloader from "../../../atoms/preloader/preloader";
import { useOffsetState } from "../hooks/use-offset-state";
import { IPreviewState } from "../hooks/use-preview-state";
import { ShiftMode } from "../hooks/use-shift-mode";
import { executeShift } from "./execute-shift";
import { formatOffsetLabel } from "./format-offset-label";
import { generateOffsetPreview } from "./generate-offset-preview";
import { PreviewErrorFiles } from "./preview-error-files";

export interface IRenderTimezoneModeProps {
  offsetState: ReturnType<typeof useOffsetState>;
  previewState: IPreviewState;
  select: string[];
  state: IArchiveProps;
  handleBack: () => void;
  handleExit: () => void;
  dispatch: React.Dispatch<ArchiveAction>;
  historyLocationSearch: string;
  undoSelection: () => void;
  collections: boolean;
  setCurrentStep?: (step: ShiftMode) => void;
}

export function renderOffsetMode(props: IRenderTimezoneModeProps) {
  const {
    offsetState,
    previewState,
    select,
    state,
    handleBack,
    handleExit,
    dispatch,
    historyLocationSearch,
    undoSelection,
    collections,
    setCurrentStep
  } = props;
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

    generateOffsetPreview({
      select,
      state,
      offset: offsetData,
      setIsLoadingPreview,
      setError,
      preview,
      setPreview,
      collections
    });
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
                {/* Display errors */}
                <PreviewErrorFiles data={preview.offsetData} />{" "}
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
            onClick={async () => {
              if (setCurrentStep) {
                // Execute shift first, then navigate to rename step
                setIsLoadingPreview(true);
                const success = await executeShift(
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
                    },
                    historyLocationSearch: historyLocationSearch
                  },
                  () => {}, // Don't update loading state from here
                  () => {}, // Don't update error from here
                  () => {}, // Don't exit yet
                  () => {}, // Don't undo selection yet
                  dispatch,
                  collections
                );
                setIsLoadingPreview(false);
                if (success !== false) {
                  // Defer navigation to next tick to avoid hook errors
                  setTimeout(() => {
                    setCurrentStep("file-rename-offset");
                  }, 0);
                }
              } else {
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
                    },
                    historyLocationSearch: historyLocationSearch
                  },
                  setIsLoadingPreview,
                  setError,
                  handleExit,
                  undoSelection,
                  dispatch,
                  collections
                );
              }
            }}
            disabled={
              isExecuting ||
              preview.offsetData.length === 0 ||
              preview.offsetData.some((x) => x.error)
            }
          >
            {isExecuting
              ? language.key(localization.MessageLoading)
              : setCurrentStep
                ? language.key(localization.MessageNext)
                : language.key(localization.MessageApplyShift)}
          </button>
        </div>
      </div>
    </>
  );
}
