import React, { memo } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { Language } from "../../../shared/language";

interface IMenuOptionProps {
  isReadOnly: boolean;
  testName: string;
  isSet: boolean;
  set: React.Dispatch<React.SetStateAction<boolean>>;
  localization: { nl: string; en: string };
  setEnableMoreMenu?: React.Dispatch<React.SetStateAction<boolean>>;
}

const MenuOption: React.FunctionComponent<IMenuOptionProps> = memo(
  ({
    localization,
    isSet,
    set,
    testName,
    isReadOnly = true,
    setEnableMoreMenu = undefined
  }) => {
    const settings = useGlobalSettings();
    const language = new Language(settings.language);
    const Message = language.key(localization);

    return (
      <>
        {
          <li
            tabIndex={0}
            data-test={testName}
            className={!isReadOnly ? "menu-option" : "menu-option disabled"}
            onClick={() => {
              if (!isReadOnly) {
                // close menu
                if (setEnableMoreMenu) {
                  setEnableMoreMenu(false);
                }
                set(!isSet);
              }
            }}
          >
            {Message}
          </li>
        }
      </>
    );
  }
);

export default MenuOption;
