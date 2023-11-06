import useGlobalSettings from "../../../hooks/use-global-settings";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import localization from "../../../localization/localization.json";
import { Language } from "../../../shared/language";

export interface IMenuOptionSelectionUndoProps {
  select: string[];
  state?: IArchiveProps;
  undoSelection: () => void;
}

export const MenuOptionSelectionUndo: React.FunctionComponent<
  IMenuOptionSelectionUndoProps
> = ({ select, state, undoSelection }) => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageUndoSelection = language.key(localization.MessageUndoSelection);

  return (
    <>
      {select.length === state?.fileIndexItems?.length ? (
        <li
          data-test="undo-selection"
          className="menu-option"
          onClick={() => undoSelection()}
          tabIndex={0}
          onKeyDown={(event) => {
            event.key === "Enter" && undoSelection();
          }}
        >
          {MessageUndoSelection}
        </li>
      ) : null}
    </>
  );
};
