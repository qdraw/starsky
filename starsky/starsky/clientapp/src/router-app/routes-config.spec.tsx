import { matchRoutes } from "react-router-dom";
// first: the pages import the router, which needs the routes (a circular import)
import "./router-app";
import { RoutesConfig } from "./routes-config";

describe("RoutesConfig tenants", () => {
  const lastMatch = (path: string) => {
    const matches = matchRoutes(RoutesConfig, path);
    return matches?.at(-1);
  };

  it.each([
    "/starsky/t/acme/",
    "/starsky/t/acme",
    "/starsky/t/acme/search",
    "/starsky/t/acme/trash",
    "/starsky/t/acme/import",
    "/starsky/t/acme/preferences",
    "/starsky/t/acme/account/login",
    "/starsky/t/acme/account/register",
    "/t/acme/",
    "/t/acme/search"
  ])("%s is a page of the tenant", (path) => {
    const match = lastMatch(path);
    expect(match?.params.tenantId).toBe("acme");
    // not the not-found route
    expect(match?.route.path).not.toBe("*");
  });

  it("an unknown page of a tenant is not found, inside the tenant", () => {
    const match = lastMatch("/starsky/t/acme/does-not-exist");
    expect(match?.route.path).toBe("*");
    expect(match?.params.tenantId).toBe("acme");
  });

  it("the default tenant routes are unchanged", () => {
    expect(lastMatch("/")?.route.path).toBe("/");
    expect(lastMatch("/starsky/")?.route.path).toBe("starsky/");
    expect(lastMatch("/starsky/search")?.route.path).toBe("starsky/search");
    expect(lastMatch("/search")?.route.path).toBe("search");
    expect(lastMatch("/starsky/preferences")?.route.path).toBe("starsky/preferences");
  });
});
