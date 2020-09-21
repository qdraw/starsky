import { Link } from '@reach/router';
import React, { memo, useEffect } from 'react';
import useLocation from '../../../hooks/use-location';
import { IFileIndexItem } from '../../../interfaces/IFileIndexItem';
import { URLPath } from '../../../shared/url-path';
import { UrlQuery } from '../../../shared/url-query';
import ListImage from '../../atoms/list-image/list-image';
import Preloader from '../../atoms/preloader/preloader';

interface IListImageBox {
  item: IFileIndexItem
}

const ListImageBox: React.FunctionComponent<IListImageBox> = memo((props) => {
  var item = props.item;

  var history = useLocation();

  // Check if select exist or Length 0 or more
  const [select, setSelect] = React.useState(new URLPath().StringToIUrl(history.location.search).select);
  useEffect(() => {
    var inSelect = new URLPath().StringToIUrl(history.location.search).select;
    setSelect(inSelect);
  }, [history.location.search]);

  function toggleSelection(fileName: string): void {
    var urlObject = new URLPath().toggleSelection(fileName, history.location.search);
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true });
    setSelect(urlObject.select);
  }

  var preloader = <Preloader isOverlay={true} isDetailMenu={false} />
  const [isPreloaderState, setPreloaderState] = React.useState(false);

  function preloaderStateOnClick(event: React.MouseEvent) {
    // Command (mac) or ctrl click means open new window
    // event.button = is only trigged in safari
    if (event.metaKey || event.ctrlKey || event.button === 1) return;
    setPreloaderState(true)
  }

  // selected state
  if (select) {
    return (
      <div className="list-image-box list-image-box--select">
        <button onClick={() => toggleSelection(item.fileName)}
          className={select.indexOf(item.fileName) === -1 ?
            "box-content colorclass--" + item.colorClass + " isDirectory-" + item.isDirectory :
            "box-content box-content--selected colorclass--" + item.colorClass + " isDirectory-" + item.isDirectory}>
          <ListImage imageFormat={item.imageFormat} alt={item.tags} fileHash={item.fileHash} />
          <div className="caption">
            <div className="name" title={item.fileName}>
              {item.fileName}
            </div>
            <div className="tags" title={item.tags}>
              {item.tags}
            </div>
          </div>
        </button>
      </div>
    )
  }

  // default state
  // data-filepath is needed to scroll to
  return (
    <div className="list-image-box box--view" data-filepath={item.filePath}>
      {/* for slow connections show preloader icon */}
      {isPreloaderState ? preloader : null}
      {/* the a href to the child page */}
      <Link onClick={preloaderStateOnClick} title={item.fileName} to={new UrlQuery().updateFilePathHash(history.location.search, item.filePath)}
        className={"box-content colorclass--" + item.colorClass + " isDirectory-" + item.isDirectory}>
        <ListImage imageFormat={item.imageFormat} alt={item.tags} fileHash={item.fileHash} />
        <div className="caption">
          <div className="name" title={item.fileName}>
            {item.fileName}
          </div>
          <div className="tags" title={item.tags}>
            {item.tags}
          </div>
        </div>
      </Link>
    </div>
  );

});

export default ListImageBox
