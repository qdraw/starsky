import { ArchiveAction } from "../../../../contexts/archive-context";
import useGlobalSettings from "../../../../hooks/use-global-settings";
import { IArchiveProps } from "../../../../interfaces/IArchiveProps";
import localization from "../../../../localization/localization.json";
import { parseDate, parseTime } from "../../../../shared/date";
import { Language } from "../../../../shared/language";
import { URLPath } from "../../../../shared/url/url-path";
import Preloader from "../../../atoms/preloader/preloader";
import SearchableDropdown from "../../../atoms/searchable-dropdown";
import { IPreviewState } from "../hooks/use-preview-state";
import { ITimezoneState } from "../hooks/use-timezone-state";
import { executeShift } from "./execute-shift";
import { fetchCityTimezones } from "./fetch-city-timezones";
import { generateTimezonePreview } from "./generate-timezone-preview";
import { PreviewErrorFiles } from "./preview-error-files";

export interface IRenderTimezoneModeProps {
  select: string[];
  state: IArchiveProps;
  timezoneState: ITimezoneState;
  previewState: IPreviewState;
  handleBack: () => void;
  handleExit: () => void;
  dispatch: React.Dispatch<ArchiveAction>;
  historyLocationSearch: string;
  undoSelection: () => void;
  collections: boolean;
}

export function renderTimezoneMode(props: IRenderTimezoneModeProps) {
  const {
    select,
    state,
    timezoneState,
    previewState,
    handleBack,
    handleExit,
    dispatch,
    historyLocationSearch,
    undoSelection,
    collections
  } = props;
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
    recordedTimezoneId,
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
  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  return (
    <>
      <div className="modal content--subheader">
        {language.key(localization.MessageChangeLocation)}
      </div>
      <div className="modal content--text">
        <div className="timezone-inputs">
          <div className="form-row">
            <label>
              {language.key(localization.MessageOriginalCity)}:
              <SearchableDropdown
                fetchResults={(city) => fetchCityTimezones(firstItemDateTime, city)}
                placeholder={language.key(localization.MessageSearchOrSelect)}
                noResultsText={language.key(localization.MessageNoResultsFound)}
                defaultValue={recordedTimezoneDisplayName}
                onSelect={(id, displayName) => {
                  setRecordedTimezoneId(id);
                  setRecordedTimezoneDisplayName(displayName);
                  generateTimezonePreview({
                    filePathList,
                    recordedTimezoneId: id,
                    correctTimezoneId,
                    setIsLoadingPreview,
                    preview,
                    setPreview,
                    setError,
                    collections
                  });
                }}
              />
            </label>
          </div>

          <div className="form-row">
            <label>
              {language.key(localization.MessageNewCity)}:
              <SearchableDropdown
                fetchResults={(city) => fetchCityTimezones(firstItemDateTime, city)}
                defaultValue={correctTimezoneDisplayName}
                placeholder={language.key(localization.MessageSearchOrSelect)}
                noResultsText={language.key(localization.MessageNoResultsFound)}
                onSelect={(id, displayName) => {
                  setCorrectTimezoneId(id);
                  setCorrectTimezoneDisplayName(displayName);
                  generateTimezonePreview({
                    filePathList,
                    recordedTimezoneId,
                    correctTimezoneId: id,
                    setIsLoadingPreview,
                    preview,
                    setPreview,
                    setError,
                    collections
                  });
                }}
              />
            </label>
          </div>
        </div>

        {isLoadingPreview ? <Preloader isWhite={false} isOverlay={false} /> : ""}

        <div className="preview-section">
          {recordedTimezoneId && correctTimezoneId && preview.timezoneData.length > 0 && (
            <>
              <h3>{language.key(localization.MessagePreview)}</h3>

              <div className="preview-result">
                <p>
                  <strong>{language.key(localization.MessageOriginal)}:</strong>
                  {parseDate(
                    preview.timezoneData[0]?.originalDateTime,
                    settings.language,
                    false
                  )}{" "}
                  {parseTime(preview.timezoneData[0]?.originalDateTime)}
                </p>
                <p>
                  <strong>{language.key(localization.MessageNewTimeResult)}:</strong>{" "}
                  {parseDate(preview.timezoneData[0]?.correctedDateTime, settings.language, false)}{" "}
                  {parseTime(preview.timezoneData[0]?.correctedDateTime)}
                </p>
                <p>
                  <strong>{language.key(localization.MessageAppliedShift)}:</strong>{" "}
                  {preview.timezoneData[0]?.delta || "N/A"} (
                  {language.key(localization.MessageDstAware)})
                </p>

                {/* Display errors */}
                {PreviewErrorFiles(preview.timezoneData)}
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
                  isOffset: false,
                  timezoneData: {
                    recordedTimezoneId,
                    correctTimezoneId
                  },
                  historyLocationSearch
                },
                setIsExecuting,
                setError,
                handleExit,
                undoSelection,
                dispatch
              )
            }
            disabled={
              isExecuting ||
              preview.timezoneData.length === 0 ||
              preview.timezoneData.some((x) => x.error)
            }
          >
            {isExecuting
              ? language.key(localization.MessageLoading)
              : language.key(localization.MessageApplyShift)}{" "}
          </button>
        </div>
      </div>
    </>
  );
}
