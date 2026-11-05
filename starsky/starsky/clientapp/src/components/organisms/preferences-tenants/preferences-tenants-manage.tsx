import React, { useCallback, useEffect, useState } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { ResetTenants } from "../../../hooks/use-tenants/use-tenants";
import { ITenantListItem, ITenantOverviewItem, TenantState } from "../../../interfaces/ITenants";
import localization from "../../../localization/localization.json";
import FetchGet from "../../../shared/fetch/fetch-get";
import FetchPost from "../../../shared/fetch/fetch-post";
import { Language } from "../../../shared/language";
import { UrlQuery } from "../../../shared/url/url-query";
import ButtonStyled from "../../atoms/button-styled/button-styled";

const DefaultTenantId = "default";

interface IPreferencesTenantsManageProps {
  /** may add and delete tenants: for others the controls are shown disabled */
  isPlatformAdmin: boolean;
  /** the tenant of this page: it can not be deleted from within itself */
  currentTenantId: string;
  /** the memberships, shown when the full list is not available */
  memberships: ITenantListItem[];
}

/**
 * Add and delete tenants. Shown to administrators of a tenant, enabled for platform
 * administrators (the API refuses everybody else)
 */
const PreferencesTenantsManage: React.FunctionComponent<IPreferencesTenantsManageProps> = ({
  isPlatformAdmin,
  currentTenantId,
  memberships
}) => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageTenantManage = language.key(localization.MessageTenantManage);
  const MessageTenantPlatformAdminOnly = language.key(localization.MessageTenantPlatformAdminOnly);
  const MessageTenantId = language.key(localization.MessageTenantId);
  const MessageTenantDisplayName = language.key(localization.MessageTenantDisplayName);
  const MessageTenantAdd = language.key(localization.MessageTenantAdd);
  const MessageTenantDelete = language.key(localization.MessageTenantDelete);
  const MessageTenantDeleteConfirm = language.key(localization.MessageTenantDeleteConfirm);
  const MessageTenantInvalid = language.key(localization.MessageTenantInvalid);
  const MessageTenantExists = language.key(localization.MessageTenantExists);
  const MessageTenantMemberError = language.key(localization.MessageTenantMemberError);

  const [overview, setOverview] = useState<ITenantOverviewItem[] | null>(null);
  const [tenantId, setTenantId] = useState("");
  const [displayName, setDisplayName] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const load = useCallback(async () => {
    if (!isPlatformAdmin) return;
    const response = await FetchGet(new UrlQuery().UrlTenants());
    if (response.statusCode === 200 && Array.isArray(response.data)) {
      setOverview(response.data as ITenantOverviewItem[]);
    }
  }, [isPlatformAdmin]);

  useEffect(() => {
    void load();
  }, [load]);

  // a platform administrator sees every tenant that still exists, others their own
  const rows: ITenantOverviewItem[] = (
    overview ??
    memberships.map((tenant) => ({
      tenantId: tenant.tenantId,
      displayName: tenant.displayName,
      state: TenantState.Active
    }))
  ).filter((tenant) => tenant.state !== TenantState.Deleted);

  const showResult = (statusCode: number): boolean => {
    if (statusCode === 200) {
      setError(null);
      return true;
    }
    if (statusCode === 400) {
      setError(MessageTenantInvalid);
    } else if (statusCode === 409) {
      setError(MessageTenantExists);
    } else {
      setError(MessageTenantMemberError);
    }
    return false;
  };

  const finish = async (statusCode: number): Promise<boolean> => {
    setLoading(false);
    const success = showResult(statusCode);
    if (success) {
      // the menu and the tenant list of this page know the new situation
      ResetTenants();
      await load();
    }
    return success;
  };

  const addTenant = async () => {
    setLoading(true);
    const response = await FetchPost(
      new UrlQuery().UrlTenants(),
      JSON.stringify({ id: tenantId.trim(), displayName: displayName.trim() }),
      "post",
      { "Content-Type": "application/json" }
    );
    if (await finish(response.statusCode)) {
      setTenantId("");
      setDisplayName("");
    }
  };

  const deleteTenant = async (id: string) => {
    if (!globalThis.confirm(MessageTenantDeleteConfirm)) return;
    setLoading(true);
    const response = await FetchPost(new UrlQuery().UrlTenant(id), "", "delete");
    await finish(response.statusCode);
  };

  return (
    <div className="preferences preferences--tenant-manage" data-test="preferences-tenant-manage">
      <div className="content--subheader">{MessageTenantManage}</div>
      {isPlatformAdmin ? null : (
        <div className="content--text" data-test="preferences-tenant-manage-readonly">
          {MessageTenantPlatformAdminOnly}
        </div>
      )}
      <table className="preferences-tenant-members__table">
        <tbody>
          {rows.map((tenant) => (
            <tr key={tenant.tenantId} data-test={`preferences-tenant-row-${tenant.tenantId}`}>
              <td className="content--text">
                {tenant.displayName} ({tenant.tenantId})
                {tenant.state === TenantState.Active ? null : ` - ${TenantState[tenant.state]}`}
              </td>
              <td>
                {tenant.tenantId === DefaultTenantId ? null : (
                  <button
                    className="btn btn--default"
                    type="button"
                    data-test={`preferences-tenant-delete-${tenant.tenantId}`}
                    disabled={
                      !isPlatformAdmin ||
                      loading ||
                      tenant.tenantId === currentTenantId ||
                      tenant.state === TenantState.Deleting
                    }
                    onClick={() => void deleteTenant(tenant.tenantId)}
                  >
                    {MessageTenantDelete}
                  </button>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <form
        className="form-inline"
        onSubmit={(e) => {
          e.preventDefault();
          if (isPlatformAdmin && tenantId.trim()) void addTenant();
        }}
      >
        <div className="content--text">
          <label htmlFor="tenant-new-id">{MessageTenantId}</label>
          <input
            className="form-control"
            id="tenant-new-id"
            maxLength={63}
            spellCheck={false}
            data-test="preferences-tenant-new-id"
            disabled={!isPlatformAdmin}
            value={tenantId}
            onChange={(e) => setTenantId(e.target.value)}
          />
          <label htmlFor="tenant-new-name">{MessageTenantDisplayName}</label>
          <input
            className="form-control"
            id="tenant-new-name"
            maxLength={80}
            data-test="preferences-tenant-new-name"
            disabled={!isPlatformAdmin}
            value={displayName}
            onChange={(e) => setDisplayName(e.target.value)}
          />
          {error && (
            <div data-test="preferences-tenant-manage-warning" className="warning-box">
              {error}
            </div>
          )}
          <ButtonStyled
            className="btn btn--default"
            type="submit"
            data-test="preferences-tenant-add"
            disabled={!isPlatformAdmin || loading || !tenantId.trim()}
          >
            {MessageTenantAdd}
          </ButtonStyled>
        </div>
      </form>
    </div>
  );
};

export default PreferencesTenantsManage;
