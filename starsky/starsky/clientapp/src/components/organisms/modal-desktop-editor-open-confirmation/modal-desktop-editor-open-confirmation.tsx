import { useState } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import localization from "../../../localization/localization.json";
import FetchPost from "../../../shared/fetch/fetch-post";
import { Language } from "../../../shared/language";
import { URLPath } from "../../../shared/url-path";
import { UrlQuery } from "../../../shared/url-query";
import Modal from "../../atoms/modal/modal";

interface IModalDesktopEditorOpenConfirmationProps {
  isOpen: boolean;
  select: Array<string> | undefined;
  handleExit(): void;
  state: IArchiveProps;
  setIsLoading: React.Dispatch<React.SetStateAction<boolean>>;
  isCollections: boolean;
}

async function OpenDesktop(
  select: string[],
  collections: boolean,
  state: IArchiveProps,
  setIsError: React.Dispatch<React.SetStateAction<string>>,
  messageDesktopEditorUnableToOpen: string
): Promise<boolean> {
  const toDesktopOpenList = new URLPath().MergeSelectFileIndexItem(select, state.fileIndexItems);
  if (!toDesktopOpenList) return false;
  const selectParams = new URLPath().ArrayToCommaSeparatedStringOneParent(toDesktopOpenList, "");
  const urlOpen = new UrlQuery().UrlApiDesktopEditorOpen();

  const bodyParams = new URLSearchParams();
  bodyParams.append("f", selectParams);
  bodyParams.append("collections", collections.toString());

  const openDesktopResult = await FetchPost(urlOpen, bodyParams.toString());
  if (openDesktopResult.statusCode >= 300) {
    setIsError(messageDesktopEditorUnableToOpen);
  }
  return true;
}

const ModalDesktopEditorOpenConfirmation: React.FunctionComponent<
  IModalDesktopEditorOpenConfirmationProps
> = ({ select, handleExit, isOpen, state, isCollections }) => {
  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageDesktopEditorConfirmationIntroText = language.key(
    localization.MessageDesktopEditorConfirmationIntroText
  );
  const MessageDesktopEditorConfirmationHeading = language.key(
    localization.MessageDesktopEditorConfirmationHeading
  );
  const MessageCancel = language.key(localization.MessageCancel);
  const MessageDesktopEditorOpenMultipleFiles = language.key(
    localization.MessageDesktopEditorOpenMultipleFiles
  );
  const MessageDesktopEditorUnableToOpen = language.key(
    localization.MessageDesktopEditorUnableToOpen
  );

  // for showing a notification
  const [isError, setIsError] = useState("");

  return (
    <Modal
      id="modal-desktop-editor-open-confirmation"
      isOpen={isOpen}
      handleExit={() => {
        handleExit();
      }}
    >
      <>
        <div className="modal content--subheader">{MessageDesktopEditorConfirmationHeading}</div>
        <div className="modal content--text">
          <p>{MessageDesktopEditorConfirmationIntroText}</p>
          {isError ? (
            <>
              <br />
              <div className="warning-box">{isError}</div>
            </>
          ) : null}

          <button
            data-test="confirmation-no"
            onClick={() => handleExit()}
            className="btn btn--info"
          >
            {MessageCancel}
          </button>
          <button
            onClick={() => {
              OpenDesktop(
                select ?? [],
                isCollections,
                state,
                setIsError,
                MessageDesktopEditorUnableToOpen
              ).then((result) => {
                if (result) {
                  handleExit();
                }
              });
            }}
            onKeyDown={(event) => {
              event.key === "Enter" &&
                OpenDesktop(
                  select ?? [],
                  isCollections,
                  state,
                  setIsError,
                  MessageDesktopEditorUnableToOpen
                ).then((result) => {
                  if (result) {
                    handleExit();
                  }
                });
            }}
            autoFocus={true}
            data-test="confirmation-yes"
            className="btn btn--default"
          >
            {MessageDesktopEditorOpenMultipleFiles}
          </button>
        </div>
      </>
    </Modal>
  );
};

export default ModalDesktopEditorOpenConfirmation;
