import React from 'react';
import useGlobalSettings from '../hooks/use-global-settings';
import { IExifStatus } from '../interfaces/IExifStatus';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import { Language } from '../shared/language';

interface ItemListProps {
  fileIndexItems: IFileIndexItem[];
  isLoading?: boolean;
  callback(path: string): void;
}
/**
 * A list with links to the items
 */
const ItemTextListView: React.FunctionComponent<ItemListProps> = ((props) => {

  // Content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageNoPhotos = language.text("Er zijn geen foto's", "There are no pictures");

  if (!props.fileIndexItems) return (<div className="warning-box">{MessageNoPhotos}</div>);

  return (<>
    {props.fileIndexItems.length === 0 ? <div className="warning-box">{MessageNoPhotos}</div> : ""}
    <ul>
      {
        props.fileIndexItems.map((item, index) => (
          <li className={item.isDirectory ? "box isDirectory-true" :
            item.status === IExifStatus.Ok || item.status === IExifStatus.Default ?
              "box isDirectory-false" :
              "box isDirectory-false error"}
            key={item.filePath + item.lastEdited}>
            {item.isDirectory ? <button data-test={"btn-" + item.fileName} onClick={() => {
              props.callback(item.filePath)
            }}>{item.fileName}</button> : null}
            {!item.isDirectory ? item.fileName : null}
            {item.status !== IExifStatus.Ok && item.status !== IExifStatus.Default ?
              <em className="error-status">{item.status}</em> : null}
          </li>
        ))
      }
    </ul></>)
});

export default ItemTextListView
