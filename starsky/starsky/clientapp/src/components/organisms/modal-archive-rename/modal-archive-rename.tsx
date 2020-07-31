import React from 'react';
import useGlobalSettings from '../../../hooks/use-global-settings';
import { Language } from '../../../shared/language';
import FormControl from '../../atoms/form-control/form-control';
import Modal from '../../atoms/modal/modal';

interface IModalRenameFolderProps {
  isOpen: boolean;
  handleExit: Function;
  subPath: string;
}

const ModalArchiveRename: React.FunctionComponent<IModalRenameFolderProps> = (props) => {

  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageRenameFolder = language.text("Huidige mapnaam wijzigen", "Rename current folder");

  // to show errors
  const useErrorHandler = (initialState: string | null) => { return initialState };
  const [error, setError] = React.useState(useErrorHandler(null));

  // when you are waiting on the API
  const [loading, setIsLoading] = React.useState(false);

  // The Updated that is send to the api
  const [folderName, setFolderName] = React.useState(props.subPath);

  // allow summit
  const [buttonState, setButtonState] = React.useState(false);

  // !!!!!!!!!! todo: FIX!!!!!
  const [isFormEnabled, setFormEnabled] = React.useState(true);


  function handleUpdateChange(event: React.ChangeEvent<HTMLDivElement> | React.KeyboardEvent<HTMLDivElement>) {
    if (!isFormEnabled) return;
    if (!event.currentTarget.textContent) return null;
    let fieldValue = event.currentTarget.textContent.trim();

    setFolderName(fieldValue);
    setButtonState(true)

    // var extensionsState = new FileExtensions().MatchExtension(state.fileIndexItem.fileName, fieldValue);
    // var isValidFileName = new FileExtensions().IsValidFileName(fieldValue);

    // if (!isValidFileName) {
    //   setError(MessageNonValidExtension);
    //   setButtonState(false);
    // }
    // else if (!extensionsState) {
    //   setError(MessageChangeToDifferentExtension);
    // }
    // else {
    //   setError(null);
    // }
  }

  function pushRenameChange() {

  }

  return <Modal
    id="rename-file-modal"
    isOpen={props.isOpen}
    handleExit={() => {
      props.handleExit()
    }}>
    <div className="content">
      <div className="modal content--subheader">{MessageRenameFolder}</div>
      <div className="modal content--text">

        <FormControl onInput={handleUpdateChange} name="filename" contentEditable={isFormEnabled}>
          {props.subPath}
        </FormControl>

        {error && <div className="warning-box--under-form warning-box">{error}</div>}

        <button disabled={props.subPath === folderName || !isFormEnabled ||
          loading || !buttonState}
          className="btn btn--default" onClick={pushRenameChange}>
          {loading ? 'Loading...' : MessageRenameFolder}
        </button>
      </div>
    </div>
  </Modal>
};

export default ModalArchiveRename
