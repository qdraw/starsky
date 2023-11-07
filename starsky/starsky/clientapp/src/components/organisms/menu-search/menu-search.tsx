import React, { useEffect, useState } from "react";
import {
  ArchiveAction,
  defaultStateFallback
} from "../../../contexts/archive-context";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useHotKeys from "../../../hooks/use-keyboard/use-hotkeys";
import useLocation from "../../../hooks/use-location/use-location";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import localization from "../../../localization/localization.json";
import { Language } from "../../../shared/language";
import { GetArchiveSearchMenuHeaderClass } from "../../../shared/menu/get-archive-search-menu-header-class";
import { Select } from "../../../shared/select";
import { Sidebar } from "../../../shared/sidebar";
import { URLPath } from "../../../shared/url-path";
import HamburgerMenuToggle from "../../atoms/hamburger-menu-toggle/hamburger-menu-toggle";
import MenuOption from "../../atoms/menu-option/menu-option";
import MoreMenu from "../../atoms/more-menu/more-menu";
import MenuSearchBar from "../../molecules/menu-inline-search/menu-inline-search";
import MenuOptionMoveToTrash from "../../molecules/menu-option-move-to-trash/menu-option-move-to-trash";
import { MenuOptionSelectionAll } from "../../molecules/menu-option-selection-all/menu-option-selection-all";
import { MenuOptionSelectionUndo } from "../../molecules/menu-option-selection-undo/menu-option-selection-undo";
import { MenuSelectCount } from "../../molecules/menu-select-count/menu-select-count";
import { MenuSelectFurther } from "../../molecules/menu-select-further/menu-select-further";
import ModalDownload from "../modal-download/modal-download";
import ModalPublishToggleWrapper from "../modal-publish/modal-publish-toggle-wrapper";
import NavContainer from "../nav-container/nav-container";

export interface IMenuSearchProps {
  state: IArchiveProps;
  dispatch: React.Dispatch<ArchiveAction>;
}

export const MenuSearch: React.FunctionComponent<IMenuSearchProps> = ({
  state,
  dispatch
}) => {
  state = defaultStateFallback(state);

  const [hamburgerMenu, setHamburgerMenu] = React.useState(false);

  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  // Content
  const MessageSelectAction = language.text("Selecteer", "Select");
  const MessageSelectAll = language.text("Alles selecteren", "Select all");

  // Selection
  const history = useLocation();
  const [select, setSelect] = React.useState(
    new URLPath().StringToIUrl(history.location.search).select
  );
  useEffect(() => {
    setSelect(new URLPath().StringToIUrl(history.location.search).select);
  }, [history.location.search]);

  const allSelection = () =>
    new Select(select, setSelect, state, history).allSelection();
  const removeSidebarSelection = () =>
    new Select(select, setSelect, state, history).removeSidebarSelection();
  const undoSelection = () =>
    new Select(select, setSelect, state, history).undoSelection();

  // Command + A for mac os || Ctrl + A for windows
  useHotKeys({ key: "a", ctrlKeyOrMetaKey: true }, allSelection, []);

  // Sidebar
  const [sidebar, setSidebar] = React.useState(
    new URLPath().StringToIUrl(history.location.search).sidebar
  );
  const [enableMoreMenu, setEnableMoreMenu] = React.useState(false);

  useEffect(() => {
    setSidebar(new URLPath().StringToIUrl(history.location.search).sidebar);
  }, [history.location.search]);
  const toggleLabels = () => new Sidebar(setSidebar, history).toggleSidebar();

  // download modal
  const [isModalExportOpen, setModalExportOpen] = useState(false);
  // publish modal
  const [isModalPublishOpen, setModalPublishOpen] = useState(false);

  return (
    <>
      {/* Modal download */}
      {isModalExportOpen ? (
        <ModalDownload
          handleExit={() => setModalExportOpen(!isModalExportOpen)}
          select={
            select
              ? new URLPath().MergeSelectFileIndexItem(
                  select,
                  state.fileIndexItems
                )
              : []
          }
          collections={false}
          isOpen={isModalExportOpen}
        />
      ) : null}

      <ModalPublishToggleWrapper
        select={select}
        stateFileIndexItems={state.fileIndexItems}
        isModalPublishOpen={isModalPublishOpen}
        setModalPublishOpen={setModalPublishOpen}
      />

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

          {/* the select button with checkbox*/}
          {!select ? (
            <div
              className={
                state.fileIndexItems.length >= 1
                  ? "item item--select"
                  : "item item--select disabled"
              }
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

          {/* when selected */}
          {select ? (
            <div
              className={"item item--labels"}
              onClick={() => toggleLabels()}
              onKeyDown={(event) => {
                event.key === "Enter" && toggleLabels();
              }}
            >
              Labels
            </div>
          ) : null}

          {/* More menu - When in normal state */}
          {!select ? (
            <MoreMenu
              setEnableMoreMenu={setEnableMoreMenu}
              enableMoreMenu={enableMoreMenu}
            />
          ) : null}

          {/* More menu - In the select context there are more options */}
          {select && select.length === 0 ? (
            <MoreMenu
              setEnableMoreMenu={setEnableMoreMenu}
              enableMoreMenu={enableMoreMenu}
            >
              <li
                tabIndex={0}
                className="menu-option"
                onClick={() => allSelection()}
                onKeyDown={(event) => {
                  event.key === "Enter" && allSelection();
                }}
              >
                {MessageSelectAll}
              </li>
            </MoreMenu>
          ) : null}

          {/* More menu - When more then 1 item is selected */}
          {select && select.length >= 1 ? (
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
                isReadOnly={false}
              />
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
};

export default MenuSearch;
