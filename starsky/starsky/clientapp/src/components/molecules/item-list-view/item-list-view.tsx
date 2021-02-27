import React, { memo, useEffect } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useLocation from "../../../hooks/use-location";
import { PageType } from "../../../interfaces/IDetailView";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import { INavigateState } from "../../../interfaces/INavigateState";
import { Language } from "../../../shared/language";
import { URLPath } from "../../../shared/url-path";
import FlatListItem from "../../atoms/flat-list-item/flat-list-item";
import ListImageChildItem from "../list-image-child-item/list-image-child-item";
import ListImageViewSelectContainer from "../list-image-view-select-container/list-image-view-select-container";
import { ShiftSelectionHelper } from "./shift-selection-helper";

interface ItemListProps {
  fileIndexItems: Array<IFileIndexItem>;
  colorClassUsage: Array<number>;
  pageType?: PageType;
  iconList?: boolean;
}
/**
 * A list with links to the items
 */
const ItemListView: React.FunctionComponent<ItemListProps> = memo((props) => {
  // feature that saves the scroll height
  var history = useLocation();
  const folderRef = React.useRef<HTMLDivElement>(null);

  // Content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageNoPhotosInFolder = language.text(
    "Er zijn geen foto's in deze map",
    "There are no photos in this folder"
  );
  const MessageItemsOutsideFilter = language.text(
    "Er zijn meer items, maar deze vallen buiten je filters. Om alles te zien klik op 'Herstel Filter'",
    "There are more items, but these are outside of your filters. To see everything click on 'Reset Filter'"
  );

  useEffect(() => {
    var navigationState = history.location.state as INavigateState;

    if (!navigationState) return;
    if (!navigationState.filePath) return;

    // for the DOM delay
    setTimeout(() => {
      var dataTagQuery = `[data-filepath="${navigationState.filePath}"]`;
      var elementList = document.querySelectorAll(dataTagQuery);
      if (elementList.length !== 1) return;

      window.scrollTo({
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

  let items = props.fileIndexItems;
  if (!items) return <div className="folder">no content</div>;

  return (
    <div className={props.iconList ? "folder" : "folder-flat"} ref={folderRef}>
      {props.pageType !== PageType.Loading ? (
        items.length === 0 ? (
          props.colorClassUsage.length >= 1 ? (
            <div className="warning-box warning-box--left">
              {MessageItemsOutsideFilter}
            </div>
          ) : (
            <div className="warning-box">{MessageNoPhotosInFolder}</div>
          )
        ) : null
      ) : null}
      {items.map((item) => (
        <ListImageViewSelectContainer
          item={item}
          className={props.iconList ? "list-image-box" : "list-flat-box"}
          key={item.fileName + item.lastEdited + item.colorClass}
          onSelectionCallback={onSelectionCallback}
        >
          {props.iconList ? (
            <ListImageChildItem {...item} />
          ) : (
            <FlatListItem item={item} />
          )}
        </ListImageViewSelectContainer>
      ))}
    </div>
  );
});

export default ItemListView;
