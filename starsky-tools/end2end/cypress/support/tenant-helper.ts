/**
 * Helpers for the multi tenancy specs (75, 76).
 * A tenant has its own url prefix: /starsky/t/{id}/..., the default tenant is /starsky/...
 */

export const defaultTenant = "default";

export function tenantBase(tenantId: string): string {
  return tenantId === defaultTenant ? "/starsky" : `/starsky/t/${tenantId}`;
}

/**
 * Ids are lowercase letters, digits and dashes; every run uses new ones, because a deleted
 * tenant keeps its id
 */
export function runId(): string {
  return Date.now().toString(36);
}

/**
 * Login with the account api, as that user (clears the session first).
 * Uses the same steps as the login form: first the antiforgery cookie, then the post.
 */
export function loginApi(email: string, password: string, tenantId = defaultTenant): void {
  const base = tenantBase(tenantId);
  cy.clearCookies();
  cy.request({ url: `${base}/account/login`, failOnStatusCode: false });
  cy.getCookie("X-XSRF-TOKEN").then((cookie) => {
    cy.request({
      method: "POST",
      url: `${base}/api/account/login`,
      followRedirect: false,
      headers: {
        "X-XSRF-TOKEN": cookie?.value ?? "",
        "Content-Type": "application/x-www-form-urlencoded",
      },
      body: `Email=${encodeURIComponent(email)}&Password=${encodeURIComponent(password)}`,
    })
      .its("status")
      .should("eq", 200);
  });
}

export function loginAsAdmin(): void {
  loginApi(Cypress.env("AUTH_USER"), Cypress.env("AUTH_PASS"));
}

/**
 * Create an account (needs a logged in user: the register page is closed for anonymous users)
 */
export function registerApi(name: string, email: string, password: string): void {
  cy.request({ url: "/starsky/account/register", failOnStatusCode: false });
  cy.getCookie("X-XSRF-TOKEN").then((cookie) => {
    cy.request({
      method: "POST",
      url: "/starsky/api/account/register",
      headers: {
        "X-XSRF-TOKEN": cookie?.value ?? "",
        "Content-Type": "application/x-www-form-urlencoded",
      },
      body: `Name=${encodeURIComponent(name)}&Email=${encodeURIComponent(email)}` +
        `&Password=${encodeURIComponent(password)}&ConfirmPassword=${encodeURIComponent(password)}`,
    })
      .its("status")
      .should("eq", 200);
  });
}

/**
 * A write to the tenant api: it needs the antiforgery token (header), like the web app sends it.
 * The token is in a cookie that every page of the app sets
 */
export function apiWrite(
  method: "POST" | "PUT" | "DELETE",
  url: string,
  body?: Record<string, unknown>
): Cypress.Chainable<Cypress.Response<unknown>> {
  cy.request({ url: "/starsky/account/login", failOnStatusCode: false });
  return cy.getCookie("X-XSRF-TOKEN").then((cookie) =>
    cy.request({
      method,
      url,
      body,
      headers: { "X-XSRF-TOKEN": cookie?.value ?? "" },
      failOnStatusCode: false,
    })
  );
}

export function createTenantApi(id: string, displayName: string): void {
  apiWrite("POST", "/starsky/api/tenants", { id, displayName })
    .its("status")
    .should("eq", 200);
}

export function addMemberApi(tenantId: string, identifier: string, role = "User"): void {
  apiWrite("POST", `${tenantBase(tenantId)}/api/tenants/members`, { identifier, role })
    .its("status")
    .should("eq", 200);
}

export interface TenantMember {
  userId: number;
  identifier: string;
  roleCode: string;
}

/**
 * Log in as a tenant administrator first
 */
export function membersOf(tenantId: string): Cypress.Chainable<TenantMember[]> {
  return cy
    .request(`${tenantBase(tenantId)}/api/tenants/members`)
    .then((response) => response.body as TenantMember[]);
}

/**
 * Status of an api url, without failing the test on 4xx
 */
export function statusOf(url: string, headers: Record<string, string> = {}) {
  return cy.request({ url, headers, failOnStatusCode: false }).its("status");
}

/**
 * Poll until the url answers with this status. Writes are handled by a background queue and
 * the tenant registry is cached, so never assert directly after a write
 */
export function waitForStatus(url: string, status: number, attempts = 20): void {
  cy.request({ url, failOnStatusCode: false }).then((response) => {
    if (response.status === status) return;
    if (attempts <= 0) {
      expect(response.status, `${url} should answer ${status}`).to.eq(status);
      return;
    }
    cy.wait(1000);
    waitForStatus(url, status, attempts - 1);
  });
}

/**
 * Poll until the folder of a tenant lists the (specific) file, never count items:
 * other specs leave files behind
 */
export function waitUntilFileIndexed(
  tenantId: string,
  folder: string,
  filePath: string,
  attempts = 25
): void {
  cy.request({
    url: `${tenantBase(tenantId)}/api/index/?f=${folder}`,
    failOnStatusCode: false,
  }).then((response) => {
    const items: { filePath: string }[] = response.body?.fileIndexItems ?? [];
    if (items.some((item) => item.filePath === filePath)) return;
    if (attempts <= 0) {
      expect(items.map((i) => i.filePath), `${filePath} in ${tenantId}`).to.include(filePath);
      return;
    }
    cy.wait(1500);
    waitUntilFileIndexed(tenantId, folder, filePath, attempts - 1);
  });
}

export function filesOf(tenantId: string, folder: string): Cypress.Chainable<string[]> {
  return cy
    .request({
      url: `${tenantBase(tenantId)}/api/index/?f=${folder}`,
      failOnStatusCode: false,
    })
    .then((response) =>
      ((response.body?.fileIndexItems ?? []) as { filePath: string }[]).map((i) => i.filePath)
    );
}

export function mkdirApi(tenantId: string, folder: string): void {
  cy.request({
    method: "POST",
    url: `${tenantBase(tenantId)}/api/disk/mkdir`,
    form: true,
    body: { f: folder },
    failOnStatusCode: false,
  });
  waitForStatus(`${tenantBase(tenantId)}/api/index/?f=${folder}`, 200);
}

export function deleteApi(tenantId: string, paths: string[]): void {
  cy.request({
    method: "DELETE",
    url: `${tenantBase(tenantId)}/api/delete`,
    qs: { f: paths.join(";") },
    failOnStatusCode: false,
  });
}

/**
 * Fill the login form of the current page
 */
export function submitLoginForm(email: string, password: string): void {
  cy.get("form").within(() => {
    cy.get("[name=email]").type(email);
    cy.get("[name=password]").type(password, { log: false });
    cy.get("button[type=submit]").click();
  });
}
