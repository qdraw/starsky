import { ArchiveAction } from "../../../contexts/archive-context";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useLocation from "../../../hooks/use-location/use-location";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import localization from "../../../localization/localization.json";
import FetchPost from "../../../shared/fetch/fetch-post";
import { Language } from "../../../shared/language";
import { ClearSearchCache } from "../../../shared/search/clear-search-cache";
import { Select } from "../../../shared/select";
import { URLPath } from "../../../shared/url/url-path";
import { UrlQuery } from "../../../shared/url/url-query";
import Modal from "../../atoms/modal/modal";

interface IModalForceDeleteProps {
  isOpen: boolean;
  select: Array<string> | undefined;
  setSelect: React.Dispatch<React.SetStateAction<string[] | undefined>>;
  handleExit: () => void;
  state: IArchiveProps;
  setIsLoading: React.Dispatch<React.SetStateAction<boolean>>;
  dispatch: React.Dispatch<ArchiveAction>;
}

const ModalForceDelete: React.FunctionComponent<IModalForceDeleteProps> = ({
  select,
  handleExit,
  isOpen,
  setSelect,
  state,
  setIsLoading,
  dispatch
}) => {
  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageDeleteIntroText = language.key(localization.MessageDeleteIntroText);
  const MessageCancel = language.key(localization.MessageCancel);
  const MessageDeleteImmediately = language.key(localization.MessageDeleteImmediately);

  const history = useLocation();

  const undoSelection = () => new Select(select, setSelect, state, history).undoSelection();

  function forceDelete() {
    if (!select) return;
    setIsLoading(true);

    const toUndoTrashList = new URLPath().MergeSelectFileIndexItem(select, state.fileIndexItems);
    if (!toUndoTrashList) return;
    const selectParams = new URLPath().ArrayToCommaSeparatedStringOneParent(toUndoTrashList, "");

    if (selectParams.length === 0) return;

    const bodyParams = new URLSearchParams();
    bodyParams.append("f", selectParams);
    bodyParams.append("collections", "false");

    undoSelection();

    FetchPost(new UrlQuery().UrlDeleteApi(), bodyParams.toString(), "delete").then((result) => {
      if (result.statusCode === 200 || result.statusCode === 404) {
        dispatch({ type: "remove", toRemoveFileList: toUndoTrashList });
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
        <div className="modal content--subheader">{MessageDeleteImmediately}</div>
        <div className="modal content--text">
          {MessageDeleteIntroText}
          <br />
          <button data-test="force-cancel" onClick={() => handleExit()} className="btn btn--info">
            {MessageCancel}
          </button>
          <button
            onClick={() => {
              forceDelete();
              handleExit();
            }}
            data-test="force-delete"
            className="btn btn--default"
          >
            {MessageDeleteImmediately}
          </button>
        </div>
      </>
    </Modal>
  );
};

export default ModalForceDelete;
