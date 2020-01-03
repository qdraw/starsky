import React, { memo } from 'react';
import { IExifStatus } from '../interfaces/IExifStatus';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';

interface ItemListProps {
  fileIndexItems: IFileIndexItem[];
  callback(path: string): void;
}
/**
 * A list with links to the items
 */
const ItemTextListView: React.FunctionComponent<ItemListProps> = memo((props) => {

  if (!props.fileIndexItems) return (<div className="warning-box">Er zijn geen foto's</div>);

  return (<>
    {props.fileIndexItems.length === 0 ? <div className="warning-box">Er zijn geen foto's</div> : ""}
    <ul>
      {
        props.fileIndexItems.map((item, index) => (
          <li className={item.isDirectory ? "box isDirectory-true" :
            item.status === IExifStatus.Ok || item.status === IExifStatus.Default ?
              "box isDirectory-false" :
              "box isDirectory-false error"}
            key={item.filePath + item.lastEdited}>
            {item.isDirectory ? <button onClick={() => { props.callback(item.filePath) }}>{item.fileName}</button> : null}
            {!item.isDirectory ? item.fileName : null}
            {item.status !== IExifStatus.Ok && item.status !== IExifStatus.Default ?
              <em className="error-status">{item.status}</em> : null}
          </li>
        ))
      }
    </ul></>)
});

export default ItemTextListView
