import { ISidebarUpdate } from "../interfaces/ISidebarUpdate";
import { SidebarUpdate } from "../shared/sidebar-update";

describe("url-path", () => {
  var sidebarUpdate = new SidebarUpdate();

  describe("CastToISideBarUpdate", () => {
    it("no fieldname", () => {
      var result = sidebarUpdate.CastToISideBarUpdate(
        "",
        "value",
        {} as ISidebarUpdate
      );
      expect(result).toStrictEqual({});
    });

    it("no value", () => {
      var result = sidebarUpdate.CastToISideBarUpdate(
        "field",
        "",
        {} as ISidebarUpdate
      );
      expect(result).toStrictEqual({});
    });

    it("tags", () => {
      var result = sidebarUpdate.CastToISideBarUpdate(
        "tags",
        "test",
        {} as ISidebarUpdate
      );
      expect(result).toStrictEqual({ tags: "test" });
    });

    it("description", () => {
      var result = sidebarUpdate.CastToISideBarUpdate(
        "description",
        "test",
        {} as ISidebarUpdate
      );
      expect(result).toStrictEqual({ description: "test" });
    });

    it("title", () => {
      var result = sidebarUpdate.CastToISideBarUpdate(
        "title",
        "test",
        {} as ISidebarUpdate
      );
      expect(result).toStrictEqual({ title: "test" });
    });

    it("replace-tags", () => {
      var result = sidebarUpdate.CastToISideBarUpdate(
        "replace-tags",
        "test",
        {} as ISidebarUpdate
      );
      expect(result).toStrictEqual({ replaceTags: "test" });
    });

    it("replace-description", () => {
      var result = sidebarUpdate.CastToISideBarUpdate(
        "replace-description",
        "test",
        {} as ISidebarUpdate
      );
      expect(result).toStrictEqual({ replaceDescription: "test" });
    });

    it("replace-title", () => {
      var result = sidebarUpdate.CastToISideBarUpdate(
        "replace-title",
        "test",
        {} as ISidebarUpdate
      );
      expect(result).toStrictEqual({ replaceTitle: "test" });
    });

    it("send emthy string replace-title", () => {
      var result = sidebarUpdate.CastToISideBarUpdate(
        "replace-title",
        "",
        {} as ISidebarUpdate
      );
      expect(result).toStrictEqual({});
    });

    it("send emthy string tags", () => {
      var result = sidebarUpdate.CastToISideBarUpdate(
        "tags",
        "",
        {} as ISidebarUpdate
      );
      expect(result).toStrictEqual({});
    });

    it("send emthy string non existing tag", () => {
      var result = sidebarUpdate.CastToISideBarUpdate(
        "testung",
        "",
        {} as ISidebarUpdate
      );
      expect(result).toStrictEqual({});
    });
  });
  describe("IsFormUsed", () => {
    it("no input", () => {
      var input = {} as ISidebarUpdate;
      var result = sidebarUpdate.IsFormUsed(input);
      expect(result).toBeFalsy();
    });

    it("replace input", () => {
      var input = { replaceTags: "hey", replaceTitle: "ok" } as ISidebarUpdate;
      var result = sidebarUpdate.IsFormUsed(input);
      expect(result).toBeFalsy();
    });

    it("tags input", () => {
      var input = { tags: "t" } as ISidebarUpdate;
      var result = sidebarUpdate.IsFormUsed(input);
      expect(result).toBeTruthy();
    });

    it("title input", () => {
      var input = { title: "t" } as ISidebarUpdate;
      var result = sidebarUpdate.IsFormUsed(input);
      expect(result).toBeTruthy();
    });

    it("description input", () => {
      var input = { description: "t" } as ISidebarUpdate;
      var result = sidebarUpdate.IsFormUsed(input);
      expect(result).toBeTruthy();
    });
  });
});
