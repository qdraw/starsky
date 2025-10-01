import React, { memo } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { ILanguageLocalization } from "../../../interfaces/ILanguageLocalization";
import { Language } from "../../../shared/language";

interface IMenuOptionModalProps {
  isReadOnly: boolean;
  testName: string;
  isSet: boolean;
  set: React.Dispatch<React.SetStateAction<boolean>>;
  localization?: ILanguageLocalization;
  setEnableMoreMenu?: React.Dispatch<React.SetStateAction<boolean>>;
  children?: React.ReactNode;
}

const MenuOptionModal: React.FunctionComponent<IMenuOptionModalProps> = memo(
  ({
    localization,
    isSet,
    set,
    testName,
    isReadOnly = true,
    setEnableMoreMenu = undefined,
    children = undefined
  }) => {
    const settings = useGlobalSettings();
    const language = new Language(settings.language);

    const message = localization ? language.key(localization) : "";

    function onClickHandler() {
      if (isReadOnly) {
        return;
      }
      // close menu
      if (setEnableMoreMenu) {
        setEnableMoreMenu(false);
      }
      set(!isSet);
    }

    return (
      <>
        {
          <li className={isReadOnly ? "menu-option disabled" : "menu-option"}>
            <button
              data-test={testName}
              onClick={onClickHandler}
              onKeyDown={(event) => {
                if (event.key === "Enter") {
                  onClickHandler();
                }
              }}
            >
              {message}
              {children}
            </button>
          </li>
        }
      </>
    );
  }
);

export default MenuOptionModal;
