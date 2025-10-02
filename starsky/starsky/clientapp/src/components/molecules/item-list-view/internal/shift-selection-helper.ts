import { IUseLocation } from "../../../../hooks/use-location/interfaces/IUseLocation";
import { IFileIndexItem } from "../../../../interfaces/IFileIndexItem";
import { URLPath } from "../../../../shared/url/url-path";

/**
 * Find the value that is the closest
 * @see: https://stackoverflow.com/a/60029167
 * @param _array
 * @param _number
 */
function closest(_array: number[], _number: number): number {
  const diffArr = _array.map((x) => Math.abs(_number - x));
  const minNumber = Math.min(...diffArr);
  return diffArr.indexOf(minNumber);
}

/**
 * Get the index value of items by the newest file that is selected
 * @param filePath subPath
 * @param items all items in the folder
 */
function getIndexOfCurrentAppendFilePath(filePath: string, items: IFileIndexItem[]): number {
  let filePathAppendIndex: number = -1;
  for (let index = 0; index < items.length; index++) {
    const element = items[index];
    if (element.filePath === filePath) {
      filePathAppendIndex = index;
    }
  }
  return filePathAppendIndex;
}

/**
 * Find the location where the selection is from (so the other location that user implicit means to select)
 * @param filePathAppendIndex - Get the index value of items by the newest file that is selected
 * @param select - current selection
 * @param items - items all items in the folder
 */
function getNearbyStartIndex(
  filePathAppendIndex: number,
  select: string[],
  items: IFileIndexItem[]
): number {
  const alreadySelectIndexes: number[] = [];
  // the order of select is done by the user

  for (let index = 0; index < items.length; index++) {
    const element = items[index];
    if (select.includes(element.fileName)) {
      alreadySelectIndexes.push(index);
    }
  }
  return alreadySelectIndexes[closest(alreadySelectIndexes, filePathAppendIndex)];
}

/**
 * Loop from the lowest value to the highest to support adding bellow and above
 * @param filePathAppendIndex - Get the index value of items by the newest file that is selected
 * @param nearbyStartIndex - the location where the selection is from
 * @param items  - items all items in the folder
 */
function toAddedLoopMinToMax(
  filePathAppendIndex: number,
  nearbyStartIndex: number,
  items: IFileIndexItem[]
): string[] {
  const toBeAddedToSelect: string[] = [];

  // loop from the lowest value to the highest
  for (
    let index = Math.min(filePathAppendIndex, nearbyStartIndex);
    index <= Math.max(filePathAppendIndex, nearbyStartIndex);
    index++
  ) {
    toBeAddedToSelect.push(items[index].fileName);
  }
  return toBeAddedToSelect;
}

/**
 * Select multiple files by pressing shift and click the range
 * @param history - react hook to navigate
 * @param select - current selection
 * @param filePath - latest click filePath
 * @param items - list of items in current view
 */
export function ShiftSelectionHelper(
  history: IUseLocation,
  select: string[],
  filePath: string,
  items: IFileIndexItem[]
): boolean {
  if (!items || select === undefined) return false;

  // when nothing is selected assume the first
  if (select.length === 0 && items.length >= 1) select = [items[0].fileName];

  const filePathAppendIndex = getIndexOfCurrentAppendFilePath(filePath, items);
  if (filePathAppendIndex === -1) return false;
  const nearbyStartIndex = getNearbyStartIndex(filePathAppendIndex, select, items);
  const toBeAddedToSelect = toAddedLoopMinToMax(filePathAppendIndex, nearbyStartIndex, items);

  // remove duplicates
  const newSelect = [...select, items[filePathAppendIndex].fileName, ...toBeAddedToSelect].filter(
    function (item, pos) {
      return (
        [...select, items[filePathAppendIndex].fileName, ...toBeAddedToSelect].indexOf(item) === pos
      );
    }
  );
  const urlObject = new URLPath().updateSelection(history.location.search, newSelect);
  history.navigate(new URLPath().IUrlToString(urlObject), { replace: true });
  return true;
}
