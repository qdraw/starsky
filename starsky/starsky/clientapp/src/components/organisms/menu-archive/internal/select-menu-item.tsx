import React from "react";
import useGlobalSettings from "../../../../hooks/use-global-settings.ts";
import localization from "../../../../localization/localization.json";
import { Language } from "../../../../shared/language.ts";

interface ISelectMenuItemProps {
  select?: string[];
  removeSidebarSelection: () => void;
  toggleLabels: (state?: boolean) => void;
}

export const SelectMenuItem: React.FunctionComponent<ISelectMenuItemProps> = ({
  select,
  removeSidebarSelection,
  toggleLabels
}) => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  // Content
  const MessageSelectAction = language.key(localization.MessageSelectAction);
  const MessageLabels = language.key(localization.MessageLabels);

  return (
    <>
      {!select ? (
        <button
          className="item item--select"
          data-test="menu-item-select"
          onClick={() => {
            removeSidebarSelection();
          }}
          onKeyDown={(event) => {
            if (event.key === "Enter") {
              removeSidebarSelection();
            }
          }}
        >
          {MessageSelectAction}
        </button>
      ) : null}

      {select ? (
        <button
          className="item item--labels"
          data-test="menu-archive-labels"
          onClick={() => toggleLabels()}
          onKeyDown={(event) => {
            if (event.key === "Enter") {
              toggleLabels();
            }
          }}
        >
          {MessageLabels}
        </button>
      ) : null}
    </>
  );
};
