import React, { memo } from 'react';
import Modal from './modal';

interface IModalRenameFileProps {
  isOpen: boolean;
  handleExit: Function;
}

const ModalRenameFile: React.FunctionComponent<IModalRenameFileProps> = memo((props) => {
  return (<>
    return (<Modal
      id="rename-file-modal"
      isOpen={props.isOpen}
      handleExit={() => {
        props.handleExit()
      }}>
      <div className="modal content--subheader">Naam wijzigen</div>

    </Modal>
  </>)
});

export default ModalRenameFile