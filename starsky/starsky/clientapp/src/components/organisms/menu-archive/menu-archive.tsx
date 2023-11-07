import React, { memo, useEffect, useState } from "react";
import {
  ArchiveContext,
  defaultStateFallback
} from "../../../contexts/archive-context";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useHotKeys from "../../../hooks/use-keyboard/use-hotkeys";
import useLocation from "../../../hooks/use-location/use-location";
import { newIFileIndexItemArray } from "../../../interfaces/IFileIndexItem";
import localization from "../../../localization/localization.json";
import { FileListCache } from "../../../shared/filelist-cache";
import { Language } from "../../../shared/language";
import { GetArchiveSearchMenuHeaderClass } from "../../../shared/menu/get-archive-search-menu-header-class";
import { Select } from "../../../shared/select";
import { Sidebar } from "../../../shared/sidebar";
import { URLPath } from "../../../shared/url-path";
import { UrlQuery } from "../../../shared/url-query";
import DropArea from "../../atoms/drop-area/drop-area";
import HamburgerMenuToggle from "../../atoms/hamburger-menu-toggle/hamburger-menu-toggle";
import MenuOption from "../../atoms/menu-option/menu-option";
import MoreMenu from "../../atoms/more-menu/more-menu";
import MenuSearchBar from "../../molecules/menu-inline-search/menu-inline-search";
import MenuOptionMoveFolderToTrash from "../../molecules/menu-option-move-folder-to-trash/menu-option-move-folder-to-trash";
import MenuOptionMoveToTrash from "../../molecules/menu-option-move-to-trash/menu-option-move-to-trash";
import { MenuOptionSelectionAll } from "../../molecules/menu-option-selection-all/menu-option-selection-all";
import { MenuOptionSelectionUndo } from "../../molecules/menu-option-selection-undo/menu-option-selection-undo";
import { MenuSelectCount } from "../../molecules/menu-select-count/menu-select-count";
import { MenuSelectFurther } from "../../molecules/menu-select-further/menu-select-further";
import ModalDropAreaFilesAdded from "../../molecules/modal-drop-area-files-added/modal-drop-area-files-added";
import ModalArchiveMkdir from "../modal-archive-mkdir/modal-archive-mkdir";
import ModalArchiveRename from "../modal-archive-rename/modal-archive-rename";
import ModalArchiveSynchronizeManually from "../modal-archive-synchronize-manually/modal-archive-synchronize-manually";
import ModalDisplayOptions from "../modal-display-options/modal-display-options";
import ModalDownload from "../modal-download/modal-download";
import ModalPublishToggleWrapper from "../modal-publish/modal-publish-toggle-wrapper";
import NavContainer from "../nav-container/nav-container";

interface IMenuArchiveProps {}

