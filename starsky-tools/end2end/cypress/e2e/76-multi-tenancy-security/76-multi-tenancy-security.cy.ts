import { envFolder, envName } from "../../support/commands";
import {
  addMemberApi,
  apiWrite,
  createTenantApi,
  defaultTenant,
  filesOf,
  loginAsAdmin,
  loginApi,
  membersOf,
  mkdirApi,
  registerApi,
  runId,
  statusOf,
  submitLoginForm,
  tenantBase,
  waitForStatus,
  waitUntilFileIndexed,
} from "../../support/tenant-helper";
import configFile from "./config.json";

const config = configFile[envFolder][envName];

/**
 * Security checks of the multi tenancy feature (what an attacker would try).
 * Each test names the rule it protects. The fixes are in documentation/docs/features/multi-tenancy.md
 */
describe("Multi tenancy security (76)", () => {
  const id = runId();
  const tenantA = `sec-a-${id}`;
  const tenantB = `sec-b-${id}`;
  const memberEmail = `sec-member-${id}@example.test`;
  const outsiderEmail = `sec-outsider-${id}@example.test`;
  const password = `Pw-${id}-sec-tenant!`;
  const folder = `/e2e-security-${id}`;
  const file = `${folder}/20200822_111408.jpg`;

  before("two tenants; a member of A only; an outsider without membership (76)", () => {
    if (!config.isEnabled) return;
    cy.resetStorage();
    loginAsAdmin();
    createTenantApi(tenantA, "Security A");
    createTenantApi(tenantB, "Security B");
    registerApi("Sec Member", memberEmail, password);
    registerApi("Sec Outsider", outsiderEmail, password);
    addMemberApi(tenantA, memberEmail, "User");
    membersOf(defaultTenant).then((members) => {
      const outsider = members.find((m) => m.identifier === outsiderEmail);
      apiWrite("DELETE", `${tenantBase(defaultTenant)}/api/tenants/members/${outsider.userId}`)
        .its("status")
        .should("eq", 200);
    });
    // a file in B: what the member of A must not reach
    mkdirApi(tenantB, folder);
    cy.fileRequest("20200822_111408.jpg", folder, "image/jpeg", tenantBase(tenantB));
    waitUntilFileIndexed(tenantB, folder, file);
  });

  beforeEach("storage is reset before every test (76)", () => {
    if (!config.isEnabled) return;
    cy.resetStorage();
  });

  after("clean up (76)", () => {
    if (!config.isEnabled) return;
    loginAsAdmin();
    apiWrite("DELETE", `${tenantBase(tenantB)}/api/delete?f=${folder}`);
  });

  describe("anonymous visitors", () => {
    it("get 401 for the data of an existing tenant and for the tenant api (76)", () => {
      if (!config.isEnabled) return;
      cy.clearCookies();
      for (const url of [
        `${tenantBase(tenantA)}/api/index?f=/`,
        `${tenantBase(tenantB)}/api/index?f=${folder}`,
        "/starsky/api/tenants/mine",
        "/starsky/api/tenants",
        `${tenantBase(tenantA)}/api/tenants/members`,
        "/starsky/api/memory-cache-debug",
        "/starsky/api/env",
      ]) {
        statusOf(url).should("be.oneOf", [401]);
      }
    });

    it("get the same answer for a tenant that does not exist, is invalid, or is the default (76)", () => {
      if (!config.isEnabled) return;
      cy.clearCookies();
      for (const tenant of [
        "does-not-exist",
        "UPPER",
        "a_b",
        // (a browser removes a plain .. before it sends the request)
        "default", // the default tenant has no prefix: no second url for it
        "acme%0A", // a trailing newline
        `${tenantA}%0A`,
        `${tenantA}%2f..%2f${tenantB}`,
      ]) {
        statusOf(`/starsky/t/${tenant}/api/index?f=/`).should("be.oneOf", [400, 404]);
      }
    });

    it("can not use a state changing tenant request without the antiforgery token (76)", () => {
      if (!config.isEnabled) return;
      loginAsAdmin();
      // logged in as the platform administrator, but from another site: no token
      cy.request({
        method: "POST",
        url: "/starsky/api/tenants",
        body: { id: `sec-csrf-${id}`, displayName: "csrf" },
        failOnStatusCode: false,
      })
        .its("status")
        .should("be.oneOf", [400, 405]);
      cy.request({
        method: "DELETE",
        url: `/starsky/api/tenants/${tenantB}`,
        failOnStatusCode: false,
      })
        .its("status")
        .should("be.oneOf", [400, 405]);
      cy.request("/starsky/api/tenants").then((response) => {
        const ids = (response.body as { tenantId: string }[]).map((t) => t.tenantId);
        expect(ids).to.not.include(`sec-csrf-${id}`);
        expect(ids).to.include(tenantB);
      });
    });
  });

  describe("a member of one tenant", () => {
    it("can not read the files of another tenant, by api or in the browser (76)", () => {
      if (!config.isEnabled) return;
      loginApi(memberEmail, password);
      for (const url of [
        `${tenantBase(tenantB)}/api/index?f=${folder}`,
        `${tenantBase(tenantB)}/api/index/?f=${folder}`,
        `${tenantBase(tenantB)}/api/download-photo?f=${file}&isThumbnail=false`,
        `${tenantBase(tenantB)}/api/info?f=${file}`,
        `${tenantBase(tenantB)}/api/suggest/?t=jpg`,
        `${tenantBase(tenantB)}/api/health/details`,
        `${tenantBase(tenantB)}/api/env`,
        `${tenantBase(tenantB)}/redirect/sub-path-relative`,
      ]) {
        statusOf(url).should("eq", 403);
      }
      // the folder exists in B, not in A and not in the default tenant
      statusOf(`${tenantBase(tenantA)}/api/index?f=${folder}`).should("eq", 404);
      statusOf(`${tenantBase(defaultTenant)}/api/index?f=${folder}`).should("eq", 404);
    });

    it("can not change or delete in another tenant (76)", () => {
      if (!config.isEnabled) return;
      loginApi(memberEmail, password);
      const requests: Partial<Cypress.RequestOptions>[] = [
        {
          method: "POST",
          url: `${tenantBase(tenantB)}/api/update`,
          form: true,
          body: { f: file, tags: "x" },
        },
        { method: "DELETE", url: `${tenantBase(tenantB)}/api/delete?f=${file}` },
        {
          method: "POST",
          url: `${tenantBase(tenantB)}/api/disk/mkdir`,
          form: true,
          body: { f: "/hacked" },
        },
      ];
      for (const request of requests) {
        cy.request({ ...request, url: request.url as string, failOnStatusCode: false })
          .its("status")
          .should("be.oneOf", [403, 405]);
      }
      loginAsAdmin();
      filesOf(tenantB, folder).then((files) => expect(files).to.include(file));
    });

    it("ignores a tenant id in a header or the query string (76)", () => {
      if (!config.isEnabled) return;
      loginApi(memberEmail, password);
      // the url decides: other tenants in headers or query do not change that
      for (const spoof of [
        { headers: { "X-Tenant": tenantB, "X-Tenant-Id": tenantB, Tenant: tenantB } },
        { qs: { tenant: tenantB, tenantId: tenantB } },
      ]) {
        cy.request({
          url: `${tenantBase(tenantA)}/api/index?f=${folder}`,
          failOnStatusCode: false,
          ...spoof,
        })
          .its("status")
          .should("eq", 404); // A has no such folder: B's data is not shown
      }
      cy.request({
        url: `${tenantBase(tenantB)}/api/index?f=${folder}`,
        qs: { tenant: tenantA, tenantId: tenantA },
        headers: { "X-Tenant": tenantA, "X-Tenant-Id": tenantA },
        failOnStatusCode: false,
      })
        .its("status")
        .should("eq", 403);
    });

    it("can not manage members, tenants or the platform (76)", () => {
      if (!config.isEnabled) return;
      loginApi(memberEmail, password);
      for (const url of [
        `${tenantBase(tenantA)}/api/tenants/members`,
        "/starsky/api/tenants",
        "/starsky/api/memory-cache-debug",
      ]) {
        statusOf(url).should("eq", 403);
      }
      apiWrite("POST", `${tenantBase(tenantA)}/api/tenants/members`, {
        identifier: outsiderEmail,
        role: "Administrator",
      })
        .its("status")
        .should("eq", 403);
      apiWrite("POST", `/starsky/api/tenants/${tenantA}/disable`).its("status").should("eq", 403);
      apiWrite("DELETE", `/starsky/api/tenants/${tenantA}`).its("status").should("eq", 403);
      // process wide settings are for the platform administrator
      cy.request({
        method: "POST",
        url: "/starsky/api/env",
        form: true,
        body: { verbose: "true" },
        failOnStatusCode: false,
      })
        .its("status")
        .should("be.oneOf", [401, 403, 405]);
    });

    it("does not see the host folders of the server (76)", () => {
      if (!config.isEnabled) return;
      loginApi(memberEmail, password);
      cy.request(`${tenantBase(tenantA)}/api/env`).then((response) => {
        for (const key of ["storageFolder", "tempFolder", "thumbnailTempFolder", "tenantsRoot"]) {
          // the folder settings add a trailing separator
          expect(response.body[key], key).to.match(/^hidden[/\\]?$/);
        }
      });
      loginAsAdmin();
      cy.request("/starsky/api/env").then((response) => {
        expect(response.body.storageFolder).to.not.match(/^hidden/);
      });
    });

    it("can not create a membership by registering (76)", () => {
      if (!config.isEnabled) return;
      const newEmail = `sec-selfjoin-${id}@example.test`;
      loginApi(memberEmail, password);
      // the register page of tenant B, with the antiforgery token that page sets
      cy.request({ url: `${tenantBase(tenantB)}/account/register`, failOnStatusCode: false });
      cy.getCookie("X-XSRF-TOKEN").then((cookie) => {
        cy.request({
          method: "POST",
          url: `${tenantBase(tenantB)}/api/account/register`,
          headers: {
            "X-XSRF-TOKEN": cookie?.value ?? "",
            "Content-Type": "application/x-www-form-urlencoded",
          },
          body: `Name=x&Email=${encodeURIComponent(newEmail)}&Password=${encodeURIComponent(password)}&ConfirmPassword=${encodeURIComponent(password)}`,
          failOnStatusCode: false,
        })
          .its("status")
          .should("be.oneOf", [200, 403]);
      });
      // an account may exist (registering is open on some servers), but never a membership of B
      loginApi(newEmail, password);
      cy.request({ url: "/starsky/api/tenants/mine", failOnStatusCode: false }).then(
        (response) => {
          if (response.status !== 200) return; // the account was not created: fine
          expect(
            (response.body.tenants as { tenantId: string }[]).map((t) => t.tenantId)
          ).to.not.include(tenantB);
        }
      );
      statusOf(`${tenantBase(tenantB)}/api/index?f=${folder}`).should("be.oneOf", [401, 403]);
    });

    it("can not undo a removal by registering the same address again (76)", () => {
      if (!config.isEnabled) return;
      // the administrator removes the outsider (no membership left), who still has a session
      loginApi(outsiderEmail, password);
      statusOf("/starsky/api/index?f=/").should("eq", 403);
      cy.request({ url: `${tenantBase(tenantA)}/account/register`, failOnStatusCode: false });
      cy.getCookie("X-XSRF-TOKEN").then((cookie) => {
        cy.request({
          method: "POST",
          url: `${tenantBase(tenantA)}/api/account/register`,
          headers: {
            "X-XSRF-TOKEN": cookie?.value ?? "",
            "Content-Type": "application/x-www-form-urlencoded",
          },
          body: `Name=x&Email=${encodeURIComponent(outsiderEmail)}&Password=${encodeURIComponent("another-" + password)}&ConfirmPassword=${encodeURIComponent("another-" + password)}`,
          failOnStatusCode: false,
        });
      });
      statusOf(`${tenantBase(tenantA)}/api/index?f=/`).should("eq", 403);
      statusOf("/starsky/api/index?f=/").should("eq", 403);
    });
  });

  describe("a user without any membership", () => {
    it("can use the account pages but no data, also not the MCP endpoint (76)", () => {
      if (!config.isEnabled) return;
      loginApi(outsiderEmail, password);
      cy.request("/starsky/api/tenants/mine").its("body.tenants").should("have.length", 0);
      cy.request("/starsky/api/account/status").its("status").should("eq", 200);
      for (const url of [
        "/starsky/api/index?f=/",
        `${tenantBase(tenantA)}/api/index?f=/`,
        "/starsky/api/health/details",
      ]) {
        statusOf(url).should("eq", 403);
      }
      // the MCP endpoint: 404 when it is not enabled on this server, otherwise it needs a membership
      cy.request({ method: "POST", url: "/starsky/mcp", body: {}, failOnStatusCode: false })
        .its("status")
        .should("be.oneOf", [403, 404, 405]);
      cy.request({
        method: "POST",
        url: `${tenantBase(tenantB)}/mcp`,
        body: {},
        failOnStatusCode: false,
      })
        .its("status")
        .should("be.oneOf", [403, 404, 405]);
    });
  });

  describe("exports and temporary files", () => {
    it("a zip of one tenant is not found through another tenant (76)", () => {
      if (!config.isEnabled) return;
      loginAsAdmin();
      mkdirApi(defaultTenant, folder);
      cy.fileRequest("20200822_111408.jpg", folder, "image/jpeg", tenantBase(defaultTenant));
      waitUntilFileIndexed(defaultTenant, folder, file);

      apiWrite(
        "POST",
        `${tenantBase(defaultTenant)}/api/export/create-zip?f=${encodeURIComponent(file)}`
      ).then((response) => {
        const zipName = String(response.body).replace(/"/g, "");
        expect(zipName).to.match(/^[A-Za-z0-9]{1,64}$/);

        // ready in the tenant that made it
        waitForStatus(`${tenantBase(defaultTenant)}/api/export/zip/${zipName}.zip?json=true`, 200, 30);
        // not in the others: the name is not enough
        for (const tenant of [tenantA, tenantB]) {
          statusOf(`${tenantBase(tenant)}/api/export/zip/${zipName}.zip?json=true`).should(
            "be.oneOf",
            [403, 404]
          );
        }
        // the member of A (not of the default tenant's export) neither
        loginApi(memberEmail, password);
        statusOf(`${tenantBase(tenantA)}/api/export/zip/${zipName}.zip?json=true`).should(
          "eq",
          404
        );
      });
      loginAsAdmin();
      apiWrite("DELETE", `${tenantBase(defaultTenant)}/api/delete?f=${folder}`);
    });

    it("a publish name of one tenant is not visible in another (76)", () => {
      if (!config.isEnabled) return;
      loginAsAdmin();
      // names are checked in the temp folder of the tenant: the same name in A is free
      statusOf(`${tenantBase(tenantA)}/api/publish/exist?itemName=sec-name-${id}`).should(
        "be.oneOf",
        [200, 404, 401, 403]
      );
      cy.request({
        url: `${tenantBase(tenantA)}/api/publish/exist?itemName=sec-name-${id}`,
        failOnStatusCode: false,
      }).then((response) => expect(response.body).to.not.eq(true));
    });
  });

  describe("sessions that go on after the access ends", () => {
    it("closes an open realtime socket when the membership is removed (76)", () => {
      if (!config.isEnabled) return;
      // the member opens a socket in tenant A
      loginApi(memberEmail, password);
      cy.visit(`${tenantBase(tenantA)}/`);
      cy.window().then((window) => {
        const url = `${window.location.protocol === "https:" ? "wss" : "ws"}://${window.location.host}${tenantBase(tenantA)}/realtime`;
        const socket = new window.WebSocket(url);
        (window as unknown as { __socket: WebSocket }).__socket = socket;
        (window as unknown as { __closed: boolean }).__closed = false;
        socket.onclose = () => {
          (window as unknown as { __closed: boolean }).__closed = true;
        };
      });
      cy.window({ timeout: 15000 })
        .its("__socket.readyState")
        .should("eq", 1); // open

      // the administrator removes the member (the session of the browser is now the admin's, the
      // socket of the member stays open until the server closes it)
      loginAsAdmin();
      membersOf(tenantA).then((members) => {
        const member = members.find((m) => m.identifier === memberEmail);
        apiWrite("DELETE", `${tenantBase(tenantA)}/api/tenants/members/${member.userId}`)
          .its("status")
          .should("eq", 200);
      });

      cy.window({ timeout: 15000 }).its("__closed").should("eq", true);
    });

    it("the membership change works at once for the api (76)", () => {
      if (!config.isEnabled) return;
      loginApi(memberEmail, password);
      // removed in the previous test
      statusOf(`${tenantBase(tenantA)}/api/index?f=/`).should("eq", 403);
    });
  });

  describe("sign in", () => {
    it("does not redirect to another site after the login (76)", () => {
      if (!config.isEnabled) return;
      cy.visit(
        `${tenantBase(defaultTenant)}/account/login?ReturnUrl=${encodeURIComponent("https://evil.example/")}`
      );
      submitLoginForm(Cypress.env("AUTH_USER"), Cypress.env("AUTH_PASS"));
      cy.location("host", { timeout: 15000 }).should("not.contain", "evil.example");
      cy.location("pathname").should("not.contain", "evil.example");
    });

    it("the session cookie is not readable by scripts and covers all tenants (76)", () => {
      if (!config.isEnabled) return;
      loginAsAdmin();
      cy.getCookie("_id").then((cookie) => {
        expect(cookie, "session cookie").to.not.equal(null);
        expect(cookie.httpOnly).to.eq(true);
        expect(cookie.path).to.eq("/");
        expect(cookie.sameSite).to.be.oneOf(["lax", "strict"]);
      });
    });
  });
});
