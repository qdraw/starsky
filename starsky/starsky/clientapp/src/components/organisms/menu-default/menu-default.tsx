import React from 'react';
import HamburgerMenuToggle from '../../atoms/hamburger-menu-toggle/hamburger-menu-toggle';
import MenuSearchBar from '../../molecules/menu-inline-search/menu-inline-search';

interface IMenuDefaultProps {
  isEnabled: boolean;
}

const MenuDefault: React.FunctionComponent<IMenuDefaultProps> = (props) => {
  const [hamburgerMenu, setHamburgerMenu] = React.useState(false);

  return (
    <>
      <header className={"header header--main"}>
        <div className="wrapper">

          <HamburgerMenuToggle select={false} hamburgerMenu={hamburgerMenu} setHamburgerMenu={setHamburgerMenu} />

          <nav className={hamburgerMenu ? "nav open" : "nav"}>
            <div className="nav__container">
              <ul className="menu">
                <MenuSearchBar callback={() => setHamburgerMenu(!hamburgerMenu)} />
              </ul>
            </div>
          </nav>
        </div>
      </header>
    </>);
};

export default MenuDefault
