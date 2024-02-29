import React, { useEffect } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useLocation from "../../../hooks/use-location/use-location";
import { IDetailView, newDetailView } from "../../../interfaces/IDetailView";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { newIFileIndexItem } from "../../../interfaces/IFileIndexItem";
import localization from "../../../localization/localization.json";
import FetchPost from "../../../shared/fetch/fetch-post";
import { FileExtensions } from "../../../shared/file-extensions";
import { FileListCache } from "../../../shared/filelist-cache";
import { Language } from "../../../shared/language";
import { UrlQuery } from "../../../shared/url/url-query";
import FormControl from "../../atoms/form-control/form-control";
import Modal from "../../atoms/modal/modal";

interface IModalRenameFileProps {
  isOpen: boolean;
  handleExit: Function;
  state: IDetailView;
}

const ModalDetailviewRenameFile: React.FunctionComponent<IModalRenameFileProps> = ({
  state,
  ...props
}) => {
  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageNonValidExtension = language.key(localization.MessageNonValidExtension);
  const MessageChangeToDifferentExtension = language.key(
    localization.MessageChangeToDifferentExtension
  );
  const MessageRenameServerError = language.key(localization.MessageRenameServerError);
  const MessageRenameFileName = language.key(localization.MessageRenameFileName);

  // Fallback for no context
  if (!state) {
    state = newDetailView();
  }
  if (!state.fileIndexItem) {
    state.fileIndexItem = newIFileIndexItem();
  }

  // to know where you are
  const history = useLocation();

  // For the display
  const [isFormEnabled, setIsFormEnabled] = React.useState(true);
  useEffect(() => {
    if (!state.fileIndexItem.status) return;
    switch (state.fileIndexItem.status) {
      case IExifStatus.Deleted:
      case IExifStatus.ReadOnly:
      case IExifStatus.ServerError:
      case IExifStatus.NotFoundSourceMissing:
        setIsFormEnabled(false);
        break;
      default:
        setIsFormEnabled(true);
        break;
    }
  }, [state.fileIndexItem.status]);

  // to show errors
  const useErrorHandler = (initialState: string | null) => {
    return initialState;
  };
  const [error, setError] = React.useState(useErrorHandler(null));

  // when you are waiting on the API
  const [isLoading, setIsLoading] = React.useState(false);

  // The Updated that is send to the api
  const [fileName, setFileName] = React.useState(state.fileIndexItem.fileName);

  // allow summit
  const [buttonState, setButtonState] = React.useState(false);

  function handleUpdateChange(
    event: React.ChangeEvent<HTMLDivElement> | React.KeyboardEvent<HTMLDivElement>
  ) {
    if (!isFormEnabled) return;
    if (!event.currentTarget.textContent) return null;
    const fieldValue = event.currentTarget.textContent.trim();

    setFileName(fieldValue);
    setButtonState(true);

    const extensionsState = new FileExtensions().MatchExtension(
      state.fileIndexItem.fileName,
      fieldValue
    );
    const isValidFileName = new FileExtensions().IsValidFileName(fieldValue);

    if (!isValidFileName) {
      setError(MessageNonValidExtension);
      setButtonState(false);
    } else if (!extensionsState) {
      setError(MessageChangeToDifferentExtension);
    } else {
      setError(null);
    }
  }

  async function pushRenameChange() {
    // Show icon with load ++ disable forms
    setIsFormEnabled(false);
    setIsLoading(true);

    const filePathAfterChange = state.fileIndexItem.filePath.replace(
      state.fileIndexItem.fileName,
      fileName
    );

    // API call
    const bodyParams = new URLSearchParams();
    bodyParams.append("f", state.fileIndexItem.filePath);
    bodyParams.append("to", filePathAfterChange);
    bodyParams.append(
      "collections",
      state.collections !== undefined ? state.collections.toString() : "true"
    );

    const result = await FetchPost(new UrlQuery().UrlDiskRename(), bodyParams.toString());

    if (result.statusCode !== 200) {
      setError(MessageRenameServerError);
      // and renewable
      setIsLoading(false);
      setIsFormEnabled(true);
      return;
    }

    new FileListCache().CacheCleanEverything();
    // redirect to new path (so if you press refresh the image is shown)
    const replacePath = new UrlQuery().updateFilePathHash(
      history.location.search,
      filePathAfterChange
    );

    history.navigate(replacePath, { replace: true });

    // Close window
    props.handleExit();
  }

  return (
    <Modal
      id="rename-file-modal"
      isOpen={props.isOpen}
      handleExit={() => {
        props.handleExit();
      }}
    >
      <div className="content">
        <div className="modal content--subheader">{MessageRenameFileName}</div>
        <div className="modal content--text">
          <FormControl onInput={handleUpdateChange} name="filename" contentEditable={isFormEnabled}>
            {state.fileIndexItem.fileName}
          </FormControl>

          {error && (
            <div
              data-test="modal-detailview-rename-file-warning-box"
              className="warning-box--under-form warning-box"
            >
              {error}
            </div>
          )}

          <button
            disabled={
              state.fileIndexItem.fileName === fileName ||
              !isFormEnabled ||
              isLoading ||
              !buttonState
            }
            data-test="modal-detailview-rename-file-btn-default"
            className="btn btn--default"
            onClick={pushRenameChange}
          >
            {isLoading ? "Loading..." : MessageRenameFileName}
          </button>
        </div>
      </div>
    </Modal>
  );
};

export default ModalDetailviewRenameFile;
