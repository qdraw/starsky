
import { newIFileIndexItem } from '../interfaces/IFileIndexItem';
import { SelectCheckIfActive } from './select-check-if-active';

describe("SelectCheckIfActive", () => {
  describe("IsActive", () => {

    it("filter nr 2 out", () => {

      var result = new SelectCheckIfActive().IsActive(["test1.jpg", "test2.jpg"], [1], [
        { ...newIFileIndexItem(), colorClass: 1, fileName: 'test1.jpg' },
        { ...newIFileIndexItem(), colorClass: 2, fileName: 'test2.jpg' }
      ]);

      expect(result.length).toBe(1)
      expect(result[0]).toBe('test1.jpg')
    });

    it("ignore files wihout colorclass marking", () => {

      var result = new SelectCheckIfActive().IsActive(["test1.jpg", "test2.jpg"], [1], [
        { ...newIFileIndexItem(), fileName: 'test1.jpg' },
        { ...newIFileIndexItem(), colorClass: 1, fileName: 'test2.jpg' }
      ]);

      expect(result.length).toBe(1)
      expect(result[0]).toBe('test2.jpg')
    });

  });
});