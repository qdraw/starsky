import React, { memo, useEffect, useState } from 'react';
import { IFileIndexItem, newIFileIndexItemArray } from '../interfaces/IFileIndexItem';

interface ItemListProps {
  fileIndexItems: Array<IFileIndexItem>,
  colorClassUsage: Array<number>,
}
/**
 * A list with links to the items
 */
const ItemTextListView: React.FunctionComponent<ItemListProps> = memo((props) => {

  const [items, setItems] = useState(newIFileIndexItemArray());

  useEffect(() => {
    console.log('hi');

    setItems(props.fileIndexItems);

  }, [props]);

  if (!items) return (<div className="folder">no content</div>);

  return (
    <ul>
      {items.length === 0 ? props.colorClassUsage.length >= 1 ? <div className="warning-box warning-box--left">Er zijn meer items, maar deze vallen buiten je filters</div> : <div className="warning-box"> Er zijn geen foto's in deze map</div> : ""}
      {
        items.map((item, index) => (
          <li key={item.filePath}>
            {item.filePath} - {item.status.toString()}
          </li>
        ))
      }
    </ul>)
});

export default ItemTextListView
