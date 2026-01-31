import useGlobalSettings from "../../../../hooks/use-global-settings";
import localization from "../../../../localization/localization.json";
import { Language } from "../../../../shared/language";

export function renderModeSelection(
  select: string[],
  handleModeSelect: (mode: "offset" | "timezone") => void,
  handleExit: () => void
) {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  return (
    <>
      <div className="modal content--subheader" data-test="modal-timezone-shift-header">
        {language.key(localization.MessageShiftPhotoTime)}
      </div>
      <div className="modal content--text">
        <p>
          {language.key(localization.MessageYouHaveSelected)} {select.length}{" "}
          {language.key(
            select.length === 1 ? localization.MessageImage : localization.MessageImages
          )}
        </p>
        <p>&nbsp;</p>

        <div className="mode-selection" data-test="modal-timezone-mode-selection">
          <p>{language.key(localization.MessageWhatDoYouWantToDo)}</p>

          <label className="radio-option">
            <input
              type="radio"
              name="shift-mode"
              value="offset"
              data-test="radio-offset-mode"
              onChange={() => handleModeSelect("offset")}
              aria-label={language.key(localization.MessageCorrectIncorrectCameraTimezone)}
            />
            <div>
              <strong>{language.key(localization.MessageCorrectIncorrectCameraTimezone)}</strong>
              <p>{language.key(localization.MessageCameraClockWrongTimezone)}</p>
            </div>
          </label>

          <label className="radio-option">
            <input
              type="radio"
              name="shift-mode"
              value="timezone"
              data-test="radio-timezone-mode"
              onChange={() => handleModeSelect("timezone")}
              aria-label={language.key(localization.MessageIMovedToDifferentPlace)}
            />
            <div>
              <strong>{language.key(localization.MessageIMovedToDifferentPlace)}</strong>
              <p>{language.key(localization.MessageITraveledAndTookPhotosInAnotherLocation)}</p>
            </div>
          </label>
        </div>

        <div className="modal-buttons">
          <button
            className="btn btn--default"
            data-test="modal-timezone-button-cancel"
            onClick={handleExit}
          >
            {language.key(localization.MessageCancel)}
          </button>
        </div>
      </div>
    </>
  );
}
