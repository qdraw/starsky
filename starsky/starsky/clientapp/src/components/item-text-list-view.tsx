import React, { memo } from 'react';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';

interface ItemListProps {
  fileIndexItems: IFileIndexItem[],
  lastUploaded: Date
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
          <li key={item.filePath + item.lastEdited}>
            {item.fileName} - {item.status.toString()}
          </li>
        ))
      }
    </ul></>)
});

export default ItemTextListView
