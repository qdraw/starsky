import React from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import localization from "../../../localization/localization.json";
import { Language } from "../../../shared/language";

type MoreMenuPropTypes = {
  children?: React.ReactNode;
  enableMoreMenu?: boolean;
  setEnableMoreMenu: React.Dispatch<boolean>;
};

const MoreMenu: React.FunctionComponent<MoreMenuPropTypes> = ({
  children,
  enableMoreMenu,
  setEnableMoreMenu
}) => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageMore = language.key(localization.MessageMore);

  const offMoreMenu = () => setEnableMoreMenu(false);

  return (
    <>
      <button
        data-test="menu-menu-button"
        className={!children ? "item item--more disabled" : "item item--more"}
        onClick={() => {
          setEnableMoreMenu(true);
        }}
      >
        <span>{MessageMore}</span>
      </button>
      {/* NoSonar(S6848) */}
      <div
        onChange={offMoreMenu}
        onClick={() => setEnableMoreMenu(false)}
        onKeyDown={(event) => {
          event.key === "Enter" && setEnableMoreMenu(false);
        }}
        data-test="menu-context"
        className={enableMoreMenu ? "menu-context" : "menu-context menu-context--hide"}
      >
        <ul data-test="menu-options" className="menu-options">
          {children}
        </ul>
      </div>
    </>
  );
};

export default MoreMenu;
