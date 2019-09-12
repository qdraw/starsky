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

    removeSelectionFromUrl();
  }

  function undoTrash() {
    if (!select) return;

    var toUndoTrashList = new URLPath().MergeSelectFileIndexItem(select, state.fileIndexItems);
    if (!toUndoTrashList) return;
    var selectParams = new URLPath().ArrayToCommaSeperatedStringOneParent(toUndoTrashList, "")
    if (selectParams.length === 0) return;

    var bodyParams = new URLSearchParams();
    bodyParams.set("fieldName", "tags");
    bodyParams.set("search", "!delete!");
    bodyParams.append("f", selectParams);

    dispatch({ 'type': 'remove', 'filesList': toUndoTrashList })
    // to replace
    // dispatch({ 'type': 'replace', 'fieldName': 'tags', files: toUpdatePaths, 'from': '!delete!', 'to': "" });
    FetchPost("/api/replace", bodyParams.toString())

    removeSelectionFromUrl();
  }

  function removeSelectionFromUrl() {
    // Remove from selection
    var urlObject = new URLPath().StringToIUrl(history.location.search);

    if (urlObject.select) {
      urlObject.select = [];
    }
    setSelect([]);
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true });
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
            Weet je zeker dat je dit bestand wil verpaatsen naar null?
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
            className="item item--first item--close">Niks geselecteerd</a> : null}
          {select && select.length >= 1 ? <a onClick={() => { selectToggle() }}
            className="item item--first item--close">{select.length} geselecteerd</a> : null}
          {!select ? <div className="item item--select" onClick={() => { selectToggle() }}>
            Selecteer
            </div> : null}

          {/* When in normal state */}
          {!select ? <MoreMenu></MoreMenu> : null}

          {/* In the select context there are more options */}
          {select && select.length === 0 ? <MoreMenu></MoreMenu> : null}

          {select && select.length >= 1 ? <MoreMenu>
            <li className="menu-option" onClick={() => undoTrash()}>Undo weggooien</li>
            <li className="menu-option" onClick={() => setModalDeleteOpen(true)}>Verwijderen</li>
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
