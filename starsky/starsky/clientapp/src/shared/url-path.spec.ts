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
  describe("GetReturnUrl", () => {
    it("default", () => {
      var test = urlPath.GetReturnUrl("?");
      expect(test).toStrictEqual("?f=/")
    });
    it("url", () => {
      var test = urlPath.GetReturnUrl("ReturnUrl=test");
      expect(test).toStrictEqual("test")
    });
  });
  describe("getParent", () => {
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
});