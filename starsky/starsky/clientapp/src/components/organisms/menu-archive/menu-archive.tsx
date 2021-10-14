import React, { memo, useEffect, useState } from "react";
import {
  ArchiveContext,
  defaultStateFallback
} from "../../../contexts/archive-context";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useHotKeys from "../../../hooks/use-keyboard/use-hotkeys";
import useLocation from "../../../hooks/use-location";
import { newIFileIndexItemArray } from "../../../interfaces/IFileIndexItem";
import { FileListCache } from "../../../shared/filelist-cache";
import { Language } from "../../../shared/language";
import { Select } from "../../../shared/select";
import { Sidebar } from "../../../shared/sidebar";
import { URLPath } from "../../../shared/url-path";
import { UrlQuery } from "../../../shared/url-query";
import DropArea from "../../atoms/drop-area/drop-area";
import HamburgerMenuToggle from "../../atoms/hamburger-menu-toggle/hamburger-menu-toggle";
import MenuOption from "../../atoms/menu-option/menu-option";
import ModalDropAreaFilesAdded from "../../atoms/modal-drop-area-files-added/modal-drop-area-files-added";
import MoreMenu from "../../atoms/more-menu/more-menu";
import MenuSearchBar from "../../molecules/menu-inline-search/menu-inline-search";
import MenuOptionMoveToTrash from "../../molecules/menu-option-move-to-trash/menu-option-move-to-trash";
import ModalArchiveMkdir from "../modal-archive-mkdir/modal-archive-mkdir";
import ModalArchiveRename from "../modal-archive-rename/modal-archive-rename";
import ModalArchiveSynchronizeManually from "../modal-archive-synchronize-manually/modal-archive-synchronize-manually";
import ModalDisplayOptions from "../modal-display-options/modal-display-options";
import ModalDownload from "../modal-download/modal-download";
import ModalPublishToggleWrapper from "../modal-publish/modal-publish-toggle-wrapper";
import NavContainer from "../nav-container/nav-container";

interface IMenuArchiveProps {}

