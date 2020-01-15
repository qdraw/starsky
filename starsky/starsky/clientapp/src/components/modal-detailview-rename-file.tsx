import React, { useEffect } from 'react';
import { DetailViewContext } from '../contexts/detailview-context';
import useGlobalSettings from '../hooks/use-globalSettings';
import useLocation from '../hooks/use-location';
import { newDetailView } from '../interfaces/IDetailView';
import { IExifStatus } from '../interfaces/IExifStatus';
import { newIFileIndexItem } from '../interfaces/IFileIndexItem';
import FetchPost from '../shared/fetch-post';
import { FileExtensions } from '../shared/file-extensions';
import { Language } from '../shared/language';
import { URLPath } from '../shared/url-path';
import { UrlQuery } from '../shared/url-query';
import Modal from './modal';

interface IModalRenameFileProps {
  isOpen: boolean;
  handleExit: Function;
}

const ModalDetailviewRenameFile: React.FunctionComponent<IModalRenameFileProps> = (props) => {

  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageNonValidExtension: string = "Dit bestand kan zo niet worden weggeschreven";
  const MessageChangeToDifferentExtension: string = "Let op! Je veranderd de extensie van het bestand, deze kan hierdoor onleesbaar worden";
  const MessageGeneralError: string = "Er is iets misgegaan met de aanvraag, probeer het later opnieuw";
  const MessageRenameFileName = language.text("Bestandsnaam wijzigen", "Rename file name");

  let { state, } = React.useContext(DetailViewContext);

  // Fallback for no context
  if (!state) {
    state = newDetailView();
  }
  if (!state.fileIndexItem) {
    state.fileIndexItem = newIFileIndexItem();
  }

  // to know where you are
  var history = useLocation();

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

  // to show errors
  const useErrorHandler = (initialState: string | null) => { return initialState };
  const [error, setError] = React.useState(useErrorHandler(null));

  // when you are waiting on the API
  const [loading, setIsLoading] = React.useState(false);

  // The Updated that is send to the api
  const [fileName, setFileName] = React.useState(state.fileIndexItem.fileName);

  // allow summit
  const [buttonState, setButtonState] = React.useState(false);

  function handleUpdateChange(event: React.ChangeEvent<HTMLDivElement> | React.KeyboardEvent<HTMLDivElement>) {
    if (!isFormEnabled) return;
    if (!event.currentTarget.textContent) return null;
    let fieldValue = event.currentTarget.textContent.trim();

    setFileName(fieldValue);
    setButtonState(true)

    var extensionsState = new FileExtensions().MatchExtension(state.fileIndexItem.fileName, fieldValue);
    var isValidFileName = new FileExtensions().IsValidFileName(fieldValue);

    if (!isValidFileName) {
      setError(MessageNonValidExtension);
      setButtonState(false);
    }
    else if (!extensionsState) {
      setError(MessageChangeToDifferentExtension);
    }
    else {
      setError(null);
    }
  }

  async function pushRenameChange(event: React.MouseEvent<HTMLButtonElement>) {
    // Show icon with load ++ disable forms
    setFormEnabled(false);
    setIsLoading(true);

    var filePathAfterChange = state.fileIndexItem.filePath.replace(state.fileIndexItem.fileName, fileName);

    // API call
    var bodyParams = new URLSearchParams();
    bodyParams.append("f", state.fileIndexItem.filePath);
    bodyParams.append("to", filePathAfterChange);

    var result = await FetchPost(new UrlQuery().UrlSyncRename(), bodyParams.toString())

    if (result.statusCode !== 200) {
      setError(MessageGeneralError);
      // and renable
      setIsLoading(false);
      setFormEnabled(true);
      return;
    };

    // redirect to new path (so if you press refresh the image is shown)
    var replacePath = new URLPath().updateFilePath(history.location.search, filePathAfterChange);
    history.navigate(replacePath, { replace: true });

    // Close window
    props.handleExit();
  }

  return <Modal
    id="rename-file-modal"
    isOpen={props.isOpen}
    handleExit={() => {
      props.handleExit()
    }}>
    <div className="content">
      <div className="modal content--subheader">{MessageRenameFileName}</div>
      <div className="modal content--text">

        <div data-name="filename"
          onInput={handleUpdateChange}
          suppressContentEditableWarning={true}
          contentEditable={isFormEnabled}
          className={isFormEnabled ? "form-control" : "form-control disabled"}>
          {state.fileIndexItem.fileName}
        </div>

        {error && <div className="warning-box--under-form warning-box">{error}</div>}

        <button disabled={state.fileIndexItem.fileName === fileName || !isFormEnabled ||
          loading || !buttonState}
          className="btn btn--default" onClick={pushRenameChange}>
          {loading ? 'Loading...' : MessageRenameFileName}
        </button>
      </div>
    </div>
  </Modal>
};

export default ModalDetailviewRenameFile