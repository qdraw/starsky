import React from 'react';
import { ArchiveContext } from '../contexts/archive-context';
import { IArchiveProps } from '../interfaces/IArchiveProps';
import { CastToInterface } from '../shared/cast-to-interface';
import FetchGet from '../shared/fetch-get';
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
  const GeneralError: string = "Er is misgegaan met het aanmaken van deze map";
  const DirectoryExistError: string = "De map bestaat al, probeer een andere naam";

  let { state, dispatch } = React.useContext(ArchiveContext);

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

    let fieldValue = "";
    if (event.currentTarget.textContent) {
      fieldValue = event.currentTarget.textContent.trim();
    }

    setDirectoryName(fieldValue);
    setButtonState(true)

    var isValidFileName = new FileExtensions().IsValidDirectoryName(fieldValue);
    console.log(fieldValue);

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

      setError(result.statusCode !== 409 ? GeneralError : DirectoryExistError);
      // and renable
      setIsLoading(false);
      setFormEnabled(true);
      return;
    };

    // Force update 
    var connectionResult = await FetchGet(new UrlQuery().UrlIndexServerApi({ f: state.subPath }))
    var forceSyncResult = new CastToInterface().MediaArchive(connectionResult.data);
    var payload = forceSyncResult.data as IArchiveProps;
    if (payload.fileIndexItems) {
      dispatch({ type: 'force-reset', payload });
    }

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
          &nbsp;
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