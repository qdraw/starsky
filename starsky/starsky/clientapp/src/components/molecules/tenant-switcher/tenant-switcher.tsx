import React, { useState } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { GoToTenant, useTenants } from "../../../hooks/use-tenants/use-tenants";
import localization from "../../../localization/localization.json";
import { Language } from "../../../shared/language";

/**
 * Avatar button in the top right of the header to switch between tenants.
 * The current tenant is shown first. Nothing is rendered without another tenant to switch to
 */
const TenantSwitcher: React.FunctionComponent = () => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageSwitchTenant = language.key(localization.MessageSwitchTenant);
  const MessageTenantCurrent = language.key(localization.MessageTenantCurrent);

  const tenants = useTenants();
  const [open, setOpen] = useState(false);

  if (!tenants || tenants.tenants.length < 2) return null;

  const current = tenants.tenants.find((tenant) => tenant.tenantId === tenants.current);
  // a current tenant the user is not a member of is no reason to show a "current" row
  const others = tenants.tenants.filter((tenant) => tenant.tenantId !== tenants.current);
  const initial = (current?.displayName ?? tenants.current).charAt(0).toUpperCase();

  return (
    <div className="tenant-switcher" data-test="tenant-switcher">
      <button
        type="button"
        className="tenant-switcher__button"
        data-test="tenant-switcher-button"
        title={MessageSwitchTenant}
        aria-label={MessageSwitchTenant}
        aria-haspopup="true"
        aria-expanded={open}
        onClick={() => setOpen(!open)}
      >
        {initial}
      </button>
      {open ? (
        <>
          <button
            type="button"
            className="tenant-switcher__backdrop"
            aria-label="close"
            tabIndex={-1}
            onClick={() => setOpen(false)}
          />
          <ul className="tenant-switcher__menu" data-test="tenant-switcher-menu">
            {current ? (
              <li
                className="tenant-switcher__item tenant-switcher__item--current"
                data-test="tenant-switcher-current"
              >
                <span className="tenant-switcher__avatar">
                  {current.displayName.charAt(0).toUpperCase()}
                </span>
                <span>
                  <strong>{current.displayName}</strong>
                  <small>{MessageTenantCurrent}</small>
                </span>
              </li>
            ) : null}
            {others.map((tenant) => (
              <li key={tenant.tenantId}>
                <button
                  type="button"
                  className="tenant-switcher__item"
                  data-test={`tenant-switcher-${tenant.tenantId}`}
                  onClick={() => GoToTenant(tenant.tenantId)}
                >
                  <span className="tenant-switcher__avatar">
                    {tenant.displayName.charAt(0).toUpperCase()}
                  </span>
                  <span>{tenant.displayName}</span>
                </button>
              </li>
            ))}
          </ul>
        </>
      ) : null}
    </div>
  );
};

export default TenantSwitcher;
