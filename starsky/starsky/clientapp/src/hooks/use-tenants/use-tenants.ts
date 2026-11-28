import { useEffect, useState } from "react";
import { ITenantsResponse } from "../../interfaces/ITenants";
import FetchGet from "../../shared/fetch/fetch-get";
import FetchPost from "../../shared/fetch/fetch-post";
import { TenantFromLocation, UrlQuery } from "../../shared/url/url-query";

let tenantsRequest: Promise<ITenantsResponse | null> | null = null;
const listeners = new Set<() => void>();

/**
 * One request per page load, shared by everything that needs the tenants (menu, last used)
 */
const loadTenants = (): Promise<ITenantsResponse | null> => {
  tenantsRequest ??= FetchGet(new UrlQuery().UrlTenantsMine())
    .then((result) => {
      const data = result.data as ITenantsResponse | null;
      return result.statusCode === 200 && Array.isArray(data?.tenants) ? data : null;
    })
    .catch(() => null);
  return tenantsRequest;
};

/**
 * Forget the loaded tenants and load them again: after a login (a new session) and in tests
 */
export const ResetTenants = (): void => {
  tenantsRequest = null;
  // the menu and the last used reporter are mounted for the whole session: load again
  for (const reload of listeners) reload();
};

/**
 * The tenants of the logged in user
 * @returns null while loading, when not logged in, or when the call fails
 */
export const useTenants = (): ITenantsResponse | null => {
  const [tenants, setTenants] = useState<ITenantsResponse | null>(null);

  useEffect(() => {
    let mounted = true;
    const load = () => {
      void loadTenants().then((result) => {
        if (mounted) setTenants(result);
      });
    };
    load();
    listeners.add(load);
    return () => {
      mounted = false;
      listeners.delete(load);
    };
  }, []);

  return tenants;
};

/**
 * Report the tenant of this page, so the next login can go there. Once per page load,
 * and only for a tenant the user is a member of
 */
export const useTenantLastUsed = (): void => {
  const tenants = useTenants();

  useEffect(() => {
    if (!tenants) return;
    const current = TenantFromLocation();
    const isMember = tenants.tenants.some((tenant) => tenant.tenantId === current);
    if (!isMember || tenants.lastUsed === current) return;

    void FetchPost(
      new UrlQuery().UrlTenantsLastUsed(),
      JSON.stringify({ tenantId: current }),
      "post",
      { "Content-Type": "application/json" }
    );
  }, [tenants]);
};

/**
 * After a login without a target: the tenant to go to when that is not the tenant of this
 * page: the last used tenant (when still a member) or the only tenant of the user
 * @returns the tenant id, or null to stay where the user is
 */
export const FetchLandingTenant = async (): Promise<string | null> => {
  // loadTenants never rejects: a failing call may never keep the user from the login redirect
  const tenants = await loadTenants();
  const landing = tenants?.landing;
  return landing && landing !== TenantFromLocation() ? landing : null;
};

/**
 * Open another tenant: a full page load, so nothing of the tenant that was open stays in the page
 * @param tenantId tenant to open
 */
export const GoToTenant = (tenantId: string): void => {
  globalThis.location.assign(new UrlQuery().UrlTenantHomePage(tenantId));
};
