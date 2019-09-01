import React, { memo } from 'react';
import MenuSearchBar from '../components/menu.searchbar';
import { IMenuProps } from '../interfaces/IMenuProps';

const MenuSearch: React.FunctionComponent<IMenuProps> = memo((props) => {
  var sidebar = false;
  var select = false;
  const [hamburgerMenu, setHamburgerMenu] = React.useState(false);

  return (
    <>
      <header className={sidebar ? "header header--main header--select header--edit" : select ? "header header--main header--select" : "header header--main "}>
        <div className="wrapper">

          {!select ? <button className="hamburger__container" onClick={() => setHamburgerMenu(!hamburgerMenu)}>
            <div className={hamburgerMenu ? "hamburger open" : "hamburger"}>
              <i></i>
              <i></i>
              <i></i>
            </div>
          </button> : null}

          <nav className={hamburgerMenu ? "nav open" : "nav"}>
            <div className="nav__container">
              <ul className="menu">
                <MenuSearchBar callback={() => setHamburgerMenu(!hamburgerMenu)}></MenuSearchBar>
              </ul>
            </div>
          </nav>
        </div>
      </header>
    </>);
});

export default MenuSearch
