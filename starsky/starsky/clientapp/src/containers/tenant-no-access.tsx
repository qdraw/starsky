import React from "react";
import useGlobalSettings from "../hooks/use-global-settings";
import { useTenants } from "../hooks/use-tenants/use-tenants";
import localization from "../localization/localization.json";
import { Language } from "../shared/language";
import { UrlQuery } from "../shared/url/url-query";

/**
 * The user is logged in, but not a member of the tenant in the url: choose one of the own
 * tenants (no list: the user is not a member of any tenant)
 */
export const TenantNoAccess: React.FunctionComponent = () => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const tenants = useTenants();

  const MessageApplicationName = "Starsky";
  const MessageNoTenantAccess = language.key(localization.MessageNoTenantAccess);
  const MessageChooseTenant = language.key(localization.MessageChooseTenant);
  const MessageNoTenants = language.key(localization.MessageNoTenants);
  const MessageLogout = language.key(localization.MessageLogout);

  return (
    <>
      <header className="header header--main header--bluegray700">
        <div className="wrapper">
          <div className="item item--first item--detective">{MessageApplicationName}</div>
        </div>
      </header>
      <div className="content" data-test="tenant-no-access">
        <div className="content--header">{MessageNoTenantAccess}</div>
        {tenants?.tenants.length ? (
          <>
            <div className="content--subheader">{MessageChooseTenant}</div>
            {tenants.tenants.map((tenant) => (
              <div className="content--subheader" key={tenant.tenantId}>
                <a
                  data-test={`tenant-no-access-${tenant.tenantId}`}
                  href={new UrlQuery().UrlTenantHomePage(tenant.tenantId)}
                >
                  <u>{tenant.displayName}</u>
                </a>
              </div>
            ))}
          </>
        ) : (
          <div className="content--subheader" data-test="tenant-no-access-none">
            {MessageNoTenants}
          </div>
        )}
        <div className="content--subheader">
          <a data-test="tenant-no-access-logout" href={new UrlQuery().UrlLogoutPage("/?f=/")}>
            <u>{MessageLogout}</u>
          </a>
        </div>
      </div>
    </>
  );
};
