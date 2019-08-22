import React, { memo, useContext } from 'react';
import HistoryContext from '../contexts/history-contexts';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import { URLPath } from '../shared/url-path';
import Link from './Link';
import ListImage from './list-image';

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

  // used for reading current location
  const history = useContext(HistoryContext);

  return (
    <>
      <div className="folder">

        {items.length === 0 ? props.colorClassUsage.length >= 1 ? <div className="warning-box warning-box--left">Er zijn meer items, maar deze vallen buiten je filters</div> : <div className="warning-box"> Er zijn geen foto's in deze map</div> : ""}

        {
          items.map((item, index) => (
            <div className="box box--view" key={index}>
              <Link title={item.fileName} href={new URLPath().updateFilePath(history.location.search, item.filePath)}
                className={"box-content colorclass--" + item.colorClass + " isDirectory-" + item.isDirectory}>

                <ListImage alt={item.tags} src={'/api/thumbnail/' + item.fileHash + '?issingleitem=true'}></ListImage>

                <div className="caption">
                  <div className="name">
                    {item.fileName}
                  </div>
                  <div className="tags">
                    {item.tags}
                  </div>
                </div>
              </Link>

            </div>
          ))
        }
      </div>
    </>
  );
});

export default ItemListView
