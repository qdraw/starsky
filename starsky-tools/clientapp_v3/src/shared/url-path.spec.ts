import { IFileIndexItem, newIFileIndexItemArray } from '../interfaces/IFileIndexItem';
import { URLPath } from './url-path';

describe("url-path", () => {
  var urlPath = new URLPath();
  describe("StringToIUrl", () => {
    it("default no content", () => {
      var test = urlPath.StringToIUrl("")
      expect(test).toStrictEqual({})
    });
    it("default fallback", () => {
      var test = urlPath.StringToIUrl("?random1=tr")
      expect(test).toStrictEqual({})
    });
    it("colorClass 1 item", () => {
      var test = urlPath.StringToIUrl("?colorClass=8")
      expect(test.colorClass).toStrictEqual([8])
    });
    it("colorClass 2 items", () => {
      var test = urlPath.StringToIUrl("?colorClass=1,2")
      expect(test.colorClass).toStrictEqual([1, 2])
    });
    it("colorClass 2 NaN items", () => {
      var test = urlPath.StringToIUrl("?colorClass=NaN,5")
      expect(test.colorClass).toStrictEqual([5])
    });
    it("colorClass 1 NaN items", () => {
      var test = urlPath.StringToIUrl("?colorClass=NaN")
      expect(test.colorClass).toStrictEqual([])
    });
    it("collections false", () => {
      var test = urlPath.StringToIUrl("?collections=false")
      expect(test.collections).toStrictEqual(false)
    });
    it("collections true", () => {
      var test = urlPath.StringToIUrl("?collections=anything")
      expect(test.collections).toStrictEqual(true)
    });
    it("details false", () => {
      var test = urlPath.StringToIUrl("?details=anything")
      expect(test.details).toStrictEqual(false)
    });
    it("details true", () => {
      var test = urlPath.StringToIUrl("?details=true")
      expect(test.details).toStrictEqual(true)
    });
    it("sidebar false", () => {
      var test = urlPath.StringToIUrl("?sidebar=anything")
      expect(test.sidebar).toStrictEqual(false)
    });
    it("sidebar true", () => {
      var test = urlPath.StringToIUrl("?sidebar=true")
      expect(test.sidebar).toStrictEqual(true)
    });
    it("f", () => {
      var test = urlPath.StringToIUrl("?f=test")
      expect(test.f).toBe("test")
    });
    it("t", () => {
      var test = urlPath.StringToIUrl("?t=test")
      expect(test.t).toBe("test")
    });
    it("p null", () => {
      var test = urlPath.StringToIUrl("?p=NaN")
      expect(test.p).toBeUndefined()
    });
    it("p 15", () => {
      var test = urlPath.StringToIUrl("?p=15")
      expect(test.p).toBe(15)
    });
    it("select 2 items", () => {
      var test = urlPath.StringToIUrl("?select=test,test2")
      expect(test.select).toStrictEqual(["test", "test2"])
    });
    it("select 1 item", () => {
      var test = urlPath.StringToIUrl("?select=test")
      expect(test.select).toStrictEqual(["test"])
    });
  });

  describe("getParent", () => {
    it("default", () => {
      var test = urlPath.getParent("");
      expect(test).toStrictEqual("/")
    });
    it("one parent", () => {
      var test = urlPath.getParent("?f=/test");
      expect(test).toStrictEqual("/")
    });
    it("two parents", () => {
      var test = urlPath.getParent("?f=/test/test");
      expect(test).toStrictEqual("/test")
    });

    it("three parents", () => {
      var test = urlPath.getParent("?f=/test/test/item.jpg");
      expect(test).toStrictEqual("/test/test")
    });
  });


  describe("getFilePath", () => {
    it("default", () => {
      var test = urlPath.getFilePath("");
      expect(test).toStrictEqual("/")
    });
    it("one", () => {
      var test = urlPath.getFilePath("?f=/test");
      expect(test).toStrictEqual("/test")
    });
  });
  describe("toggleSelection", () => {
    it("add", () => {
      var test = urlPath.toggleSelection("test.jpg", "");
      if (!test.select) throw Error('select is null');
      expect(test.select[0]).toStrictEqual("test.jpg")
    });

    it("add + remove (aka toggle)", () => {
      // two times
      var first = urlPath.toggleSelection("test.jpg", "");
      var test = urlPath.toggleSelection("test.jpg", urlPath.IUrlToString(first));
      if (test.select === undefined) throw Error('select is null');
      expect(test.select.length).toStrictEqual(0)
    });
  });
  describe("getSelect", () => {
    it("default", () => {
      var test = urlPath.getSelect("");
      expect(test).toStrictEqual([])
    });
    it("selectResult !== undefined", () => {
      var test = urlPath.getSelect("?select=");
      expect(test).toStrictEqual([])
    });
  });
  describe("MergeSelectParent", () => {
    it("default", () => {
      var test = urlPath.MergeSelectParent([], undefined);
      expect(test).toStrictEqual([])
    });
    it("default (2)", () => {
      var test = urlPath.MergeSelectParent(undefined, "");
      expect(test).toStrictEqual([])
    });

    it("merge", () => {
      var test = urlPath.MergeSelectParent(["test.jpg"], "/test");
      expect(test[0]).toStrictEqual("/test/test.jpg")
    });

    it("merge home item", () => {
      var test = urlPath.MergeSelectParent(["test.jpg"], "/");
      expect(test[0]).toStrictEqual("/test.jpg")
    });
  });
  describe("MergeSelectFileIndexItem", () => {
    it("default", () => {
      var test = urlPath.MergeSelectFileIndexItem([], newIFileIndexItemArray());
      expect(test).toStrictEqual([])
    });

    it("item already exist", () => {
      var list = newIFileIndexItemArray();
      list.push({ parentDirectory: '/test', fileName: 'test.jpg' } as IFileIndexItem)
      var test = urlPath.MergeSelectFileIndexItem(["test.jpg"], list);
      expect(test[0]).toStrictEqual("/test/test.jpg")
      expect(test.length).toStrictEqual(1)
    });

    it("home - item already exist", () => {
      var list = newIFileIndexItemArray();
      list.push({ parentDirectory: '/', fileName: 'test.jpg' } as IFileIndexItem)
      var test = urlPath.MergeSelectFileIndexItem(["test.jpg"], list);
      expect(test[0]).toStrictEqual("/test.jpg")
      expect(test.length).toStrictEqual(1)
    });
  });

  describe("ArrayToCommaSeperatedString", () => {

    it("default", () => {
      var test = urlPath.ArrayToCommaSeperatedString([]);
      expect(test).toStrictEqual("")
    });

    it("list 1 item", () => {
      var test = urlPath.ArrayToCommaSeperatedString(["/test.jpg"]);
      expect(test).toStrictEqual("/test.jpg")
    });
    it("list 2 items", () => {
      var test = urlPath.ArrayToCommaSeperatedString(["/parent/test.jpg", "/parent/test2.jpg"]);
      expect(test).toStrictEqual("/parent/test.jpg;/parent/test2.jpg")
    });
  });

  describe("ArrayToCommaSeperatedStringOneParent", () => {

    it("default", () => {
      var test = urlPath.ArrayToCommaSeperatedStringOneParent([], "");
      expect(test).toStrictEqual("")
    });

    it("list 1 item", () => {
      var test = urlPath.ArrayToCommaSeperatedStringOneParent(["test.jpg"], "/parent");
      expect(test).toStrictEqual("/parent/test.jpg")
    });
    it("list 2 items", () => {
      var test = urlPath.ArrayToCommaSeperatedStringOneParent(["test.jpg", "test2.jpg"], "/parent");
      expect(test).toStrictEqual("/parent/test.jpg;/parent/test2.jpg")
    });
  });

  describe("ObjectToSearchParams", () => {
    it("default bool", () => {
      var bodyParams = new URLPath().ObjectToSearchParams({ append: true });
      expect(bodyParams.toString()).toBe("append=true")
    });

    it("default value", () => {
      var bodyParams = new URLPath().ObjectToSearchParams({ number: "1" });
      expect(bodyParams.toString()).toBe("number=1")
    });
  });

  describe("getChild", () => {
    it("slash", () => {
      var path = urlPath.getChild("/");
      expect(path).toBe("")
    });

    it("get image without ending slash", () => {
      var path = urlPath.getChild("/test/img");
      expect(path).toBe("img")
    });

    it("get image ending slash", () => {
      var path = urlPath.getChild("/test/img/");
      expect(path).toBe("img")
    });
  });

  describe("encodeURI", () => {
    it("default", () => {
      var encoded = new URLPath().encodeURI("€£@test")
      expect(encoded).toBe("%E2%82%AC%C2%A3@test")
    });

    it("+", () => {
      var encoded = new URLPath().encodeURI("+")
      expect(encoded).toBe("%2B")
    });
  });

});