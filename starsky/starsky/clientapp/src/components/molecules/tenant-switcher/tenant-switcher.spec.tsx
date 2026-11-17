import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import * as useTenantsModule from "../../../hooks/use-tenants/use-tenants";
import { ITenantsResponse } from "../../../interfaces/ITenants";
import TenantSwitcher from "./tenant-switcher";

const response = (ids: string[], current = "default"): ITenantsResponse => ({
  current,
  tenants: ids.map((id) => ({ tenantId: id, displayName: `Name ${id}`, roleCode: "User" }))
});

describe("TenantSwitcher", () => {
  afterEach(() => jest.restoreAllMocks());

  it("renders nothing while loading", () => {
    jest.spyOn(useTenantsModule, "useTenants").mockReturnValue(null);
    render(<TenantSwitcher />);
    expect(screen.queryByTestId("tenant-switcher")).toBeNull();
  });

  it("renders nothing with a single tenant", () => {
    jest.spyOn(useTenantsModule, "useTenants").mockReturnValue(response(["default"]));
    render(<TenantSwitcher />);
    expect(screen.queryByTestId("tenant-switcher")).toBeNull();
  });

  it("shows the current tenant first and the others after it", async () => {
    jest.spyOn(useTenantsModule, "useTenants").mockReturnValue(response(["acme", "default"]));
    render(<TenantSwitcher />);
    fireEvent.click(screen.getByTestId("tenant-switcher-button"));

    const items = screen.getByTestId("tenant-switcher-menu").querySelectorAll("li");
    await waitFor(() => expect(items.length).toBe(2));
    expect(items[0].getAttribute("data-test")).toBe("tenant-switcher-current");
    expect(items[0].textContent).toContain("Name default");
    expect(items[1].textContent).toContain("Name acme");
  });

  it("opens the chosen tenant", () => {
    jest.spyOn(useTenantsModule, "useTenants").mockReturnValue(response(["acme", "default"]));
    const go = jest.spyOn(useTenantsModule, "GoToTenant").mockImplementation(() => {});
    render(<TenantSwitcher />);
    fireEvent.click(screen.getByTestId("tenant-switcher-button"));
    fireEvent.click(screen.getByTestId("tenant-switcher-acme"));
    expect(go).toHaveBeenCalledWith("acme");
  });

  it("closes on a click outside the menu", () => {
    jest.spyOn(useTenantsModule, "useTenants").mockReturnValue(response(["acme", "default"]));
    const { container } = render(<TenantSwitcher />);
    fireEvent.click(screen.getByTestId("tenant-switcher-button"));
    fireEvent.click(container.querySelector(".tenant-switcher__backdrop") as HTMLElement);
    expect(screen.queryByTestId("tenant-switcher-menu")).toBeNull();
  });
});
