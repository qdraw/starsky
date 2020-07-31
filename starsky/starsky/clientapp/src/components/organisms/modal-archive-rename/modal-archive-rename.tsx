import React from 'react';
import useGlobalSettings from '../../../hooks/use-global-settings';
import useLocation from '../../../hooks/use-location';
import FetchPost from '../../../shared/fetch-post';
import { FileExtensions } from '../../../shared/file-extensions';
import { Language } from '../../../shared/language';
import { UrlQuery } from '../../../shared/url-query';
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
  const MessageNonValidDirectoryName: string = language.text("Deze mapnaam is niet valide", "Directory name is not valid");;
  const MessageGeneralError: string = language.text("Er is iets misgegaan met de aanvraag, probeer het later opnieuw",
    "Something went wrong with the request, please try again later");

  // to show errors
  const useErrorHandler = (initialState: string | null) => { return initialState };
  const [error, setError] = React.useState(useErrorHandler(null));

  // when you are waiting on the API
  const [loading, setIsLoading] = React.useState(false);

  // The Updated that is send to the api
  const [folderName, setFolderName] = React.useState(new FileExtensions().GetFileName(props.subPath));

  const [isFormEnabled, setFormEnabled] = React.useState(true);

  // to know where you are
  var history = useLocation();

  /**
   * Update status and check if input is valid
   * @param event [Change Event]
   */
  function handleUpdateChange(event: React.ChangeEvent<HTMLDivElement> | React.KeyboardEvent<HTMLDivElement>) {
    if (!isFormEnabled) return;
    if (!event.currentTarget.textContent) return null;
    let fieldValue = event.currentTarget.textContent.trim();

    setFolderName(fieldValue);

    var isValidDirectoryName = new FileExtensions().IsValidDirectoryName(fieldValue);
    if (!isValidDirectoryName) {
      setError(MessageNonValidDirectoryName);
    }
    else {
      setError(null);
    }
  }

  /**
   * Send Change request to back-end services
   * @param event : change vent
   */
  async function pushRenameChange(event: React.MouseEvent<HTMLButtonElement>) {
    // Show icon with load ++ disable forms
    setFormEnabled(false);
    setIsLoading(true);

    // subPath style including parent folder
    const filePathAfterChange = props.subPath.replace(new FileExtensions().GetFileName(props.subPath), folderName);

    // API call
    const bodyParams = new URLSearchParams();
    bodyParams.append("f", props.subPath);
    bodyParams.append("to", filePathAfterChange);

    const result = await FetchPost(new UrlQuery().UrlSyncRename(), bodyParams.toString());

    if (result.statusCode !== 200) {
      setError(MessageGeneralError);
      // and renable
      setIsLoading(false);
      setFormEnabled(true);
      return
    }

    // redirect to new path (so if you press refresh the image is shown)
    const replacePath = new UrlQuery().updateFilePathHash(history.location.search, filePathAfterChange);
    await history.navigate(replacePath, { replace: true });

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
      <div className="modal content--subheader">{MessageRenameFolder}</div>
      <div className="modal content--text">

        <FormControl onInput={handleUpdateChange} name="foldername" contentEditable={isFormEnabled}>
          {new FileExtensions().GetFileName(props.subPath)}
        </FormControl>

        {error && <div className="warning-box--under-form warning-box">{error}</div>}

        <button disabled={new FileExtensions().GetFileName(props.subPath) === folderName || !!error || loading}
          className="btn btn--default" onClick={pushRenameChange}>
          {loading ? 'Loading...' : MessageRenameFolder}
        </button>
      </div>
    </div>
  </Modal>
};

export default ModalArchiveRename
