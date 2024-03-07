import useGlobalSettings from "../../../hooks/use-global-settings";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import localization from "../../../localization/localization.json";
import { Language } from "../../../shared/language";
import { ReactNode, FunctionComponent } from "react";
import { GetBoxClassName } from "./internal/get-box-class-name.ts";

interface ItemListProps {
  children?: ReactNode;
  fileIndexItems: IFileIndexItem[];
  isLoading?: boolean;

  callback?(path: string): void;
}

/**
 * A list with links to the items
 */
const ItemTextListView: FunctionComponent<ItemListProps> = (props) => {
  // Content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageNoPhotos = language.key(localization.MessageNoPhotos);

  if (!props.fileIndexItems)
    return (
      <div className="warning-box" data-test="list-text-view-no-photos-in-folder">
        {MessageNoPhotos}
      </div>
    );

  return (
    <>
      {props.fileIndexItems.length === 0 ? (
        <div className="warning-box" data-test="list-text-view-no-photos-in-folder">
          {MessageNoPhotos}
        </div>
      ) : (
        ""
      )}
      <ul>
        {props.fileIndexItems.map((item) => (
          <li className={GetBoxClassName(item)} key={item.filePath + item.lastEdited}>
            {item.isDirectory ? (
              <button
                data-test={"btn-" + item.fileName}
                onClick={() => {
                  if (!props.callback) return;
                  props.callback(item.filePath);
                }}
              >
                {item.fileName}
              </button>
            ) : null}
            {!item.isDirectory ? item.fileName : null}
            {item.status !== IExifStatus.Ok && item.status !== IExifStatus.Default ? (
              <em className="error-status">{item.status}</em>
            ) : null}
          </li>
        ))}
      </ul>
    </>
  );
};

export default ItemTextListView;
