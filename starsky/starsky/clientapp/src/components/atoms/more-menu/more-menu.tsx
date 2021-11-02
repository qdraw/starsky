import React, { useEffect } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { Language } from "../../../shared/language";

type MoreMenuPropTypes = {
  children?: React.ReactNode;
  defaultEnableMenu?: boolean;
};

export const MoreMenuEventCloseConst = "CLOSE_MORE_MENU";

const MoreMenu: React.FunctionComponent<MoreMenuPropTypes> = ({
  children,
  defaultEnableMenu
}) => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageMore = language.text("Meer", "More");
  const [enabledMenu, setEnabledMenu] = React.useState(defaultEnableMenu);

  function toggleMoreMenu() {
    if (!children) return;
    setEnabledMenu(!enabledMenu);
  }

  var offMoreMenu = () => setEnabledMenu(false);

  useEffect(() => {
    // Bind the event listener
    window.addEventListener(MoreMenuEventCloseConst, offMoreMenu);

    return () => {
      // Unbind the event listener on clean up
      window.removeEventListener(MoreMenuEventCloseConst, offMoreMenu);
    };
  });

  return (
    <button
      className={!children ? "item item--more disabled" : "item item--more"}
      onClick={toggleMoreMenu}
    >
      <span>{MessageMore}</span>
      <div
        onChange={offMoreMenu}
        onClick={toggleMoreMenu}
        data-test="menu-context"
        className={
          enabledMenu ? "menu-context" : "menu-context menu-context--hide"
        }
      >
        <ul data-test="menu-options" className="menu-options">
          {children}
        </ul>
      </div>
    </button>
  );
};

export default MoreMenu;
