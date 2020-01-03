import React, { memo } from 'react';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';

interface ItemListProps {
  fileIndexItems: IFileIndexItem[];
  callback(path: string): void;
}
/**
 * A list with links to the items
 */
const ItemTextListView: React.FunctionComponent<ItemListProps> = memo((props) => {

  if (!props.fileIndexItems) return (<div className="folder">no content</div>);

  return (<>
    {props.fileIndexItems.length === 0 ? <div className="warning-box"> Er zijn geen foto's</div> : ""}
    <ul>
      {
        props.fileIndexItems.map((item, index) => (
          <li className={item.isDirectory ? "box isDirectory-true" : "box isDirectory-false"} key={item.filePath + item.lastEdited}>
            {item.isDirectory ? <button onClick={() => { props.callback(item.filePath) }}></button> : null}
            {!item.isDirectory ? item.fileName : null}
          </li>
        ))
      }
    </ul></>)
});

export default ItemTextListView
