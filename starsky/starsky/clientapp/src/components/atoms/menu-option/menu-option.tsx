import React, { memo } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { Language } from "../../../shared/language";

interface IMenuOptionProps {
  isReadOnly: boolean;
  testName: string;
  onClickKeydown: () => void;
  localization?: { nl: string; en: string };
  children?: React.ReactNode;
}

const MenuOption: React.FunctionComponent<IMenuOptionProps> = memo(
  ({
    localization,
    onClickKeydown,
    testName,
    isReadOnly = true,
    children = undefined
  }) => {
    const settings = useGlobalSettings();
    const language = new Language(settings.language);
    const Message = !localization ? "" : language.key(localization);

    return (
      <>
        {
          <li className={!isReadOnly ? "menu-option" : "menu-option disabled"}>
            <button
              data-test={testName}
              onClick={onClickKeydown}
              onKeyDown={(event) => {
                event.key === "Enter" && onClickKeydown();
              }}
            >
              {Message}
              {children}
            </button>
          </li>
        }
      </>
    );
  }
);

export default MenuOption;
