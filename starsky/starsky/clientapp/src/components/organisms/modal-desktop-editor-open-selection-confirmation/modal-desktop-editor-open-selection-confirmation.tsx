import { useState } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import localization from "../../../localization/localization.json";
import FetchPost from "../../../shared/fetch/fetch-post";
import { Language } from "../../../shared/language";
import { URLPath } from "../../../shared/url/url-path";
import { UrlQuery } from "../../../shared/url/url-query";
import Modal from "../../atoms/modal/modal";

interface IModalDesktopEditorOpenConfirmationProps {
  isOpen: boolean;
  select: Array<string> | undefined;
  handleExit(): void;
  state: IArchiveProps;
  setIsLoading: React.Dispatch<React.SetStateAction<boolean>>;
  isCollections: boolean;
}

export async function OpenDesktop(
  select: string[],
  collections: boolean,
  state: IArchiveProps,
  setIsError: React.Dispatch<React.SetStateAction<string>>,
  messageDesktopEditorUnableToOpen: string
): Promise<boolean> {
  const toDesktopOpenList = new URLPath().MergeSelectFileIndexItem(select, state.fileIndexItems);
  if (!toDesktopOpenList || toDesktopOpenList.length == 0) return false;
  const selectParams = new URLPath().ArrayToCommaSeparatedStringOneParent(toDesktopOpenList, "");
  const urlOpen = new UrlQuery().UrlApiDesktopEditorOpen();

  const bodyParams = new URLSearchParams();
  bodyParams.append("f", selectParams);
  bodyParams.append("collections", collections.toString());

  const openDesktopResult = await FetchPost(urlOpen, bodyParams.toString());
  if (openDesktopResult.statusCode >= 300) {
    setIsError(messageDesktopEditorUnableToOpen);
    return false;
  }
  return true;
}

const ModalDesktopEditorOpenSelectionConfirmation: React.FunctionComponent<
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
        <div data-test="editor-open-heading" className="modal content--subheader">
          {MessageDesktopEditorConfirmationHeading}
        </div>
        <div data-test="editor-open-text" className="modal content--text">
          <p>{MessageDesktopEditorConfirmationIntroText}</p>
          {isError ? (
            <>
              <br />
              <div data-test="editor-open-error" className="warning-box">
                {isError}
              </div>
            </>
          ) : null}

          <button
            data-test="editor-open-confirmation-no"
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
              if (event.key === "Enter") {
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
              }
            }}
            autoFocus={true}
            data-test="editor-open-confirmation-yes"
            className="btn btn--default"
          >
            {MessageDesktopEditorOpenMultipleFiles}
          </button>
        </div>
      </>
    </Modal>
  );
};

export default ModalDesktopEditorOpenSelectionConfirmation;
