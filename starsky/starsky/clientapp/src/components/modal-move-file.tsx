import React from 'react';
import useFileList from '../hooks/use-filelist';
import { IArchive } from '../interfaces/IArchive';
import FetchPost from '../shared/fetch-post';
import { FileExtensions } from '../shared/file-extensions';
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

  async function move() {
    var bodyParams = new URLSearchParams();
    bodyParams.append("f", props.selectedSubPath);
    bodyParams.append("to", currentFolderPath);
    bodyParams.append("collections", true.toString());

    var resultDo = await FetchPost(new UrlQuery().UrlSyncRename(), bodyParams.toString())
    if (resultDo.statusCode !== 200) {
      console.error(resultDo);
      return;
    }
    // update url hash

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
      <div className="modal modal-move content--text">

        {currentFolderPath !== "/" ?
          <ul>
            <li className={"box parent"}><button onClick={() => { setCurrentFolderPath(new FileExtensions().GetParentPath(currentFolderPath)) }}> {new FileExtensions().GetParentPath(currentFolderPath)}</button></li>
          </ul>
          : null}

        <ItemTextListView fileIndexItems={archive.fileIndexItems} callback={(path) => { setCurrentFolderPath(path) }} />
      </div>
      <div className="modal modal-move-button">
        <button onClick={move}>Verplaats</button>
      </div>
    </div>
  </Modal>)
};

export default ModalMoveFile