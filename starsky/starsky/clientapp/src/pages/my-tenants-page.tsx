import { FunctionComponent, useEffect } from "react";
import Preloader from "../components/atoms/preloader/preloader";
import useFetch from "../hooks/use-fetch";
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

export const MyTenantsPage: FunctionComponent = () => {
  const urlQuery = new UrlQuery();

  useEffect(() => {
    new DocumentTitle().SetDocumentTitlePrefix("My tenants");
  }, []);

  const response = useFetch(urlQuery.UrlTenantsMineApi(), "get");
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

  const tenants = (Array.isArray(response.data) ? response.data : []) as ITenantMineResponseItem[];

  return (
    <div className="content" data-test="my-tenants-content">
      <div className="content--header">My tenants</div>
      {tenants.length === 0 ? (
        <div>You are not a member of any tenant yet.</div>
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
    </div>
  );
};
