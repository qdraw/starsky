import React, { useEffect, useState } from "react";
import { DetailViewAction } from "../../../contexts/detailview-context";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useKeyboardEvent from "../../../hooks/use-keyboard/use-keyboard-event";
import useLocation from "../../../hooks/use-location/use-location";
import { IDetailView } from "../../../interfaces/IDetailView";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import { INavigateState } from "../../../interfaces/INavigateState";
import localization from "../../../localization/localization.json";
import { Comma } from "../../../shared/comma";
import { IsEditedNow } from "../../../shared/date";
import FetchPost from "../../../shared/fetch/fetch-post";
import { FileListCache } from "../../../shared/filelist-cache";
import { Keyboard } from "../../../shared/keyboard";
import { Language } from "../../../shared/language";
import { ClearSearchCache } from "../../../shared/search/clear-search-cache";
import { URLPath } from "../../../shared/url-path";
import { UrlQuery } from "../../../shared/url-query";
import Link from "../../atoms/link/link";
import MenuOptionModal from "../../atoms/menu-option-modal/menu-option-modal";
import MenuOption from "../../atoms/menu-option/menu-option";
import MoreMenu from "../../atoms/more-menu/more-menu";
import Preloader from "../../atoms/preloader/preloader";
import IsSearchQueryMenuSearchItem from "../../molecules/is-search-query-menu-search-item/is-search-query-menu-search-item";
import MenuOptionDesktopEditorOpenSingle from "../../molecules/menu-option-desktop-editor-open-single/menu-option-desktop-editor-open-single.tsx";
import MenuOptionRotateImage90 from "../../molecules/menu-option-rotate-image-90/menu-option-rotate-image-90.tsx";
import ModalDetailviewRenameFile from "../modal-detailview-rename-file/modal-detailview-rename-file";
import ModalDownload from "../modal-download/modal-download";
import ModalMoveFile from "../modal-move-file/modal-move-file";
import ModalPublishToggleWrapper from "../modal-publish/modal-publish-toggle-wrapper";
import { GoToParentFolder } from "./internal/go-to-parent-folder";

interface MenuDetailViewProps {
  state: IDetailView;
  dispatch: React.Dispatch<DetailViewAction>;
}

function GetHeaderClass(isDetails: boolean | undefined, isMarkedAsDeleted: boolean): string {
  if (isDetails) {
    if (isMarkedAsDeleted) {
      return "header header--main header--edit header--deleted";
    } else {
      return "header header--main header--edit";
    }
  } else {
    return isMarkedAsDeleted ? "header header--main header--deleted" : "header header--main";
  }
}

