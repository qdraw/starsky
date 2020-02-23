import React from 'react';
import useGlobalSettings from '../hooks/use-global-settings';
import { Language } from '../shared/language';
import FormControl from './form-control';
import Modal from './modal';

interface IModalRenameFileProps {
  isOpen: boolean;
  handleExit: Function;
}

const ModalDatetime: React.FunctionComponent<IModalRenameFileProps> = (props) => {

  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageModalDatetime = language.text("Datum en tijd aanpassen", "Update Datetime");

  const [isFormEnabled, setFormEnabled] = React.useState(true);

  function handleUpdateChange(event: React.ChangeEvent<HTMLDivElement> | React.KeyboardEvent<HTMLDivElement>) {
    setFormEnabled(true);
  }

  return <Modal
    id="modal-archive-mkdir"
    isOpen={props.isOpen}
    handleExit={() => {
      props.handleExit()
    }}>
    <div className="content">
      <div className="modal content--subheader">{MessageModalDatetime}</div>
      <div className="modal content--text">

        <FormControl name="directoryname"
          onInput={handleUpdateChange}
          contentEditable={isFormEnabled}>
          &nbsp;
        </FormControl>

      </div>
    </div>
  </Modal>
};

export default ModalDatetime