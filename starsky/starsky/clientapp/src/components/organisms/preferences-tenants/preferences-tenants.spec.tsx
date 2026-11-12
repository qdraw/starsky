import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { ResetTenants } from "../../../hooks/use-tenants/use-tenants";
import { ITenantsResponse } from "../../../interfaces/ITenants";
import PreferencesTenants from "./preferences-tenants";

const tenantsResponse = (roleInDefault: string, isPlatformAdmin = false): ITenantsResponse => ({
  isPlatformAdmin,
  current: "default",
  tenants: [
    { tenantId: "default", displayName: "Default", roleCode: roleInDefault },
    { tenantId: "acme", displayName: "Acme", roleCode: "Administrator" }
  ],
  lastUsed: null,
  landing: null
});

const members = [
  { userId: 1, name: "Owner", identifier: "owner@x.nl", roleCode: "Administrator" },
  { userId: 2, name: "Member", identifier: "user@x.nl", roleCode: "User" }
];

const json = (status: number, body: unknown) => new Response(JSON.stringify(body), { status });

const mockFetch = (roleInDefault: string, memberStatus = 200) =>
  jest.spyOn(globalThis, "fetch").mockImplementation((input, init) => {
    const url = String(input);
    const method = init?.method ?? "GET";
    if (url.includes("/api/tenants/mine")) {
      return Promise.resolve(json(200, tenantsResponse(roleInDefault)));
    }
    if (url.includes("/api/tenants/members") && method === "GET") {
      return Promise.resolve(json(200, members));
    }
    if (url.includes("/api/tenants/members")) {
      return Promise.resolve(json(memberStatus, {}));
    }
    return Promise.resolve(json(200, {}));
  });

const calls = (spy: jest.SpyInstance, method: string) =>
  spy.mock.calls.filter(([, init]) => (init?.method ?? "GET") === method);

describe("PreferencesTenants", () => {
  beforeEach(() => ResetTenants());
  afterEach(() => jest.restoreAllMocks());

  it("a user sees the tenants they are a member of, without the admin section", async () => {
    const spy = mockFetch("User");
    render(<PreferencesTenants />);

    await waitFor(() => expect(screen.getByTestId("preferences-tenant-acme")).toBeTruthy());
    expect(screen.getByTestId("preferences-tenant-default").textContent).toContain("User");
    expect(screen.getByTestId("preferences-tenant-acme").textContent).toContain("Administrator");
    expect(screen.queryByTestId("preferences-tenant-members")).toBeNull();
    expect(spy.mock.calls.some(([url]) => String(url).includes("/api/tenants/members"))).toBe(
      false
    );
  });

  it("shows that the user is no member of any tenant", async () => {
    jest
      .spyOn(globalThis, "fetch")
      .mockResolvedValue(json(200, { current: "default", tenants: [] }));
    render(<PreferencesTenants />);

    await waitFor(() => expect(screen.getByTestId("preferences-tenants-none")).toBeTruthy());
  });

  describe("administrator of the current tenant", () => {
    it("lists the members", async () => {
      mockFetch("Administrator");
      render(<PreferencesTenants />);

      await waitFor(() => expect(screen.getByTestId("preferences-tenant-member-2")).toBeTruthy());
      expect(screen.getByTestId("preferences-tenant-member-1").textContent).toContain("owner@x.nl");
    });

    it("changes the role of a member", async () => {
      const spy = mockFetch("Administrator");
      render(<PreferencesTenants />);
      const select = await screen.findByTestId("preferences-tenant-member-role-2");

      fireEvent.change(select, { target: { value: "Administrator" } });

      await waitFor(() => expect(calls(spy, "put")).toHaveLength(1));
      expect(String(calls(spy, "put")[0][0])).toContain("/api/tenants/members/2");
      expect(calls(spy, "put")[0][1].body).toBe(JSON.stringify({ role: "Administrator" }));
    });

    it("removes a member", async () => {
      const spy = mockFetch("Administrator");
      render(<PreferencesTenants />);
      const button = await screen.findByTestId("preferences-tenant-member-remove-2");

      fireEvent.click(button);

      await waitFor(() => expect(calls(spy, "delete")).toHaveLength(1));
      expect(String(calls(spy, "delete")[0][0])).toContain("/api/tenants/members/2");
    });

    it("adds an existing user and clears the field", async () => {
      const spy = mockFetch("Administrator");
      render(<PreferencesTenants />);
      const input = await screen.findByTestId("preferences-tenant-member-email");

      fireEvent.change(input, { target: { value: " new@x.nl " } });
      fireEvent.click(screen.getByTestId("preferences-tenant-member-add"));

      await waitFor(() => expect(calls(spy, "post")).toHaveLength(1));
      expect(calls(spy, "post")[0][1].body).toBe(
        JSON.stringify({ identifier: "new@x.nl", role: "User" })
      );
      await waitFor(() => expect((input as HTMLInputElement).value).toBe(""));
    });

    it.each([
      [404, "not found"],
      [409, "last administrator"],
      [500, "could not be saved"]
    ])("shows a warning on %s", async (status, text) => {
      mockFetch("Administrator", status);
      render(<PreferencesTenants />);
      const input = await screen.findByTestId("preferences-tenant-member-email");

      fireEvent.change(input, { target: { value: "new@x.nl" } });
      fireEvent.click(screen.getByTestId("preferences-tenant-member-add"));

      const warning = await screen.findByTestId("preferences-tenant-member-warning");
      expect(warning.textContent?.toLowerCase()).toContain(text);
      expect((input as HTMLInputElement).value).toBe("new@x.nl");
    });
  });

  it("tenant administrators get the tenant management, others do not", async () => {
    mockFetch("Administrator");
    render(<PreferencesTenants />);
    await waitFor(() => expect(screen.getByTestId("preferences-tenant-manage")).toBeTruthy());
    expect(screen.getByTestId("preferences-tenant-manage-readonly")).toBeTruthy();
  });

  it("users do not get the tenant management", async () => {
    mockFetch("User");
    render(<PreferencesTenants />);
    await waitFor(() => expect(screen.getByTestId("preferences-tenant-acme")).toBeTruthy());
    expect(screen.queryByTestId("preferences-tenant-manage")).toBeNull();
  });
});