const MenuArchive: React.FunctionComponent<IMenuArchiveProps> = memo(() => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  // Content
  const MessageSelectAction = language.text("Selecteer", "Select");
  const MessageSelectPresentPerfect = language.text("geselecteerd", "selected");
  const MessageNoneSelected = language.text(
    "Niets geselecteerd",
    "Nothing selected"
  );
  const MessageMkdir = language.text("Map maken", "Create folder");
  const MessageRenameDir = language.text("Naam wijzigen", "Rename");
  const MessageDisplayOptions = language.text(
    "Weergave opties",
    "Display options"
  );

  const MessageSelectFurther = language.text(
    "Verder selecteren",
    "Select further"
  );
  const MessageSelectAll = language.text("Alles selecteren", "Select all");
  const MessageUndoSelection = language.text("Undo selectie", "Undo selection");

  const [hamburgerMenu, setHamburgerMenu] = React.useState(false);
  let { state, dispatch } = React.useContext(ArchiveContext);
  state = defaultStateFallback(state);

  var history = useLocation();

  var allSelection = () =>
    new Select(select, setSelect, state, history).allSelection();
  var undoSelection = () =>
    new Select(select, setSelect, state, history).undoSelection();
  var removeSidebarSelection = () =>
    new Select(select, setSelect, state, history).removeSidebarSelection();

  // Command + A for mac os || Ctrl + A for windows
  useHotKeys({ key: "a", ctrlKeyOrMetaKey: true }, allSelection, []);

  /* only update when the state is changed */
  const [isReadOnly, setReadOnly] = React.useState(state.isReadOnly);
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

  var toggleLabels = () =>
    new Sidebar(sidebar, setSidebar, history).toggleSidebar();

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
  const [
    isSynchronizeManuallyOpen,
    setSynchronizeManuallyOpen
  ] = React.useState(false);
  const [isModalMkdirOpen, setModalMkdirOpen] = React.useState(false);
  const [isModalRenameFolder, setModalRenameFolder] = React.useState(false);
  const [dropAreaUploadFilesList, setDropAreaUploadFilesList] = React.useState(
    newIFileIndexItemArray()
  );

  const UploadMenuItem = () => {
    if (isReadOnly)
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
      {isModalMkdirOpen && !isReadOnly ? (
        <ModalArchiveMkdir
          state={state}
          dispatch={dispatch}
          handleExit={() => setModalMkdirOpen(!isModalMkdirOpen)}
          isOpen={isModalMkdirOpen}
        />
      ) : null}

      {isModalRenameFolder && !isReadOnly && state.subPath !== "/" ? (
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
      <header
        className={
          sidebar
            ? "header header--main header--select header--edit"
            : select
            ? "header header--main header--select"
            : "header header--main "
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
              data-test="selected-0"
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
              data-test={`selected-${select.length}`}
              onClick={() => {
                removeSidebarSelection();
              }}
              className="item item--first item--close"
            >
              {select.length} {MessageSelectPresentPerfect}
            </button>
          ) : null}
          {!select ? (
            <div
              className="item item--select"
              data-test="menu-item-select"
              onClick={() => {
                removeSidebarSelection();
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
            <MoreMenu>
              <li
                className={!isReadOnly ? "menu-option" : "menu-option disabled"}
                data-test="mkdir"
                onClick={() => setModalMkdirOpen(!isModalMkdirOpen)}
              >
                {MessageMkdir}
              </li>
              <li
                className="menu-option"
                data-test="display-options"
                onClick={() => setDisplayOptionsOpen(!isDisplayOptionsOpen)}
              >
                {MessageDisplayOptions}
              </li>
              <MenuOption
                testName="synchronize-manually"
                isSet={isSynchronizeManuallyOpen}
                set={setSynchronizeManuallyOpen}
                nl="Handmatig synchroniseren"
                en="Synchronize manually"
              />
              {state ? <UploadMenuItem /> : null}
              <li
                className={
                  !isReadOnly && state.subPath !== "/"
                    ? "menu-option"
                    : "menu-option disabled"
                }
                data-test="rename"
                onClick={() => setModalRenameFolder(!isModalRenameFolder)}
              >
                {MessageRenameDir}
              </li>
            </MoreMenu>
          ) : null}

          {/* In the select context there are more options */}
          {select ? (
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
              {select.length >= 1 ? (
                <>
                  <MenuOption
                    testName="export"
                    isSet={isModalExportOpen}
                    set={setModalExportOpen}
                    nl="Download"
                    en="Download"
                  />
                  <MenuOption
                    testName="publish"
                    isSet={isModalPublishOpen}
                    set={setModalPublishOpen}
                    nl="Publiceren"
                    en="Publish"
                  />
                  <MenuOptionMoveToTrash
                    state={state}
                    dispatch={dispatch}
                    select={select}
                    setSelect={setSelect}
                    isReadOnly={isReadOnly}
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
                testName="synchronize-manually"
                isSet={isSynchronizeManuallyOpen}
                set={setSynchronizeManuallyOpen}
                nl="Handmatig synchroniseren"
                en="Synchronize manually"
              />
              {state ? <UploadMenuItem /> : null}
            </MoreMenu>
          ) : null}

          <NavContainer hamburgerMenu={hamburgerMenu}>
            <MenuSearchBar callback={() => setHamburgerMenu(!hamburgerMenu)} />
          </NavContainer>
        </div>
      </header>

      {select ? (
        <div className="header header--sidebar header--border-left">
          <div
            className="item item--continue"
            onClick={() => {
              toggleLabels();
            }}
          >
            {MessageSelectFurther}
          </div>
        </div>
      ) : (
        ""
      )}
    </>
  );
});

export default MenuArchive;
