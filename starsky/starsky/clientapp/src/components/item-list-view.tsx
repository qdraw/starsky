import React, { memo, useEffect } from 'react';
import useLocation from '../hooks/use-location';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import { INavigateState } from '../interfaces/INavigateState';
import ListImageBox from './list-image-box';

interface ItemListProps {
  fileIndexItems: Array<IFileIndexItem>,
  colorClassUsage: Array<number>,
}
/**
 * A list with links to the items
 */
const ItemListView: React.FunctionComponent<ItemListProps> = memo((props) => {

  // feature that saves the scroll height
  var history = useLocation();
  const folderRef = React.useRef<HTMLDivElement>(null);

  useEffect(() => {
    var navigationState = history.location.state as INavigateState;

    if (!navigationState) return;
    if (!navigationState.filePath) return;

    // for the DOM delay
    setTimeout(() => {
      var query = '[data-filepath="' + navigationState.filePath + '"]';
      var elementList = document.querySelectorAll(query);
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
      {items.length === 0 ? props.colorClassUsage.length >= 1 ? <div className="warning-box warning-box--left">Er zijn meer items, maar deze vallen buiten je filters</div> : <div className="warning-box"> Er zijn geen foto's in deze map</div> : ""}
      {
        items.map((item, index) => (
          <ListImageBox item={item} key={item.fileName + item.lastEdited}></ListImageBox>
        ))
      }
    </div>)
});

export default ItemListView
