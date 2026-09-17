import React, { memo, useEffect } from "react";
import useLocation from "../../../hooks/use-location/use-location";
import { PageType } from "../../../interfaces/IDetailView";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import { INavigateState } from "../../../interfaces/INavigateState";
import { IDragMoveData, moveDragAndDropFiles } from "../../../shared/move-file-helper";
import { URLPath } from "../../../shared/url/url-path";
import FlatListItem from "../../atoms/flat-list-item/flat-list-item";
import ListImageChildItem from "../../atoms/list-image-child-item/list-image-child-item";
import ListImageViewSelectContainer from "../list-image-view-select-container/list-image-view-select-container";
import { ShiftSelectionHelper } from "./internal/shift-selection-helper";
import { WarningBoxNoPhotosFilter } from "./internal/warning-box-no-photos-filter";

interface ItemListProps {
  fileIndexItems: Array<IFileIndexItem>;
  colorClassUsage: Array<number>;
  pageType?: PageType;
  iconList?: boolean;
  subPath?: string;
}

/**
 * A list with links to the items
 */
const ItemListView: React.FunctionComponent<ItemListProps> = memo((props) => {
  // feature that saves the scroll height
  const history = useLocation();
  const folderRef = React.useRef<HTMLDivElement>(null);

  useEffect(() => {
    const navigationState = history.location.state as INavigateState;

    if (!navigationState?.filePath) return;

    // for the DOM delay
    setTimeout(() => {
      console.log("scroll to", navigationState.filePath);

      const dataTagQuery = `[data-filepath="${navigationState.filePath}"]`;
      const elementList = document.querySelectorAll(dataTagQuery);
      if (elementList.length !== 1) return;

      globalThis.scrollTo({
        top: elementList[0] ? (elementList[0] as HTMLDivElement).offsetTop : 0
      });

      // reset afterwards (when you refresh the state isn't cleared)
      history.navigate(history.location.href, { replace: true });
    }, 100);
  }, [history, history.location.state]);

  function onSelectionCallback(filePath: string) {
    ShiftSelectionHelper(
      history,
      new URLPath().getSelect(history.location.search),
      filePath,
      items
    );
  }

  function getDragSelection(): IDragMoveData {
    const selectedNames = new URLPath().getSelect(history.location.search) ?? [];
    const filePaths: string[] = [];
    const folderPaths: string[] = [];
    for (const fileIndexItem of items) {
      if (!selectedNames.includes(fileIndexItem.fileName)) continue;
      if (fileIndexItem.isDirectory) {
        folderPaths.push(fileIndexItem.filePath);
      } else {
        filePaths.push(fileIndexItem.filePath);
      }
    }
    return { filePaths, folderPaths };
  }

  async function handleDropToFolder(targetFolderPath: string) {
    const { filePaths, folderPaths } = getDragSelection();
    await moveDragAndDropFiles(filePaths, folderPaths, targetFolderPath);
    history.navigate(history.location.href, { replace: true });
  }

  const items = props.fileIndexItems;
  if (!items) return <div className="folder">no content</div>;

  const inSelectMode = !!new URLPath().StringToIUrl(history.location.search).select;

  return (
    <div className={props.iconList ? "folder" : "folder-flat"} ref={folderRef}>
      <WarningBoxNoPhotosFilter
        pageType={props.pageType}
        subPath={props.subPath}
        items={items}
        colorClassUsage={props.colorClassUsage}
      />

      {items.map((item) => (
        <ListImageViewSelectContainer
          item={item}
          className={props.iconList ? "list-image-box" : "list-flat-box"}
          key={item.fileName + item.lastEdited + item.colorClass}
          onSelectionCallback={onSelectionCallback}
          onDragStart={inSelectMode ? getDragSelection : undefined}
          onDropFiles={inSelectMode ? handleDropToFolder : undefined}
        >
          {props.iconList ? <ListImageChildItem {...item} /> : <FlatListItem item={item} />}
        </ListImageViewSelectContainer>
      ))}
    </div>
  );
});

export default ItemListView;