// eslint-disable-next-line react/display-name
const MenuArchive: React.FunctionComponent<IMenuArchiveProps> = memo(() => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  // Content
  const MessageSelectAction = language.text("Selecteer", "Select");
  const MessageMkdir = language.text("Map maken", "Create folder");
  const MessageRenameDir = language.text("Naam wijzigen", "Rename");
  const MessageDisplayOptions = language.text(
    "Weergave opties",
    "Display options"
  );

  const [hamburgerMenu, setHamburgerMenu] = React.useState(false);
  const [enableMoreMenu, setEnableMoreMenu] = React.useState(false);

  // eslint-disable-next-line prefer-const
  let { state, dispatch } = React.useContext(ArchiveContext);
  state = defaultStateFallback(state);

  const history = useLocation();

  const allSelection = () =>
    new Select(select, setSelect, state, history).allSelection();
  const undoSelection = () =>
    new Select(select, setSelect, state, history).undoSelection();
  const removeSidebarSelection = () =>
    new Select(select, setSelect, state, history).removeSidebarSelection();

  // Command + A for mac os || Ctrl + A for windows
  useHotKeys({ key: "a", ctrlKeyOrMetaKey: true }, allSelection, []);

  /* only update when the state is changed */
  const [readOnly, setReadOnly] = React.useState(state.isReadOnly);
  useEffect(() => {
    setReadOnly(state.isReadOnly);
  }, [state.isReadOnly]);

  // Sidebar
  const [sidebar, setSidebar] = React.useState(
    new URLPath().StringToIUrl(history.location.search).sidebar
  );
  useEffect(() => {
    setSidebar(new URLPath().StringToIUrl(history.location.search).sidebar);
  }, [history.location.search]);

  const toggleLabels = () => new Sidebar(setSidebar, history).toggleSidebar();

  const [isModalExportOpen, setModalExportOpen] = useState(false);
  const [isModalPublishOpen, setModalPublishOpen] = useState(false);

  // Selection
  const [select, setSelect] = React.useState(
    new URLPath().StringToIUrl(history.location.search).select
  );
  useEffect(() => {
    setSelect(new URLPath().StringToIUrl(history.location.search).select);
  }, [history.location.search]);

  const [isDisplayOptionsOpen, setDisplayOptionsOpen] = React.useState(false);
  const [isSynchronizeManuallyOpen, setSynchronizeManuallyOpen] =
    React.useState(false);
  const [isModalMkdirOpen, setModalMkdirOpen] = React.useState(false);
  const [isModalRenameFolder, setModalRenameFolder] = React.useState(false);
  const [dropAreaUploadFilesList, setDropAreaUploadFilesList] = React.useState(
    newIFileIndexItemArray()
  );

  const UploadMenuItem = () => {
    if (readOnly)
      return (
        <li data-test="upload" className="menu-option disabled">
          Upload
        </li>
      );
    return (
      <li className="menu-option menu-option--input">
        <DropArea
          callback={(add) => {
            new FileListCache().CacheCleanEverything();
            setDropAreaUploadFilesList(add);
            dispatch({ type: "add", add });
          }}
          endpoint={new UrlQuery().UrlUploadApi()}
          folderPath={state.subPath}
          enableInputButton={true}
          enableDragAndDrop={true}
        />
      </li>
    );
  };

  return (
    <>
      {/* Modal download */}
      {isModalExportOpen ? (
        <ModalDownload
          handleExit={() => setModalExportOpen(!isModalExportOpen)}
          select={new URLPath().MergeSelectParent(
            select,
            new URLPath().StringToIUrl(history.location.search).f
          )}
          collections={
            new URLPath().StringToIUrl(history.location.search).collections !==
            false
          }
          isOpen={isModalExportOpen}
        />
      ) : null}

      {/* Modal Display options */}
      {isDisplayOptionsOpen ? (
        <ModalDisplayOptions
          parentFolder={new URLPath().StringToIUrl(history.location.search).f}
          handleExit={() => setDisplayOptionsOpen(!isDisplayOptionsOpen)}
          isOpen={isDisplayOptionsOpen}
        />
      ) : null}

      {/*  Synchronize Manually */}
      {isSynchronizeManuallyOpen ? (
        <ModalArchiveSynchronizeManually
          parentFolder={new URLPath().StringToIUrl(history.location.search).f}
          handleExit={() =>
            setSynchronizeManuallyOpen(!isSynchronizeManuallyOpen)
          }
          isOpen={isSynchronizeManuallyOpen}
        />
      ) : null}

      {/* Modal new directory */}
      {isModalMkdirOpen && !readOnly ? (
        <ModalArchiveMkdir
          state={state}
          dispatch={dispatch}
          handleExit={() => setModalMkdirOpen(!isModalMkdirOpen)}
          isOpen={isModalMkdirOpen}
        />
      ) : null}

      {isModalRenameFolder && !readOnly && state.subPath !== "/" ? (
        <ModalArchiveRename
          subPath={state.subPath}
          dispatch={dispatch}
          handleExit={() => {
            setModalRenameFolder(!isModalRenameFolder);
          }}
          isOpen={isModalRenameFolder}
        />
      ) : null}

      {/* Upload drop Area */}
      {dropAreaUploadFilesList.length !== 0 ? (
        <ModalDropAreaFilesAdded
          handleExit={() =>
            setDropAreaUploadFilesList(newIFileIndexItemArray())
          }
          uploadFilesList={dropAreaUploadFilesList}
          isOpen={dropAreaUploadFilesList.length !== 0}
        />
      ) : null}

      <ModalPublishToggleWrapper
        select={select}
        stateFileIndexItems={state.fileIndexItems}
        isModalPublishOpen={isModalPublishOpen}
        setModalPublishOpen={setModalPublishOpen}
      />

      {/* Menu */}
      <header className={GetArchiveSearchMenuHeaderClass(sidebar, select)}>
        <div className="wrapper">
          <HamburgerMenuToggle
            select={select}
            hamburgerMenu={hamburgerMenu}
            setHamburgerMenu={setHamburgerMenu}
          />

          <MenuSelectCount
            select={select}
            removeSidebarSelection={removeSidebarSelection}
          />

          {!select ? (
            <div
              className="item item--select"
              data-test="menu-item-select"
              onClick={() => {
                removeSidebarSelection();
              }}
              onKeyDown={(event) => {
                event.key === "Enter" && removeSidebarSelection();
              }}
            >
              {MessageSelectAction}
            </div>
          ) : null}

          {select ? (
            <div className="item item--labels" onClick={() => toggleLabels()}>
              Labels
            </div>
          ) : null}

          {/* default more menu */}
          {!select ? (
            <MoreMenu
              setEnableMoreMenu={setEnableMoreMenu}
              enableMoreMenu={enableMoreMenu}
            >
              <li
                className={!readOnly ? "menu-option" : "menu-option disabled"}
                data-test="mkdir"
                tabIndex={0}
                onClick={() => setModalMkdirOpen(!isModalMkdirOpen)}
                onKeyDown={(event) => {
                  event.key === "Enter" && setModalMkdirOpen(!isModalMkdirOpen);
                }}
              >
                {MessageMkdir}
              </li>
              <li
                className="menu-option"
                data-test="display-options"
                tabIndex={0}
                onClick={() => setDisplayOptionsOpen(!isDisplayOptionsOpen)}
                onKeyDown={(event) => {
                  event.key === "Enter" &&
                    setDisplayOptionsOpen(!isModalMkdirOpen);
                }}
              >
                {MessageDisplayOptions}
              </li>
              <MenuOption
                isReadOnly={false}
                testName="synchronize-manually"
                isSet={isSynchronizeManuallyOpen}
                set={setSynchronizeManuallyOpen}
                localization={localization.MessageSynchronizeManually}
              />
              {state ? <UploadMenuItem /> : null}
              <li
                className={
                  !readOnly && state.subPath !== "/"
                    ? "menu-option"
                    : "menu-option disabled"
                }
                data-test="rename"
                onClick={() => setModalRenameFolder(!isModalRenameFolder)}
              >
                {MessageRenameDir}
              </li>

              <MenuOptionMoveFolderToTrash
                isReadOnly={readOnly || state.subPath === "/"}
                subPath={state.subPath}
                dispatch={dispatch}
                setEnableMoreMenu={setEnableMoreMenu}
              />
            </MoreMenu>
          ) : null}

          {/* In the select context there are more options */}
          {select ? (
            <MoreMenu
              setEnableMoreMenu={setEnableMoreMenu}
              enableMoreMenu={enableMoreMenu}
            >
              <MenuOptionSelectionUndo
                select={select}
                state={state}
                undoSelection={undoSelection}
              />

              <MenuOptionSelectionAll
                select={select}
                state={state}
                allSelection={allSelection}
              />

              {select.length >= 1 ? (
                <>
                  <MenuOption
                    isReadOnly={false}
                    testName="export"
                    isSet={isModalExportOpen}
                    set={setModalExportOpen}
                    localization={localization.MessageDownload}
                  />
                  <MenuOption
                    isReadOnly={false}
                    testName="publish"
                    isSet={isModalPublishOpen}
                    set={setModalPublishOpen}
                    localization={localization.MessagePublish}
                  />
                  <MenuOptionMoveToTrash
                    state={state}
                    dispatch={dispatch}
                    select={select}
                    setSelect={setSelect}
                    isReadOnly={readOnly}
                  />
                </>
              ) : null}
              <li
                className="menu-option"
                data-test="display-options"
                onClick={() => setDisplayOptionsOpen(!isDisplayOptionsOpen)}
              >
                {MessageDisplayOptions}
              </li>
              <MenuOption
                setEnableMoreMenu={setEnableMoreMenu}
                isReadOnly={false}
                testName="synchronize-manually"
                isSet={isSynchronizeManuallyOpen}
                set={setSynchronizeManuallyOpen}
                localization={localization.MessageSynchronizeManually}
              />
              {state ? <UploadMenuItem /> : null}
            </MoreMenu>
          ) : null}

          <NavContainer hamburgerMenu={hamburgerMenu}>
            <MenuSearchBar callback={() => setHamburgerMenu(!hamburgerMenu)} />
          </NavContainer>
        </div>
      </header>

      <MenuSelectFurther select={select} toggleLabels={toggleLabels} />
    </>
  );
});

export default MenuArchive;
