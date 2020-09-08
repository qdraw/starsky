import React from 'react';

/**
 * When select is true, the menu will be disabled
 */
type HamburgerMenuPropTypes = {
  select?: string[] | boolean
  hamburgerMenu: boolean
  setHamburgerMenu(option: boolean): void;
}

/**
 * When select is true, the menu will be disabled
 * @param param0 type HamburgerMenuPropTypes
 */
const HamburgerMenuToggle: React.FunctionComponent<HamburgerMenuPropTypes> = ({ select, hamburgerMenu, setHamburgerMenu }) => {

  return (
    <>
      {!select ? <button data-test="hamburger"
        className="hamburger-menu-toggle hamburger__container" onClick={() => setHamburgerMenu(!hamburgerMenu)}>
        Enable Navigation Menu
        <div aria-hidden="true" className={hamburgerMenu ? "hamburger open" : "hamburger"}>
          <i />
          <i />
          <i />
        </div>
      </button> : null}
    </>
  );
};

export default HamburgerMenuToggle
