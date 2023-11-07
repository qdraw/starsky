import { useState } from "react";
import { ArchiveAction } from "../../../contexts/archive-context";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { CastToInterface } from "../../../shared/cast-to-interface";
import FetchGet from "../../../shared/fetch-get";
import FetchPost from "../../../shared/fetch-post";
import { FileExtensions } from "../../../shared/file-extensions";
import { FileListCache } from "../../../shared/filelist-cache";
import { Language } from "../../../shared/language";
import { UrlQuery } from "../../../shared/url-query";
import FormControl from "../../atoms/form-control/form-control";
import Modal from "../../atoms/modal/modal";

interface IModalRenameFileProps {
  isOpen: boolean;
  handleExit: Function;
  state: IArchiveProps;
  dispatch: React.Dispatch<ArchiveAction>;
}

const ModalArchiveMkdir: React.FunctionComponent<IModalRenameFileProps> = ({
  state,
  dispatch,
  ...props
}) => {
  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageFeatureName = language.text(
    "Nieuwe map aanmaken",
    "Create new folder"
  );
  const MessageNonValidDirectoryName = language.text(
    "Controleer de naam, deze map kan niet zo worden aangemaakt",
    "Check the name, this folder cannot be created in this way"
  );
  const MessageGeneralMkdirCreateError = language.text(
    "Er is misgegaan met het aanmaken van deze map",
    "An error occurred while creating this folder"
  );
  const MessageDirectoryExistError = language.text(
    "De map bestaat al, probeer een andere naam",
    "The folder already exists, try a different name"
  );

  // to show errors
  const useErrorHandler = (initialState: string | null) => {
    return initialState;
  };
  const [error, setError] = useState(useErrorHandler(null));

  // when you are waiting on the API
  const [isLoading, setIsLoading] = useState(false);

  // The directory name to submit
  const [directoryName, setDirectoryName] = useState("");

  // allow summit
  const [buttonState, setButtonState] = useState(false);

  const [isFormEnabled, setIsFormEnabled] = useState(true);

  function handleUpdateChange(
    event:
      | React.ChangeEvent<HTMLDivElement>
      | React.KeyboardEvent<HTMLDivElement>
  ) {
    let fieldValue = "";
    if (event.currentTarget.textContent) {
      fieldValue = event.currentTarget.textContent.trim();
    }

    setDirectoryName(fieldValue);
    setButtonState(true);

    const isValidFileName = new FileExtensions().IsValidDirectoryName(
      fieldValue
    );

    if (!isValidFileName) {
      setError(MessageNonValidDirectoryName);
      setButtonState(false);
    } else {
      setError(null);
    }
  }

  async function pushRenameChange() {
    // Show icon with load ++ disable forms
    setIsFormEnabled(false);
    setIsLoading(true);

    const newDirectorySubPath = `${state.subPath}/${directoryName}`;

    // API call
    const bodyParams = new URLSearchParams();
    bodyParams.append("f", newDirectorySubPath);

    const result = await FetchPost(
      new UrlQuery().UrlDiskMkdir(),
      bodyParams.toString()
    );

    if (result.statusCode !== 200) {
      setError(
        result.statusCode !== 409
          ? MessageGeneralMkdirCreateError
          : MessageDirectoryExistError
      );
      // and renewable
      setIsLoading(false);
      setIsFormEnabled(true);
      return;
    }

    // Force update
    const connectionResult = await FetchGet(
      new UrlQuery().UrlIndexServerApi({ f: state.subPath })
    );
    const forceSyncResult = new CastToInterface().MediaArchive(
      connectionResult.data
    );
    const payload = forceSyncResult.data as IArchiveProps;
    if (payload.fileIndexItems) {
      dispatch({ type: "force-reset", payload });
    }

    new FileListCache().CacheCleanEverything();
    // Close window
    props.handleExit();
  }

  return (
    <Modal
      id="modal-archive-mkdir"
      isOpen={props.isOpen}
      handleExit={() => {
        props.handleExit();
      }}
    >
      <div className="content">
        <div className="modal content--subheader">{MessageFeatureName}</div>
        <div className="modal content--text">
          <FormControl
            name="directoryname"
            onInput={handleUpdateChange}
            contentEditable={isFormEnabled}
          >
            &nbsp;
          </FormControl>

          {error && (
            <div
              data-test="modal-archive-mkdir-warning-box"
              className="warning-box--under-form warning-box"
            >
              {error}
            </div>
          )}

          <button
            disabled={!isFormEnabled || isLoading || !buttonState}
            className="btn btn--default"
            data-test="modal-archive-mkdir-btn-default"
            onClick={pushRenameChange}
          >
            {isLoading ? "Loading..." : MessageFeatureName}
          </button>
        </div>
      </div>
    </Modal>
  );
};

export default ModalArchiveMkdir;
