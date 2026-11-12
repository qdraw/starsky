import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { TenantState } from "../../../interfaces/ITenants";
import PreferencesTenantsManage from "./preferences-tenants-manage";

const memberships = [
  { tenantId: "default", displayName: "Default", roleCode: "Administrator" },
  { tenantId: "acme", displayName: "Acme", roleCode: "Administrator" }
];

const overview = [
  { tenantId: "acme", displayName: "Acme", state: TenantState.Active },
  { tenantId: "beta", displayName: "Beta", state: TenantState.Disabled },
  { tenantId: "gone", displayName: "Gone", state: TenantState.Deleted },
  { tenantId: "going", displayName: "Going", state: TenantState.Deleting }
];

const json = (status: number, body: unknown) => new Response(JSON.stringify(body), { status });

const mockFetch = (writeStatus = 200) =>
  jest.spyOn(globalThis, "fetch").mockImplementation((_input, init) => {
    if ((init?.method ?? "GET") === "GET") return Promise.resolve(json(200, overview));
    return Promise.resolve(json(writeStatus, {}));
  });

const calls = (spy: jest.SpyInstance, method: string) =>
  spy.mock.calls.filter(([, init]) => init?.method === method);

const renderManage = (isPlatformAdmin: boolean) =>
  render(
    <PreferencesTenantsManage
      isPlatformAdmin={isPlatformAdmin}
      currentTenantId="default"
      memberships={memberships}
    />
  );

describe("PreferencesTenantsManage", () => {
  afterEach(() => jest.restoreAllMocks());

  describe("not a platform administrator", () => {
    it("shows the own tenants and every control disabled, without asking for all tenants", () => {
      const spy = mockFetch();
      renderManage(false);

      expect(screen.getByTestId("preferences-tenant-manage-readonly")).toBeTruthy();
      expect(screen.getByTestId("preferences-tenant-row-acme")).toBeTruthy();
      expect(screen.getByTestId("preferences-tenant-delete-acme")).toHaveProperty("disabled", true);
      expect(screen.getByTestId("preferences-tenant-new-id")).toHaveProperty("disabled", true);
      expect(screen.getByTestId("preferences-tenant-add")).toHaveProperty("disabled", true);
      expect(spy).not.toHaveBeenCalled();
    });

    it("never offers to delete the default tenant", () => {
      renderManage(false);
      expect(screen.queryByTestId("preferences-tenant-delete-default")).toBeNull();
    });

    it("does not add on submit", () => {
      const spy = mockFetch();
      renderManage(false);

      fireEvent.submit(screen.getByTestId("preferences-tenant-add").closest("form")!);

      expect(spy).not.toHaveBeenCalled();
    });
  });

  describe("platform administrator", () => {
    it("lists all tenants that still exist, without the deleted ones", async () => {
      mockFetch();
      renderManage(true);

      await waitFor(() => expect(screen.getByTestId("preferences-tenant-row-beta")).toBeTruthy());
      expect(screen.getByTestId("preferences-tenant-row-beta").textContent).toContain("Disabled");
      expect(screen.queryByTestId("preferences-tenant-row-gone")).toBeNull();
      expect(screen.queryByTestId("preferences-tenant-manage-readonly")).toBeNull();
      expect(screen.getByTestId("preferences-tenant-delete-beta")).toHaveProperty(
        "disabled",
        false
      );
      // a tenant that is being deleted is not deleted again
      expect(screen.getByTestId("preferences-tenant-delete-going")).toHaveProperty(
        "disabled",
        true
      );
    });

    it("can not delete the tenant it is in", async () => {
      jest
        .spyOn(globalThis, "fetch")
        .mockResolvedValue(
          json(200, [{ tenantId: "default", displayName: "Default", state: TenantState.Active }])
        );
      render(
        <PreferencesTenantsManage
          isPlatformAdmin={true}
          currentTenantId="acme"
          memberships={memberships}
        />
      );
      await waitFor(() =>
        expect(screen.getByTestId("preferences-tenant-row-default")).toBeTruthy()
      );
      expect(screen.queryByTestId("preferences-tenant-delete-default")).toBeNull();
    });

    it("adds a tenant and clears the form", async () => {
      const spy = mockFetch();
      renderManage(true);
      const id = await screen.findByTestId("preferences-tenant-new-id");

      fireEvent.change(id, { target: { value: " newco " } });
      fireEvent.change(screen.getByTestId("preferences-tenant-new-name"), {
        target: { value: "New Co" }
      });
      fireEvent.click(screen.getByTestId("preferences-tenant-add"));

      await waitFor(() => expect(calls(spy, "post")).toHaveLength(1));
      expect(calls(spy, "post")[0][1].body).toBe(
        JSON.stringify({ id: "newco", displayName: "New Co" })
      );
      await waitFor(() => expect((id as HTMLInputElement).value).toBe(""));
    });

    it.each([
      [400, "not valid"],
      [409, "already exists"],
      [500, "could not be saved"]
    ])("shows a warning on %s", async (status, text) => {
      mockFetch(status);
      renderManage(true);
      const id = await screen.findByTestId("preferences-tenant-new-id");

      fireEvent.change(id, { target: { value: "newco" } });
      fireEvent.click(screen.getByTestId("preferences-tenant-add"));

      const warning = await screen.findByTestId("preferences-tenant-manage-warning");
      expect(warning.textContent).toContain(text);
      expect((id as HTMLInputElement).value).toBe("newco");
    });

    it("deletes a tenant after a confirmation", async () => {
      const spy = mockFetch();
      jest.spyOn(globalThis, "confirm").mockReturnValue(true);
      renderManage(true);

      fireEvent.click(await screen.findByTestId("preferences-tenant-delete-beta"));

      await waitFor(() => expect(calls(spy, "delete")).toHaveLength(1));
      expect(String(calls(spy, "delete")[0][0])).toContain("/api/tenants/beta");
    });

    it("does not delete when the confirmation is declined", async () => {
      const spy = mockFetch();
      jest.spyOn(globalThis, "confirm").mockReturnValue(false);
      renderManage(true);

      fireEvent.click(await screen.findByTestId("preferences-tenant-delete-beta"));

      expect(calls(spy, "delete")).toHaveLength(0);
    });
  });
});
