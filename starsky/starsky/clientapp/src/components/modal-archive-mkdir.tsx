import React from 'react';
import { ArchiveContext } from '../contexts/archive-context';
import FetchPost from '../shared/fetch-post';
import { FileExtensions } from '../shared/file-extensions';
import { UrlQuery } from '../shared/url-query';
import Modal from './modal';

interface IModalRenameFileProps {
  isOpen: boolean;
  handleExit: Function;
}

const ModalArchiveMkdir: React.FunctionComponent<IModalRenameFileProps> = (props) => {

  const FeatureName: string = "Nieuwe Map aanmaken ~ Feature incompleet"
  const NonValidDirectoryName: string = "Controlleer de naam, deze map kan niet zo worden aangemaakt";
  const GeneralError: string = "Er is iets misgegaan met de aanvraag, probeer het later opnieuw";

  let { state, } = React.useContext(ArchiveContext);

  // to show errors
  const useErrorHandler = (initialState: string | null) => { return initialState };
  const [error, setError] = React.useState(useErrorHandler(null));

  // when you are waiting on the API
  const [loading, setIsLoading] = React.useState(false);

  // The directory name to submit
  const [directoryName, setDirectoryName] = React.useState('');

  // allow summit
  const [buttonState, setButtonState] = React.useState(false);

  const [isFormEnabled, setFormEnabled] = React.useState(true);

  function handleUpdateChange(event: React.ChangeEvent<HTMLDivElement> | React.KeyboardEvent<HTMLDivElement>) {
    if (!event.currentTarget.textContent) return null;
    let fieldValue = event.currentTarget.textContent.trim();
    setDirectoryName(fieldValue);

    setDirectoryName(fieldValue);
    setButtonState(true)

    var isValidFileName = new FileExtensions().IsValidDirectoryName(fieldValue);

    if (!isValidFileName) {
      setError(NonValidDirectoryName);
      setButtonState(false);
    }
    else {
      setError(null);
    }
  }

  async function pushRenameChange(event: React.MouseEvent<HTMLButtonElement>) {
    // Show icon with load ++ disable forms
    setFormEnabled(false);
    setIsLoading(true);

    var newDirectorySubPath = `${state.subPath}/${directoryName}`

    // API call
    var bodyParams = new URLSearchParams();
    bodyParams.append("f", newDirectorySubPath);

    var result = await FetchPost(new UrlQuery().UrlSyncMkdir(), bodyParams.toString())

    if (result.statusCode !== 200) {
      setError(GeneralError);
      // and renable
      setIsLoading(false);
      setFormEnabled(true);
      return;
    };

    // Close window
    props.handleExit();
  }

  return <Modal
    id="modal-archive-mkdir"
    isOpen={props.isOpen}
    handleExit={() => {
      props.handleExit()
    }}>
    <div className="content">
      <div className="modal content--subheader">{FeatureName}</div>
      <div className="modal content--text">

        <div data-name="directoryname"
          onInput={handleUpdateChange}
          suppressContentEditableWarning={true}
          contentEditable={isFormEnabled}
          className={isFormEnabled ? "form-control" : "form-control disabled"}>
        </div>

        {error && <div className="warning-box--under-form warning-box">{error}</div>}

        <button disabled={!isFormEnabled || loading || !buttonState}
          className="btn btn--default" onClick={pushRenameChange}>
          {loading ? 'Loading...' : 'Opslaan'}
        </button>
      </div>
    </div>
  </Modal>
};

export default ModalArchiveMkdir