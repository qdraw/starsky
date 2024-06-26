import React, { memo } from "react";
import { ArchiveAction } from "../../../contexts/archive-context";
import useHotKeys from "../../../hooks/use-keyboard/use-hotkeys";
import useLocation from "../../../hooks/use-location/use-location";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import localization from "../../../localization/localization.json";
import FetchPost from "../../../shared/fetch/fetch-post";
import { FileListCache } from "../../../shared/filelist-cache";
import { ClearSearchCache } from "../../../shared/search/clear-search-cache";
import { Select } from "../../../shared/select";
import { URLPath } from "../../../shared/url/url-path.ts";
import { UrlQuery } from "../../../shared/url/url-query.ts";
import MenuOption from "../../atoms/menu-option/menu-option.tsx";

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
const MenuOptionMoveToTrash: React.FunctionComponent<IMenuOptionMoveToTrashProps> = memo(
  ({ state, dispatch, select, setSelect, isReadOnly }) => {
    const undoSelection = () => new Select(select, setSelect, state, history).undoSelection();

    const history = useLocation();

    async function moveToTrashSelection() {
      if (!select || isReadOnly) return;

      const toUndoTrashList = new URLPath().MergeSelectFileIndexItem(select, state.fileIndexItems);

      if (!toUndoTrashList) return;
      const selectParams = new URLPath().ArrayToCommaSeparatedStringOneParent(toUndoTrashList, "");
      if (selectParams.length === 0) return;

      const bodyParams = new URLSearchParams();
      // noinspection PointlessBooleanExpressionJS
      const collections = new URLPath().StringToIUrl(history.location.search).collections !== false;

      bodyParams.append("f", selectParams);
      bodyParams.set("Tags", "!delete!");
      bodyParams.set("append", "true");
      bodyParams.set("Colorclass", "8");
      bodyParams.set("collections", collections.toString());

      const resultDo = await FetchPost(new UrlQuery().UrlMoveToTrashApi(), bodyParams.toString());

      if (
        resultDo.statusCode === 404 ||
        resultDo.statusCode === 400 ||
        resultDo.statusCode === 500 ||
        resultDo.statusCode === 502
      ) {
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
      moveToTrashSelection().then(() => {
        // do nothing
      });
    });

    return (
      <>
        {select.length >= 1 ? (
          <MenuOption
            isReadOnly={isReadOnly}
            testName={"trash"}
            onClickKeydown={moveToTrashSelection}
            localization={localization.MessageMoveToTrash}
          />
        ) : null}
      </>
    );
  }
);

export default MenuOptionMoveToTrash;
