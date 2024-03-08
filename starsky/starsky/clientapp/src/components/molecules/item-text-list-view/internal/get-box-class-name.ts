import { IFileIndexItem } from "../../../../interfaces/IFileIndexItem";
import { IExifStatus } from "../../../../interfaces/IExifStatus";

export function GetBoxClassName(item: IFileIndexItem): string {
  if (item.isDirectory) {
    return "box isDirectory-true";
  } else if (
    item.status === IExifStatus.Ok ||
    item.status === IExifStatus.Default ||
    item.status === IExifStatus.OkAndSame
  ) {
    return "box isDirectory-false";
  } else {
    return "box isDirectory-false error";
  }
}
