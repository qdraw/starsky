import { IFileIndexItem } from "../interfaces/IFileIndexItem";

export class SelectCheckIfActive {
  public IsActive(
    select: string[] | undefined,
    colorclasses: number[],
    fileIndexItems: IFileIndexItem[]
  ) {
    if (!select) return [];

    // it should contain on the colorclasses
    const fileNameList = fileIndexItems
      .filter(
        (el) =>
          el.colorClass !== undefined &&
          colorclasses.indexOf(el.colorClass) !== -1
      )
      .map((ele) => ele.fileName);

    // you can't select an item thats not shown
    select.forEach((selectedPath) => {
      if (fileNameList.indexOf(selectedPath) === -1) {
        const selectIndex = select.indexOf(selectedPath);
        select.splice(selectIndex, 1);
      }
    });
    // remove emthy items from the list
    return select.filter((res) => res);
  }
}
