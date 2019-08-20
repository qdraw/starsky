
import React, { memo, useContext } from 'react';
import HistoryContext from '../contexts/history-contexts';
import { IMenuProps } from '../interfaces/IMenuProps';
import { URLPath } from '../shared/url-path';
import { MenuSearchBar } from './menu.searchbar';
import MoreMenu from './more-menu';

const MenuArchive: React.FunctionComponent<IMenuProps> = memo((props) => {

  const [hamburgerMenu, setHamburgerMenu] = React.useState(false);

  function toggleHamburger() {
    setHamburgerMenu(!hamburgerMenu);
  }

  const history = useContext(HistoryContext);
  const [sidebar, setSidebar] = React.useState(new URLPath().StringToIUrl(history.location.hash).sidebar);


  function sidebarToggle() {
    var urlObject = new URLPath().StringToIUrl(history.location.hash);
    if (!urlObject.sidebar) {
      urlObject.sidebar = [];
    }
    else {
      urlObject.sidebar = null;
    }
    setSidebar(urlObject.sidebar)
    history.replace(new URLPath().IUrlToString(urlObject))
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

        {sidebar ? <a onClick={() => { sidebarToggle() }} className="item item--first item--close">{sidebar.length} geselecteerd</a> : null}

        {!sidebar ? <div className="item item--select" onClick={() => { sidebarToggle() }}>
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

