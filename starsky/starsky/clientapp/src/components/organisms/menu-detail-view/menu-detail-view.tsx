import React, { useEffect, useState } from "react";
import { DetailViewAction } from "../../../contexts/detailview-context";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useKeyboardEvent from "../../../hooks/use-keyboard/use-keyboard-event";
import useLocation from "../../../hooks/use-location";
import { IDetailView } from "../../../interfaces/IDetailView";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import {
  IFileIndexItem,
  Orientation
} from "../../../interfaces/IFileIndexItem";
import { INavigateState } from "../../../interfaces/INavigateState";
import localization from "../../../localization/localization.json";
import { CastToInterface } from "../../../shared/cast-to-interface";
import { Comma } from "../../../shared/comma";
import { IsEditedNow } from "../../../shared/date";
import FetchGet from "../../../shared/fetch-get";
import FetchPost from "../../../shared/fetch-post";
import { FileListCache } from "../../../shared/filelist-cache";
import { Keyboard } from "../../../shared/keyboard";
import { Language } from "../../../shared/language";
import { ClearSearchCache } from "../../../shared/search/clear-search-cache";
import { URLPath } from "../../../shared/url-path";
import { UrlQuery } from "../../../shared/url-query";
import Link from "../../atoms/link/link";
import MenuOption from "../../atoms/menu-option/menu-option";
import MoreMenu from "../../atoms/more-menu/more-menu";
import Preloader from "../../atoms/preloader/preloader";
import IsSearchQueryMenuSearchItem from "../../molecules/is-search-query-menu-search-item/is-search-query-menu-search-item";
import ModalDetailviewRenameFile from "../modal-detailview-rename-file/modal-detailview-rename-file";
import ModalDownload from "../modal-download/modal-download";
import ModalMoveFile from "../modal-move-file/modal-move-file";
import ModalPublishToggleWrapper from "../modal-publish/modal-publish-toggle-wrapper";

export interface MenuDetailViewProps {
  state: IDetailView;
  dispatch: React.Dispatch<DetailViewAction>;
}

function GetHeaderClass(
  isDetails: boolean | undefined,
  isMarkedAsDeleted: boolean
): string {
  if (isDetails) {
    if (isMarkedAsDeleted) {
      return "header header--main header--edit header--deleted";
    } else {
      return "header header--main header--edit";
    }
  } else {
    if (isMarkedAsDeleted) {
      return "header header--main header--deleted";
    } else {
      return "header header--main";
    }
  }
}

