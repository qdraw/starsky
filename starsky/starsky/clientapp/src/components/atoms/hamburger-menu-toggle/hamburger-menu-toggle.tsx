import React from "react";

/**
 * When select is true, the menu will be disabled
 */
type HamburgerMenuPropTypes = {
  select?: string[] | boolean;
  hamburgerMenu: boolean;
  setHamburgerMenu(option: boolean): void;
};

/**
 * When select is true, the menu will be disabled
 * @param param0 type HamburgerMenuPropTypes
 */
const HamburgerMenuToggle: React.FunctionComponent<HamburgerMenuPropTypes> = ({
  select,
  hamburgerMenu,
  setHamburgerMenu
}) => {
  const className = hamburgerMenu ? "hamburger open" : "hamburger";
  return (
    <>
      {!select ? (
        <button
          data-test="hamburger"
          className="hamburger-menu-toggle hamburger__container"
          onClick={() => setHamburgerMenu(!hamburgerMenu)}
        >
          <div aria-hidden="true" className={className}>
            <i />
            <i />
            <i />
          </div>
          <div className="text">Menu</div>
        </button>
      ) : null}
    </>
  );
};

export default HamburgerMenuToggle;
