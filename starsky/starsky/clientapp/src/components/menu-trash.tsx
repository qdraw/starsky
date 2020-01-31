import React, { memo, useEffect } from 'react';
import { ArchiveContext } from '../contexts/archive-context';
import useGlobalSettings from '../hooks/use-global-settings';
import useLocation from '../hooks/use-location';
import { newIArchive } from '../interfaces/IArchive';
import FetchPost from '../shared/fetch-post';
import { Language } from '../shared/language';
import { URLPath } from '../shared/url-path';
import { UrlQuery } from '../shared/url-query';
import MenuSearchBar from './menu.searchbar';
import Modal from './modal';
import MoreMenu from './more-menu';

const MenuTrash: React.FunctionComponent<any> = memo((_) => {

  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  // Content
  const MessageSelectAction = language.text("Selecteer", "Select");
  const MessageSelectPresentPerfect = language.text("geselecteerd", "selected");
  const MessageNoneSelected = language.text("Niets geselecteerd", "Nothing selected");
  const MessageSelectAll = language.text("Alles selecteren", "Select all");
  const MessageUndoSelection = language.text("Undo selectie", "Undo selection");
  const MessageRestoreFromTrash = language.text("Zet terug uit prullenmand", "Restore from Trash");
  const MessageDeleteImmediately = language.text("Verwijder onmiddellijk", "Delete immediately");
  const MessageDeleteIntroText = language.text("Weet je zeker dat je dit bestand wilt verwijderen van alle devices?",
    "Are you sure you want to delete this file from all devices?");
  const MessageCancel = language.text("Annuleren", "Cancel");

  const [hamburgerMenu, setHamburgerMenu] = React.useState(false);

  var history = useLocation();

  // Selection
  const [select, setSelect] = React.useState(new URLPath().StringToIUrl(history.location.search).select);
  useEffect(() => {
    setSelect(new URLPath().StringToIUrl(history.location.search).select)
  }, [history.location.search]);

  // Select All items
  function allSelection() {
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
    var selectVar: string[] = urlObject.select ? urlObject.select : [];
    if (!urlObject.select) {
      urlObject.select = [];
    }
    else {
      delete urlObject.sidebar;
      delete urlObject.select;
    }
    if (selectVar) {
      setSelect(selectVar);
    }
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true });
  }

  let { state, dispatch } = React.useContext(ArchiveContext);
  // fallback
  if (!state) state = {
    ...newIArchive(),
    collectionsCount: 0,
    fileIndexItems: []
  };

  async function forceDelete() {
    if (!select) return;

    var toUndoTrashList = new URLPath().MergeSelectFileIndexItem(select, state.fileIndexItems);
    if (!toUndoTrashList) return;
    var selectParams = new URLPath().ArrayToCommaSeperatedStringOneParent(toUndoTrashList, "");
    if (selectParams.length === 0) return;

    var bodyParams = new URLSearchParams();
    bodyParams.append("f", selectParams);
    await FetchPost(new UrlQuery().UrlDeleteApi(), bodyParams.toString(), 'delete');

    undoSelection();
    dispatch({ 'type': 'remove', 'filesList': toUndoTrashList })
  }

  async function undoTrash() {
    if (!select) return;

    var toUndoTrashList = new URLPath().MergeSelectFileIndexItem(select, state.fileIndexItems);
    if (!toUndoTrashList) return;
    var selectPaths = new URLPath().ArrayToCommaSeperatedStringOneParent(toUndoTrashList, "");
    if (selectPaths.length === 0) return;

    var bodyParams = new URLSearchParams();
    bodyParams.set("fieldName", "tags");
    bodyParams.set("search", "!delete!");
    bodyParams.append("f", selectPaths);

    // to replace
    await FetchPost(new UrlQuery().UrlReplaceApi(), bodyParams.toString());

    dispatch({ type: 'remove', filesList: toUndoTrashList });

    undoSelection();
  }

  const [isModalDeleteOpen, setModalDeleteOpen] = React.useState(false);

  return (
    <>
      {isModalDeleteOpen ? <Modal
        id="delete-modal"
        isOpen={isModalDeleteOpen}
        handleExit={() => {
          setModalDeleteOpen(false)
        }}><>
          <div className="modal content--subheader">{MessageDeleteImmediately}</div>
          <div className="modal content--text">
            {MessageDeleteIntroText}
            <br />
            <button onClick={() => setModalDeleteOpen(false)} className="btn btn--info">{MessageCancel}</button>
            <button onClick={() => {
              forceDelete();
              setModalDeleteOpen(false);
            }} className="btn btn--default">{MessageDeleteImmediately}</button>
          </div>
        </></Modal> : null}

      <header className={select ? "header header--main header--select" : "header header--main "}>
        <div className="wrapper">

          {!select ? <button className="hamburger__container" onClick={() => setHamburgerMenu(!hamburgerMenu)}>
            <div className={hamburgerMenu ? "hamburger open" : "hamburger"}>
              <i />
              <i />
              <i />
            </div>
          </button> : null}

          {select && select.length === 0 ? <button onClick={() => { selectToggle() }}
            className="item item--first item--close">{MessageNoneSelected}</button> : null}
          {select && select.length >= 1 ? <button onClick={() => { selectToggle() }}
            className="item item--first item--close">{select.length} {MessageSelectPresentPerfect}</button> : null}

          {!select && state.fileIndexItems.length >= 1 ? <div className="item item--select" onClick={() => { selectToggle() }}>
            {MessageSelectAction}
          </div> : null}

          {/* there are no items in the trash */}
          {!select && state.fileIndexItems.length === 0 ? <div className="item item--select disabled">
            {MessageSelectAction}
          </div> : null}

          {/* When in normal state */}
          {!select ? <MoreMenu /> : null}

          {/* In the select context there are more options */}
          {select && select.length === 0 ?
            <MoreMenu>
              <li className="menu-option" onClick={() => allSelection()}>{MessageSelectAll}</li>
            </MoreMenu> : null}

          {select && select.length >= 1 ?
            <MoreMenu>
              {select.length === state.fileIndexItems.length ? <li className="menu-option" onClick={() => undoSelection()}>
                {MessageUndoSelection}</li> : null}
              {select.length !== state.fileIndexItems.length ? <li className="menu-option" onClick={() => allSelection()}>
                {MessageSelectAll}</li> : null}
              <li className="menu-option" data-test="restore-from-trash" onClick={() => undoTrash()}>{MessageRestoreFromTrash}</li>
              <li className="menu-option" data-test="delete" onClick={() => setModalDeleteOpen(true)}>{MessageDeleteImmediately}</li>
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
    </>);
});

export default MenuTrash
