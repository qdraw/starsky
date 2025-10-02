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
    for (let i = select.length - 1; i >= 0; i--) {
      const selectedPath = select[i];
      if (!fileNameList.includes(selectedPath)) {
        select.splice(i, 1);
      }
    }
    // remove empty items from the list
    return select.filter(Boolean);
  }
}
