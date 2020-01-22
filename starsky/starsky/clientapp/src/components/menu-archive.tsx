
import React, { memo, useEffect } from 'react';
import { ArchiveContext } from '../contexts/archive-context';
import useGlobalSettings from '../hooks/use-global-settings';
import useLocation from '../hooks/use-location';
import FetchPost from '../shared/fetch-post';
import { Language } from '../shared/language';
import { URLPath } from '../shared/url-path';
import { UrlQuery } from '../shared/url-query';
import DropArea from './drop-area';
import MenuSearchBar from './menu.searchbar';
import ModalArchiveMkdir from './modal-archive-mkdir';
import ModalDisplayOptions from './modal-display-options';
import ModalExport from './modal-export';
import MoreMenu from './more-menu';

interface IMenuArchiveProps {
}

const MenuArchive: React.FunctionComponent<IMenuArchiveProps> = memo(() => {

  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  // Content
  const MessageSelectAction = language.text("Selecteer", "Select");
  const MessageSelectPresentPerfect = language.text("geselecteerd", "selected");
  const MessageNoneSelected = language.text("Niets geselecteerd", "Nothing selected");
  const MessageMkdir = language.text("Map maken", "Create folder");
  const MessageDisplayOptions = language.text("Weergave opties", "Display options");
  const MessageUndoSelection = language.text("Undo selectie", "Undo selection");
  const MessageSelectFurther = language.text("Verder selecteren", "Select further");
  const MessageSelectAll = language.text("Alles selecteren", "Select all");
  const MessageMoveToTrash = language.text("Verplaats naar prullenmand", "Move to Trash");

  const [hamburgerMenu, setHamburgerMenu] = React.useState(false);
  let { state, dispatch } = React.useContext(ArchiveContext);

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

  // Select All items
  function selectAll() {
    if (!select) return;
    var updatedSelect = new URLPath().GetAllSelection(select, state.fileIndexItems);

    var urlObject = new URLPath().updateSelection(history.location.search, updatedSelect);
    setSelect(urlObject.select);
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true });
  }

  // Undo Selection
  function undoSelection() {
    var urlObject = new URLPath().updateSelection(history.location.search, []);
    setSelect(urlObject.select);
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true });
  }

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

  async function moveToTrashSelection() {
    if (!select) return;

    var toUndoTrashList = new URLPath().MergeSelectFileIndexItem(select, state.fileIndexItems);
    if (!toUndoTrashList) return;
    var selectParams = new URLPath().ArrayToCommaSeperatedStringOneParent(toUndoTrashList, "");
    if (selectParams.length === 0) return;

    var bodyParams = new URLSearchParams();

    bodyParams.append("f", selectParams);
    bodyParams.set("Tags", "!delete!");
    bodyParams.set("append", "true");
    bodyParams.set("Colorclass", "8");

    var resultDo = await FetchPost(new UrlQuery().UrlUpdateApi(), bodyParams.toString());
    if (resultDo.statusCode !== 404 && resultDo.statusCode !== 200) {
      return;
    }
    undoSelection();
    dispatch({ 'type': 'remove', 'filesList': toUndoTrashList })
  }


  const [isDisplayOptionsOpen, setDisplayOptionsOpen] = React.useState(false);
  const [isModalMkdirOpen, setModalMkdirOpen] = React.useState(false);

  return (
    <>
      {/* Modals  */}
      {isModalExportOpen ? <ModalExport handleExit={() =>
        setModalExportOpen(!isModalExportOpen)} select={new URLPath().MergeSelectParent(select, new URLPath().StringToIUrl(history.location.search).f)}
        isOpen={isModalExportOpen} /> : null}

      {isDisplayOptionsOpen ? <ModalDisplayOptions parentFolder={new URLPath().StringToIUrl(history.location.search).f} handleExit={() =>
        setDisplayOptionsOpen(!isDisplayOptionsOpen)} isOpen={isDisplayOptionsOpen} /> : null}

      {isModalMkdirOpen ? <ModalArchiveMkdir handleExit={() => setModalMkdirOpen(!isModalMkdirOpen)} isOpen={isModalMkdirOpen} /> : null}

      {/* Menu */}
      <header className={sidebar ? "header header--main header--select header--edit" : select ? "header header--main header--select" : "header header--main "}>
        <div className="wrapper">
          {!select ? <button data-test="hamburger" className="hamburger__container" onClick={() => setHamburgerMenu(!hamburgerMenu)}>
            <div className={hamburgerMenu ? "hamburger open" : "hamburger"}>
              <i />
              <i />
              <i />
            </div>
          </button> : null}

          {select && select.length === 0 ? <button data-test="selected-0" onClick={() => { selectToggle() }}
            className="item item--first item--close">{MessageNoneSelected}</button> : null}
          {select && select.length >= 1 ? <button data-test={`selected-${select.length}`} onClick={() => { selectToggle() }}
            className="item item--first item--close">{select.length} {MessageSelectPresentPerfect}</button> : null}
          {!select ? <div className="item item--select" onClick={() => { selectToggle() }}>
            {MessageSelectAction}
          </div> : null}

          {select ? <div className="item item--labels" onClick={() => toggleLabels()}>Labels</div> : null}

          {/* default more menu */}
          {!select ? <MoreMenu>
            <li className="menu-option" data-test="mkdir" onClick={() => setModalMkdirOpen(!isModalMkdirOpen)}>{MessageMkdir}</li>
            <li className="menu-option" onClick={() => setDisplayOptionsOpen(!isDisplayOptionsOpen)}>{MessageDisplayOptions}</li>
            <li className="menu-option menu-option--input">
              <DropArea endpoint={new UrlQuery().UrlUploadApi()} folderPath={""} enableInputButton={true} enableDragAndDrop={true}></DropArea>
            </li>
          </MoreMenu> : null}

          {/* In the select context there are more options */}
          {select ? <MoreMenu>
            {select.length === state.fileIndexItems.length ? <li className="menu-option" onClick={() => undoSelection()}>{MessageUndoSelection}</li> : null}
            {select.length !== state.fileIndexItems.length ? <li className="menu-option" onClick={() => selectAll()}>{MessageSelectAll}</li> : null}
            <li className="menu-option" onClick={() => setModalExportOpen(!isModalExportOpen)}>Download</li>
            <li className="menu-option" onClick={() => moveToTrashSelection()}>{MessageMoveToTrash}</li>
            <li className="menu-option menu-option--input">
              <DropArea endpoint={new UrlQuery().UrlUploadApi()} folderPath={""} enableInputButton={true} enableDragAndDrop={true}></DropArea>
            </li>

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

    </>);
});

export default MenuArchive

