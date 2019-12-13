import React, { memo, useEffect } from 'react';
import { DetailViewContext } from '../contexts/detailview-context';
import { IExifStatus } from '../interfaces/IExifStatus';
import Modal from './modal';

interface IModalRenameFileProps {
  isOpen: boolean;
  handleExit: Function;
}

const ModalDetailviewRenameFile: React.FunctionComponent<IModalRenameFileProps> = memo((props) => {

  let { state, dispatch } = React.useContext(DetailViewContext);

  // For the display
  const [isFormEnabled, setFormEnabled] = React.useState(true);
  useEffect(() => {
    if (!state.fileIndexItem.status) return;
    switch (state.fileIndexItem.status) {
      case IExifStatus.Deleted:
      case IExifStatus.ReadOnly:
      case IExifStatus.ServerError:
      case IExifStatus.NotFoundSourceMissing:
        setFormEnabled(false);
        break;
      default:
        setFormEnabled(true);
        break;
    }
  }, [state.fileIndexItem.status]);

  // The Updated that is send to the api
  const [fileName, setFileName] = React.useState(state.fileIndexItem.fileName);

  function handleUpdateChange(event: React.ChangeEvent<HTMLDivElement> | React.KeyboardEvent<HTMLDivElement>) {
    if (!isFormEnabled) return;
    if (!event.currentTarget.textContent) return null;
    let fieldValue = event.currentTarget.textContent.trim();
    setFileName(fieldValue);
    if (fieldValue.endsWith('.jpg')) {
      console.log('hi');

    }
  }


  function pushRenameChange(event: React.MouseEvent<HTMLButtonElement>) {

  }

  return (<>
    return (<Modal
      id="rename-file-modal"
      isOpen={props.isOpen}
      handleExit={() => {
        props.handleExit()
      }}>
      <div className="modal content--subheader">Naam wijzigen</div>
      <div className="modal content--text">

        <div data-name="filename"
          onInput={handleUpdateChange}
          suppressContentEditableWarning={true}
          contentEditable={isFormEnabled}
          className={isFormEnabled ? "form-control" : "form-control disabled"}>
          {state.fileIndexItem.fileName}
        </div>

        <button className="btn btn--default" onClick={pushRenameChange}>Opslaan</button>

      </div>
    </Modal>
  </>)
});

export default ModalDetailviewRenameFile