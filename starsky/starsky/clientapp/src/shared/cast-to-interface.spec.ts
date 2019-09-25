import { PageType } from '../interfaces/IDetailView';
import { IMedia } from '../interfaces/IMedia';
import { CastToInterface } from './cast-to-interface';

describe("cast-to-interface", () => {
  describe("getPageType", () => {
    it("getPageType null", () => {
      var test = new CastToInterface().getPageType(null);
      expect(test).toBe(PageType.ApplicationException);
    });

    it("getPageType toBeUndefined", () => {
      var test = new CastToInterface().getPageType({});
      expect(test).toBeUndefined();
    });

    it("getPageType DetailView", () => {
      var test = new CastToInterface().getPageType({
        "pageType": "DetailView"
      });
      expect(test).toBe(PageType.DetailView);
    });
  });
  describe("MediaDetailView", () => {
    it("DetailView default", () => {
      var test = new CastToInterface().MediaDetailView({});
      expect(test.data.status).toBeNull();
    });

    it("DetailView", () => {
      var test = new CastToInterface().MediaDetailView({
        "pageType": "DetailView",
        "fileIndexItem": {
          "tags": "test"
        }
      });
      expect(test.data.pageType).toBe(PageType.DetailView);
      expect(test.data.fileIndexItem.tags).toBe("test");
    });
  });

  describe("MediaArchive", () => {
    it("Archive default", () => {
      var test = new CastToInterface().MediaArchive({});
      expect(test.data).toStrictEqual({} as IMedia<'Archive'>)
    });
    it("Archive", () => {
      var test = new CastToInterface().MediaArchive({
        "pageType": "Archive",
        "fileIndexItems": [
          { "tags": "test" }
        ]
      });
      expect(test.data.pageType).toBe(PageType.Archive);
      expect(test.data.fileIndexItems[0].tags).toBe("test");
    });
  });

  describe("InfoFileIndexArray", () => {
    it("one item", () => {
      var test = new CastToInterface().InfoFileIndexArray([
        { "tags": "test" }
      ]);
      expect(test[0].tags).toBe("test");
    });
  });

});