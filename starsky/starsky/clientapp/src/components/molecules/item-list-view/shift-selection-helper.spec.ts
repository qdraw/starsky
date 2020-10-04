import { globalHistory } from '@reach/router';
import { newIFileIndexItemArray } from '../../../interfaces/IFileIndexItem';
import { ShiftSelectionHelper } from './shift-selection-helper';

describe("ShiftSelectionHelper", () => {

  it("filePath not found", () => {
    var result = ShiftSelectionHelper(globalHistory, [], "test", newIFileIndexItemArray());
    expect(result).toBeFalsy();
  });
});