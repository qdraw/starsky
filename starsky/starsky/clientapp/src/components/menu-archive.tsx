
import React, { memo, useEffect } from 'react';
import { ArchiveContext, defaultStateFallback } from '../contexts/archive-context';
import useGlobalSettings from '../hooks/use-global-settings';
import useLocation from '../hooks/use-location';
import { newIFileIndexItemArray } from '../interfaces/IFileIndexItem';
import FetchPost from '../shared/fetch-post';
import { Language } from '../shared/language';
import { Select } from '../shared/select';
import { Sidebar } from '../shared/sidebar';
import { URLPath } from '../shared/url-path';
import { UrlQuery } from '../shared/url-query';
import DropArea from './drop-area';
import MenuSearchBar from './menu.searchbar';
import ModalArchiveMkdir from './modal-archive-mkdir';
import ModalDisplayOptions from './modal-display-options';
import ModalDropAreaFilesAdded from './modal-drop-area-files-added';
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
  const MessageSelectFurther = language.text("Verder selecteren", "Select further");
  const MessageSelectAll = language.text("Alles selecteren", "Select all");
  const MessageUndoSelection = language.text("Undo selectie", "Undo selection");
  const MessageMoveToTrash = language.text("Verplaats naar prullenmand", "Move to Trash");

  const [hamburgerMenu, setHamburgerMenu] = React.useState(false);
  let { state, dispatch } = React.useContext(ArchiveContext);
  state = defaultStateFallback(state);

  var history = useLocation();

  var allSelection = () => new Select(select, setSelect, state, history).allSelection();
  var undoSelection = () => new Select(select, setSelect, state, history).undoSelection();
  var removeSidebarSelection = () => new Select(select, setSelect, state, history).removeSidebarSelection();

  /* only update when the state is changed */
  const [isReadOnly, setReadOnly] = React.useState(state.isReadOnly);
  useEffect(() => {
    setReadOnly(state.isReadOnly);
  }, [state.isReadOnly]);

  // Sidebar
  const [sidebar, setSidebar] = React.useState(new URLPath().StringToIUrl(history.location.search).sidebar);
  useEffect(() => {
    setSidebar(new URLPath().StringToIUrl(history.location.search).sidebar)
  }, [history.location.search]);

  var toggleLabels = () => new Sidebar(sidebar, setSidebar, history).toggleSidebar()

  const [isModalExportOpen, setModalExportOpen] = React.useState(false);

  // Selection
  const [select, setSelect] = React.useState(new URLPath().StringToIUrl(history.location.search).select);
  useEffect(() => {
    setSelect(new URLPath().StringToIUrl(history.location.search).select)
  }, [history.location.search]);

  async function moveToTrashSelection() {
    if (!select || isReadOnly) return;

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
    dispatch({ 'type': 'remove', toRemoveFileList: toUndoTrashList })
  }

  const [isDisplayOptionsOpen, setDisplayOptionsOpen] = React.useState(false);
  const [isModalMkdirOpen, setModalMkdirOpen] = React.useState(false);
  const [dropAreaUploadFilesList, setDropAreaUploadFilesList] = React.useState(newIFileIndexItemArray());

  const UploadMenuItem = () => {
    if (isReadOnly) return <li data-test="upload" className="menu-option disabled">Upload</li>
    return <li className="menu-option menu-option--input">
      <DropArea callback={(add) => {
        setDropAreaUploadFilesList(add);
        dispatch({ 'type': 'add', add });
      }}
        endpoint={new UrlQuery().UrlUploadApi()}
        folderPath={state.subPath} enableInputButton={true}
        enableDragAndDrop={true} />
    </li>
  };

  return (
    <>
      {/* Modal download */}
      {isModalExportOpen ? <ModalExport handleExit={() => setModalExportOpen(!isModalExportOpen)}
        select={new URLPath().MergeSelectParent(select, new URLPath().StringToIUrl(history.location.search).f)}
        collections={new URLPath().StringToIUrl(history.location.search).collections !== false}
        isOpen={isModalExportOpen} /> : null}

      {/* Modal Display options */}
      {isDisplayOptionsOpen ? <ModalDisplayOptions parentFolder={new URLPath().StringToIUrl(history.location.search).f} handleExit={() =>
        setDisplayOptionsOpen(!isDisplayOptionsOpen)} isOpen={isDisplayOptionsOpen} /> : null}

      {/* Modal new directory */}
      {isModalMkdirOpen && !isReadOnly ? <ModalArchiveMkdir handleExit={() => setModalMkdirOpen(!isModalMkdirOpen)} isOpen={isModalMkdirOpen} /> : null}

      {/* Upload drop Area */}
      {dropAreaUploadFilesList.length !== 0 ? <ModalDropAreaFilesAdded
        handleExit={() => setDropAreaUploadFilesList(newIFileIndexItemArray())}
        uploadFilesList={dropAreaUploadFilesList}
        isOpen={dropAreaUploadFilesList.length !== 0} /> : null}

      {/* Menu */}
      <header className={sidebar ? "header header--main header--select header--edit" :
        select ? "header header--main header--select" : "header header--main "}>
        <div className="wrapper">
          {!select ? <button data-test="hamburger" className="hamburger__container" onClick={() => setHamburgerMenu(!hamburgerMenu)}>
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
          {!select ? <div className="item item--select" onClick={() => { removeSidebarSelection() }}>
            {MessageSelectAction}
          </div> : null}

          {select ? <div className="item item--labels" onClick={() => toggleLabels()}>Labels</div> : null}

          {/* default more menu */}
          {!select ? <MoreMenu>
            <li className={!isReadOnly ? "menu-option" : "menu-option disabled"} data-test="mkdir" onClick={() =>
              setModalMkdirOpen(!isModalMkdirOpen)}>{MessageMkdir}</li>
            <li className="menu-option" onClick={() => setDisplayOptionsOpen(!isDisplayOptionsOpen)}>{MessageDisplayOptions}</li>
            {state ? <UploadMenuItem /> : null}
          </MoreMenu> : null}

          {/* In the select context there are more options */}
          {select ? <MoreMenu>
            {select.length === state.fileIndexItems.length ? <li className="menu-option" onClick={() => undoSelection()}>{MessageUndoSelection}</li> : null}
            {select.length !== state.fileIndexItems.length ? <li className="menu-option" onClick={() => allSelection()}>{MessageSelectAll}</li> : null}
            {select.length >= 1 ? <li data-test="export" className="menu-option" onClick={() => setModalExportOpen(!isModalExportOpen)}>Download</li> : null}
            {select.length >= 1 ? <li data-test="trash" className={!isReadOnly ? "menu-option" : "menu-option disabled"}
              onClick={() => moveToTrashSelection()}>{MessageMoveToTrash}</li> : null}
            {state ? <UploadMenuItem /> : null}
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

