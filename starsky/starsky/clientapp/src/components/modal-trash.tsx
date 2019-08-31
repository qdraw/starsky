import React, { memo, useEffect } from 'react';
import Modal from './modal';

interface IModalTrashProps {
  isOpen: boolean;
  isMarkedAsDeleted?: boolean;
}
{/* <ModalTrash isOpen={isTrashModalOpen} isMarkedAsDeleted={props.isMarkedAsDeleted}></ModalTrash> */ }

const ModalTrash: React.FunctionComponent<IModalTrashProps> = memo((props) => {
  const [isTrashModalOpen, setTrashModalOpen] = React.useState(false);
  const [isMarkedAsDeleted, setMarkedAsDeleted] = React.useState(false);

  useEffect(() => {
    setTrashModalOpen(props.isOpen);
    if (!props.isMarkedAsDeleted) return;
    setMarkedAsDeleted(props.isMarkedAsDeleted);
  }, [props]);

  if (isMarkedAsDeleted) {
    return (<>isMarkedAsDeleted</>)
  }

  return (<Modal
    id="trash-modal"
    root="root"
    isOpen={isTrashModalOpen}
    handleExit={() => setTrashModalOpen(false)}
  >
    <div className="modal content--subheader">Weggooien</div>
    <div className="modal content--text">
      Weet je zeker dat je dit bestand wil verpaatsen naar de prullenmand?
       <br />
      <a onClick={() => setTrashModalOpen(false)} className="btn btn--info">Annuleren</a>
      <button onClick={() => {

      }} className="btn btn--default">Weggooien</button>
    </div>
  </Modal>)


});

export default ModalTrash