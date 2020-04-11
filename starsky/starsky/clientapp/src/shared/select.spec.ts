
import { globalHistory } from '@reach/router';
import { IArchiveProps } from '../interfaces/IArchiveProps';
import { Select } from './select';

describe("select", () => {
  describe("removeSidebarSelection", () => {

    it("single disable", () => {

      globalHistory.navigate("/?select=");
      var setSelectSpy = jest.fn();
      var select = new Select([], setSelectSpy, {} as IArchiveProps, globalHistory);
      select.removeSidebarSelection();

      expect(globalHistory.location.search).toBe("");
      expect(setSelectSpy).toBeCalled();
      expect(setSelectSpy).toBeCalledWith([]);

    });

    it("multiple disable", () => {

      globalHistory.navigate("/?select=1,2");
      var setSelectSpy = jest.fn();
      var select = new Select([], setSelectSpy, {} as IArchiveProps, globalHistory);
      select.removeSidebarSelection();

      expect(globalHistory.location.search).toBe("");
      expect(setSelectSpy).toBeCalled();
      expect(setSelectSpy).toBeCalledWith(['1', '2']);
    });
  });
});