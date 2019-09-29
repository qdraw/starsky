import React, { memo, useEffect } from 'react';
import { ArchiveContext } from '../contexts/archive-context';
import useLocation from '../hooks/use-location';
import FetchPost from '../shared/fetch-post';
import { URLPath } from '../shared/url-path';
import MenuSearchBar from './menu.searchbar';
import Modal from './modal';
import MoreMenu from './more-menu';


const MenuTrash: React.FunctionComponent<any> = memo((props) => {
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

  function forceDelete() {
    if (!select) return;

    var toUndoTrashList = new URLPath().MergeSelectFileIndexItem(select, state.fileIndexItems);
    if (!toUndoTrashList) return;
    var selectParams = new URLPath().ArrayToCommaSeperatedStringOneParent(toUndoTrashList, "")
    if (selectParams.length === 0) return;

    dispatch({ 'type': 'remove', 'filesList': toUndoTrashList })

    var bodyParams = new URLSearchParams();
    bodyParams.append("f", selectParams);
    FetchPost("/api/delete", bodyParams.toString(), 'delete')

    undoSelection();
  }

  function undoTrash() {
    if (!select) return;

    var toUndoTrashList = new URLPath().MergeSelectFileIndexItem(select, state.fileIndexItems);
    if (!toUndoTrashList) return;
    var selectPaths = new URLPath().ArrayToCommaSeperatedStringOneParent(toUndoTrashList, "")
    if (selectPaths.length === 0) return;

    var bodyParams = new URLSearchParams();
    bodyParams.set("fieldName", "tags");
    bodyParams.set("search", "!delete!");
    bodyParams.append("f", selectPaths);

    // to replace
    // dispatch({ 'type': 'replace', 'fieldName': 'tags', files: toUpdatePaths, 'from': '!delete!', 'to': "" });
    FetchPost("/api/replace", bodyParams.toString())

    undoSelection();
    dispatch({ 'type': 'remove', 'filesList': toUndoTrashList })
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
          <div className="modal content--subheader">Verwijderen</div>
          <div className="modal content--text">
            Weet je zeker dat je dit bestand wilt verwijderen van alle devices?
             <br />
            <a onClick={() => setModalDeleteOpen(false)} className="btn btn--info">Annuleren</a>
            <button onClick={() => {
              forceDelete();
              setModalDeleteOpen(false);
            }} className="btn btn--default">Verwijderen</button>
          </div>
        </></Modal> : null}

      <header className={select ? "header header--main header--select" : "header header--main "}>
        <div className="wrapper">

          {!select ? <button className="hamburger__container" onClick={() => setHamburgerMenu(!hamburgerMenu)}>
            <div className={hamburgerMenu ? "hamburger open" : "hamburger"}>
              <i></i>
              <i></i>
              <i></i>
            </div>
          </button> : null}

          {select && select.length === 0 ? <a onClick={() => { selectToggle() }}
            className="item item--first item--close">Niets geselecteerd</a> : null}
          {select && select.length >= 1 ? <a onClick={() => { selectToggle() }}
            className="item item--first item--close">{select.length} geselecteerd</a> : null}

          {!select && state.fileIndexItems.length >= 1 ? <div className="item item--select" onClick={() => { selectToggle() }}>
            Selecteer
            </div> : null}

          {/* there are no items in the trash */}
          {!select && state.fileIndexItems.length === 0 ? <div className="item item--select disabled">
            Selecteer
            </div> : null}

          {/* When in normal state */}
          {!select ? <MoreMenu></MoreMenu> : null}

          {/* In the select context there are more options */}
          {select && select.length === 0 ?
            <MoreMenu>
              <li className="menu-option" onClick={() => allSelection()}>Alles selecteren</li>
            </MoreMenu> : null}

          {select && select.length >= 1 ?
            <MoreMenu>
              {select.length === state.fileIndexItems.length ? <li className="menu-option" onClick={() => undoSelection()}>Undo selectie</li> : null}
              {select.length !== state.fileIndexItems.length ? <li className="menu-option" onClick={() => allSelection()}>Alles selecteren</li> : null}
              <li className="menu-option" onClick={() => undoTrash()}>Zet terug uit prullenmand</li>
              <li className="menu-option" onClick={() => setModalDeleteOpen(true)}>Verwijder onmiddelijk</li>
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
    </>);
});

export default MenuTrash
