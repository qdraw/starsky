import React from 'react';
import HamburgerMenuToggle from '../../atoms/hamburger-menu-toggle/hamburger-menu-toggle';
import MenuSearchBar from '../../molecules/menu-inline-search/menu-inline-search';
import NavContainer from '../nav-container/nav-container';

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

          <NavContainer hamburgerMenu={hamburgerMenu}>
            <MenuSearchBar callback={() => setHamburgerMenu(!hamburgerMenu)} />
          </NavContainer>

        </div>
      </header>
    </>);
};

export default MenuDefault
