import { useSocketsEventName } from "../../../hooks/realtime/use-sockets.const";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useLocation from "../../../hooks/use-location/use-location";
import { IApiNotificationResponseModel } from "../../../interfaces/IApiNotificationResponseModel";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import localization from "../../../localization/localization.json";
import FetchPost from "../../../shared/fetch/fetch-post";
import { FileExtensions } from "../../../shared/file-extensions";
import { Language } from "../../../shared/language";
import { ClearSearchCache } from "../../../shared/search/clear-search-cache";
import { UrlQuery } from "../../../shared/url-query";
import Modal from "../../atoms/modal/modal";

interface IModalMoveFolderToTrashProps {
  isOpen: boolean;
  subPath: string;
  handleExit: Function;
  setIsLoading: React.Dispatch<React.SetStateAction<boolean>>;
}

const ModalMoveFolderToTrash: React.FunctionComponent<IModalMoveFolderToTrashProps> = ({
  subPath,
  handleExit,
  isOpen,
  setIsLoading
}) => {
  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageMoveFolderIntoTrashIntroText = language.key(
    localization.MessageMoveFolderIntoTrashIntroText
  );
  const MessageCancel = language.key(localization.MessageCancel);
  const MessageMoveToTrash = language.key(localization.MessageMoveToTrash);
  const history = useLocation();

  function moveFolderIntoTrash() {
    if (!subPath) return;
    setIsLoading(true);

    const bodyParams = new URLSearchParams();
    bodyParams.append("f", subPath);

    FetchPost(new UrlQuery().UrlMoveToTrashApi(), bodyParams.toString(), "post").then((result) => {
      if (result.statusCode === 200 || result.statusCode === 404) {
        document.body.dispatchEvent(
          new CustomEvent<IApiNotificationResponseModel<IFileIndexItem[]>>(useSocketsEventName, {
            bubbles: false,
            detail: {
              data: result.data,
              type: "move-folder-to-trash-internal"
            }
          })
        );

        history.navigate(
          new UrlQuery().updateFilePathHash(
            history.location.search,
            new FileExtensions().GetParentPath(subPath),
            true
          )
        );
        // nothing happens after navigating away
        return;
      }

      ClearSearchCache(history.location.search);
      setIsLoading(false);
    });
  }

  return (
    <Modal
      id="delete-modal"
      isOpen={isOpen}
      handleExit={() => {
        handleExit();
      }}
    >
      <>
        <div className="modal content--subheader">{MessageMoveToTrash}</div>
        <div className="modal content--text">
          {MessageMoveFolderIntoTrashIntroText}
          <br />
          <button data-test="force-cancel" onClick={() => handleExit()} className="btn btn--info">
            {MessageCancel}
          </button>
          <button
            onClick={() => {
              handleExit();
              moveFolderIntoTrash();
            }}
            data-test="move-folder-to-trash"
            className="btn btn--default"
          >
            {MessageMoveToTrash}
          </button>
        </div>
      </>
    </Modal>
  );
};

export default ModalMoveFolderToTrash;
