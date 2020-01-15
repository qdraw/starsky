import React from 'react';
import { ArchiveContext } from '../contexts/archive-context';
import useGlobalSettings from '../hooks/use-globalSettings';
import { IArchiveProps } from '../interfaces/IArchiveProps';
import { CastToInterface } from '../shared/cast-to-interface';
import FetchGet from '../shared/fetch-get';
import FetchPost from '../shared/fetch-post';
import { FileExtensions } from '../shared/file-extensions';
import { Language } from '../shared/language';
import { UrlQuery } from '../shared/url-query';
import Modal from './modal';

interface IModalRenameFileProps {
  isOpen: boolean;
  handleExit: Function;
}

const ModalArchiveMkdir: React.FunctionComponent<IModalRenameFileProps> = (props) => {

  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageFeatureName = language.text("Nieuwe map aanmaken", "Create new folder");
  const MessageNonValidDirectoryName = language.text("Controleer de naam, deze map kan niet zo worden aangemaakt",
    "Check the name, this folder cannot be created in this way");
  const MessageGeneralMkdirCreateError = language.text("Er is misgegaan met het aanmaken van deze map",
    "An error occurred while creating this folder");
  const MessageDirectoryExistError = language.text("De map bestaat al, probeer een andere naam",
    "The folder already exists, try a different name");

  // Context of Archive
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

    if (!isValidFileName) {
      setError(MessageNonValidDirectoryName);
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
      setError(result.statusCode !== 409 ? MessageGeneralMkdirCreateError : MessageDirectoryExistError);
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
      <div className="modal content--subheader">{MessageFeatureName}</div>
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
          {loading ? 'Loading...' : MessageFeatureName}
        </button>
      </div>
    </div>
  </Modal>
};

export default ModalArchiveMkdir