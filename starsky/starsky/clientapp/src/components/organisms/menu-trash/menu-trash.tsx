import React, { useEffect } from "react";
import { ArchiveAction } from "../../../contexts/archive-context";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useLocation from "../../../hooks/use-location";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import FetchPost from "../../../shared/fetch-post";
import { FileListCache } from "../../../shared/filelist-cache";
import { Language } from "../../../shared/language";
import { ClearSearchCache } from "../../../shared/search/clear-search-cache";
import { Select } from "../../../shared/select";
import { URLPath } from "../../../shared/url-path";
import { UrlQuery } from "../../../shared/url-query";
import HamburgerMenuToggle from "../../atoms/hamburger-menu-toggle/hamburger-menu-toggle";
import MoreMenu from "../../atoms/more-menu/more-menu";
import Preloader from "../../atoms/preloader/preloader";
import MenuSearchBar from "../../molecules/menu-inline-search/menu-inline-search";
import ModalForceDelete from "../modal-force-delete/modal-force-delete";
import NavContainer from "../nav-container/nav-container";

export interface IMenuTrashProps {
  state: IArchiveProps;
  dispatch: React.Dispatch<ArchiveAction>;
}

const MenuTrash: React.FunctionComponent<IMenuTrashProps> = ({
  state,
  dispatch
}) => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  // Content
  const MessageSelectAction = language.text("Selecteer", "Select");
  const MessageSelectPresentPerfect = language.text("geselecteerd", "selected");
  const MessageNoneSelected = language.text(
    "Niets geselecteerd",
    "Nothing selected"
  );
  const MessageSelectAll = language.text("Alles selecteren", "Select all");
  const MessageUndoSelection = language.text("Undo selectie", "Undo selection");
  const MessageRestoreFromTrash = language.text(
    "Zet terug uit prullenmand",
    "Restore from Trash"
  );
  const MessageDeleteImmediately = language.text(
    "Verwijder onmiddellijk",
    "Delete immediately"
  );

  const [hamburgerMenu, setHamburgerMenu] = React.useState(false);
  const [isLoading, setIsLoading] = React.useState(false);

  var history = useLocation();

  // Selection
  const [select, setSelect] = React.useState(
    new URLPath().StringToIUrl(history.location.search).select
  );
  useEffect(() => {
    setSelect(new URLPath().StringToIUrl(history.location.search).select);
  }, [history.location.search]);

  const allSelection = () =>
    new Select(select, setSelect, state, history).allSelection();
  const undoSelection = () =>
    new Select(select, setSelect, state, history).undoSelection();
  const removeSidebarSelection = () =>
    new Select(select, setSelect, state, history).removeSidebarSelection();

  function undoTrash() {
    if (!select) return;
    setIsLoading(true);

    var toUndoTrashList = new URLPath().MergeSelectFileIndexItem(
      select,
      state.fileIndexItems
    );
    if (!toUndoTrashList) return;
    var selectPaths = new URLPath().ArrayToCommaSeperatedStringOneParent(
      toUndoTrashList,
      ""
    );
    if (selectPaths.length === 0) return;

    var bodyParams = new URLSearchParams();
    bodyParams.set("fieldName", "tags");
    bodyParams.set("search", "!delete!");
    bodyParams.append("f", selectPaths);

    undoSelection();

    // to replace
    FetchPost(new UrlQuery().UrlReplaceApi(), bodyParams.toString()).then(
      () => {
        dispatch({ type: "remove", toRemoveFileList: toUndoTrashList });
        ClearSearchCache(history.location.search);
        setIsLoading(false);
        new FileListCache().CacheCleanEverything();
      }
    );
  }

  const [isModalDeleteOpen, setModalDeleteOpen] = React.useState(false);

  return (
    <>
      {isLoading ? <Preloader isOverlay={true} /> : null}
      {isModalDeleteOpen ? (
        <ModalForceDelete
          setSelect={setSelect}
          state={state}
          isOpen={isModalDeleteOpen}
          select={select}
          dispatch={dispatch}
          setIsLoading={setIsLoading}
          handleExit={() => setModalDeleteOpen(!isModalDeleteOpen)}
        />
      ) : null}

      <header
        className={
          select ? "header header--main header--select" : "header header--main "
        }
      >
        <div className="wrapper">
          <HamburgerMenuToggle
            select={select}
            hamburgerMenu={hamburgerMenu}
            setHamburgerMenu={setHamburgerMenu}
          />

          {select && select.length === 0 ? (
            <button
              onClick={() => {
                removeSidebarSelection();
              }}
              className="item item--first item--close"
            >
              {MessageNoneSelected}
            </button>
          ) : null}
          {select && select.length >= 1 ? (
            <button
              onClick={() => {
                removeSidebarSelection();
              }}
              className="item item--first item--close"
            >
              {select.length} {MessageSelectPresentPerfect}
            </button>
          ) : null}

          {!select && state.fileIndexItems.length >= 1 ? (
            <div
              className="item item--select"
              onClick={() => {
                removeSidebarSelection();
              }}
            >
              {MessageSelectAction}
            </div>
          ) : null}

          {/* there are no items in the trash */}
          {!select && state.fileIndexItems.length === 0 ? (
            <div className="item item--select disabled">
              {MessageSelectAction}
            </div>
          ) : null}

          {/* More menu - When in normal state */}
          {!select ? <MoreMenu /> : null}

          {/* More menu - In the select context there are more options */}
          {select && select.length === 0 ? (
            <MoreMenu>
              <li
                className="menu-option"
                data-test="select-all"
                onClick={() => allSelection()}
              >
                {MessageSelectAll}
              </li>
            </MoreMenu>
          ) : null}

          {/* More menu - When more then 1 item is selected */}
          {select && select.length >= 1 ? (
            <MoreMenu>
              {select.length === state.fileIndexItems.length ? (
                <li className="menu-option" onClick={() => undoSelection()}>
                  {MessageUndoSelection}
                </li>
              ) : null}
              {select.length !== state.fileIndexItems.length ? (
                <li
                  className="menu-option"
                  data-test="select-all"
                  onClick={() => allSelection()}
                >
                  {MessageSelectAll}
                </li>
              ) : null}
              <li
                className="menu-option"
                data-test="restore-from-trash"
                onClick={() => undoTrash()}
              >
                {MessageRestoreFromTrash}
              </li>
              <li
                className="menu-option"
                data-test="delete"
                onClick={() => setModalDeleteOpen(true)}
              >
                {MessageDeleteImmediately}
              </li>
            </MoreMenu>
          ) : null}

          <NavContainer hamburgerMenu={hamburgerMenu}>
            <MenuSearchBar callback={() => setHamburgerMenu(!hamburgerMenu)} />
          </NavContainer>
        </div>
      </header>
    </>
  );
};

export default MenuTrash;
