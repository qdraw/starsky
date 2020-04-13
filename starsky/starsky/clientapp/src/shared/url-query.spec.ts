import { UrlQuery } from './url-query';

describe("url-query", () => {
  var urlQuery = new UrlQuery();

  it("UrlQuerySearchApi", () => {
    var result = urlQuery.UrlQuerySearchApi("for", 1);
    expect(result).toContain("for")
    expect(result).toContain(1)
  });

  it("UrlSearchTrashApi", () => {
    var result = urlQuery.UrlSearchTrashApi(1);
    expect(result).toContain(1)
  });

  it("UrlQueryServerApi", () => {
    var result = urlQuery.UrlQueryServerApi("?f=test&colorClass=1&collections=false&details=true");
    expect(result).toContain(1)
    expect(result).toContain("false")
    expect(result).toContain("test")
  });

  it("UrlIndexServerApi", () => {
    var result = urlQuery.UrlIndexServerApi({ f: "/test" });
    expect(result).toContain("test")
    expect(result).toBe(urlQuery.prefix + "/api/index?f=/test")
  });

  it("UrlIndexServerApi nothing", () => {
    var result = urlQuery.UrlIndexServerApi({});
    expect(result).toBe(urlQuery.prefix + "/api/index?")
  });

  it("UrlQueryInfoApi", () => {
    var result = urlQuery.UrlQueryInfoApi("/test");
    expect(result).toContain("/test")
  });

  it("UrlQueryInfoApi null", () => {
    var result = urlQuery.UrlQueryInfoApi("");
    expect(result).toBe("")
  });

  it("UrlQueryInfoApi slash", () => {
    var result = urlQuery.UrlQueryInfoApi("/");
    expect(result).toBe(urlQuery.prefix + "/api/info?f=/&json=true")
  });

  it("UrlQueryUpdateApi", () => {
    var result = urlQuery.UrlUpdateApi();
    expect(result).toContain("update")
  });

  it("UrlQueryThumbnailJsonApi", () => {
    var result = urlQuery.UrlThumbnailJsonApi("0000");
    expect(result).toContain("0000")
  });

  it("UrlDownloadPhotoApi", () => {
    var result = urlQuery.UrlDownloadPhotoApi("0000");
    expect(result).toContain("0000")
  });

  it("UrlExportPostZipApi", () => {
    var result = urlQuery.UrlExportPostZipApi();
    expect(result).toContain("export")
  });

  it("UrlExportZipApi", () => {
    var result = urlQuery.UrlExportZipApi("123");
    expect(result).toContain("123")
  });

  it("UrlDeleteApi", () => {
    var result = urlQuery.UrlDeleteApi();
    expect(result).toContain("delete")
  });

  it("UrlSearchTrashApi", () => {
    var result = urlQuery.UrlSearchTrashApi();
    expect(result).toContain("trash")
  });
  it("UrlQuerySearchApi", () => {
    var result = urlQuery.UrlQuerySearchApi("test");
    expect(result).toContain("test")
  });

  describe("GetReturnUrl", () => {
    it("default", () => {
      var test = urlQuery.GetReturnUrl("?");
      expect(test).toStrictEqual("/?f=/")
    });
    it("url", () => {
      var test = urlQuery.GetReturnUrl("ReturnUrl=test");
      expect(test).toStrictEqual("test")
    });
  });


});