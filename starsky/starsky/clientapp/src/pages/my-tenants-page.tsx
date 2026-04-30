import { FunctionComponent, useEffect, useState } from "react";
import Preloader from "../components/atoms/preloader/preloader";
import useFetch from "../hooks/use-fetch";
import FetchPost from "../shared/fetch/fetch-post";
import { DocumentTitle } from "../shared/document-title";
import { UrlQuery } from "../shared/url/url-query";

interface ITenantMineResponseItem {
  slug?: string;
  Slug?: string;
  name?: string;
  Name?: string;
  role?: string;
  Role?: string;
  isEnabled?: boolean;
  IsEnabled?: boolean;
}

interface ITenantMineResponse {
  tenants?: ITenantMineResponseItem[];
  isGlobalAdmin?: boolean;
  isEmpty?: boolean;
  canCreateFirstTenant?: boolean;
}

export const MyTenantsPage: FunctionComponent = () => {
  const urlQuery = new UrlQuery();
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [tenantSlug, setTenantSlug] = useState("");
  const [tenantName, setTenantName] = useState("");
  const [createError, setCreateError] = useState<string | null>(null);
  const [createLoading, setCreateLoading] = useState(false);
  const [createSuccess, setCreateSuccess] = useState(false);

  useEffect(() => {
    new DocumentTitle().SetDocumentTitlePrefix("My tenants");
  }, []);

  const response = useFetch(urlQuery.UrlTenantsMineApi(), "get");

  const handleCreateTenant = async (e: React.FormEvent) => {
    e.preventDefault();
    setCreateError(null);
    setCreateSuccess(false);

    if (!tenantSlug.trim() || !tenantName.trim()) {
      setCreateError("Both slug and name are required");
      return;
    }

    setCreateLoading(true);
    try {
      const result = await FetchPost(
        urlQuery.UrlTenantCreateApi(),
        JSON.stringify({
          slug: tenantSlug,
          name: tenantName
        }),
        "post",
        {
          "Content-Type": "application/json"
        }
      );

      if (result.statusCode === 200) {
        setCreateSuccess(true);
        setTenantSlug("");
        setTenantName("");
        setShowCreateForm(false);
        // Refresh the page after a short delay
        setTimeout(() => {
          globalThis.location.reload();
        }, 1000);
      } else {
        const errorMessage = typeof result.data === "string" ? result.data :
                           (result.data as Record<string, unknown>)?.message as string || "Failed to create tenant";
        setCreateError(errorMessage);
      }
    } catch (error) {
      setCreateError((error as { message: string }).message || "Error creating tenant");
    } finally {
      setCreateLoading(false);
    }
  };

  if (response.statusCode === 999) {
    return <Preloader isOverlay={true}></Preloader>;
  }

  if (response.statusCode === 401) {
    return (
      <div className="content">
        <div className="content--header">My tenants</div>
        <div className="content--error-true">Please sign in first.</div>
      </div>
    );
  }

  const data = response.data as ITenantMineResponse;
  const tenants = Array.isArray(data?.tenants) ? data.tenants : ([] as ITenantMineResponseItem[]);
  const isGlobalAdmin = data?.isGlobalAdmin ?? false;

  return (
    <div className="content" data-test="my-tenants-content">
      <div className="content--header">My tenants</div>

      {tenants.length === 0 ? (
        <div data-test="no-tenants-message">You are not a member of any tenant yet.</div>
      ) : (
        <ul>
          {tenants.map((tenant) => {
            const slug = tenant.slug ?? tenant.Slug ?? "";
            const name = tenant.name ?? tenant.Name ?? slug;
            const role = tenant.role ?? tenant.Role ?? "Member";
            const isEnabled = tenant.isEnabled ?? tenant.IsEnabled ?? true;

            return (
              <li key={slug}>
                <a href={`${urlQuery.prefix}/${slug}/account/login`}>{name}</a>
                {` (${role}${isEnabled ? "" : ", disabled"})`}
              </li>
            );
          })}
        </ul>
      )}

      {isGlobalAdmin && (
        <div style={{ marginTop: "20px" }}>
          {showCreateForm ? (
            <form onSubmit={handleCreateTenant} data-test="create-tenant-form" style={{ marginTop: "10px" }}>
              <div style={{ marginBottom: "10px" }}>
                <label htmlFor="tenant-slug">Tenant Slug:</label>
                <input
                  id="tenant-slug"
                  type="text"
                  data-test="tenant-slug-input"
                  placeholder="e.g., my-photos"
                  value={tenantSlug}
                  onChange={(e) => setTenantSlug(e.target.value)}
                  disabled={createLoading}
                  maxLength={50}
                  pattern="^[a-z0-9][a-z0-9-]{1,48}[a-z0-9]$"
                  title="Must start and end with lowercase letter or number, can contain hyphens"
                  className="form-control"
                  style={{ marginTop: "5px" }}
                />
                <small style={{ display: "block", marginTop: "5px" }}>
                  Lowercase letters, numbers, and hyphens only. 3-50 characters.
                </small>
              </div>

              <div style={{ marginBottom: "10px" }}>
                <label htmlFor="tenant-name">Tenant Name:</label>
                <input
                  id="tenant-name"
                  type="text"
                  data-test="tenant-name-input"
                  placeholder="e.g., My Photos"
                  value={tenantName}
                  onChange={(e) => setTenantName(e.target.value)}
                  disabled={createLoading}
                  maxLength={100}
                  className="form-control"
                  style={{ marginTop: "5px" }}
                />
              </div>

              {createError && (
                <div data-test="create-error" className="content--error-true" style={{ marginBottom: "10px" }}>
                  {createError}
                </div>
              )}

              {createSuccess && (
                <div data-test="create-success" className="content--success" style={{ marginBottom: "10px", color: "green" }}>
                  Tenant created successfully! Redirecting...
                </div>
              )}

              <div style={{ marginTop: "10px" }}>
                <button
                  type="submit"
                  className="btn btn--default"
                  disabled={createLoading}
                  data-test="create-tenant-submit"
                >
                  {createLoading ? "Creating..." : "Create Tenant"}
                </button>
                <button
                  type="button"
                  className="btn btn--info"
                  onClick={() => {
                    setShowCreateForm(false);
                    setCreateError(null);
                    setTenantSlug("");
                    setTenantName("");
                  }}
                  disabled={createLoading}
                  style={{ marginLeft: "10px" }}
                  data-test="cancel-create-button"
                >
                  Cancel
                </button>
              </div>
            </form>
          ) : (
            <button
              onClick={() => setShowCreateForm(true)}
              className="btn btn--default"
              data-test="create-tenant-button"
            >
              Create New Tenant
            </button>
          )}
        </div>
      )}
    </div>
  );
};
