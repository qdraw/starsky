import React, { useEffect } from 'react';
import { ArchiveContext, defaultStateFallback } from '../../../contexts/archive-context';
import useGlobalSettings from '../../../hooks/use-global-settings';
import useLocation from '../../../hooks/use-location';
import { Language } from '../../../shared/language';
import { Select } from '../../../shared/select';
import { Sidebar } from '../../../shared/sidebar';
import { URLPath } from '../../../shared/url-path';
import MoreMenu from '../../atoms/more-menu/more-menu';
import MenuSearchBar from '../../molecules/menu-inline-search/menu-inline-search';
import ModalDownload from '../modal-download/modal-download';

export const MenuSearch: React.FunctionComponent<any> = (_) => {

  let { state } = React.useContext(ArchiveContext);
  state = defaultStateFallback(state);

  const [hamburgerMenu, setHamburgerMenu] = React.useState(false);

  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  // Content
  const MessageNoneSelected = language.text("Niets geselecteerd", "Nothing selected");
  const MessageSelectPresentPerfect = language.text("geselecteerd", "selected");
  const MessageSelectAction = language.text("Selecteer", "Select");
  const MessageSelectAll = language.text("Alles selecteren", "Select all");
  const MessageUndoSelection = language.text("Undo selectie", "Undo selection");
  const MessageSelectFurther = language.text("Verder selecteren", "Select further");

  // Selection
  var history = useLocation();
  const [select, setSelect] = React.useState(new URLPath().StringToIUrl(history.location.search).select);
  useEffect(() => {
    setSelect(new URLPath().StringToIUrl(history.location.search).select)
  }, [history.location.search]);

  var allSelection = () => new Select(select, setSelect, state, history).allSelection();
  var removeSidebarSelection = () => new Select(select, setSelect, state, history).removeSidebarSelection();
  var undoSelection = () => new Select(select, setSelect, state, history).undoSelection();

  // Sidebar
  const [sidebar, setSidebar] = React.useState(new URLPath().StringToIUrl(history.location.search).sidebar);
  useEffect(() => {
    setSidebar(new URLPath().StringToIUrl(history.location.search).sidebar)
  }, [history.location.search]);
  var toggleLabels = () => new Sidebar(sidebar, setSidebar, history).toggleSidebar()

  // download modal
  const [isModalExportOpen, setModalExportOpen] = React.useState(false);

  return (
    <>
      {/* Modal download */}
      {isModalExportOpen ? <ModalDownload handleExit={() => setModalExportOpen(!isModalExportOpen)}
        select={select ? new URLPath().MergeSelectFileIndexItem(select, state.fileIndexItems) : []}
        collections={false}
        isOpen={isModalExportOpen} /> : null}

      <header className={sidebar ? "header header--main header--select header--edit" : select ? "header header--main header--select" : "header header--main "}>
        <div className="wrapper">

          {!select ? <button className="hamburger__container" onClick={() => setHamburgerMenu(!hamburgerMenu)}>
            <div className={hamburgerMenu ? "hamburger open" : "hamburger"}>
              <i />
              <i />
              <i />
            </div>
          </button> : null}

          {select && select.length === 0 ? <button data-test="selected-0" onClick={() => { removeSidebarSelection() }}
            className="item item--first item--close">{MessageNoneSelected}</button> : null}
          {select && select.length >= 1 ? <button data-test={`selected-${select.length}`} onClick={() => { removeSidebarSelection() }}
            className="item item--first item--close">{select.length} {MessageSelectPresentPerfect}</button> : null}

          {/* te select button with checkbox*/}
          {!select ? <div className={state.fileIndexItems.length >= 1 ?
            "item item--select" : "item item--select disabled"} onClick={() => { removeSidebarSelection() }}>
            {MessageSelectAction}
          </div> : null}

          {/* when selected */}
          {select ? <div className={"item item--labels"} onClick={() => toggleLabels()}>Labels</div> : null}

          {/* More menu - When in normal state */}
          {!select ? <MoreMenu /> : null}

          {/* More menu - In the select context there are more options */}
          {select && select.length === 0 ?
            <MoreMenu>
              <li className="menu-option" onClick={() => allSelection()}>{MessageSelectAll}</li>
            </MoreMenu> : null}

          {/* More menu - When more then 1 item is selected */}
          {select && select.length >= 1 ?
            <MoreMenu>
              {select.length === state.fileIndexItems.length ? <li className="menu-option" onClick={() => undoSelection()}>
                {MessageUndoSelection}</li> : null}
              {select.length !== state.fileIndexItems.length ? <li className="menu-option" onClick={() => allSelection()}>
                {MessageSelectAll}</li> : null}
              <li data-test="export" className="menu-option" onClick={() => setModalExportOpen(!isModalExportOpen)}>Download</li>
            </MoreMenu> : null}

          <nav className={hamburgerMenu ? "nav open" : "nav"}>
            <div className="nav__container">
              <ul className="menu">
                <MenuSearchBar callback={() => setHamburgerMenu(!hamburgerMenu)} />
              </ul>
            </div>
          </nav>
        </div>
      </header>

      {select ? <div className="header header--sidebar header--border-left">
        <div className="item item--continue" onClick={() => { toggleLabels(); }}>{MessageSelectFurther}</div>
      </div> : ""}
    </>
  );
};

export default MenuSearch
