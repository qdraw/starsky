
import React, { memo, useEffect } from 'react';
import useLocation from '../hooks/use-location';
import { URLPath } from '../shared/url-path';
import MenuSearchBar from './menu.searchbar';
import ModalDisplayOptions from './modal-display-options';
import ModalExport from './modal-export';
import MoreMenu from './more-menu';

interface IMenuArchiveProps {
}

const MenuArchive: React.FunctionComponent<IMenuArchiveProps> = memo(() => {

  const [hamburgerMenu, setHamburgerMenu] = React.useState(false);

  var history = useLocation();

  // Sidebar
  const [sidebar, setSidebar] = React.useState(new URLPath().StringToIUrl(history.location.search).sidebar);
  useEffect(() => {
    setSidebar(new URLPath().StringToIUrl(history.location.search).sidebar)
  }, [history.location.search]);

  function toggleLabels() {
    var urlObject = new URLPath().StringToIUrl(history.location.search);
    urlObject.sidebar = !urlObject.sidebar;

    setSidebar(urlObject.details);
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true })
  }
  const [isModalExportOpen, setModalExportOpen] = React.useState(false);

  // Selection
  const [select, setSelect] = React.useState(new URLPath().StringToIUrl(history.location.search).select);
  useEffect(() => {
    setSelect(new URLPath().StringToIUrl(history.location.search).select)
  }, [history.location.search]);

  function selectToggle() {
    var urlObject = new URLPath().StringToIUrl(history.location.search);
    if (!urlObject.select) {
      urlObject.select = [];
    }
    else {
      delete urlObject.sidebar;
      delete urlObject.select;
    }
    setSelect(urlObject.select);
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true });
  }

  const [isDisplayOptionsOpen, setDisplayOptionsOpen] = React.useState(false);

  return (
    <>
      {isModalExportOpen ? <ModalExport handleExit={() =>
        setModalExportOpen(!isModalExportOpen)} select={new URLPath().MergeSelectParent(select, new URLPath().StringToIUrl(history.location.search).f)}
        isOpen={isModalExportOpen} /> : null}

      {isDisplayOptionsOpen ? <ModalDisplayOptions parentFolder={new URLPath().StringToIUrl(history.location.search).f} handleExit={() =>
        setDisplayOptionsOpen(!isDisplayOptionsOpen)} isOpen={isDisplayOptionsOpen} /> : null}

      <header className={sidebar ? "header header--main header--select header--edit" : select ? "header header--main header--select" : "header header--main "}>
        <div className="wrapper">
          {!select ? <button className="hamburger__container" onClick={() => setHamburgerMenu(!hamburgerMenu)}>
            <div className={hamburgerMenu ? "hamburger open" : "hamburger"}>
              <i></i>
              <i></i>
              <i></i>
            </div>
          </button> : null}

          {select && select.length === 0 ? <a onClick={() => { selectToggle() }}
            className="item item--first item--close">Niks geselecteerd</a> : null}
          {select && select.length >= 1 ? <a onClick={() => { selectToggle() }}
            className="item item--first item--close">{select.length} geselecteerd</a> : null}

          {!select ? <div className="item item--select" onClick={() => { selectToggle() }}>
            Selecteer
            </div> : null}
          {select ? <div className="item item--labels" onClick={() => toggleLabels()}>Labels</div> : null}

          {!select ? <MoreMenu>
            <li className="menu-option disabled" onClick={() => { alert("Map maken werkt nog niet"); }}>Map maken</li>
            <li className="menu-option disabled" onClick={() => { alert("Uploaden werkt nog niet, ga naar importeren in het hoofdmenu"); }}>Uploaden</li>
            <li className="menu-option" onClick={() => setDisplayOptionsOpen(!isDisplayOptionsOpen)}>Weergave opties</li>

          </MoreMenu> : null}

          {/* In the select context there are more options */}
          {select ? <MoreMenu>
            <li className="menu-option" onClick={() => setModalExportOpen(!isModalExportOpen)}>Exporteer</li>
            <li className="menu-option disabled" onClick={() => { alert("Uploaden werkt nog niet, ga naar importeren in het hoofdmenu"); }}>Uploaden</li>
          </MoreMenu> : null}

          <nav className={hamburgerMenu ? "nav open" : "nav"}>
            <div className="nav__container">
              <ul className="menu">
                <MenuSearchBar callback={() => setHamburgerMenu(!hamburgerMenu)}></MenuSearchBar>
              </ul>
            </div>
          </nav>
        </div>
      </header>

      {select ? <div className="header header--sidebar header--border-left">
        <div className="item item--continue" onClick={() => { toggleLabels(); }}>Verder selecteren</div>
      </div> : ""}

    </>);
});

export default MenuArchive

