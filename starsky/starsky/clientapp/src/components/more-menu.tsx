import React from 'react';
import useGlobalSettings from '../hooks/use-global-settings';
import { Language } from '../shared/language';

type MoreMenuPropTypes = {
  children?: React.ReactNode;
}

export const MoreMenuEventCloseConst = "CLOSE_MORE_MENU";

const MoreMenu: React.FunctionComponent<MoreMenuPropTypes> = ({ children }) => {

  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageMore = language.text("Meer", "More");

  const [enabledMenu, setEnabledMenu] = React.useState(false);

  function toggleMoreMenu() {
    if (!children) return;
    setEnabledMenu(!enabledMenu);
  }

  // useEffect(() => {
  //   // Bind the event listener
  //   window.addEventListener(MoreMenuEventCloseConst, () => setEnabledMenu(false));

  //   return () => {
  //     // Unbind the event listener on clean up
  //     window.removeEventListener(MoreMenuEventCloseConst, () => setEnabledMenu(false));
  //   };
  // });

  return (
    <div className={!children ? "item item--more disabled" : "item item--more"} onClick={toggleMoreMenu}>
      <span>{MessageMore}</span>
      <div onClick={toggleMoreMenu} className={enabledMenu ? "menu-context" : "menu-context menu-context--hide"}>
        <ul className="menu-options">
          {children}
        </ul>
      </div>
    </div>
  );
};

export default MoreMenu
