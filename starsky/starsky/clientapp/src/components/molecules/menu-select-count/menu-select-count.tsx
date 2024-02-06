import useGlobalSettings from "../../../hooks/use-global-settings";
import localization from "../../../localization/localization.json";
import { Language } from "../../../shared/language";

interface IMenuSelectCountProps {
  select?: string[];
  removeSidebarSelection: () => void;
}

export const MenuSelectCount: React.FunctionComponent<IMenuSelectCountProps> = ({
  select,
  removeSidebarSelection
}) => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageNoneSelected = language.key(localization.MessageNoneSelected);

  const MessageSelectPresentPerfect = language.key(localization.MessageSelectPresentPerfect);

  return (
    <>
      {select && select.length === 0 ? (
        <button
          data-test="selected-0"
          onClick={() => {
            removeSidebarSelection();
          }}
          onKeyDown={(event) => {
            event.key === "Enter" && removeSidebarSelection();
          }}
          className="item item--first item--close"
        >
          {MessageNoneSelected}
        </button>
      ) : null}

      {select && select.length >= 1 ? (
        <button
          data-test={`selected-${select.length}`}
          onClick={() => {
            removeSidebarSelection();
          }}
          onKeyDown={(event) => {
            event.key === "Enter" && removeSidebarSelection();
          }}
          className="item item--first item--close"
        >
          {select.length} {MessageSelectPresentPerfect}
        </button>
      ) : null}
    </>
  );
};
