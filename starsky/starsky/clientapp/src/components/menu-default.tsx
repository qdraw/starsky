import React, { memo } from 'react';
import MenuSearchBar from './menu.searchbar';

interface IMenuDefaultProps {
  isEnabled: boolean;
}

const MenuDefault: React.FunctionComponent<IMenuDefaultProps> = memo((props) => {
  const [hamburgerMenu, setHamburgerMenu] = React.useState(false);

  return (
    <>
      <header className={"header header--main"}>
        <div className="wrapper">

          {props.isEnabled ? <button className="hamburger__container" onClick={() => setHamburgerMenu(!hamburgerMenu)}>
            <div className={hamburgerMenu ? "hamburger open" : "hamburger"}>
              <i/>
              <i/>
              <i/>
            </div>
          </button> : null}

          <nav className={hamburgerMenu ? "nav open" : "nav"}>
            <div className="nav__container">
              <ul className="menu">
                <MenuSearchBar callback={() => setHamburgerMenu(!hamburgerMenu)}/>
              </ul>
            </div>
          </nav>
        </div>
      </header>
    </>);
});

export default MenuDefault
