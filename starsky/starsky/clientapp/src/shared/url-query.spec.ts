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
    expect(result).toBe("/api/info?f=/&json=true")
  });

  it("UrlQueryUpdateApi", () => {
    var result = urlQuery.UrlQueryUpdateApi();
    expect(result).toContain("update")
  });

  it("UrlQueryThumbnailJsonApi", () => {
    var result = urlQuery.UrlQueryThumbnailJsonApi("0000");
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
});