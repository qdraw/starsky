import useGlobalSettings from "../../../hooks/use-global-settings";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import localization from "../../../localization/localization.json";
import { Language } from "../../../shared/language";

export interface IMenuOptionUndoSelectionProps {
  select: string[];
  state?: IArchiveProps;
  allSelection: () => void;
}

export const MenuOptionSelectionAll: React.FunctionComponent<
  IMenuOptionUndoSelectionProps
> = ({ select, state, allSelection }) => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageSelectAll = language.key(localization.MessageSelectAll);

  return (
    <>
      {select.length !== state?.fileIndexItems?.length ? (
        <li
          className="menu-option"
          data-test="select-all"
          onClick={() => allSelection()}
          tabIndex={0}
          onKeyDown={(event) => {
            event.key === "Enter" && allSelection();
          }}
        >
          {MessageSelectAll}
        </li>
      ) : null}
    </>
  );
};
