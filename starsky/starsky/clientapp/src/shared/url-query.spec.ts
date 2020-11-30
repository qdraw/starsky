import * as isDev from './is-dev';
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

  it("UrlPublish", () => {
    var result = urlQuery.UrlPublish();
    expect(result).toContain("publish")
  });
  it("UrlPublishCreate", () => {
    var result = urlQuery.UrlPublishCreate();
    expect(result).toContain("publish/create")
  });

  it("UrlPublishExist", () => {
    var result = urlQuery.UrlPublishExist('name');
    expect(result).toContain("itemName=name")
  });

  it("UrlSearchTrashApi should contain trash", () => {
    var result = urlQuery.UrlSearchTrashApi();
    expect(result).toContain("trash")
  });
  it("UrlQuerySearchApi should contain test", () => {
    var result = urlQuery.UrlQuerySearchApi("test");
    expect(result).toContain("test")
  });

  it("UrlLoginApi", () => {
    var result = urlQuery.UrlLoginApi();
    expect(result).toContain("login")
  });

  it("UrlLogoutApi", () => {
    var result = urlQuery.UrlLogoutApi();
    expect(result).toContain("logout")
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

  describe("updateFilePath", () => {
    it("default", () => {
      var test = urlQuery.updateFilePathHash("?f=test", "test1");
      expect(test).toStrictEqual("/?f=test1")
    });

    it("contains colorclass", () => {
      var test = urlQuery.updateFilePathHash("?f=test&colorclass=1", "test1");
      expect(test).toStrictEqual("/?f=test1&colorClass=1")
    });

    it("remove search query", () => {
      var test = urlQuery.updateFilePathHash("?f=test&colorclass=1&t=1", "test1", true);
      expect(test).toStrictEqual("/?f=test1&colorClass=1")
    });

    it("keep search query", () => {
      var test = urlQuery.updateFilePathHash("?f=test&colorclass=1&t=1", "test1");
      expect(test).toStrictEqual("/?f=test1&colorClass=1&t=1")
    });

    it("keep search query and remove select", () => {
      var test = urlQuery.updateFilePathHash("?f=test&colorclass=1&t=1&select=t5", "test1", false, true);
      expect(test).toStrictEqual("/?f=test1&colorClass=1&t=1&select=");
    });

  });

  describe("UrlGeoSync", () => {
    it("should contain geo sync", () => {
      var test = urlQuery.UrlGeoSync();
      expect(test).toContain("/geo/sync")
    });
  });

  describe("UrlGeoStatus", () => {
    it("should contain status and parm", () => {
      var test = urlQuery.UrlGeoStatus("parm");
      expect(test).toContain("/geo/status");
      expect(test).toContain("parm")
    });
  });

  describe("UrlRealtime", () => {
    const { location } = window;
    /**
     * Mock the location feature
     * @see: https://wildwolf.name/jest-how-to-mock-window-location-href/
     */
    beforeAll(() => {
      // @ts-ignore
      delete window.location;
      // @ts-ignore
      window.location = {
        href: '',
      };
    });

    afterAll((): void => {
      window.location = location;
    });

    it("default secure context", () => {

      window.location.protocol = "https:";
      window.location.host = "google.com";
      var url = urlQuery.UrlRealtime();
      expect(url).toBe("wss://google.com/starsky/realtime");
      expect(url).toContain("realtime")

    });

    it("default non-secure context", () => {
      window.location.protocol = "http:";
      window.location.host = "localhost:7382";
      var url = new UrlQuery().UrlRealtime();
      expect(url).toBe("ws://localhost:7382/starsky/realtime");
      expect(url).toContain("realtime")

    });

    it("when not in Dev ignore port 3000 replacement", () => {

      jest.spyOn(isDev, 'default').mockImplementationOnce(() => false);

      window.location.protocol = "http:";
      window.location.host = "localhost:3000";
      var url = new UrlQuery().UrlRealtime();
      expect(url).toBe("ws://localhost:3000/starsky/realtime");
      expect(url).toContain("realtime")

    });
    it("replace port 3000 with 5000 in dev context", () => {
      jest.spyOn(isDev, 'default').mockImplementationOnce(() => true);

      window.location.protocol = "http:";
      window.location.host = "localhost:3000";
      var url = new UrlQuery().UrlRealtime();
      expect(url).toBe("ws://localhost:5000/starsky/realtime");
      expect(url).toContain("realtime")
    });

  });
});