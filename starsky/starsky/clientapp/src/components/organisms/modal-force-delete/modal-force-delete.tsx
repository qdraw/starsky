import React from "react";
import { ArchiveAction } from "../../../contexts/archive-context";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useLocation from "../../../hooks/use-location";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import FetchPost from "../../../shared/fetch-post";
import { Language } from "../../../shared/language";
import { ClearSearchCache } from "../../../shared/search/clear-search-cache";
import { Select } from "../../../shared/select";
import { URLPath } from "../../../shared/url-path";
import { UrlQuery } from "../../../shared/url-query";
import Modal from "../../atoms/modal/modal";

interface IModalForceDeleteProps {
  isOpen: boolean;
  select: Array<string> | undefined;
  setSelect: React.Dispatch<React.SetStateAction<string[] | undefined>>;
  handleExit: Function;
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
  const MessageDeleteIntroText = language.text(
    "Weet je zeker dat je dit bestand wilt verwijderen van alle devices?",
    "Are you sure you want to delete this file from all devices?"
  );
  const MessageCancel = language.text("Annuleren", "Cancel");
  const MessageDeleteImmediately = language.text(
    "Verwijder onmiddellijk",
    "Delete immediately"
  );
  var history = useLocation();

  const undoSelection = () =>
    new Select(select, setSelect, state, history).undoSelection();

  function forceDelete() {
    if (!select) return;
    setIsLoading(true);

    const toUndoTrashList = new URLPath().MergeSelectFileIndexItem(
      select,
      state.fileIndexItems
    );
    if (!toUndoTrashList) return;
    var selectParams = new URLPath().ArrayToCommaSeperatedStringOneParent(
      toUndoTrashList,
      ""
    );

    if (selectParams.length === 0) return;

    var bodyParams = new URLSearchParams();
    bodyParams.append("f", selectParams);
    bodyParams.append("collections", "false");

    undoSelection();

    FetchPost(
      new UrlQuery().UrlDeleteApi(),
      bodyParams.toString(),
      "delete"
    ).then((result) => {
      if (result.statusCode === 200) {
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
        <div className="modal content--subheader">
          {MessageDeleteImmediately}
        </div>
        <div className="modal content--text">
          {MessageDeleteIntroText}
          <br />
          <button
            data-test="force-cancel"
            onClick={() => handleExit()}
            className="btn btn--info"
          >
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