const MenuDetailView: React.FunctionComponent<MenuDetailViewProps> = ({
  state,
  dispatch
}) => {
  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  // Not close anymore because its looks like closing a window
  const MessageCloseDialogBackToFolder = language.key(
    localization.MessageCloseDialogBackToFolder
  );
  const MessageCloseDetailScreenDialog = language.key(
    localization.MessageCloseDetailScreenDialog
  );
  const MessageSaved = language.key(localization.MessageSaved);
  const MessageMoveToTrash = language.key(localization.MessageMoveToTrash);
  const MessageIncludingColonWord = language.key(
    localization.MessageIncludingColonWord
  );
  const MessageRestoreFromTrash = language.key(
    localization.MessageRestoreFromTrash
  );
  const MessageMove = language.key(localization.MessageMove);

  const MessageRenameFileName = language.text(
    "Bestandsnaam wijzigen",
    "Rename file name"
  );
  const MessageRotateToRight = language.text(
    "Rotatie naar rechts",
    "Rotation to the right"
  );
  const MessageGoToParentFolder = language.text(
    "Ga naar bovenliggende map",
    "Go to parent folder"
  );

  const history = useLocation();

  const [isDetails, setDetails] = React.useState(
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
  const [isRecentEdited, setRecentEdited] = React.useState(
    IsEditedNow(state?.fileIndexItem?.lastEdited)
  );
  useEffect(() => {
    if (!state?.fileIndexItem?.lastEdited) return;
    const isEditedNow = IsEditedNow(state.fileIndexItem.lastEdited);
    if (!isEditedNow) {
      setRecentEdited(false);
      return;
    }
    setRecentEdited(isEditedNow);
  }, [state?.fileIndexItem?.lastEdited]);

  function toggleLabels() {
    const urlObject = new URLPath().StringToIUrl(history.location.search);
    urlObject.details = !isDetails;
    setDetails(urlObject.details);
    setRecentEdited(false); // disable to avoid animation
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true });
  }

  const [isMarkedAsDeleted, setMarkedAsDeleted] = React.useState(
    state?.fileIndexItem?.status === IExifStatus.Deleted
  );
  const [enableMoreMenu, setEnableMoreMenu] = React.useState(false);

  /* only update when the state is changed */
  useEffect(() => {
    setMarkedAsDeleted(state.fileIndexItem.status === IExifStatus.Deleted);
  }, [state.fileIndexItem.status, history.location.search]);

  const [isSourceMissing, setSourceMissing] = React.useState(
    state.fileIndexItem.status === IExifStatus.NotFoundSourceMissing
  );

  useEffect(() => {
    setSourceMissing(
      state.fileIndexItem.status === IExifStatus.NotFoundSourceMissing
    );
    setReadOnly(
      state.fileIndexItem.status === IExifStatus.NotFoundSourceMissing
    );
  }, [state.fileIndexItem.status, history.location.search]);

  /* only update when the state is changed */
  const [isReadOnly, setReadOnly] = React.useState(state.isReadOnly);
  useEffect(() => {
    if (state.fileIndexItem.status === IExifStatus.NotFoundSourceMissing)
      return;
    setReadOnly(state.isReadOnly);
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
      const resultDo = await FetchPost(
        new UrlQuery().UrlMoveToTrashApi(),
        bodyParams.toString()
      );
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
      const resultUndo = await FetchPost(
        new UrlQuery().UrlReplaceApi(),
        bodyParams.toString()
      );
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

  /**
   * Checks if the hash is changes and update Context:  orientation + fileHash
   */
  async function requestNewFileHash(): Promise<boolean | null> {
    const resultGet = await FetchGet(
      new UrlQuery().UrlIndexServerApi({ f: state.subPath })
    );
    if (!resultGet) return null;
    if (resultGet.statusCode !== 200) {
      console.error(resultGet);
      setIsLoading(false);
      return null;
    }
    const media = new CastToInterface().MediaDetailView(resultGet.data).data;
    const orientation = media?.fileIndexItem?.orientation
      ? media.fileIndexItem.orientation
      : Orientation.Horizontal;

    // the hash changes if you rotate an image
    if (media.fileIndexItem.fileHash === state.fileIndexItem.fileHash)
      return false;

    dispatch({
      type: "update",
      orientation,
      fileHash: media.fileIndexItem.fileHash,
      filePath: media.fileIndexItem.filePath
    });
    setIsLoading(false);
    return true;
  }

  /**
   * Update the rotation status
   */
  async function rotateImage90() {
    if (isMarkedAsDeleted || isReadOnly) return;
    setIsLoading(true);

    const bodyParams = newBodyParams();
    bodyParams.set("rotateClock", "1");
    const resultPost = await FetchPost(
      new UrlQuery().UrlUpdateApi(),
      bodyParams.toString()
    );
    if (resultPost.statusCode !== 200) {
      console.error(resultPost);
      return;
    }

    // there is an async backend event triggered, sometimes there is an que
    setTimeout(() => {
      requestNewFileHash().then((result) => {
        if (result === false) {
          setTimeout(() => {
            requestNewFileHash().then(() => {
              // when it didn't change after two tries
              setIsLoading(false);
            });
          }, 7000);
        }
      });
    }, 3000);
  }

  useKeyboardEvent(/(Delete)/, (event: KeyboardEvent) => {
    if (new Keyboard().isInForm(event)) return;
    event.preventDefault();
    TrashFile();
  });

  const [isModalExportOpen, setModalExportOpen] = React.useState(false);
  const [isModalRenameFileOpen, setModalRenameFileOpen] = React.useState(false);
  const [isModalMoveFile, setModalMoveFile] = React.useState(false);
  const [isModalPublishOpen, setModalPublishOpen] = useState(false);

  const goToParentFolderJSX: React.JSX.Element | null = isSearchQuery ? (
    <li
      className="menu-option"
      data-test="go-to-parent-folder"
      onClick={() =>
        history.navigate(
          new UrlQuery().updateFilePathHash(
            history.location.search,
            state.fileIndexItem.parentDirectory,
            true
          ),
          {
            state: {
              filePath: state.fileIndexItem.filePath
            } as INavigateState
          }
        )
      }
    >
      {MessageGoToParentFolder}
    </li>
  ) : null;

  return (
    <>
      {isLoading ? <Preloader isWhite={false} isOverlay={true} /> : ""}

      {/* allowed in readonly to download */}
      {isModalExportOpen && state && !isSourceMissing ? (
        <ModalDownload
          collections={false}
          handleExit={() => setModalExportOpen(!isModalExportOpen)}
          select={[state.subPath]}
          isOpen={isModalExportOpen}
        />
      ) : null}
      {isModalRenameFileOpen && state && !isReadOnly ? (
        <ModalDetailviewRenameFile
          state={state}
          handleExit={() => setModalRenameFileOpen(!isModalRenameFileOpen)}
          isOpen={isModalRenameFileOpen}
        />
      ) : null}
      {isModalMoveFile && state && !isReadOnly ? (
        <ModalMoveFile
          selectedSubPath={state.fileIndexItem.filePath}
          parentDirectory={state.fileIndexItem.parentDirectory}
          handleExit={() => setModalMoveFile(!isModalMoveFile)}
          isOpen={isModalMoveFile}
        />
      ) : null}

      <ModalPublishToggleWrapper
        select={[state.fileIndexItem.fileName]}
        stateFileIndexItems={[state.fileIndexItem]}
        isModalPublishOpen={isModalPublishOpen}
        setModalPublishOpen={setModalPublishOpen}
      />

      <header className={GetHeaderClass(isDetails, isMarkedAsDeleted)}>
        <div className="wrapper">
          {/* in directory state aka no search */}
          {!isSearchQuery ? (
            <Link
              className="item item--first item--close"
              data-test="menu-detail-view-close"
              state={
                { filePath: state.fileIndexItem.filePath } as INavigateState
              }
              onClick={(event) => {
                // Command (mac) or ctrl click means open new window
                // event.button = is only trigged in safari
                if (event.metaKey || event.ctrlKey || event.button === 1)
                  return;
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

          <button
            className="item item--labels"
            data-test="menu-detail-view-labels"
            onClick={() => {
              toggleLabels();
            }}
          >
            Labels
          </button>
          <MoreMenu
            setEnableMoreMenu={setEnableMoreMenu}
            enableMoreMenu={enableMoreMenu}
          >
            {goToParentFolderJSX}
            <li
              tabIndex={0}
              className={
                !isSourceMissing ? "menu-option" : "menu-option disabled"
              }
              data-test="export"
              onClick={() => setModalExportOpen(!isModalExportOpen)}
            >
              Download
            </li>
            {!isDetails ? (
              <li
                tabIndex={0}
                className="menu-option"
                data-test="labels"
                onClick={toggleLabels}
              >
                Labels
              </li>
            ) : null}
            <li
              tabIndex={0}
              className={!isReadOnly ? "menu-option" : "menu-option disabled"}
              data-test="move"
              onClick={() => setModalMoveFile(!isModalMoveFile)}
            >
              {MessageMove}
            </li>
            <li
              tabIndex={0}
              className={!isReadOnly ? "menu-option" : "menu-option disabled"}
              data-test="rename"
              onClick={() => setModalRenameFileOpen(!isModalRenameFileOpen)}
            >
              {MessageRenameFileName}
            </li>
            <li
              tabIndex={0}
              className={!isReadOnly ? "menu-option" : "menu-option disabled"}
              data-test="trash"
              onClick={TrashFile}
            >
              {!isMarkedAsDeleted
                ? MessageMoveToTrash
                : MessageRestoreFromTrash}

              {state.collections &&
              state.fileIndexItem.collectionPaths &&
              state.fileIndexItem.collectionPaths?.length >= 2 ? (
                <em data-test="trash-including">
                  {MessageIncludingColonWord}
                  {new Comma().CommaSpaceLastDot(
                    state.fileIndexItem.collectionPaths
                  )}
                </em>
              ) : null}
            </li>
            <li
              tabIndex={0}
              className={!isReadOnly ? "menu-option" : "menu-option disabled"}
              data-test="rotate"
              onClick={rotateImage90}
            >
              {MessageRotateToRight}
            </li>
            <MenuOption
              isReadOnly={false}
              testName="publish"
              isSet={isModalPublishOpen}
              set={setModalPublishOpen}
              localization={localization.MessagePublish}
            />
          </MoreMenu>
        </div>
      </header>

      {isDetails ? (
        <div className="header header--sidebar">
          <div
            className="item item--close"
            onClick={() => {
              toggleLabels();
            }}
          >
            {MessageCloseDetailScreenDialog}
            {isRecentEdited ? (
              <div data-test="menu-detail-view-autosave" className="autosave">
                {MessageSaved}
              </div>
            ) : null}
          </div>
        </div>
      ) : (
        ""
      )}
    </>
  );
};

export default MenuDetailView;
