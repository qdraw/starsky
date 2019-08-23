
import React, { memo, useEffect } from 'react';
import useLocation from '../hooks/use-location';
import { IMenuProps } from '../interfaces/IMenuProps';
import { URLPath } from '../shared/url-path';
import { MenuSearchBar } from './menu.searchbar';
import MoreMenu from './more-menu';

const MenuArchive: React.FunctionComponent<IMenuProps> = memo((props) => {

  const [hamburgerMenu, setHamburgerMenu] = React.useState(false);

  function toggleHamburger() {
    setHamburgerMenu(!hamburgerMenu);
  }

  var history = useLocation();
  const [sidebar, setSidebar] = React.useState(new URLPath().StringToIUrl(history.location.search).sidebar);

  const [select, setSelect] = React.useState(new URLPath().StringToIUrl(history.location.search).select);
  useEffect(() => {
    setSelect(new URLPath().StringToIUrl(history.location.search).select)
  }, [history.location.search]);


  function selectToggle() {
    var urlObject = new URLPath().StringToIUrl(history.location.search);
    urlObject.sidebar = !urlObject.sidebar;
    setSidebar(urlObject.sidebar)
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true });
  }

  return (
    <header className="header header--main">
      <div className="wrapper">
        {!sidebar ? <button className="hamburger__container" onClick={() => { toggleHamburger(); }}>
          <div className={hamburgerMenu ? "hamburger open" : "hamburger"}>
            <i></i>
            <i></i>
            <i></i>
          </div>
        </button> : null}

        {sidebar && !select ? <a onClick={() => { selectToggle() }}
          className="item item--first item--close">Geen geselecteerd</a> : null}
        {sidebar && select ? <a onClick={() => { selectToggle() }}
          className="item item--first item--close">{select.length} geselecteerd</a> : null}

        {!sidebar ? <div className="item item--select" onClick={() => { selectToggle() }}>
          Selecteer
        </div> : null}

        {sidebar ? <div className="item item--labels" onClick={() => { }}>Labels</div> : null}

        <MoreMenu>
          <li className="menu-option">Werkt nog niet!</li>
          <li className="menu-option" onClick={() => { alert("Map maken werkt nog niet"); }}>Map maken</li>
          <li className="menu-option" onClick={() => { alert("Uploaden nog niet"); }}>Uploaden</li>
        </MoreMenu>

        <nav className={hamburgerMenu ? "nav open" : "nav"}>
          <div className="nav__container">
            <ul className="menu">
              <MenuSearchBar></MenuSearchBar>
            </ul>
          </div>
        </nav>
      </div>
    </header>);
});

export default MenuArchive

