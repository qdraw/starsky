import { useState } from "react";
import { ArchiveAction } from "../../../contexts/archive-context";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useLocation from "../../../hooks/use-location/use-location";
import FetchPost from "../../../shared/fetch-post";
import { FileExtensions } from "../../../shared/file-extensions";
import { FileListCache } from "../../../shared/filelist-cache";
import { Language } from "../../../shared/language";
import { UrlQuery } from "../../../shared/url-query";
import FormControl from "../../atoms/form-control/form-control";
import Modal from "../../atoms/modal/modal";

interface IModalRenameFolderProps {
  isOpen: boolean;
  handleExit: (state?: string) => void;
  subPath: string;
  dispatch?: React.Dispatch<ArchiveAction>;
}

const ModalArchiveRename: React.FunctionComponent<IModalRenameFolderProps> = (
  props
) => {
  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageRenameFolder = language.text(
    "Huidige mapnaam wijzigen",
    "Rename current folder"
  );
  const MessageNonValidDirectoryName: string = language.text(
    "Deze mapnaam is niet valide",
    "Directory name is not valid"
  );
  const MessageGeneralError: string = language.text(
    "Er is iets misgegaan met de aanvraag, probeer het later opnieuw",
    "Something went wrong with the request, please try again later"
  );

  // to show errors
  const useErrorHandler = (initialState: string | null) => {
    return initialState;
  };
  const [error, setError] = useState(useErrorHandler(null));

  // when you are waiting on the API
  const [loading, setIsLoading] = useState(false);

  // The Updated that is send to the api
  const [folderName, setFolderName] = useState(
    new FileExtensions().GetFileName(props.subPath)
  );

  const [isFormEnabled, setFormEnabled] = useState(true);

  // to know where you are
  const history = useLocation();

  /**
   * Update status and check if input is valid
   * @param event [Change Event]
   */
  function handleUpdateChange(
    event:
      | React.ChangeEvent<HTMLDivElement>
      | React.KeyboardEvent<HTMLDivElement>
  ) {
    if (!isFormEnabled) return;
    if (!event.currentTarget.textContent) return null;
    const fieldValue = event.currentTarget.textContent.trim();

    setFolderName(fieldValue);

    const isValidDirectoryName = new FileExtensions().IsValidDirectoryName(
      fieldValue
    );
    if (!isValidDirectoryName) {
      setError(MessageNonValidDirectoryName);
    } else {
      setError(null);
    }
  }

  function dispatchRename(path: string) {
    if (!props.dispatch) return;
    props.dispatch({
      type: "rename-folder",
      path: path
    });
  }

  /**
   * Send Change request to back-end services
   * @param event : change vent
   */
  async function pushRenameChange() {
    // Show icon with load ++ disable forms
    setFormEnabled(false);
    setIsLoading(true);

    // subPath style including parent folder
    const filePathAfterChange = props.subPath.replace(
      new FileExtensions().GetFileName(props.subPath),
      folderName
    );

    // do a rename in the current context
    // before due trigger of useDiskWatcher
    dispatchRename(filePathAfterChange);

    // API call
    const bodyParams = new URLSearchParams();
    bodyParams.append("f", props.subPath);
    bodyParams.append("to", filePathAfterChange);

    const result = await FetchPost(
      new UrlQuery().UrlDiskRename(),
      bodyParams.toString()
    );

    if (result.statusCode !== 200) {
      // undo dispatch
      dispatchRename(props.subPath);

      setError(MessageGeneralError);
      // and renewable
      setIsLoading(false);
      setFormEnabled(true);
      return;
    }

    // clean user cache
    new FileListCache().CacheCleanEverything();

    // redirect to new path (so if you press refresh the image is shown)
    const replacePath = new UrlQuery().updateFilePathHash(
      history.location.search,
      filePathAfterChange
    );

    history.navigate(replacePath, { replace: true });

    // Close window
    props.handleExit(filePathAfterChange);
  }

  return (
    <Modal
      id="rename-file-modal"
      isOpen={props.isOpen}
      handleExit={() => {
        props.handleExit();
      }}
    >
      <div className="content" data-test="modal-archive-rename">
        <div className="modal content--subheader">{MessageRenameFolder}</div>
        <div className="modal content--text">
          <FormControl
            onInput={handleUpdateChange}
            name="foldername"
            contentEditable={isFormEnabled}
          >
            {new FileExtensions().GetFileName(props.subPath)}
          </FormControl>

          {error && (
            <div
              data-test="modal-archive-rename-warning-box"
              className="warning-box--under-form warning-box"
            >
              {error}
            </div>
          )}

          <button
            disabled={
              new FileExtensions().GetFileName(props.subPath) === folderName ||
              !!error ||
              loading
            }
            data-test="modal-archive-rename-btn-default"
            className="btn btn--default"
            onClick={pushRenameChange}
          >
            {loading ? "Loading..." : MessageRenameFolder}
          </button>
        </div>
      </div>
    </Modal>
  );
};

export default ModalArchiveRename;
