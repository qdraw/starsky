import { render, screen, waitFor } from "@testing-library/react";
import { ResetTenants } from "../hooks/use-tenants/use-tenants";
import { TenantNoAccess } from "./tenant-no-access";

const mockTenants = (tenants: { tenantId: string; displayName: string; roleCode: string }[]) =>
  jest.spyOn(globalThis, "fetch").mockResolvedValue(
    new Response(JSON.stringify({ current: "other", tenants, lastUsed: null, landing: null }), {
      status: 200
    })
  );

describe("TenantNoAccess", () => {
  beforeEach(() => ResetTenants());
  afterEach(() => {
    jest.restoreAllMocks();
    globalThis.history.pushState({}, "", "/");
  });

  it("lists the tenants of the user as links to their home page", async () => {
    globalThis.history.pushState({}, "", "/starsky/t/other/");
    mockTenants([
      { tenantId: "default", displayName: "Default", roleCode: "Administrator" },
      { tenantId: "acme", displayName: "Acme Inc", roleCode: "User" }
    ]);

    render(<TenantNoAccess />);

    const acme = await screen.findByTestId("tenant-no-access-acme");
    expect(acme.getAttribute("href")).toBe("/starsky/t/acme/");
    expect(screen.getByTestId("tenant-no-access-default").getAttribute("href")).toBe("/starsky/");
    expect(screen.queryByTestId("tenant-no-access-none")).toBeNull();
  });

  it("says so when the user is not a member of any tenant", async () => {
    mockTenants([]);

    render(<TenantNoAccess />);

    await waitFor(() => expect(screen.getByTestId("tenant-no-access-none")).toBeTruthy());
  });

  it("says so while the tenants are not known (not logged in)", () => {
    jest.spyOn(globalThis, "fetch").mockResolvedValue(new Response("false", { status: 401 }));

    render(<TenantNoAccess />);

    expect(screen.getByTestId("tenant-no-access-none")).toBeTruthy();
  });
});
