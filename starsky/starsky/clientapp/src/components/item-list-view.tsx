import React, { memo } from 'react';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import ListImageBox from './list-image-box';

interface ItemListProps {
  fileIndexItems: Array<IFileIndexItem>,
  colorClassUsage: Array<number>,
}
/**
 * A list with links to the items
 */
const ItemListView: React.FunctionComponent<ItemListProps> = memo((props) => {

  let items = props.fileIndexItems;

  if (!items) return (<div className="folder">no content</div>);

  return (
    <>
      <div className="folder">

        {items.length === 0 ? props.colorClassUsage.length >= 1 ? <div className="warning-box warning-box--left">Er zijn meer items, maar deze vallen buiten je filters</div> : <div className="warning-box"> Er zijn geen foto's in deze map</div> : ""}

        {
          items.map((item, index) => (
            <ListImageBox item={item} key={index}></ListImageBox>
          ))
        }
      </div>
    </>
  );
});

export default ItemListView
