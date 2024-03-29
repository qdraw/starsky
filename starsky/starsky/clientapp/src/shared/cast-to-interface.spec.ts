import { PageType } from "../interfaces/IDetailView";
import { IMedia } from "../interfaces/IMedia";
import { CastToInterface } from "./cast-to-interface";

describe("cast-to-interface", () => {
  describe("MediaDetailView", () => {
    it("DetailView default", () => {
      const test = new CastToInterface().MediaDetailView({});
      expect(test.data.lastUpdated).toBeUndefined();
    });

    it("DetailView", () => {
      const test = new CastToInterface().MediaDetailView({
        pageType: "DetailView",
        fileIndexItem: {
          tags: "test"
        }
      });
      expect(test.data.pageType).toBe(PageType.DetailView);
      expect(test.data.fileIndexItem.tags).toBe("test");
    });
  });

  describe("MediaArchive", () => {
    it("Archive default", () => {
      const test = new CastToInterface().MediaArchive({});
      expect(test.data).toStrictEqual({} as IMedia<"Archive">);
    });
    it("Archive", () => {
      const test = new CastToInterface().MediaArchive({
        pageType: "Archive",
        fileIndexItems: [{ tags: "test" }]
      });
      expect(test.data.pageType).toBe(PageType.Archive);
      expect(test.data.fileIndexItems[0].tags).toBe("test");
    });
  });

  describe("InfoFileIndexArray", () => {
    it("one item", () => {
      const test = new CastToInterface().InfoFileIndexArray([{ tags: "test" }]);
      expect(test[0].tags).toBe("test");
    });

    it("should ignore string", () => {
      const test = new CastToInterface().InfoFileIndexArray("test");
      expect(test).toStrictEqual([]);
    });
  });
});
