import React from 'react';
import useFileList from '../hooks/use-filelist';
import useLocation from '../hooks/use-location';
import { IArchive } from '../interfaces/IArchive';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import FetchPost from '../shared/fetch-post';
import { FileExtensions } from '../shared/file-extensions';
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

  const archive: IArchive | undefined = usesFileList ? usesFileList.archive : undefined;

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

  if (!archive) return <>Input Not found</>

  return (<Modal
    id="move-file-modal"
    isOpen={props.isOpen}
    handleExit={() => {
      props.handleExit()
    }}>
    <div className="content">
      <div className="modal content--subheader">Verplaats {new FileExtensions().GetFileName(props.selectedSubPath)} naar: <b>{currentFolderPath}</b></div>
      <div className={error ? "modal modal-move content--text modal-move--error-space" : "modal modal-move content--text"}>
        {currentFolderPath !== "/" ?
          <ul>
            <li className={"box parent"}>
              <button onClick={() => {
                setCurrentFolderPath(new FileExtensions().GetParentPath(currentFolderPath))
              }}>
                {new FileExtensions().GetParentPath(currentFolderPath)}
              </button>
            </li>
          </ul>
          : null}
        <ItemTextListView fileIndexItems={archive.fileIndexItems} callback={(path) => { setCurrentFolderPath(path) }} />
      </div>
      <div className="modal modal-move-button">
        {error && <div className="warning-box">{error}</div>}
        <button disabled={currentFolderPath === props.parentDirectory} className="btn btn--default" onClick={move}>Verplaats</button>
      </div>
    </div>
  </Modal>)
};

export default ModalMoveFile