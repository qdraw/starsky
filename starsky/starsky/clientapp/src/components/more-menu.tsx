import React, { memo } from 'react';

const MoreMenu: React.FunctionComponent<any> = memo((props) => {
  const [enabledMenu, setEnabledMenu] = React.useState(false);

  function toggle() {
    setEnabledMenu(!enabledMenu);
  }
  return (
    <div className="item item--more" onClick={() => toggle()}>
      <span>Meer</span>
      <div onClick={() => toggle()} className={enabledMenu ? "menu-context" : "menu-context menu-context--hide"}>
        <ul className="menu-options">
          {/* <li className="menu-option" onClick={() => { alert("hi"); }}>Back</li> */}
          {props.children}
        </ul>
      </div>
    </div>
  );
});

export default MoreMenu
