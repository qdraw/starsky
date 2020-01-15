import React from 'react';
import useGlobalSettings from '../hooks/use-globalSettings';
import { Language } from '../shared/language';

const MoreMenu: React.FunctionComponent = ({ children }) => {

  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageMore = language.text("Meer", "More");

  const [enabledMenu, setEnabledMenu] = React.useState(false);

  function toggle() {
    if (!children) return;
    setEnabledMenu(!enabledMenu);
  }

  return (
    <div className={!children ? "item item--more disabled" : "item item--more"} onClick={() => toggle()}>
      <span>{MessageMore}</span>
      <div onClick={() => toggle()} className={enabledMenu ? "menu-context" : "menu-context menu-context--hide"}>
        <ul className="menu-options">
          {children}
        </ul>
      </div>
    </div>
  );
};

export default MoreMenu
