import { act, renderHook, waitFor } from "@testing-library/react";
import { ITenantsResponse } from "../../interfaces/ITenants";
import { FetchLandingTenant, ResetTenants, useTenantLastUsed, useTenants } from "./use-tenants";

const mine = (overrides: Partial<ITenantsResponse> = {}): ITenantsResponse => ({
  current: "default",
  tenants: [
    { tenantId: "default", displayName: "Default", roleCode: "Administrator" },
    { tenantId: "acme", displayName: "Acme", roleCode: "User" }
  ],
  lastUsed: null,
  landing: null,
  ...overrides
});

const mockFetch = (status: number, body: unknown) =>
  jest.spyOn(globalThis, "fetch").mockImplementation((input: RequestInfo | URL) => {
    const url = String(input);
    if (url.includes("/api/tenants/last-used")) {
      return Promise.resolve(new Response(JSON.stringify({}), { status: 200 }));
    }
    return Promise.resolve(new Response(JSON.stringify(body), { status }));
  });

describe("use-tenants", () => {
  beforeEach(() => ResetTenants());
  afterEach(() => {
    jest.restoreAllMocks();
    globalThis.history.pushState({}, "", "/");
  });

  it("returns the tenants of the user", async () => {
    mockFetch(200, mine());

    const { result } = renderHook(() => useTenants());

    await waitFor(() => expect(result.current).not.toBeNull());
    expect(result.current?.tenants).toHaveLength(2);
  });

  it("null when not logged in", async () => {
    const spy = mockFetch(401, "false");

    const { result } = renderHook(() => useTenants());

    await waitFor(() => expect(spy).toHaveBeenCalled());
    expect(result.current).toBeNull();
  });

  it("null when the response is not a tenant list", async () => {
    const spy = mockFetch(200, { something: "else" });

    const { result } = renderHook(() => useTenants());

    await waitFor(() => expect(spy).toHaveBeenCalled());
    expect(result.current).toBeNull();
  });

  it("one request for everything that needs the tenants", async () => {
    const spy = mockFetch(200, mine());

    const first = renderHook(() => useTenants());
    const second = renderHook(() => useTenants());

    await waitFor(() => expect(second.result.current).not.toBeNull());
    expect(first.result.current).not.toBeNull();
    expect(spy).toHaveBeenCalledTimes(1);
  });

  describe("useTenantLastUsed", () => {
    it("reports the tenant of the page when it differs", async () => {
      globalThis.history.pushState({}, "", "/starsky/t/acme/");
      const spy = mockFetch(200, mine({ current: "acme", lastUsed: "default" }));

      renderHook(() => useTenantLastUsed());

      await waitFor(() =>
        expect(spy.mock.calls.some(([url]) => String(url).includes("/api/tenants/last-used"))).toBe(
          true
        )
      );
      const call = spy.mock.calls.find(([url]) => String(url).includes("last-used"));
      expect(String(call?.[0])).toBe("/starsky/t/acme/api/tenants/last-used");
      expect((call?.[1] as RequestInit).body).toBe(JSON.stringify({ tenantId: "acme" }));
      expect(((call?.[1] as RequestInit).headers as Record<string, string>)["Content-Type"]).toBe(
        "application/json"
      );
    });

    it("does not report when it is already the last used", async () => {
      globalThis.history.pushState({}, "", "/starsky/t/acme/");
      const spy = mockFetch(200, mine({ current: "acme", lastUsed: "acme" }));

      const { result } = renderHook(() => useTenants());
      renderHook(() => useTenantLastUsed());
      await waitFor(() => expect(result.current).not.toBeNull());
      await act(async () => {
        await Promise.resolve();
      });

      expect(spy.mock.calls.some(([url]) => String(url).includes("last-used"))).toBe(false);
    });

    it("does not report a tenant the user is not a member of", async () => {
      globalThis.history.pushState({}, "", "/starsky/t/other/");
      const spy = mockFetch(200, mine({ current: "other" }));

      renderHook(() => useTenantLastUsed());
      await waitFor(() => expect(spy).toHaveBeenCalled());
      await act(async () => {
        await Promise.resolve();
      });

      expect(spy.mock.calls.some(([url]) => String(url).includes("last-used"))).toBe(false);
    });

    it("does not report when not logged in", async () => {
      const spy = mockFetch(401, "false");

      renderHook(() => useTenantLastUsed());
      await waitFor(() => expect(spy).toHaveBeenCalled());
      await act(async () => {
        await Promise.resolve();
      });

      expect(spy.mock.calls).toHaveLength(1);
    });
  });

  describe("FetchLandingTenant", () => {
    it("the landing tenant when it is another tenant than this page", async () => {
      mockFetch(200, mine({ landing: "acme" }));
      expect(await FetchLandingTenant()).toBe("acme");
    });

    it("null when the landing is this tenant, or the user must choose", async () => {
      mockFetch(200, mine({ landing: "default" }));
      expect(await FetchLandingTenant()).toBeNull();

      mockFetch(200, mine({ landing: null }));
      expect(await FetchLandingTenant()).toBeNull();
    });

    it("null when the call fails (the login redirect goes on)", async () => {
      jest.spyOn(globalThis, "fetch").mockRejectedValue(new Error("network"));
      expect(await FetchLandingTenant()).toBeNull();
    });

    it("asks again after a reset (a login is a new session)", async () => {
      const spy = mockFetch(401, "false");
      await FetchLandingTenant();
      spy.mockRestore();
      mockFetch(200, mine({ landing: "acme" }));
      ResetTenants();

      expect(await FetchLandingTenant()).toBe("acme");
    });
  });

  it("mounted users load again after a reset (menu after login)", async () => {
    mockFetch(401, "false");
    const { result } = renderHook(() => useTenants());
    await waitFor(() => expect(result.current).toBeNull());

    jest.restoreAllMocks();
    mockFetch(200, mine());
    act(() => ResetTenants());

    await waitFor(() => expect(result.current?.tenants).toHaveLength(2));
  });
});
