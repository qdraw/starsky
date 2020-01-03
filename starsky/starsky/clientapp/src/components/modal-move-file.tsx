import React from 'react';
import { DetailViewContext } from '../contexts/detailview-context';
import useFileList from '../hooks/use-filelist';
import { IArchive } from '../interfaces/IArchive';
import { FileExtensions } from '../shared/file-extensions';
import ItemTextListView from './item-text-list-view';
import Modal from './modal';

interface IModalMoveFileProps {
  isOpen: boolean;
  handleExit: Function;
}

const ModalMoveFile: React.FunctionComponent<IModalMoveFileProps> = (props) => {
  let { state, } = React.useContext(DetailViewContext);



  const [currentFolderPath, setCurrentFolderPath] = React.useState(state.fileIndexItem.parentDirectory);

  console.log(state ? "?f=" + state.fileIndexItem.parentDirectory : "/");

  var usesFileList = useFileList("?f=" + currentFolderPath);

  const archive: IArchive | undefined = usesFileList ? usesFileList.archive : undefined;

  if (!archive) return <>Input Not found</>


  return (<Modal
    id="move-file-modal"
    isOpen={props.isOpen}
    handleExit={() => {
      props.handleExit()
    }}>
    <div className="content">
      <div className="modal content--subheader"><b>{currentFolderPath}</b></div>
      <div className="modal modal-move content--text">

        {currentFolderPath !== "/" ?
          <ul>
            <li className={"box parent"}><button onClick={() => { setCurrentFolderPath(new FileExtensions().GetParentPath(currentFolderPath)) }}> {new FileExtensions().GetParentPath(currentFolderPath)}</button></li>
          </ul>
          : null}

        <ItemTextListView fileIndexItems={archive.fileIndexItems} callback={(path) => { setCurrentFolderPath(path) }} />
      </div>
      <div className="modal modal-move-button">
        <button> Verplaats</button>
      </div>
    </div>
  </Modal>)
};

export default ModalMoveFile