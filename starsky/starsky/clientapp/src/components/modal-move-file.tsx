import React, { useEffect, useState } from 'react';
import useFileList from '../hooks/use-filelist';
import useLocation from '../hooks/use-location';
import { IArchive, newIArchive } from '../interfaces/IArchive';
import { PageType } from '../interfaces/IDetailView';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import FetchPost from '../shared/fetch-post';
import { FileExtensions } from '../shared/file-extensions';
import { StringOptions } from '../shared/string-options';
import { URLPath } from '../shared/url-path';
import { UrlQuery } from '../shared/url-query';
import ItemTextListView from './item-text-list-view';
import Modal from './modal';

interface IModalMoveFileProps {
  isOpen: boolean;
  handleExit: Function;
  selectedSubPath: string,
  parentDirectory: string;
}

const ModalMoveFile: React.FunctionComponent<IModalMoveFileProps> = (props) => {

  const [currentFolderPath, setCurrentFolderPath] = React.useState(props.parentDirectory);

  var usesFileList = useFileList("?f=" + currentFolderPath);

  let archive: IArchive | undefined = usesFileList ? usesFileList.archive : undefined;
  const [pageType, setPageType] = useState(usesFileList ? usesFileList.pageType : PageType.Loading);

  if (!archive) {
    archive = newIArchive();
  }

  useEffect(() => {
    setPageType(usesFileList ? usesFileList.pageType : PageType.Loading)
  }, [archive])

  // only for navigation in this file
  var history = useLocation();

  // to show errors
  const useErrorHandler = (initialState: string | null) => { return initialState };
  const [error, setError] = React.useState(useErrorHandler(null));

  /**
   * Move {props.selectedSubPath} to {currentFolderPath}
   */
  async function move() {
    var bodyParams = new URLSearchParams();
    bodyParams.append("f", props.selectedSubPath);
    bodyParams.append("to", currentFolderPath);
    bodyParams.append("collections", true.toString());

    var resultDo = await FetchPost(new UrlQuery().UrlSyncRename(), bodyParams.toString())

    if (!resultDo.data || resultDo.data.length === 0) {
      console.error('server error');
      setError("Server error")
      return;
    }

    var fileIndexItems = resultDo.data as IFileIndexItem[];
    if (resultDo.statusCode !== 200) {
      console.error(resultDo);
      setError(fileIndexItems[0].status.toString())
      return;
    }

    // now go to the new location
    var toNavigateUrl = new URLPath().updateFilePath(history.location.search, fileIndexItems[0].filePath)
    history.navigate(toNavigateUrl, { replace: true });

    // and close window
    props.handleExit();
  }

  return (<Modal
    id="move-file-modal"
    isOpen={props.isOpen}
    handleExit={() => {
      props.handleExit()
    }}>
    <div className="content">
      <div className="modal content--subheader">
        Verplaats {new StringOptions().LimitLength(new FileExtensions().GetFileName(props.selectedSubPath), 30)} naar:
        <b>{new StringOptions().LimitLength(currentFolderPath, 44)}</b>
      </div>
      <div className={error ? "modal modal-move content--text modal-move--error-space" : "modal modal-move content--text"}>
        {currentFolderPath !== "/" ?
          <ul>
            <li className={"box parent"}>
              <button data-test="parent" onClick={() => {
                setCurrentFolderPath(new FileExtensions().GetParentPath(currentFolderPath));
                setPageType(PageType.Loading);
              }}>
                {new FileExtensions().GetParentPath(currentFolderPath)}
              </button>
            </li>
          </ul>
          : null}
        {pageType !== PageType.Loading ? <ItemTextListView fileIndexItems={archive.fileIndexItems} callback={(path) => {
          setCurrentFolderPath(path);
          setPageType(PageType.Loading);
        }} /> : null}

      </div>
      <div className="modal modal-move-button">
        {error && <div className="warning-box">{error}</div>}
        <button disabled={currentFolderPath === props.parentDirectory} className="btn btn--default" onClick={move}>Verplaats</button>
      </div>
    </div>
  </Modal>)
};

export default ModalMoveFile