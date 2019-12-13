import React, { memo, useEffect } from 'react';
import { DetailViewContext } from '../contexts/detailview-context';
import useLocation from '../hooks/use-location';
import { IExifStatus } from '../interfaces/IExifStatus';
import { newIFileIndexItem } from '../interfaces/IFileIndexItem';
import FetchPost from '../shared/fetch-post';
import { FileExtensions } from '../shared/file-extensions';
import { URLPath } from '../shared/url-path';
import { UrlQuery } from '../shared/url-query';
import Modal from './modal';

interface IModalRenameFileProps {
  isOpen: boolean;
  handleExit: Function;
}

const ModalDetailviewRenameFile: React.FunctionComponent<IModalRenameFileProps> = memo((props) => {

  const ChangeToDifferentExtension: string = "Let op! Je veranderd de extensie van het bestand, deze kan hierdoor onleesbaar worden";
  const GeneralError: string = "Er is iets misgegaan met de aanvraag, probeer het later opnieuw";

  let { state, } = React.useContext(DetailViewContext);

  var fileIndexItem = newIFileIndexItem();
  if (state) {
    fileIndexItem = state.fileIndexItem;
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
  }, [fileIndexItem.status]);

  const useErrorHandler = (initialState: string | null) => { return initialState };
  const [error, setError] = React.useState(useErrorHandler(null));

  // The Updated that is send to the api
  const [fileName, setFileName] = React.useState(fileIndexItem.fileName);

  function handleUpdateChange(event: React.ChangeEvent<HTMLDivElement> | React.KeyboardEvent<HTMLDivElement>) {
    if (!isFormEnabled) return;
    if (!event.currentTarget.textContent) return null;
    let fieldValue = event.currentTarget.textContent.trim();

    setFileName(fieldValue);
    var extensionsState = new FileExtensions().MatchExtension(state.fileIndexItem.fileName, fieldValue);
    if (!extensionsState) {
      setError(ChangeToDifferentExtension)
    }
    else {
      setError(null)
    }
  }


  async function pushRenameChange(event: React.MouseEvent<HTMLButtonElement>) {
    var filePathAfterChange = state.fileIndexItem.filePath.replace(state.fileIndexItem.fileName, fileName);

    var bodyParams = new URLSearchParams();
    bodyParams.append("f", state.fileIndexItem.filePath);
    bodyParams.append("to", filePathAfterChange);

    var result = await FetchPost(new UrlQuery().UrlSyncRename(), bodyParams.toString())

    if (result.statusCode !== 200) {
      setError(GeneralError);
      return;
    };

    // redirect to new path (so if you press refresh the image is shown)
    var replacePath = new URLPath().updateFilePath(history.location.search, filePathAfterChange);
    history.navigate(replacePath, { replace: true });

    // Close window
    props.handleExit();
  }

  return (<>
    return (<Modal
      id="rename-file-modal"
      isOpen={props.isOpen}
      handleExit={() => {
        props.handleExit()
      }}>
      <div className="content">
        <div className="modal content--subheader">Naam wijzigen</div>
        <div className="modal content--text">

          <div data-name="filename"
            onInput={handleUpdateChange}
            suppressContentEditableWarning={true}
            contentEditable={isFormEnabled}
            className={isFormEnabled ? "form-control" : "form-control disabled"}>
            {fileIndexItem.fileName}
          </div>

          {error && <div className="warning-box--under-form warning-box">{error}</div>}

          <button disabled={fileIndexItem.fileName === fileName || !isFormEnabled}
            className="btn btn--default" onClick={pushRenameChange}>
            Opslaan
          </button>
        </div>
      </div>
    </Modal>
  </>)
});

export default ModalDetailviewRenameFile