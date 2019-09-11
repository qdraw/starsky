import React, { memo, useEffect } from 'react';
import { ArchiveContext } from '../contexts/archive-context';
import useLocation from '../hooks/use-location';
import FetchPost from '../shared/fetch-post';
import { URLPath } from '../shared/url-path';
import MenuSearchBar from './menu.searchbar';
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

  // function TrashFile() {
  //   var bodyParams = new URLSearchParams();
  //   bodyParams.set("f", detailView.subPath);

  //   // Add remove tag
  //   if (!isMarkedAsDeleted) {
  //     bodyParams.set("Tags", "!delete!");
  //     bodyParams.set("append", "true");
  //     FetchPost("/api/update", bodyParams.toString())
  //     dispatch({ 'type': 'add', tags: "!delete!" });
  //     dispatch({ 'type': 'update', status: IExifStatus.Deleted });
  //   }
  //   // Undo trash
  //   else {
  //     bodyParams.set("fieldName", "tags");
  //     bodyParams.set("search", "!delete!");
  //     FetchPost("/api/replace", bodyParams.toString())
  //     dispatch({ 'type': 'remove', tags: "!delete!" });
  //     dispatch({ 'type': 'update', status: IExifStatus.Ok });
  //   }
  // }

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

    // to replace
    // dispatch({ 'type': 'replace', 'fieldName': 'tags', files: toUpdatePaths, 'from': '!delete!', 'to': "" });

    dispatch({ 'type': 'remove', 'filesList': toUndoTrashList })
    FetchPost("/api/replace", bodyParams.toString())

    // Remove from selection
    var urlObject = new URLPath().StringToIUrl(history.location.search);
    console.log(urlObject);

    if (urlObject.select) {
      urlObject.select = [];
    }
    setSelect([]);
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true });
  }

  return (
    <>
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
            <li className="menu-option disabled" onClick={() => { alert("Uploaden werkt nog niet, ga naar importeren in het hoofdmenu"); }}>Verwijderen</li>
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
