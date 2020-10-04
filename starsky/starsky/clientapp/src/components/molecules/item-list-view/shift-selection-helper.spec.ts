import { globalHistory } from '@reach/router';
import { newIFileIndexItemArray } from '../../../interfaces/IFileIndexItem';
import { ShiftSelectionHelper } from './shift-selection-helper';

describe("ShiftSelectionHelper", () => {

  it("renders (without state component)", () => {
    ShiftSelectionHelper(globalHistory, [], "test", newIFileIndexItemArray());
  });
});