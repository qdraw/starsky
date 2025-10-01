import { UrlQuery } from "./url-query";

/**
 * UrlQueryTest
 */
describe("url-query", () => {
  const urlQuery = new UrlQuery();

  it("UrlQuerySearchApi", () => {
    const result = urlQuery.UrlQuerySearchApi("for", 1);
    expect(result).toContain("for");
    expect(result).toContain("1");
  });

  it("UrlSearchTrashApi", () => {
    const result = urlQuery.UrlSearchTrashApi(1);
    expect(result).toContain("1");
  });

  it("UrlQueryServerApi f/colorclass/collections", () => {
    const result = urlQuery.UrlQueryServerApi(
      "?f=test&colorClass=1&collections=false&details=true"
    );
    expect(result).toContain("1");
    expect(result).toContain("false");
    expect(result).toContain("test");
  });

  it("UrlQueryServerApi sort", () => {
    const result = urlQuery.UrlQueryServerApi("?sort=fileName");
    expect(result).toContain("sort");
    expect(result).toContain("fileName");
  });

  it("UrlIndexServerApi", () => {
    const result = urlQuery.UrlIndexServerApi({ f: "/test" });
    expect(result).toContain("test");
    expect(result).toBe(urlQuery.prefix + "/api/index?f=/test");
  });

  it("UrlIndexServerApi nothing", () => {
    const result = urlQuery.UrlIndexServerApi({});
    expect(result).toBe(urlQuery.prefix + "/api/index?");
  });

  it("UrlQueryInfoApi", () => {
    const result = urlQuery.UrlQueryInfoApi("/test");
    expect(result).toContain("/test");
  });

  it("UrlQueryInfoApi null", () => {
    const result = urlQuery.UrlQueryInfoApi("");
    expect(result).toBe("");
  });

  it("UrlQueryInfoApi slash", () => {
    const result = urlQuery.UrlQueryInfoApi("/");
    expect(result).toBe(urlQuery.prefix + "/api/info?f=/&json=true");
  });

  it("UrlQueryUpdateApi", () => {
    const result = urlQuery.UrlUpdateApi();
    expect(result).toContain("update");
  });

  it("UrlQueryThumbnailJsonApi", () => {
    const result = urlQuery.UrlThumbnailJsonApi("0000");
    expect(result).toContain("0000");
  });

  it("UrlDownloadPhotoApi", () => {
    const result = urlQuery.UrlDownloadPhotoApi("0000");
    expect(result).toContain("0000");
  });

  it("UrlExportPostZipApi", () => {
    const result = urlQuery.UrlExportPostZipApi();
    expect(result).toContain("export");
  });

  it("UrlExportZipApi", () => {
    const result = urlQuery.UrlExportZipApi("123");
    expect(result).toContain("123");
  });

  it("UrlDeleteApi", () => {
    const result = urlQuery.UrlDeleteApi();
    expect(result).toContain("delete");
  });

  it("UrlPublish", () => {
    const result = urlQuery.UrlPublish();
    expect(result).toContain("publish");
  });
  it("UrlPublishCreate", () => {
    const result = urlQuery.UrlPublishCreate();
    expect(result).toContain("publish/create");
  });

  it("UrlPublishExist", () => {
    const result = urlQuery.UrlPublishExist("name");
    expect(result).toContain("itemName=name");
  });

  it("UrlHomePage", () => {
    const result = new UrlQuery().UrlHomePage();
    expect(result).toBe("/");
  });

  it("UrlHomeIndexPage", () => {
    const result = urlQuery.UrlHomeIndexPage("name");
    expect(result).toBe("/name");
  });

  it("UrlHomeIndexPage http link", () => {
    const result = urlQuery.UrlHomeIndexPage("https://google.com");
    expect(result).toBe("/");
  });

  it("UrlSearchTrashApi should contain trash", () => {
    const result = urlQuery.UrlSearchTrashApi();
    expect(result).toContain("trash");
  });
  it("UrlQuerySearchApi should contain test", () => {
    const result = urlQuery.UrlQuerySearchApi("test");
    expect(result).toContain("test");
  });

  it("UrlLoginApi", () => {
    const result = urlQuery.UrlLoginApi();
    expect(result).toContain("login");
  });

  it("UrlLogoutApi", () => {
    const result = urlQuery.UrlLogoutApi();
    expect(result).toContain("logout");
  });

  it("UrlLogoutPage ", () => {
    const result = urlQuery.UrlLogoutPage("test");
    expect(result).toContain("/account/logout?ReturnUrl=test");
  });

  it("UrlLogoutPage 2", () => {
    const result = urlQuery.UrlLogoutPage("https://google.com");
    expect(result).toContain("/account/logout?ReturnUrl=/?f=/");
  });

  describe("GetReturnUrl", () => {
    it("default", () => {
      const test = urlQuery.GetReturnUrl("?");
      expect(test).toStrictEqual("/?f=/");
    });
    it("url", () => {
      const test = urlQuery.GetReturnUrl("ReturnUrl=test");
      expect(test).toStrictEqual("test");
    });

    it("UrlHealthReleaseInfo default", () => {
      const result = urlQuery.UrlHealthReleaseInfo(null!);
      expect(result).toBe(urlQuery.prefix + "/api/health/release-info");
    });

    it("UrlHealthReleaseInfo version", () => {
      const result = urlQuery.UrlHealthReleaseInfo("0.6.0");
      expect(result).toBe(urlQuery.prefix + "/api/health/release-info?v=0.6.0");
    });
  });

  describe("updateFilePath", () => {
    it("default", () => {
      const test = urlQuery.updateFilePathHash("?f=test", "test1");
      expect(test).toStrictEqual("/?f=test1");
    });

    it("contains colorclass", () => {
      const test = urlQuery.updateFilePathHash("?f=test&colorclass=1", "test1");
      expect(test).toStrictEqual("/?f=test1&colorClass=1");
    });

    it("remove search query", () => {
      const test = urlQuery.updateFilePathHash("?f=test&colorclass=1&t=1", "test1", true);
      expect(test).toStrictEqual("/?f=test1&colorClass=1");
    });

    it("keep search query", () => {
      const test = urlQuery.updateFilePathHash("?f=test&colorclass=1&t=1", "test1");
      expect(test).toStrictEqual("/?f=test1&colorClass=1&t=1");
    });

    it("keep search query and remove select", () => {
      const test = urlQuery.updateFilePathHash(
        "?f=test&colorclass=1&t=1&select=t5",
        "test1",
        false,
        true
      );
      expect(test).toStrictEqual("/?f=test1&colorClass=1&t=1&select=");
    });
  });

  describe("UrlGeoSync", () => {
    it("should contain geo sync", () => {
      const test = urlQuery.UrlGeoSync();
      expect(test).toContain("/geo/sync");
    });
  });

  describe("UrlGeoStatus", () => {
    it("should contain status and parm", () => {
      const test = urlQuery.UrlGeoStatus("parm");
      expect(test).toContain("/geo/status");
      expect(test).toContain("parm");
    });
  });

  describe("UrlThumbnailImage", () => {
    it("should contain hash_test (issingleitem false)", () => {
      const test = urlQuery.UrlThumbnailImage("hash_test", false);
      expect(test).toContain("hash_test");
    });

    it("should contain hash_test (issingleitem true)", () => {
      const test = urlQuery.UrlThumbnailImage("hash_test", true);
      expect(test).toContain("hash_test");
    });
  });

  describe("UrlIndexServerApiPath", () => {
    it("returns the correct URL", () => {
      const urlQuery = new UrlQuery();
      urlQuery.prefix = "https://example.com";
      const path = "example/path";
      const expectedUrl = "https://example.com/api/index?f=example/path";
      const result = urlQuery.UrlIndexServerApiPath(path);
      expect(result).toBe(expectedUrl);
    });
  });

  describe("UrlThumbnailImageLargeOrExtraLarge", () => {
    it("should contain hash_test (large false)", () => {
      const test = urlQuery.UrlThumbnailImageLargeOrExtraLarge("hash_test", "filePath", false);
      expect(test).toContain("hash_test");
    });

    it("should contain hash_test (large true)", () => {
      const test = urlQuery.UrlThumbnailImageLargeOrExtraLarge("hash_test", "filePath", true);
      expect(test).toContain("hash_test");
    });
  });

  it("DocsGettingStartedFirstSteps", () => {
    const test = urlQuery.DocsGettingStartedFirstSteps();
    expect(test).toContain("docs/getting-started/first-steps");
  });

  describe("UrlRealtime", () => {
    const { location } = window;
    /**
     * Mock the location feature
     * @see: https://wildwolf.name/jest-how-to-mock-window-location-href/
     */
    beforeAll(() => {
      // eslint-disable-next-line @typescript-eslint/ban-ts-comment
      // @ts-ignore
      delete globalThis.location;
      // eslint-disable-next-line @typescript-eslint/ban-ts-comment
      // @ts-ignore
      globalThis.location = {
        href: ""
      };
    });

    afterAll((): void => {
      globalThis.location = location;
    });

    it("default secure context", () => {
      globalThis.location.protocol = "https:";
      globalThis.location.host = "google.com";
      const url = urlQuery.UrlRealtime();
      expect(url).toBe("wss://google.com/starsky/realtime");
      expect(url).toContain("realtime");
    });

    it("default non-secure context", () => {
      globalThis.location.protocol = "http:";
      globalThis.location.host = "localhost:7382";
      const url = new UrlQuery().UrlRealtime();
      expect(url).toBe("ws://localhost:7382/starsky/realtime");
      expect(url).toContain("realtime");
    });
  });
});
