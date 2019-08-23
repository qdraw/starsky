import { Link } from '@reach/router';
import React, { memo, useEffect } from 'react';
import useLocation from '../hooks/use-location';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import { URLPath } from '../shared/url-path';
import ListImage from './list-image';

interface IListImageBox {
  item: IFileIndexItem
}

const ListImageBox: React.FunctionComponent<IListImageBox> = memo((props) => {
  var item = props.item;

  var history = useLocation();

  const [sidebar, setSidebar] = React.useState(new URLPath().StringToIUrl(history.location.search).sidebar);
  useEffect(() => {
    setSidebar(new URLPath().StringToIUrl(history.location.search).sidebar)
  }, [history.location.search]);

  const [select, setSelect] = React.useState(new URLPath().StringToIUrl(history.location.search).select);

  function toggleSelection(fileName: string): void {

    var urlObject = new URLPath().StringToIUrl(history.location.search);
    if (!urlObject.select) {
      urlObject.select = [];
    }

    if (!urlObject.select || urlObject.select.indexOf(fileName) === -1) {
      urlObject.select.push(fileName)
    }
    else {
      var index = urlObject.select.indexOf(fileName);
      if (index !== -1) urlObject.select.splice(index, 1);
    }
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true });

    setSelect(urlObject.select);
  }


  // selected state
  if (sidebar === true && select) {
    return (
      <div className="box box--select">
        <div onClick={() => toggleSelection(item.fileName)}
          className={select.indexOf(item.fileName) === -1 ?
            "box-content colorclass--" + item.colorClass + " isDirectory-" + item.isDirectory :
            "box-content box-content--selected colorclass--" + item.colorClass + " isDirectory-" + item.isDirectory}>

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
    )
  }

  // default state
  return (
    <div className="box box--view">
      <Link title={item.fileName} to={new URLPath().updateFilePath("history.location.search", item.filePath)}
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
  );



});

export default ListImageBox