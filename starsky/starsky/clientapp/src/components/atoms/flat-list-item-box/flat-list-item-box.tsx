import React from "react";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import { parseRelativeDate } from "../../../shared/date";
import { SupportedLanguages } from "../../../shared/language";

interface IFlatListItemBox {
  item: IFileIndexItem;
  /**
   * When selecting and pressing shift
   * @param filePath the entire path (subPath style)
   */
  onSelectionCallback?(filePath: string): void;
}

const FlatListItemBox: React.FunctionComponent<IFlatListItemBox> = ({
  item
}) => {
  return (
    <div className="flatlistitem">
      <div className="name">{item.fileName}</div>
      <div className="lastedited">
        {parseRelativeDate(item.lastEdited, SupportedLanguages.nl)}
      </div>
      <div className="size">{!item.isDirectory ? item.size : "--"}</div>
      <div className="imageformat">{item.imageFormat}</div>
    </div>
  );
};

export default FlatListItemBox;
