import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import ListImage from "../list-image/list-image";
import { FunctionComponent } from "react";

const ListImageChildItem: FunctionComponent<IFileIndexItem> = (item) => {
  return (
    <>
      <ListImage
        filePath={item.filePath}
        imageFormat={item.imageFormat}
        alt={item.tags}
        fileHash={item.fileHash}
      />
      <div className="caption">
        <div className="name" data-test="list-image-name" title={item.fileName}>
          {item.fileName}
        </div>
        <div className="tags" data-test="list-image-tags" title={item.tags}>
          {item.tags}
        </div>
      </div>
    </>
  );
};

export default ListImageChildItem;
