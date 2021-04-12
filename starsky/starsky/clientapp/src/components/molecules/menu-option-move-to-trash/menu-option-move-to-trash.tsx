import React from "react";
import { ArchiveAction } from "../../../contexts/archive-context";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useHotKeys from "../../../hooks/use-keyboard/use-hotkeys";
import useLocation from "../../../hooks/use-location";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import FetchPost from "../../../shared/fetch-post";
import { FileListCache } from "../../../shared/filelist-cache";
import { Language } from "../../../shared/language";
import { ClearSearchCache } from "../../../shared/search/clear-search-cache";
import { Select } from "../../../shared/select";
import { URLPath } from "../../../shared/url-path";
import { UrlQuery } from "../../../shared/url-query";

interface IMenuOptionMoveToTrashProps {
  select: string[];
  setSelect: React.Dispatch<React.SetStateAction<string[] | undefined>>;
  isReadOnly: boolean;
  state: IArchiveProps;
  dispatch: React.Dispatch<ArchiveAction>;
}

/**
 * Used from Archive and Search
 */
const MenuOptionMoveToTrash: React.FunctionComponent<IMenuOptionMoveToTrashProps> = ({
  state,
  dispatch,
  select,
  setSelect,
  isReadOnly
}) => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  var undoSelection = () =>
    new Select(select, setSelect, state, history).undoSelection();

  const MessageMoveToTrash = language.text(
    "Verplaats naar prullenmand",
    "Move to Trash"
  );

  var history = useLocation();

  async function moveToTrashSelection() {
    if (!select || isReadOnly) return;

    var toUndoTrashList = new URLPath().MergeSelectFileIndexItem(
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
    bodyParams.set("Tags", "!delete!");
    bodyParams.set("append", "true");
    bodyParams.set("Colorclass", "8");
    bodyParams.set(
      "collections",
      (
        new URLPath().StringToIUrl(history.location.search).collections !==
        false
      ).toString()
    );

    var resultDo = await FetchPost(
      new UrlQuery().UrlUpdateApi(),
      bodyParams.toString()
    );
    if (resultDo.statusCode !== 404 && resultDo.statusCode !== 200) {
      return;
    }

    undoSelection();
    dispatch({ type: "remove", toRemoveFileList: toUndoTrashList });
    ClearSearchCache(history.location.search);
    // Client side Caching: the order of files in a normal folder has changed
    new FileListCache().CacheCleanEverything();
  }

  /**
   * When pressing delete its moved to the trash
   */
  useHotKeys({ key: "Delete" }, () => {
    moveToTrashSelection();
  });

  return (
    <>
      {select.length >= 1 ? (
        <li
          data-test="trash"
          className={!isReadOnly ? "menu-option" : "menu-option disabled"}
          onClick={() => moveToTrashSelection()}
        >
          {MessageMoveToTrash}
        </li>
      ) : null}
    </>
  );
};

export default MenuOptionMoveToTrash;
