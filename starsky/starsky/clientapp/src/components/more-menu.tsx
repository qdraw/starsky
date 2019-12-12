import React from 'react';

const MoreMenu: React.FunctionComponent = ({ children }) => {
  const [enabledMenu, setEnabledMenu] = React.useState(false);

  function toggle() {
    if (!children) return;
    setEnabledMenu(!enabledMenu);
  }

  return (
    <div className={!children ? "item item--more disabled" : "item item--more"} onClick={() => toggle()}>
      <span>Meer</span>
      <div onClick={() => toggle()} className={enabledMenu ? "menu-context" : "menu-context menu-context--hide"}>
        <ul className="menu-options">
          {children}
        </ul>
      </div>
    </div>
  );
};

export default MoreMenu
