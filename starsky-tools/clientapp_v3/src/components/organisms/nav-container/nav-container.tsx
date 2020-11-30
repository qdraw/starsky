import React from 'react';

type NavContainerPropTypes = {
  children?: React.ReactNode;
  hamburgerMenu: boolean;
}

const NavContainer: React.FunctionComponent<NavContainerPropTypes> = ({ children, hamburgerMenu }) => {
  return (
    <nav className={hamburgerMenu ? "nav open" : "nav"}>
      <div className="nav__container">
        <div className="menu">
          {children}
        </div>
      </div>
    </nav>
  );
};

export default NavContainer
