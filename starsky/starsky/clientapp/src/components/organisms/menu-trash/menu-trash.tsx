import React, { useEffect } from "react";
import { ArchiveAction } from "../../../contexts/archive-context";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useHotKeys from "../../../hooks/use-keyboard/use-hotkeys";
import useLocation from "../../../hooks/use-location/use-location";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import localization from "../../../localization/localization.json";
import FetchPost from "../../../shared/fetch/fetch-post";
import { FileListCache } from "../../../shared/filelist-cache";
import { Language } from "../../../shared/language";
import { ClearSearchCache } from "../../../shared/search/clear-search-cache";
import { Select } from "../../../shared/select";
import { URLPath } from "../../../shared/url-path";
import { UrlQuery } from "../../../shared/url-query";
import HamburgerMenuToggle from "../../atoms/hamburger-menu-toggle/hamburger-menu-toggle";
import MenuOptionModal from "../../atoms/menu-option-modal/menu-option-modal.tsx";
import MenuOption from "../../atoms/menu-option/menu-option.tsx";
import MoreMenu from "../../atoms/more-menu/more-menu";
import Preloader from "../../atoms/preloader/preloader";
import MenuSearchBar from "../../molecules/menu-inline-search/menu-inline-search";
import { MenuOptionSelectionAll } from "../../molecules/menu-option-selection-all/menu-option-selection-all";
import { MenuOptionSelectionUndo } from "../../molecules/menu-option-selection-undo/menu-option-selection-undo";
import { MenuSelectCount } from "../../molecules/menu-select-count/menu-select-count";
import ModalForceDelete from "../modal-force-delete/modal-force-delete";
import NavContainer from "../nav-container/nav-container";

interface IMenuTrashProps {
  state: IArchiveProps;
  dispatch: React.Dispatch<ArchiveAction>;
}

const MenuTrash: React.FunctionComponent<IMenuTrashProps> = ({ state, dispatch }) => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  // Content
  const MessageSelectAction = language.key(localization.MessageSelectAction);

  const [hamburgerMenu, setHamburgerMenu] = React.useState(false);
  const [isLoading, setIsLoading] = React.useState(false);
  const [enableMoreMenu, setEnableMoreMenu] = React.useState(false);

  const history = useLocation();

  // Selection
  const [select, setSelect] = React.useState(
    new URLPath().StringToIUrl(history.location.search).select
  );
  useEffect(() => {
    setSelect(new URLPath().StringToIUrl(history.location.search).select);
  }, [history.location.search]);

  const allSelection = () => new Select(select, setSelect, state, history).allSelection();
  const undoSelection = () => new Select(select, setSelect, state, history).undoSelection();
  const removeSidebarSelection = () =>
    new Select(select, setSelect, state, history).removeSidebarSelection();

  // Command + A for mac os || Ctrl + A for windows
  useHotKeys({ key: "a", ctrlKeyOrMetaKey: true }, allSelection, []);

  function undoTrash() {
    if (!select) return;
    setIsLoading(true);

    const toUndoTrashList = new URLPath().MergeSelectFileIndexItem(select, state.fileIndexItems);
    if (!toUndoTrashList) return;
    const selectPaths = new URLPath().ArrayToCommaSeparatedStringOneParent(toUndoTrashList, "");
    if (selectPaths.length === 0) return;

    const bodyParams = new URLSearchParams();
    bodyParams.set("fieldName", "tags");
    bodyParams.set("search", "!delete!");
    bodyParams.append("f", selectPaths);

    undoSelection();

    // do it double since to avoid switching to fast
    new FileListCache().CacheCleanEverything();

    // to replace
    FetchPost(new UrlQuery().UrlReplaceApi(), bodyParams.toString()).then(() => {
      dispatch({ type: "remove", toRemoveFileList: toUndoTrashList });
      ClearSearchCache(history.location.search);
      setIsLoading(false);
      new FileListCache().CacheCleanEverything();
    });
  }

  const [isModalDeleteOpen, setIsModalDeleteOpen] = React.useState(false);

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
          handleExit={() => setIsModalDeleteOpen(!isModalDeleteOpen)}
        />
      ) : null}

      <header className={select ? "header header--main header--select" : "header header--main "}>
        <div className="wrapper">
          <HamburgerMenuToggle
            select={select}
            hamburgerMenu={hamburgerMenu}
            setHamburgerMenu={setHamburgerMenu}
          />

          <MenuSelectCount select={select} removeSidebarSelection={removeSidebarSelection} />

          {!select && state.fileIndexItems.length >= 1 ? (
            <button
              data-test="menu-trash-item-select"
              className="item item--select"
              onClick={() => {
                removeSidebarSelection();
              }}
              onKeyDown={(event) => {
                event.key === "Enter" && removeSidebarSelection();
              }}
            >
              {MessageSelectAction}
            </button>
          ) : null}

          {/* there are no items in the trash */}
          {!select && state.fileIndexItems.length === 0 ? (
            <div className="item item--select disabled">{MessageSelectAction}</div>
          ) : null}

          {/* More menu - When in normal state */}
          {!select ? (
            <MoreMenu setEnableMoreMenu={setEnableMoreMenu} enableMoreMenu={enableMoreMenu} />
          ) : null}

          {/* More menu - In the select context there are more options */}
          {select && select.length === 0 ? (
            <MoreMenu setEnableMoreMenu={setEnableMoreMenu} enableMoreMenu={enableMoreMenu}>
              <MenuOptionSelectionAll select={select} state={state} allSelection={allSelection} />
            </MoreMenu>
          ) : null}

          {/* More menu - When more then 1 item is selected */}
          {select && select.length >= 1 ? (
            <MoreMenu setEnableMoreMenu={setEnableMoreMenu} enableMoreMenu={enableMoreMenu}>
              <MenuOptionSelectionUndo
                select={select}
                state={state}
                undoSelection={undoSelection}
              />

              <MenuOptionSelectionAll select={select} state={state} allSelection={allSelection} />

              <MenuOption
                isReadOnly={false}
                testName="restore-from-trash"
                onClickKeydown={() => undoTrash()}
                localization={localization.MessageRestoreFromTrash}
              />

              <MenuOptionModal
                isReadOnly={false}
                testName="delete"
                set={setIsModalDeleteOpen}
                isSet={isModalDeleteOpen}
                localization={localization.MessageDeleteImmediately}
              />
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
