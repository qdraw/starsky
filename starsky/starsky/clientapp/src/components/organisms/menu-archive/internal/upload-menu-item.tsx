import { ArchiveAction } from "../../../../contexts/archive-context";
import { IArchiveProps } from "../../../../interfaces/IArchiveProps";
import { IFileIndexItem } from "../../../../interfaces/IFileIndexItem";
import { FileListCache } from "../../../../shared/filelist-cache";
import { UrlQuery } from "../../../../shared/url-query";
import DropArea from "../../../atoms/drop-area/drop-area";

export interface IUploadMenuItemProps {
  readOnly: boolean;
  setDropAreaUploadFilesList: React.Dispatch<React.SetStateAction<IFileIndexItem[]>>;
  dispatch: React.Dispatch<ArchiveAction>;
  state: IArchiveProps;
}

export const UploadMenuItem: React.FunctionComponent<IUploadMenuItemProps> = ({
  readOnly,
  setDropAreaUploadFilesList,
  dispatch,
  state
}) => {
  if (readOnly)
    return (
      <li data-test="upload" className="menu-option disabled">
        Upload
      </li>
    );
  return (
    <li className="menu-option menu-option--input">
      <DropArea
        callback={(add) => {
          new FileListCache().CacheCleanEverything();
          setDropAreaUploadFilesList(add);
          dispatch({ type: "add", add });
        }}
        endpoint={new UrlQuery().UrlUploadApi()}
        folderPath={state.subPath}
        enableInputButton={true}
        enableDragAndDrop={true}
      />
    </li>
  );
};
