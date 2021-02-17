import { SortType } from "../interfaces/IArchive";
import { IFileIndexItem, ImageFormat } from "../interfaces/IFileIndexItem";

export function sorter(
  concatenatedFileIndexItems: IFileIndexItem[],
  sort?: SortType
): IFileIndexItem[] {
  switch (sort) {
    case undefined:
    case SortType.fileName:
      // order by this to match c# AND not supported in jest
      return [...concatenatedFileIndexItems].sort((a, b) =>
        a.fileName.localeCompare(b.fileName, "en", { sensitivity: "base" })
      );
    case SortType.imageFormat:
      return [...concatenatedFileIndexItems].sort((a, b) => {
        if (!a.imageFormat) a.imageFormat = ImageFormat.unknown;
        if (!b.imageFormat) b.imageFormat = ImageFormat.unknown;

        console.log("---");

        console.log(a.imageFormat, b.imageFormat);

        const enumOrder = Object.values(ImageFormat);

        return (
          enumOrder.indexOf(a.imageFormat) - enumOrder.indexOf(b.imageFormat)
        );
      });
    default:
      break;
  }
  return [];
}
