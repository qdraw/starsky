import { IFileIndexItem, newIFileIndexItemArray } from "../interfaces/IFileIndexItem";
import { URLPath } from "./url-path";

describe("url-path", () => {
  const urlPath = new URLPath();
  describe("StringToIUrl", () => {
    it("default no content", () => {
      const test = urlPath.StringToIUrl("");
      expect(test).toStrictEqual({});
    });
    it("default fallback", () => {
      const test = urlPath.StringToIUrl("?random1=tr");
      expect(test).toStrictEqual({});
    });
    it("colorClass 1 item", () => {
      const test = urlPath.StringToIUrl("?colorClass=8");
      expect(test.colorClass).toStrictEqual([8]);
    });
    it("colorClass 2 items", () => {
      const test = urlPath.StringToIUrl("?colorClass=1,2");
      expect(test.colorClass).toStrictEqual([1, 2]);
    });
    it("colorClass 2 NaN items", () => {
      const test = urlPath.StringToIUrl("?colorClass=NaN,5");
      expect(test.colorClass).toStrictEqual([5]);
    });
    it("colorClass 1 NaN items", () => {
      const test = urlPath.StringToIUrl("?colorClass=NaN");
      expect(test.colorClass).toStrictEqual([]);
    });
    it("collections false", () => {
      const test = urlPath.StringToIUrl("?collections=false");
      expect(test.collections).toStrictEqual(false);
    });
    it("collections true", () => {
      const test = urlPath.StringToIUrl("?collections=anything");
      expect(test.collections).toStrictEqual(true);
    });
    it("details false", () => {
      const test = urlPath.StringToIUrl("?details=anything");
      expect(test.details).toStrictEqual(false);
    });
    it("details true", () => {
      const test = urlPath.StringToIUrl("?details=true");
      expect(test.details).toStrictEqual(true);
    });
    it("sidebar false", () => {
      const test = urlPath.StringToIUrl("?sidebar=anything");
      expect(test.sidebar).toStrictEqual(false);
    });
    it("sidebar true", () => {
      const test = urlPath.StringToIUrl("?sidebar=true");
      expect(test.sidebar).toStrictEqual(true);
    });
    it("f", () => {
      const test = urlPath.StringToIUrl("?f=test");
      expect(test.f).toBe("test");
    });
    it("t", () => {
      const test = urlPath.StringToIUrl("?t=test");
      expect(test.t).toBe("test");
    });
    it("p null", () => {
      const test = urlPath.StringToIUrl("?p=NaN");
      expect(test.p).toBeUndefined();
    });
    it("p 15", () => {
      const test = urlPath.StringToIUrl("?p=15");
      expect(test.p).toBe(15);
    });
    it("select 2 items", () => {
      const test = urlPath.StringToIUrl("?select=test,test2");
      expect(test.select).toStrictEqual(["test", "test2"]);
    });
    it("select 1 item", () => {
      const test = urlPath.StringToIUrl("?select=test");
      expect(test.select).toStrictEqual(["test"]);
    });
    it("list default", () => {
      const test = urlPath.StringToIUrl("?list=undefined");
      expect(test.list).toStrictEqual(false);
    });
    it("list false", () => {
      const test = urlPath.StringToIUrl("?list=false");
      expect(test.list).toStrictEqual(false);
    });
    it("list true", () => {
      const test = urlPath.StringToIUrl("?list=true");
      expect(test.list).toStrictEqual(true);
    });
  });

  describe("getParent", () => {
    it("default", () => {
      const test = urlPath.getParent("");
      expect(test).toStrictEqual("/");
    });
    it("one parent", () => {
      const test = urlPath.getParent("?f=/test");
      expect(test).toStrictEqual("/");
    });
    it("two parents", () => {
      const test = urlPath.getParent("?f=/test/test");
      expect(test).toStrictEqual("/test");
    });

    it("three parents", () => {
      const test = urlPath.getParent("?f=/test/test/item.jpg");
      expect(test).toStrictEqual("/test/test");
    });
  });

  describe("getFilePath", () => {
    it("default", () => {
      const test = urlPath.getFilePath("");
      expect(test).toStrictEqual("/");
    });
    it("one", () => {
      const test = urlPath.getFilePath("?f=/test");
      expect(test).toStrictEqual("/test");
    });
  });
  describe("toggleSelection", () => {
    it("add", () => {
      const test = urlPath.toggleSelection("test.jpg", "");
      if (!test.select) throw Error("select is null");
      expect(test.select[0]).toStrictEqual("test.jpg");
    });

    it("add + remove (aka toggle)", () => {
      // two times
      const first = urlPath.toggleSelection("test.jpg", "");
      const test = urlPath.toggleSelection("test.jpg", urlPath.IUrlToString(first));
      if (test.select === undefined) throw Error("select is null");
      expect(test.select.length).toStrictEqual(0);
    });
  });
  describe("getSelect", () => {
    it("default", () => {
      const test = urlPath.getSelect("");
      expect(test).toStrictEqual([]);
    });
    it("selectResult !== undefined", () => {
      const test = urlPath.getSelect("?select=");
      expect(test).toStrictEqual([]);
    });
  });
  describe("MergeSelectParent", () => {
    it("default", () => {
      const test = urlPath.MergeSelectParent([], undefined);
      expect(test).toStrictEqual([]);
    });
    it("default (2)", () => {
      const test = urlPath.MergeSelectParent(undefined, "");
      expect(test).toStrictEqual([]);
    });

    it("merge", () => {
      const test = urlPath.MergeSelectParent(["test.jpg"], "/test");
      expect(test[0]).toStrictEqual("/test/test.jpg");
    });

    it("merge home item", () => {
      const test = urlPath.MergeSelectParent(["test.jpg"], "/");
      expect(test[0]).toStrictEqual("/test.jpg");
    });
  });
  describe("MergeSelectFileIndexItem", () => {
    it("default", () => {
      const test = urlPath.MergeSelectFileIndexItem([], newIFileIndexItemArray());
      expect(test).toStrictEqual([]);
    });

    it("item already exist", () => {
      const list = newIFileIndexItemArray();
      list.push({
        parentDirectory: "/test",
        fileName: "test.jpg"
      } as IFileIndexItem);
      const test = urlPath.MergeSelectFileIndexItem(["test.jpg"], list);
      expect(test[0]).toStrictEqual("/test/test.jpg");
      expect(test.length).toStrictEqual(1);
    });

    it("home - item already exist", () => {
      const list = newIFileIndexItemArray();
      list.push({
        parentDirectory: "/",
        fileName: "test.jpg"
      } as IFileIndexItem);
      const test = urlPath.MergeSelectFileIndexItem(["test.jpg"], list);
      expect(test[0]).toStrictEqual("/test.jpg");
      expect(test.length).toStrictEqual(1);
    });
  });

  describe("ArrayToCommaSeparatedString", () => {
    it("default", () => {
      const test = urlPath.ArrayToCommaSeparatedString([]);
      expect(test).toStrictEqual("");
    });

    it("list 1 item", () => {
      const test = urlPath.ArrayToCommaSeparatedString(["/test.jpg"]);
      expect(test).toStrictEqual("/test.jpg");
    });
    it("list 2 items", () => {
      const test = urlPath.ArrayToCommaSeparatedString(["/parent/test.jpg", "/parent/test2.jpg"]);
      expect(test).toStrictEqual("/parent/test.jpg;/parent/test2.jpg");
    });
  });

  describe("ArrayToCommaSeparatedStringOneParent", () => {
    it("default", () => {
      const test = urlPath.ArrayToCommaSeparatedStringOneParent([], "");
      expect(test).toStrictEqual("");
    });

    it("list 1 item", () => {
      const test = urlPath.ArrayToCommaSeparatedStringOneParent(["test.jpg"], "/parent");
      expect(test).toStrictEqual("/parent/test.jpg");
    });
    it("list 2 items", () => {
      const test = urlPath.ArrayToCommaSeparatedStringOneParent(
        ["test.jpg", "test2.jpg"],
        "/parent"
      );
      expect(test).toStrictEqual("/parent/test.jpg;/parent/test2.jpg");
    });
  });

  describe("ObjectToSearchParams", () => {
    it("default bool", () => {
      const bodyParams = new URLPath().ObjectToSearchParams({ append: true });
      expect(bodyParams.toString()).toBe("append=true");
    });

    it("default value", () => {
      const bodyParams = new URLPath().ObjectToSearchParams({ number: "1" });
      expect(bodyParams.toString()).toBe("number=1");
    });
  });

  describe("getChild", () => {
    it("slash", () => {
      const path = urlPath.getChild("/");
      expect(path).toBe("");
    });

    it("get image without ending slash", () => {
      const path = urlPath.getChild("/test/img");
      expect(path).toBe("img");
    });

    it("get image ending slash", () => {
      const path = urlPath.getChild("/test/img/");
      expect(path).toBe("img");
    });
  });

  describe("encodeURI", () => {
    it("default", () => {
      const encoded = new URLPath().encodeURI("€£@test");
      expect(encoded).toBe("%E2%82%AC%C2%A3@test");
    });

    it("+", () => {
      const encoded = new URLPath().encodeURI("+");
      expect(encoded).toBe("%2B");
    });
  });

  describe("StartOnSlash", () => {
    it("undefined input", () => {
      try {
        new URLPath().StartOnSlash("");
      } catch (error) {
        return;
      }
      throw new Error("should not pass");
    });

    it("/", () => {
      const encoded = new URLPath().StartOnSlash("/");
      expect(encoded).toBe("/");
    });

    it("test", () => {
      const encoded = new URLPath().StartOnSlash("test");
      expect(encoded).toBe("/test");
    });

    it("/test", () => {
      const encoded = new URLPath().StartOnSlash("/test");
      expect(encoded).toBe("/test");
    });
  });
});
