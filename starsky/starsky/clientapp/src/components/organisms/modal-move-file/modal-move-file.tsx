import { useState } from "react";
import useFileList, { IFileList } from "../../../hooks/use-filelist";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useLocation from "../../../hooks/use-location/use-location";
import { newIArchive } from "../../../interfaces/IArchive";
import { PageType } from "../../../interfaces/IDetailView";
import { IFileIndexItem, newIFileIndexItemArray } from "../../../interfaces/IFileIndexItem";
import localization from "../../../localization/localization.json";
import FetchPost from "../../../shared/fetch/fetch-post";
import { FileExtensions } from "../../../shared/file-extensions";
import { FileListCache } from "../../../shared/filelist-cache";
import { Language } from "../../../shared/language";
import { StringOptions } from "../../../shared/string-options";
import { UrlQuery } from "../../../shared/url/url-query";
import Modal from "../../atoms/modal/modal";
import ItemTextListView from "../../molecules/item-text-list-view/item-text-list-view";

interface IModalMoveFileProps {
  isOpen: boolean;
  handleExit: () => void;
  selectedSubPath: string;
  parentDirectory: string;
}

const ModalMoveFile: React.FunctionComponent<IModalMoveFileProps> = (props) => {
  const [currentFolderPath, setCurrentFolderPath] = useState(props.parentDirectory);

  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageMove = language.key(localization.MessageMove);
  const MessageTo = language.key(localization.MessageTo);

  let usesFileList = useFileList("?f=" + currentFolderPath, true);

  // only for navigation in this file
  const history = useLocation();

  // to show errors
  const useErrorHandler = (initialState: string | null) => initialState;
  const [error, setError] = useState(useErrorHandler(null));

  /**
   * Move {props.selectedSubPath} to {currentFolderPath}
   */
  async function MoveFile() {
    const bodyParams = new URLSearchParams();
    bodyParams.append("f", props.selectedSubPath);

    // selectedSubPath can contain ; as a separator for multiple files the "to" path
    // must contain the same number of paths which is currentFolderPath
    // the "to" path is "{currentFolderPath};{currentFolderPath};{currentFolderPath}"
    if (props.selectedSubPath.includes(";")) {
      const selectedPaths = props.selectedSubPath.split(";").filter(Boolean);
      // Create a string like "folder;folder;folder" with the same count as selectedPaths
      const toValue = Array(selectedPaths.length).fill(currentFolderPath).join(";");
      bodyParams.append("to", toValue);
    } else {
      bodyParams.append("to", currentFolderPath);
    }
    bodyParams.append("collections", true.toString());

    const resultDo = await FetchPost(new UrlQuery().UrlDiskRename(), bodyParams.toString());

    if (!Array.isArray(resultDo.data) || resultDo.data.length === 0 || !resultDo.data[0].status) {
      console.error("server error");
      setError("Server error");
      return null;
    }

    const fileIndexItems = resultDo.data as IFileIndexItem[];
    if (resultDo.statusCode !== 200) {
      console.error(resultDo);
      setError(fileIndexItems[0].status.toString());
      return null;
    }

    // clean user cache
    new FileListCache().CacheCleanEverything();

    // now go to the new location
    if (props.selectedSubPath.includes(";")) {
      // when selecting multiple files
      const toNavigateUrl = new UrlQuery().updateFilePathHash(
        history.location.search,
        fileIndexItems[0].parentDirectory
      );
      history.navigate(toNavigateUrl, { replace: true });
    } else {
      // a single file
      const toNavigateUrl = new UrlQuery().updateFilePathHash(
        history.location.search,
        fileIndexItems[0].filePath
      );
      history.navigate(toNavigateUrl, { replace: true });
    }

    // and close window
    props.handleExit();
  }

  /**
   * Fallback if there is no result or when mounting with no context
   */
  if (!usesFileList?.archive) {
    usesFileList = {
      archive: newIArchive(),
      pageType: PageType.Loading
    } as IFileList;
  }
  const usesFileListArchive = usesFileList.archive
    ? usesFileList.archive.fileIndexItems
    : newIFileIndexItemArray();

  return (
    <Modal
      id="move-file-modal"
      isOpen={props.isOpen}
      handleExit={() => {
        props.handleExit();
      }}
    >
      <div className="content" data-test="modal-move-file">
        <div className="modal content--subheader">
          {MessageMove}{" "}
          {new StringOptions().LimitLength(
            new FileExtensions().GetFileName(props.selectedSubPath),
            30
          )}{" "}
          {MessageTo}:&nbsp;
          <b>{new StringOptions().LimitLength(currentFolderPath, 44)}</b>
        </div>
        <div
          className={
            error
              ? "modal modal-move content--text modal-move--error-space"
              : "modal modal-move content--text"
          }
        >
          {currentFolderPath !== "/" ? (
            <ul>
              <li className={"box parent"}>
                <button
                  data-test="parent"
                  onClick={() => {
                    setCurrentFolderPath(new FileExtensions().GetParentPath(currentFolderPath));
                  }}
                >
                  {new FileExtensions().GetParentPath(currentFolderPath)}
                </button>
              </li>
            </ul>
          ) : null}

          {usesFileList.pageType === PageType.Loading ? (
            <div data-test="preloader-inside" className="preloader preloader--inside"></div>
          ) : null}
          {usesFileList.pageType !== PageType.Loading ? (
            <ItemTextListView
              fileIndexItems={usesFileListArchive}
              callback={(path) => {
                setCurrentFolderPath(path);
              }}
            />
          ) : null}
        </div>
        <div className="modal modal-move-button">
          {error && (
            <div data-test="modal-move-file-warning-box" className="warning-box">
              {error}
            </div>
          )}
          <button
            disabled={
              currentFolderPath === props.parentDirectory ||
              usesFileList.pageType === PageType.Loading
            }
            data-test="modal-move-file-btn-default"
            className="btn btn--default"
            onClick={MoveFile}
          >
            {MessageMove}
          </button>
        </div>
      </div>
    </Modal>
  );
};

export default ModalMoveFile;
