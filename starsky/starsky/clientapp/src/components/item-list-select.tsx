import React, { memo, useContext } from 'react';
import HistoryContext from '../contexts/history-contexts';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import { URLPath } from '../shared/url-path';
import ListImage from './list-image';

interface ItemListProps {
  fileIndexItems: Array<IFileIndexItem>,
  colorClassUsage: Array<number>,
}
/**
 * Select items from a list
 */
const ItemListSelect: React.FunctionComponent<ItemListProps> = memo((props) => {

  let items = props.fileIndexItems;

  if (!items) return (<div className="folder">no content</div>);

  // used for reading current location
  const history = useContext(HistoryContext);

  const [sidebar, setSidebar] = React.useState(getArraySidebar());

  function getArraySidebar(): Array<string> {
    var sidebar = new URLPath().StringToIUrl(history.location.hash).sidebar;
    if (!sidebar) return [];
    return sidebar;
  }

  function toggleSelection(fileName: string): void {

    var urlObject = new URLPath().StringToIUrl(history.location.hash);
    if (!urlObject.sidebar) {
      urlObject.sidebar = [];
    }

    if (!urlObject.sidebar || urlObject.sidebar.indexOf(fileName) === -1) {
      urlObject.sidebar.push(fileName)
    }
    else {
      var index = urlObject.sidebar.indexOf(fileName);
      if (index !== -1) urlObject.sidebar.splice(index, 1);
    }

    setSidebar(urlObject.sidebar)
    history.replace(new URLPath().IUrlToString(urlObject));
  }

  return (
    <>
      <div className="folder folder--select">

        {items.length === 0 ? props.colorClassUsage.length >= 1 ? <div className="warning-box warning-box--left">Er zijn meer items, maar deze vallen buiten je filters</div> : <div className="warning-box"> Er zijn geen foto's in deze map</div> : ""}

        {
          items.map((item, index) => (
            <div className="box" key={index}>
              <div onClick={() => toggleSelection(item.fileName)}
                className={sidebar.indexOf(item.fileName) === -1 ? "box-content colorclass--" + item.colorClass + " isDirectory-" + item.isDirectory : "box-content box-content--selected colorclass--" + item.colorClass + " isDirectory-" + item.isDirectory}>

                <ListImage alt={item.tags} src={'/api/thumbnail/' + item.fileHash + '?issingleitem=true'}></ListImage>

                <div className="caption">
                  <div className="name">
                    {item.fileName}
                  </div>
                  <div className="tags">
                    {item.tags}
                  </div>
                </div>
              </div>

            </div>
          ))
        }
      </div>
    </>
  );
});

export default ItemListSelect
