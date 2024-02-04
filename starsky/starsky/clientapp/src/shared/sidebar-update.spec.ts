import { ISidebarUpdate } from "../interfaces/ISidebarUpdate";
import { SidebarUpdate } from "../shared/sidebar-update";

describe("url-path", () => {
  const sidebarUpdate = new SidebarUpdate();

  describe("CastToISideBarUpdate", () => {
    it("no fieldname", () => {
      const result = sidebarUpdate.CastToISideBarUpdate("", "value", {} as ISidebarUpdate);
      expect(result).toStrictEqual({});
    });

    it("no value", () => {
      const result = sidebarUpdate.CastToISideBarUpdate("field", "", {} as ISidebarUpdate);
      expect(result).toStrictEqual({});
    });

    it("tags", () => {
      const result = sidebarUpdate.CastToISideBarUpdate("tags", "test", {} as ISidebarUpdate);
      expect(result).toStrictEqual({ tags: "test" });
    });

    it("description", () => {
      const result = sidebarUpdate.CastToISideBarUpdate(
        "description",
        "test",
        {} as ISidebarUpdate
      );
      expect(result).toStrictEqual({ description: "test" });
    });

    it("title", () => {
      const result = sidebarUpdate.CastToISideBarUpdate("title", "test", {} as ISidebarUpdate);
      expect(result).toStrictEqual({ title: "test" });
    });

    it("replace-tags", () => {
      const result = sidebarUpdate.CastToISideBarUpdate(
        "replace-tags",
        "test",
        {} as ISidebarUpdate
      );
      expect(result).toStrictEqual({ replaceTags: "test" });
    });

    it("replace-description", () => {
      const result = sidebarUpdate.CastToISideBarUpdate(
        "replace-description",
        "test",
        {} as ISidebarUpdate
      );
      expect(result).toStrictEqual({ replaceDescription: "test" });
    });

    it("replace-title", () => {
      const result = sidebarUpdate.CastToISideBarUpdate(
        "replace-title",
        "test",
        {} as ISidebarUpdate
      );
      expect(result).toStrictEqual({ replaceTitle: "test" });
    });

    it("send empty string replace-title", () => {
      const result = sidebarUpdate.CastToISideBarUpdate("replace-title", "", {} as ISidebarUpdate);
      expect(result).toStrictEqual({});
    });

    it("send empty string tags", () => {
      const result = sidebarUpdate.CastToISideBarUpdate("tags", "", {} as ISidebarUpdate);
      expect(result).toStrictEqual({});
    });

    it("send empty string non existing tag", () => {
      const result = sidebarUpdate.CastToISideBarUpdate("test", "", {} as ISidebarUpdate);
      expect(result).toStrictEqual({});
    });
  });
  describe("Change", () => {
    it("no field name should return null", () => {
      const result = sidebarUpdate.Change(
        { currentTarget: { textContent: null, dataset: {} } } as any,
        {} as ISidebarUpdate
      );
      expect(result).toStrictEqual(null);
    });

    it("no text should return empty object", () => {
      const result = sidebarUpdate.Change(
        {
          currentTarget: { textContent: null, dataset: { name: "test" } }
        } as any,
        {} as ISidebarUpdate
      );
      expect(result).toStrictEqual({});
    });

    it("has text and tag name", () => {
      const result = sidebarUpdate.Change(
        {
          currentTarget: { textContent: "test", dataset: { name: "tags" } }
        } as any,
        {} as ISidebarUpdate
      );
      expect(result).toStrictEqual({
        tags: "test"
      });
    });
  });

  describe("IsFormUsed", () => {
    it("no input", () => {
      const input = {} as ISidebarUpdate;
      const result = sidebarUpdate.IsFormUsed(input);
      expect(result).toBeFalsy();
    });

    it("replace input", () => {
      const input = {
        replaceTags: "hey",
        replaceTitle: "ok"
      } as ISidebarUpdate;
      const result = sidebarUpdate.IsFormUsed(input);
      expect(result).toBeFalsy();
    });

    it("tags input", () => {
      const input = { tags: "t" } as ISidebarUpdate;
      const result = sidebarUpdate.IsFormUsed(input);
      expect(result).toBeTruthy();
    });

    it("title input", () => {
      const input = { title: "t" } as ISidebarUpdate;
      const result = sidebarUpdate.IsFormUsed(input);
      expect(result).toBeTruthy();
    });

    it("description input", () => {
      const input = { description: "t" } as ISidebarUpdate;
      const result = sidebarUpdate.IsFormUsed(input);
      expect(result).toBeTruthy();
    });
  });
});
