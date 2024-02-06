import { IFileIndexItem } from "../interfaces/IFileIndexItem";

export class SelectCheckIfActive {
  public IsActive(
    select: string[] | undefined,
    colorClasses: number[],
    fileIndexItems: IFileIndexItem[]
  ) {
    if (!select) return [];

    // it should contain on the colorClasses
    const fileNameList = fileIndexItems
      .filter((el) => el.colorClass !== undefined && colorClasses.indexOf(el.colorClass) !== -1)
      .map((ele) => ele.fileName);

    // you can't select an item that's not shown
    select.forEach((selectedPath) => {
      if (fileNameList.indexOf(selectedPath) === -1) {
        const selectIndex = select.indexOf(selectedPath);
        select.splice(selectIndex, 1);
      }
    });
    // remove empty items from the list
    return select.filter((res) => res);
  }
}
