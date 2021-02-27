import React from "react";
import {
  IFileIndexItem,
  ImageFormat
} from "../../../interfaces/IFileIndexItem";
import BytesFormat from "../../../shared/bytes-format";
import {
  parseDateDate,
  parseDateMonth,
  parseDateYear,
  parseTime
} from "../../../shared/date";

interface IFlatListItem {
  item: IFileIndexItem;
  /**
   * When selecting and pressing shift
   * @param filePath the entire path (subPath style)
   */
  onSelectionCallback?(filePath: string): void;
}

const FlatListItem: React.FunctionComponent<IFlatListItem> = ({ item }) => {
  return (
    <div className="flatlistitem">
      <div className="name">{item.fileName}</div>
      <div className="lastedited">
        {parseDateYear(item.lastEdited) !== 1 ? (
          <>
            {parseDateDate(item.lastEdited)}-{parseDateMonth(item.lastEdited)}-
            {parseDateYear(item.lastEdited)} {parseTime(item.lastEdited)}
          </>
        ) : (
          "--"
        )}
      </div>
      <div className="size">
        {!item.isDirectory && item.size ? BytesFormat(item.size) : "--"}
      </div>
      <div className="imageformat">
        {item.imageFormat !== ImageFormat.unknown ? item.imageFormat : "--"}
      </div>
    </div>
  );
};

export default FlatListItem;
