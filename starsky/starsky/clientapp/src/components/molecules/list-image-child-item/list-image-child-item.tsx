import React from "react";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import ListImage from "../../atoms/list-image/list-image";

const ListImageChildItem: React.FunctionComponent<IFileIndexItem> = (item) => {
  return (
    <>
      <ListImage
        imageFormat={item.imageFormat}
        alt={item.tags}
        fileHash={item.fileHash}
      />
      <div className="caption">
        <div className="name" title={item.fileName}>
          {item.fileName}
        </div>
        <div className="tags" title={item.tags}>
          {item.tags}
        </div>
      </div>
    </>
  );
};

export default ListImageChildItem;
