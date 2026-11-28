import { TenantBasePath, TenantFromLocation, UrlQuery } from "./url-query";

const goTo = (path: string) => globalThis.history.pushState({}, "", path);

describe("url-query tenant", () => {
  afterEach(() => goTo("/"));

  describe("TenantFromLocation", () => {
    it.each([
      ["/starsky/t/acme/", "acme"],
      ["/starsky/t/acme", "acme"],
      ["/starsky/t/acme-2/search", "acme-2"],
      ["/t/acme/search", "acme"],
      ["/starsky/", "default"],
      ["/", "default"],
      ["/starsky/search", "default"],
      // not a tenant: only a valid slug directly after /t/
      ["/starsky/t/Acme/", "default"],
      ["/starsky/t/../x", "default"],
      ["/starsky/t/", "default"],
      ["/starsky/tx/acme/", "default"],
      ["/starsky/account/t/acme", "default"]
    ])("%s is %s", (pathname, expected) => {
      expect(TenantFromLocation(pathname)).toBe(expected);
    });
  });

  it("TenantBasePath", () => {
    expect(TenantBasePath("default")).toBe("/starsky");
    expect(TenantBasePath("acme")).toBe("/starsky/t/acme");
  });

  describe("default tenant keeps the existing urls", () => {
    it("api prefix and pages", () => {
      goTo("/starsky/search?t=a");
      const urlQuery = new UrlQuery();
      expect(urlQuery.prefix).toBe("/starsky");
      expect(urlQuery.UrlHomePage()).toBe("/starsky/");
      expect(urlQuery.UrlSearchPage("x")).toBe("/starsky/search?t=x");
      expect(urlQuery.UrlTrashPage()).toBe("/starsky/trash?t=!delete!");
      expect(urlQuery.UrlLoginPage()).toBe("/starsky/account/login");
      expect(urlQuery.GetReturnUrl("")).toBe("/?f=/");
    });

    it("dev server without base path", () => {
      goTo("/search?t=a");
      const urlQuery = new UrlQuery();
      expect(urlQuery.UrlHomePage()).toBe("/");
      expect(urlQuery.UrlSearchPage("x")).toBe("/search?t=x");
    });
  });

  describe("a tenant in the url", () => {
    beforeEach(() => goTo("/starsky/t/acme/search?t=a"));

    it("the api is under the tenant", () => {
      const urlQuery = new UrlQuery();
      expect(urlQuery.prefix).toBe("/starsky/t/acme");
      expect(urlQuery.UrlIndexServerApi({ f: "/test" })).toBe("/starsky/t/acme/api/index?f=/test");
      expect(urlQuery.UrlLoginApi()).toBe("/starsky/t/acme/api/account/login");
      expect(urlQuery.UrlTenantsMine()).toBe("/starsky/t/acme/api/tenants/mine");
      expect(urlQuery.UrlTenantsLastUsed()).toBe("/starsky/t/acme/api/tenants/last-used");
    });

    it("pages and links stay in the tenant", () => {
      const urlQuery = new UrlQuery();
      expect(urlQuery.UrlHomePage()).toBe("/starsky/t/acme/");
      expect(urlQuery.UrlHomeIndexPage("/2020")).toBe("/starsky/t/acme/2020");
      expect(urlQuery.UrlSearchPage("x")).toBe("/starsky/t/acme/search?t=x");
      expect(urlQuery.UrlTrashPage()).toBe("/starsky/t/acme/trash?t=!delete!");
      expect(urlQuery.UrlImportPage()).toBe("/starsky/t/acme/import");
      expect(urlQuery.UrlPreferencesPage()).toBe("/starsky/t/acme/preferences");
      expect(urlQuery.UrlPreferencesPage("x")).toBe("/starsky/t/acme/preferences/?tab=x");
      expect(urlQuery.UrlLoginPage()).toBe("/starsky/t/acme/account/login");
      expect(urlQuery.UrlLogoutPage("/?f=/")).toBe(
        "/starsky/t/acme/account/logout?ReturnUrl=/?f=/"
      );
    });

    it("the realtime socket is of the tenant", () => {
      expect(new UrlQuery().UrlRealtime()).toContain("/starsky/t/acme/realtime");
    });

    it("a login without target stays in the tenant", () => {
      expect(new UrlQuery().GetReturnUrl("")).toBe("/starsky/t/acme/?f=/");
    });

    it("a ReturnUrl is used as it is, an off-site one is refused", () => {
      expect(new UrlQuery().GetReturnUrl("?ReturnUrl=%2Fstarsky%2Ft%2Facme%2Ftrash")).toBe(
        "/starsky/t/acme/trash"
      );
      expect(new UrlQuery().GetReturnUrl("?ReturnUrl=https%3A%2F%2Fevil.example")).toBe(
        "/starsky/t/acme/?f=/"
      );
    });
  });

  describe("a tenant on the dev server (no base path)", () => {
    beforeEach(() => goTo("/t/acme/search?t=a"));

    it("pages have the tenant, the api the full prefix", () => {
      const urlQuery = new UrlQuery();
      expect(urlQuery.UrlHomePage()).toBe("/t/acme/");
      expect(urlQuery.UrlSearchPage("x")).toBe("/t/acme/search?t=x");
      expect(urlQuery.prefix).toBe("/starsky/t/acme");
    });
  });

  describe("UrlTenantHomePage", () => {
    it("on the server (base path)", () => {
      goTo("/starsky/search");
      expect(new UrlQuery().UrlTenantHomePage("default")).toBe("/starsky/");
      expect(new UrlQuery().UrlTenantHomePage("acme")).toBe("/starsky/t/acme/");
    });

    it("on the dev server", () => {
      goTo("/search");
      expect(new UrlQuery().UrlTenantHomePage("default")).toBe("/");
      expect(new UrlQuery().UrlTenantHomePage("acme")).toBe("/t/acme/");
    });
  });

  describe("IsDefaultReturnUrl", () => {
    it("the logout link and no ReturnUrl are not a real target", () => {
      goTo("/starsky/");
      expect(new UrlQuery().IsDefaultReturnUrl("/?f=/")).toBe(true);
      expect(new UrlQuery().IsDefaultReturnUrl(new UrlQuery().GetReturnUrl(""))).toBe(true);
      expect(
        new UrlQuery().IsDefaultReturnUrl(new UrlQuery().GetReturnUrl("?ReturnUrl=%2F%3Ff%3D%2F"))
      ).toBe(true);
    });

    it("the default of a tenant", () => {
      goTo("/starsky/t/acme/");
      expect(new UrlQuery().IsDefaultReturnUrl("/starsky/t/acme/?f=/")).toBe(true);
    });

    it("a page the user came from is a real target", () => {
      goTo("/starsky/");
      expect(new UrlQuery().IsDefaultReturnUrl("/starsky/trash")).toBe(false);
      expect(new UrlQuery().IsDefaultReturnUrl("/?f=/2020")).toBe(false);
    });
  });
});
