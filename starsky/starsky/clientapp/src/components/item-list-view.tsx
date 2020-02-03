import React, { memo, useEffect } from 'react';
import useGlobalSettings from '../hooks/use-global-settings';
import useLocation from '../hooks/use-location';
import { PageType } from '../interfaces/IDetailView';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import { INavigateState } from '../interfaces/INavigateState';
import { Language } from '../shared/language';
import ListImageBox from './list-image-box';

interface ItemListProps {
  fileIndexItems: Array<IFileIndexItem>,
  colorClassUsage: Array<number>,
  pageType?: PageType;
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
  const MessageNoPhotosInFolder = language.text("Er zijn geen foto's in deze map",
    "There are no photos in this folder");
  const MessageItemsOutsideFilter = language.text("Er zijn meer items, maar deze vallen buiten je filters",
    "There are more items, but these are outside of your filters");

  useEffect(() => {
    var navigationState = history.location.state as INavigateState;

    if (!navigationState) return;
    if (!navigationState.filePath) return;

    // for the DOM delay
    setTimeout(() => {
      var dataTagQuery = `[data-filepath="${navigationState.filePath}"]`
      var elementList = document.querySelectorAll(dataTagQuery);
      if (elementList.length !== 1) return;

      window.scrollTo({
        top: elementList[0] ? (elementList[0] as HTMLDivElement).offsetTop : 0
      });

      // reset afterwards (when you refresh the state isn't cleared)
      history.navigate(history.location.href, { replace: true });
    }, 100);

  }, [history, history.location.state]);

  let items = props.fileIndexItems;
  if (!items) return (<div className="folder">no content</div>);

  return (
    <div className="folder" ref={folderRef} >
      {props.pageType !== PageType.Loading ?
        items.length === 0 ? props.colorClassUsage.length >= 1 ?
          <div className="warning-box warning-box--left">
            {MessageItemsOutsideFilter}
          </div> : <div className="warning-box">
            {MessageNoPhotosInFolder}
          </div> : null : null
      }
      {
        items.map((item, index) => (
          <ListImageBox item={item} key={item.fileName + item.lastEdited}></ListImageBox>
        ))
      }
    </div>)
});

export default ItemListView