const MenuDetailView: React.FunctionComponent<MenuDetailViewProps> = ({ state, dispatch }) => {
  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  // Not close anymore because its looks like closing a window
  const MessageCloseDialogBackToFolder = language.key(localization.MessageCloseDialogBackToFolder);
  const MessageCloseDetailScreenDialog = language.key(localization.MessageCloseDetailScreenDialog);
  const MessageSaved = language.key(localization.MessageSaved);
  const MessageMoveToTrash = language.key(localization.MessageMoveToTrash);
  const MessageIncludingColonWord = language.key(localization.MessageIncludingColonWord);
  const MessageRestoreFromTrash = language.key(localization.MessageRestoreFromTrash);

  const history = useLocation();

  const [details, setDetails] = React.useState(
    new URLPath().StringToIUrl(history.location.search).details
  );
  useEffect(() => {
    const details = new URLPath().StringToIUrl(history.location.search).details;
    setDetails(details);
  }, [history.location.search]);

  // know if you searching ?t= in url
  const [isSearchQuery, setIsSearchQuery] = React.useState(
    !!new URLPath().StringToIUrl(history.location.search).t
  );
  useEffect(() => {
    setIsSearchQuery(!!new URLPath().StringToIUrl(history.location.search).t);
  }, [history.location.search]);

  /* show marker with 'Saved' */
  const [isRecentEdited, setIsRecentEdited] = React.useState(
    IsEditedNow(state?.fileIndexItem?.lastEdited)
  );
  useEffect(() => {
    if (!state?.fileIndexItem?.lastEdited) return;
    const isEditedNow = IsEditedNow(state.fileIndexItem.lastEdited);
    if (!isEditedNow) {
      setIsRecentEdited(false);
      return;
    }
    setIsRecentEdited(isEditedNow);
  }, [state?.fileIndexItem?.lastEdited]);

  function toggleLabels() {
    const urlObject = new URLPath().StringToIUrl(history.location.search);
    urlObject.details = !details;
    setDetails(urlObject.details);
    setIsRecentEdited(false); // disable to avoid animation
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true });
  }

  const [isMarkedAsDeleted, setIsMarkedAsDeleted] = React.useState(
    state?.fileIndexItem?.status === IExifStatus.Deleted
  );
  const [enableMoreMenu, setEnableMoreMenu] = React.useState(false);

  /* only update when the state is changed */
  useEffect(() => {
    setIsMarkedAsDeleted(state.fileIndexItem.status === IExifStatus.Deleted);
  }, [state.fileIndexItem.status, history.location.search]);

  const [isSourceMissing, setIsSourceMissing] = React.useState(
    state.fileIndexItem.status === IExifStatus.NotFoundSourceMissing
  );

  useEffect(() => {
    setIsSourceMissing(state.fileIndexItem.status === IExifStatus.NotFoundSourceMissing);
    setIsReadOnly(state.fileIndexItem.status === IExifStatus.NotFoundSourceMissing);
  }, [state.fileIndexItem.status, history.location.search]);

  /* only update when the state is changed */
  const [isReadOnly, setIsReadOnly] = React.useState(state.isReadOnly);
  useEffect(() => {
    if (state.fileIndexItem.status === IExifStatus.NotFoundSourceMissing) return;
    setIsReadOnly(state.isReadOnly);
  }, [state.isReadOnly, state.fileIndexItem.status]);

  // preloading icon
  const [isLoading, setIsLoading] = useState(false);

  /**
   * Create body params to do url queries
   */
  function newBodyParams(): URLSearchParams {
    const bodyParams = new URLSearchParams();
    bodyParams.set("f", state.subPath);
    return bodyParams;
  }

  // Trash and Undo Trash
  async function TrashFile() {
    if (!state || isReadOnly) return;

    setIsLoading(true);
    const bodyParams = newBodyParams();
    if (state.collections !== undefined) {
      bodyParams.set("collections", state.collections.toString());
    }
    const subPath = state.subPath;

    // Add remove tag
    if (!isMarkedAsDeleted) {
      const resultDo = await FetchPost(new UrlQuery().UrlMoveToTrashApi(), bodyParams.toString());
      if (
        resultDo.statusCode &&
        resultDo.statusCode !== 200 &&
        resultDo.statusCode !== 404 &&
        resultDo.statusCode !== 400
      ) {
        // 404: file can already been deleted
        console.error(resultDo);
        setIsLoading(false);
        return;
      }

      let newStatus = (resultDo.data as IFileIndexItem[])?.find(
        (x) => x.filePath === subPath
      )?.status;
      if (!newStatus) {
        newStatus = IExifStatus.Deleted;
      }
      dispatch({
        type: "update",
        filePath: subPath,
        status: newStatus,
        lastEdited: new Date().toISOString()
      });
      setIsLoading(false);
    }
    // Undo trash
    else {
      bodyParams.set("fieldName", "tags");
      bodyParams.set("search", "!delete!");
      const resultUndo = await FetchPost(new UrlQuery().UrlReplaceApi(), bodyParams.toString());
      if (resultUndo.statusCode !== 200) {
        console.error(resultUndo);
        setIsLoading(false);
        return;
      }
      dispatch({ type: "remove", tags: "!delete!" });
      dispatch({
        type: "update",
        filePath: subPath,
        status: IExifStatus.Ok,
        lastEdited: new Date().toISOString()
      });
      setIsLoading(false);
    }

    ClearSearchCache(history.location.search);

    // Client side Caching: the order of files in a normal folder has changed
    // Entire cache because the relativeObjects objects can reference to this page
    new FileListCache().CacheCleanEverything();
  }

  useKeyboardEvent(/(Delete)/, (event: KeyboardEvent) => {
    if (new Keyboard().isInForm(event)) return;
    event.preventDefault();
    TrashFile();
  });

  const [isModalDownloadOpen, setIsModalDownloadOpen] = React.useState(false);
  const [isModalRenameFileOpen, setIsModalRenameFileOpen] = React.useState(false);
  const [isModalMoveFile, setIsModalMoveFile] = React.useState(false);
  const [isModalPublishOpen, setIsModalPublishOpen] = useState(false);

  return (
    <>
      {isLoading ? <Preloader isWhite={false} isOverlay={true} /> : ""}

      {/* allowed in readonly to download */}
      {isModalDownloadOpen && state && !isSourceMissing ? (
        <ModalDownload
          collections={false}
          handleExit={() => setIsModalDownloadOpen(!isModalDownloadOpen)}
          select={[state.subPath]}
          isOpen={isModalDownloadOpen}
        />
      ) : null}
      {isModalRenameFileOpen && state && !isReadOnly ? (
        <ModalDetailviewRenameFile
          state={state}
          handleExit={() => setIsModalRenameFileOpen(!isModalRenameFileOpen)}
          isOpen={isModalRenameFileOpen}
        />
      ) : null}
      {isModalMoveFile && state && !isReadOnly ? (
        <ModalMoveFile
          selectedSubPath={state.fileIndexItem.filePath}
          parentDirectory={state.fileIndexItem.parentDirectory}
          handleExit={() => setIsModalMoveFile(!isModalMoveFile)}
          isOpen={isModalMoveFile}
        />
      ) : null}

      <ModalPublishToggleWrapper
        select={[state.fileIndexItem.fileName]}
        stateFileIndexItems={[state.fileIndexItem]}
        isModalPublishOpen={isModalPublishOpen}
        setModalPublishOpen={setIsModalPublishOpen}
      />

      <header className={GetHeaderClass(details, isMarkedAsDeleted)}>
        <div className="wrapper">
          {/* in directory state aka no search */}
          {!isSearchQuery ? (
            <Link
              className="item item--first item--close"
              data-test="menu-detail-view-close"
              state={{ filePath: state.fileIndexItem.filePath } as INavigateState}
              onClick={(event) => {
                // Command (mac) or ctrl click means open new window
                // event.button = is only trigged in safari
                if (event.metaKey || event.ctrlKey || event.button === 1) return;
                setIsLoading(true);
              }}
              to={new UrlQuery().updateFilePathHash(
                history.location.search,
                state.fileIndexItem.parentDirectory
              )}
            >
              {MessageCloseDialogBackToFolder}
            </Link>
          ) : null}

          {/* in search state aka search query */}
          <IsSearchQueryMenuSearchItem
            isSearchQuery={isSearchQuery}
            setIsLoading={setIsLoading}
            state={state}
            history={history}
          />

          {/* not more menu */}
          <button
            className="item item--labels"
            data-test="menu-detail-view-labels"
            onClick={() => {
              toggleLabels();
            }}
            onKeyDown={(event) => {
              event.key === "Enter" && toggleLabels();
            }}
          >
            Labels
          </button>

          <MoreMenu setEnableMoreMenu={setEnableMoreMenu} enableMoreMenu={enableMoreMenu}>
            <GoToParentFolder isSearchQuery={isSearchQuery} history={history} state={state} />
            <MenuOptionModal
              // Export or Download
              isReadOnly={isSourceMissing}
              isSet={isModalDownloadOpen}
              set={() => setIsModalDownloadOpen(!isModalDownloadOpen)}
              localization={localization.MessageDownload}
              testName="download"
            />
            {!details ? (
              <MenuOption
                isReadOnly={false}
                onClickKeydown={toggleLabels}
                testName="labels"
                localization={localization.MessageLabels}
              />
            ) : null}
            <MenuOptionModal
              isReadOnly={isReadOnly}
              isSet={isModalMoveFile}
              set={() => setIsModalMoveFile(!isModalMoveFile)}
              localization={localization.MessageMove}
              testName="move"
            />

            <MenuOptionModal
              isReadOnly={isReadOnly}
              isSet={isModalRenameFileOpen}
              set={() => setIsModalRenameFileOpen(!isModalRenameFileOpen)}
              localization={localization.MessageRenameFileName}
              testName="rename"
            />

            <MenuOption isReadOnly={isReadOnly} onClickKeydown={TrashFile} testName="trash">
              {!isMarkedAsDeleted ? MessageMoveToTrash : MessageRestoreFromTrash}

              {state.collections &&
              state.fileIndexItem.collectionPaths &&
              state.fileIndexItem.collectionPaths?.length >= 2 ? (
                <em data-test="trash-including">
                  <br />
                  {MessageIncludingColonWord}
                  {new Comma().CommaSpaceLastDot(state.fileIndexItem.collectionPaths)}
                </em>
              ) : null}
            </MenuOption>

            <MenuOptionRotateImage90
              setIsLoading={setIsLoading}
              state={state}
              dispatch={dispatch}
              isMarkedAsDeleted={isMarkedAsDeleted}
              isReadOnly={isReadOnly}
            ></MenuOptionRotateImage90>

            <MenuOptionModal
              isReadOnly={false}
              testName="publish"
              isSet={isModalPublishOpen}
              set={setIsModalPublishOpen}
              localization={localization.MessagePublish}
            />

            <MenuOptionDesktopEditorOpenSingle
              subPath={state.subPath}
              isReadOnly={state.isReadOnly}
              collections={state.collections === true}
            />
          </MoreMenu>
        </div>
      </header>

      {details ? (
        <div className="header header--sidebar">
          <button
            className="item item--close"
            data-test="menu-detail-view-close-details"
            onClick={() => {
              toggleLabels();
            }}
            onKeyDown={(event) => {
              event.key === "Enter" && toggleLabels();
            }}
          >
            {MessageCloseDetailScreenDialog}
            {isRecentEdited ? (
              <div data-test="menu-detail-view-autosave" className="autosave">
                {MessageSaved}
              </div>
            ) : null}
          </button>
        </div>
      ) : (
        ""
      )}
    </>
  );
};

export default MenuDetailView;
