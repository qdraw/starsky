import React, { memo } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { Language } from "../../../shared/language";

interface IMenuOptionProps {
  testName: string;
  isSet: boolean;
  set: React.Dispatch<React.SetStateAction<boolean>>;
  nl: string;
  en: string;
}

const MenuOption: React.FunctionComponent<IMenuOptionProps> = memo(
  ({ nl, en, isSet, set, testName }) => {
    const settings = useGlobalSettings();
    const language = new Language(settings.language);
    const Message = language.text(nl, en);

    return (
      <>
        {
          <li
            tabIndex={0}
            data-test={testName}
            className="menu-option"
            onClick={() => set(!isSet)}
          >
            {Message}
          </li>
        }
      </>
    );
  }
);

export default MenuOption;
