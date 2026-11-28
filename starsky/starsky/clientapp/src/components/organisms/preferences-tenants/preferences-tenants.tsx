import React from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { useTenants } from "../../../hooks/use-tenants/use-tenants";
import localization from "../../../localization/localization.json";
import { Language } from "../../../shared/language";
import { UrlQuery } from "../../../shared/url/url-query";
import PreferencesTenantsManage from "./preferences-tenants-manage";
import PreferencesTenantsMembers from "./preferences-tenants-members";

const AdministratorRole = "Administrator";

/**
 * Every user: the tenants they are a member of. Administrators of the current tenant also
 * manage its members
 */
const PreferencesTenants: React.FunctionComponent = () => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageTenantMemberships = language.key(localization.MessageTenantMemberships);
  const MessageTenantCurrent = language.key(localization.MessageTenantCurrent);
  const MessageNoTenants = language.key(localization.MessageNoTenants);
  const MessageRole = language.key(localization.MessageRole);

  const tenants = useTenants();

  const isAdministrator =
    tenants?.tenants.find((tenant) => tenant.tenantId === tenants.current)?.roleCode ===
    AdministratorRole;

  return (
    <>
      <div className="content--subheader">{MessageTenantMemberships}</div>
      <div className="content--text" data-test="preferences-tenants">
        {tenants?.tenants.length ? (
          <ul className="preferences-tenants__list">
            {tenants.tenants.map((tenant) => (
              <li key={tenant.tenantId} data-test={`preferences-tenant-${tenant.tenantId}`}>
                {tenant.tenantId === tenants.current ? (
                  <strong>{tenant.displayName}</strong>
                ) : (
                  <a href={new UrlQuery().UrlTenantHomePage(tenant.tenantId)}>
                    <u>{tenant.displayName}</u>
                  </a>
                )}
                {tenant.tenantId === tenants.current ? ` (${MessageTenantCurrent})` : null}
                {` - ${MessageRole}: ${tenant.roleCode}`}
              </li>
            ))}
          </ul>
        ) : (
          <div data-test="preferences-tenants-none">{MessageNoTenants}</div>
        )}
      </div>
      {isAdministrator ? <PreferencesTenantsMembers /> : null}
      {isAdministrator && tenants ? (
        <PreferencesTenantsManage
          isPlatformAdmin={tenants.isPlatformAdmin === true}
          currentTenantId={tenants.current}
          memberships={tenants.tenants}
        />
      ) : null}
    </>
  );
};

export default PreferencesTenants;
