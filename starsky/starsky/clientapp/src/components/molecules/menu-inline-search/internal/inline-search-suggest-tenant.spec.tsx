import { render, screen, waitFor } from "@testing-library/react";
import { ResetTenants } from "../../../../hooks/use-tenants/use-tenants";
import { IConnectionDefault } from "../../../../interfaces/IConnectionDefault";
import InlineSearchSuggest, { IInlineSearchSuggestProps } from "./inline-search-suggest";

const props: IInlineSearchSuggestProps = {
  suggest: [],
  setFormFocus: jest.fn(),
  inputFormControlReference: { current: null },
  featuresResult: {} as IConnectionDefault
};

const mockTenants = (tenantIds: string[], current = "default") =>
  jest.spyOn(globalThis, "fetch").mockResolvedValue(
    new Response(
      JSON.stringify({
        current,
        tenants: tenantIds.map((tenantId) => ({
          tenantId,
          displayName: tenantId.toUpperCase(),
          roleCode: "User"
        })),
        lastUsed: null,
        landing: null
      }),
      { status: 200 }
    )
  );

describe("inline-search-suggest tenant switcher", () => {
  beforeEach(() => ResetTenants());
  afterEach(() => {
    jest.restoreAllMocks();
    globalThis.history.pushState({}, "", "/");
  });

  it("offers the other tenants of a user with more than one", async () => {
    globalThis.history.pushState({}, "", "/starsky/");
    mockTenants(["default", "acme", "beta"]);

    render(<InlineSearchSuggest {...props} />);

    const acme = await screen.findByTestId("default-menu-item-tenant-acme");
    expect(acme.querySelector("a")?.getAttribute("href")).toBe("/starsky/t/acme/");
    expect(screen.getByTestId("default-menu-item-tenant-beta")).toBeTruthy();
    // not the tenant that is open
    expect(screen.queryByTestId("default-menu-item-tenant-default")).toBeNull();
  });

  it("from a tenant to the default tenant", async () => {
    globalThis.history.pushState({}, "", "/starsky/t/acme/");
    mockTenants(["default", "acme"], "acme");

    render(<InlineSearchSuggest {...props} />);

    const item = await screen.findByTestId("default-menu-item-tenant-default");
    expect(item.querySelector("a")?.getAttribute("href")).toBe("/starsky/");
    expect(screen.queryByTestId("default-menu-item-tenant-acme")).toBeNull();
  });

  it("shows no switcher for a user with one tenant (desktop)", async () => {
    const spy = mockTenants(["default"]);

    render(<InlineSearchSuggest {...props} />);

    await waitFor(() => expect(spy).toHaveBeenCalled());
    await waitFor(() => expect(screen.getByText("Home")).toBeTruthy());
    expect(screen.queryByTestId("default-menu-item-tenant-default")).toBeNull();
    expect(document.querySelectorAll('[data-test^="default-menu-item-tenant-"]')).toHaveLength(0);
  });

  it("shows no switcher when not logged in", async () => {
    const spy = jest
      .spyOn(globalThis, "fetch")
      .mockResolvedValue(new Response("false", { status: 401 }));

    render(<InlineSearchSuggest {...props} />);

    await waitFor(() => expect(spy).toHaveBeenCalled());
    expect(document.querySelectorAll('[data-test^="default-menu-item-tenant-"]')).toHaveLength(0);
  });

  it("no switcher while the user searches (suggestions are shown)", async () => {
    mockTenants(["default", "acme"]);

    render(<InlineSearchSuggest {...props} suggest={["query1"]} />);

    await waitFor(() => expect(screen.getByText("query1")).toBeTruthy());
    expect(document.querySelectorAll('[data-test^="default-menu-item-tenant-"]')).toHaveLength(0);
  });
});
