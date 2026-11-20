import { envFolder, envName } from "../../support/commands";
import {
  addMemberApi,
  apiWrite,
  createTenantApi,
  defaultTenant,
  deleteApi,
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
 * One installation, separate libraries: tenants. See documentation/docs/features/multi-tenancy.md
 * All data of this spec uses ids of this run (a deleted tenant keeps its id).
 */
describe("Multi tenancy (75)", () => {
  const id = runId();
  const tenantA = `e2e-a-${id}`;
  const tenantB = `e2e-b-${id}`;
  const memberEmail = `member-${id}@example.test`;
  const outsiderEmail = `outsider-${id}@example.test`;
  const password = `Pw-${id}-e2e-tenant!`;
  const folder = `/e2e-tenancy-${id}`;

  // 20200822_111408 only in A, 20200822_112430 only in default, 20200822_134151 in both
  const onlyA = `${folder}/20200822_111408.jpg`;
  const onlyDefault = `${folder}/20200822_112430.jpg`;
  const inBoth = `${folder}/20200822_134151.jpg`;

  before("create two tenants, a member of one and an outsider of all (75)", () => {
    if (!config.isEnabled) return;
    cy.resetStorage();
    loginAsAdmin();
    createTenantApi(tenantA, "E2E Tenant A");
    createTenantApi(tenantB, "E2E Tenant B");
    registerApi("E2E Member", memberEmail, password);
    registerApi("E2E Outsider", outsiderEmail, password);
    // the tenant administrator (the creator) adds the member to A, only
    addMemberApi(tenantA, memberEmail, "User");
    // new accounts are a member of the default tenant: the outsider is removed from it
    membersOf(defaultTenant).then((members) => {
      const outsider = members.find((m) => m.identifier === outsiderEmail);
      expect(outsider, "outsider is a member of the default tenant").to.not.equal(undefined);
      apiWrite("DELETE", `${tenantBase(defaultTenant)}/api/tenants/members/${outsider.userId}`)
        .its("status")
        .should("eq", 200);
    });
  });

  beforeEach("storage is reset before every test (75)", () => {
    if (!config.isEnabled) return;
    cy.resetStorage();
  });

  after("clean up what this run created (75)", () => {
    if (!config.isEnabled) return;
    loginAsAdmin();
    deleteApi(defaultTenant, [folder]);
    deleteApi(tenantA, [folder]);
  });

  it("the platform administrator lists the tenants (75)", () => {
    if (!config.isEnabled) return;
    loginAsAdmin();
    cy.request("/starsky/api/tenants").then((response) => {
      const ids = (response.body as { tenantId: string }[]).map((t) => t.tenantId);
      expect(ids).to.include.members([tenantA, tenantB]);
    });
  });

  it("a member has a role in each of its tenants (75)", () => {
    if (!config.isEnabled) return;
    loginApi(memberEmail, password);
    cy.request("/starsky/api/tenants/mine").then((response) => {
      const tenants = response.body.tenants as { tenantId: string; roleCode: string }[];
      expect(tenants.map((t) => t.tenantId)).to.have.members([defaultTenant, tenantA]);
      expect(tenants.find((t) => t.tenantId === tenantA).roleCode).to.eq("User");
      expect(tenants.some((t) => t.tenantId === tenantB)).to.eq(false);
    });
  });

  it("a user without a membership has no tenants (75)", () => {
    if (!config.isEnabled) return;
    loginApi(outsiderEmail, password);
    cy.request("/starsky/api/tenants/mine").then((response) => {
      expect(response.body.tenants).to.have.length(0);
      expect(response.body.landing).to.eq(null);
    });
    statusOf("/starsky/api/index?f=/").should("eq", 403);
    statusOf(`${tenantBase(tenantA)}/api/index?f=/`).should("eq", 403);
  });

  it("a member opens its tenant: login on the tenant url returns to the tenant (75)", () => {
    if (!config.isEnabled) return;
    cy.visit(`${tenantBase(tenantA)}/`);
    cy.location("pathname", { timeout: 15000 }).should("include", `/t/${tenantA}/account/login`);
    submitLoginForm(memberEmail, password);
    cy.location("pathname", { timeout: 15000 }).should("eq", `${tenantBase(tenantA)}/`);
    cy.get("[data-test=tenant-no-access]").should("not.exist");
  });

  it("a member with more tenants sees how to switch (75)", () => {
    if (!config.isEnabled) return;
    loginApi(memberEmail, password);
    cy.visit(`${tenantBase(tenantA)}/`);
    cy.get("[data-test=hamburger]", { timeout: 15000 }).click();
    // the other tenant, not the one that is open; a full page load to its home page
    cy.get(`[data-test=default-menu-item-tenant-${defaultTenant}]`, { timeout: 15000 })
      .find("a")
      .should("have.attr", "href", "/starsky/");
    cy.get(`[data-test=default-menu-item-tenant-${tenantA}]`).should("not.exist");
  });

  it("switching goes to the other tenant (75)", () => {
    if (!config.isEnabled) return;
    loginApi(memberEmail, password);
    cy.visit(`${tenantBase(tenantA)}/`);
    cy.get("[data-test=hamburger]", { timeout: 15000 }).click();
    cy.get(`[data-test=default-menu-item-tenant-${defaultTenant}] a`, { timeout: 15000 }).click();
    cy.location("pathname", { timeout: 15000 }).should("eq", "/starsky/");
  });

  it("a member that is not in a tenant gets the choose page, not the data (75)", () => {
    if (!config.isEnabled) return;
    loginApi(memberEmail, password);
    cy.visit(`${tenantBase(tenantB)}/`);
    cy.get("[data-test=tenant-no-access]", { timeout: 15000 }).should("be.visible");
    cy.get(`[data-test=tenant-no-access-${tenantA}]`).should(
      "have.attr",
      "href",
      `${tenantBase(tenantA)}/`
    );
    cy.get(`[data-test=tenant-no-access-${tenantB}]`).should("not.exist");
    statusOf(`${tenantBase(tenantB)}/api/index?f=/`).should("eq", 403);
  });

  it("a user without any tenant is told so (75)", () => {
    if (!config.isEnabled) return;
    loginApi(outsiderEmail, password);
    cy.visit(`${tenantBase(tenantA)}/`);
    cy.get("[data-test=tenant-no-access-none]", { timeout: 15000 }).should("be.visible");
  });

  it("unknown, invalid and other-case tenants are not found, with the same answer (75)", () => {
    if (!config.isEnabled) return;
    loginApi(memberEmail, password);
    for (const tenant of ["does-not-exist", "UPPER", "a_b", "-x", `${tenantA}x`]) {
      statusOf(`/starsky/t/${tenant}/api/index?f=/`).should("eq", 404);
    }
    // an encoded traversal is not a tenant either
    statusOf("/starsky/t/..%2f..%2fetc/api/index?f=/").should("be.oneOf", [400, 404]);
  });

  it("the same folder in two tenants holds different files (75)", () => {
    if (!config.isEnabled) return;
    loginAsAdmin();

    mkdirApi(defaultTenant, folder);
    mkdirApi(tenantA, folder);
    cy.fileRequest("20200822_111408.jpg", folder, "image/jpeg", tenantBase(tenantA));
    cy.fileRequest("20200822_112430.jpg", folder, "image/jpeg", tenantBase(defaultTenant));
    cy.fileRequest("20200822_134151.jpg", folder, "image/jpeg", tenantBase(tenantA));
    cy.fileRequest("20200822_134151.jpg", folder, "image/jpeg", tenantBase(defaultTenant));

    waitUntilFileIndexed(tenantA, folder, onlyA);
    waitUntilFileIndexed(tenantA, folder, inBoth);
    waitUntilFileIndexed(defaultTenant, folder, onlyDefault);
    waitUntilFileIndexed(defaultTenant, folder, inBoth);

    filesOf(tenantA, folder).then((files) => {
      expect(files).to.include.members([onlyA, inBoth]);
      expect(files).to.not.include(onlyDefault);
      expect(files.filter((f) => f === inBoth)).to.have.length(1);
    });
    filesOf(defaultTenant, folder).then((files) => {
      expect(files).to.include.members([onlyDefault, inBoth]);
      expect(files).to.not.include(onlyA);
      expect(files.filter((f) => f === inBoth)).to.have.length(1);
    });
    // the folder does not exist in the third tenant
    statusOf(`${tenantBase(tenantB)}/api/index/?f=${folder}`).should("eq", 404);
  });

  it("the file of the same path is not readable from another tenant (75)", () => {
    if (!config.isEnabled) return;
    loginAsAdmin();
    // only in the default tenant: tenant A has no such file, whatever the default tenant has
    waitUntilFileIndexed(defaultTenant, folder, onlyDefault);
    statusOf(`${tenantBase(defaultTenant)}/api/download-photo?f=${onlyDefault}&isThumbnail=false`).should(
      "eq",
      200
    );
    statusOf(`${tenantBase(tenantA)}/api/download-photo?f=${onlyDefault}&isThumbnail=false`).should(
      "be.oneOf",
      [404, 400]
    );
    statusOf(`${tenantBase(defaultTenant)}/api/download-photo?f=${onlyA}&isThumbnail=false`).should(
      "be.oneOf",
      [404, 400]
    );
  });

  it("simultaneous requests for different tenants never mix their data (75)", () => {
    if (!config.isEnabled) return;
    loginAsAdmin();
    waitUntilFileIndexed(tenantA, folder, onlyA);
    waitUntilFileIndexed(defaultTenant, folder, onlyDefault);
    // a page of the app: the requests are made from its origin, with its cookies
    cy.visit(`${tenantBase(defaultTenant)}/account/login`);
    cy.window().then(async (win) => {
      const tenants = [defaultTenant, tenantA];
      // 80 at the same time, alternating, through the same process
      const results = await Promise.all(
        Array.from({ length: 80 }, async (_, index) => {
          const tenant = tenants[index % 2];
          const response = await win.fetch(`${tenantBase(tenant)}/api/index/?f=${folder}`, {
            credentials: "include",
          });
          const body = await response.json();
          return {
            tenant,
            status: response.status,
            files: (body.fileIndexItems as { filePath: string }[]).map((i) => i.filePath),
          };
        })
      );
      for (const result of results) {
        expect(result.status).to.eq(200);
        expect(result.files).to.include(inBoth);
        if (result.tenant === tenantA) {
          expect(result.files).to.include(onlyA);
          expect(result.files).to.not.include(onlyDefault);
        } else {
          expect(result.files).to.include(onlyDefault);
          expect(result.files).to.not.include(onlyA);
        }
      }
    });
  });

  it("the folder shown in the browser is the one of the tenant (75)", () => {
    if (!config.isEnabled) return;
    loginAsAdmin();
    waitUntilFileIndexed(tenantA, folder, onlyA);
    cy.visit(`${tenantBase(tenantA)}/?f=${folder}`);
    cy.get(`[data-filepath="${onlyA}"]`, { timeout: 20000 }).should("have.length", 1);
    cy.get(`[data-filepath="${onlyDefault}"]`).should("not.exist");

    // the list cache of a tenant is not shown in another tenant
    cy.visit(`${tenantBase(defaultTenant)}/?f=${folder}`);
    cy.get(`[data-filepath="${onlyDefault}"]`, { timeout: 20000 }).should("have.length", 1);
    cy.get(`[data-filepath="${onlyA}"]`).should("not.exist");
  });

  it("login without a target page goes to the tenant used last (75)", () => {
    if (!config.isEnabled) return;
    cy.visit(`${tenantBase(tenantA)}/`);
    submitLoginForm(memberEmail, password);
    cy.location("pathname", { timeout: 15000 }).should("eq", `${tenantBase(tenantA)}/`);

    // the page reports its tenant once: poll until saved
    const waitUntilLastUsed = (attempts = 15) => {
      cy.request("/starsky/api/tenants/mine").then((response) => {
        if (response.body.lastUsed === tenantA) return;
        expect(attempts, "last used tenant is saved").to.be.greaterThan(0);
        cy.wait(1000);
        waitUntilLastUsed(attempts - 1);
      });
    };
    waitUntilLastUsed();

    // log out (the logout link passes the placeholder return url) and log in on the default tenant
    cy.visit("/starsky/account/logout?ReturnUrl=%2F%3Ff%3D%2F");
    cy.location("pathname").should("include", "/account/login");
    submitLoginForm(memberEmail, password);
    cy.location("pathname", { timeout: 15000 }).should("eq", `${tenantBase(tenantA)}/`);
  });

  it("an explicit target page wins over the tenant used last (75)", () => {
    if (!config.isEnabled) return;
    loginApi(memberEmail, password);
    // last used is A (previous test); log in again on the default tenant with a real target
    cy.visit("/starsky/account/logout?ReturnUrl=%2Fstarsky%2Fsearch%3Ft%3Dtest");
    cy.location("pathname").should("include", "/account/login");
    submitLoginForm(memberEmail, password);
    cy.location("pathname", { timeout: 15000 }).should("eq", "/starsky/search");
  });

  it("tenant members: administrators manage, users do not (75)", () => {
    if (!config.isEnabled) return;
    // a member with the role User
    loginApi(memberEmail, password);
    statusOf(`${tenantBase(tenantA)}/api/tenants/members`).should("eq", 403);
    apiWrite("POST", `${tenantBase(tenantA)}/api/tenants/members`, {
      identifier: outsiderEmail,
      role: "User",
    })
      .its("status")
      .should("eq", 403);
    // and no tenants of its own
    apiWrite("POST", "/starsky/api/tenants", { id: `e2e-c-${id}`, displayName: "x" })
      .its("status")
      .should("eq", 403);
    statusOf("/starsky/api/tenants").should("eq", 403);

    // the administrator promotes, and the new administrator can manage
    loginAsAdmin();
    membersOf(tenantA).then((members) => {
      const member = members.find((m) => m.identifier === memberEmail);
      apiWrite("PUT", `${tenantBase(tenantA)}/api/tenants/members/${member.userId}`, {
        role: "Administrator",
      })
        .its("status")
        .should("eq", 200);
    });
    loginApi(memberEmail, password);
    statusOf(`${tenantBase(tenantA)}/api/tenants/members`).should("eq", 200);
    // but only in A
    statusOf(`${tenantBase(tenantB)}/api/tenants/members`).should("eq", 403);
  });

  it("a tenant keeps at least one administrator (75)", () => {
    if (!config.isEnabled) return;
    loginAsAdmin();
    membersOf(tenantB).then((members) => {
      expect(members).to.have.length(1);
      const admin = members[0];
      apiWrite("DELETE", `${tenantBase(tenantB)}/api/tenants/members/${admin.userId}`)
        .its("status")
        .should("eq", 409);
      apiWrite("PUT", `${tenantBase(tenantB)}/api/tenants/members/${admin.userId}`, {
        role: "User",
      })
        .its("status")
        .should("eq", 409);
    });
  });

  it("a membership that is removed works at once (75)", () => {
    if (!config.isEnabled) return;
    loginAsAdmin();
    membersOf(tenantA).then((members) => {
      const member = members.find((m) => m.identifier === memberEmail);
      // there must be another administrator before: the creator is one
      apiWrite("DELETE", `${tenantBase(tenantA)}/api/tenants/members/${member.userId}`)
        .its("status")
        .should("eq", 200);
    });
    loginApi(memberEmail, password);
    statusOf(`${tenantBase(tenantA)}/api/index?f=/`).should("eq", 403);
    cy.request("/starsky/api/tenants/mine").then((response) => {
      expect((response.body.tenants as { tenantId: string }[]).map((t) => t.tenantId)).to.not.include(
        tenantA
      );
    });
  });

  it("a disabled tenant is gone for everybody, and can come back (75)", () => {
    if (!config.isEnabled) return;
    loginAsAdmin();
    apiWrite("POST", `/starsky/api/tenants/${tenantB}/disable`).its("status").should("eq", 200);
    waitForStatus(`${tenantBase(tenantB)}/api/index?f=/`, 404);

    apiWrite("POST", `/starsky/api/tenants/${tenantB}/enable`).its("status").should("eq", 200);
    waitForStatus(`${tenantBase(tenantB)}/api/index?f=/`, 200);
  });

  it("a deleted tenant is gone and can not be enabled again (75)", () => {
    if (!config.isEnabled) return;
    loginAsAdmin();
    apiWrite("DELETE", `/starsky/api/tenants/${tenantB}`).its("status").should("eq", 200);
    waitForStatus(`${tenantBase(tenantB)}/api/index?f=/`, 404);
    apiWrite("POST", `/starsky/api/tenants/${tenantB}/enable`).its("status").should("eq", 409);
    // the id is not free again
    apiWrite("POST", "/starsky/api/tenants", { id: tenantB, displayName: "again" })
      .its("status")
      .should("eq", 409);
  });

  it("a tenant is purged only after delete, with its id as confirmation (75)", () => {
    if (!config.isEnabled) return;
    loginAsAdmin();
    // an active tenant is not purged, whatever the confirmation says
    apiWrite("POST", `/starsky/api/tenants/${tenantA}/purge?confirm=${tenantA}`)
      .its("status")
      .should("eq", 409);
    // the one that is deleting needs the confirmation
    apiWrite("POST", `/starsky/api/tenants/${tenantB}/purge`).its("status").should("eq", 400);
    apiWrite("POST", `/starsky/api/tenants/${tenantB}/purge?confirm=${tenantA}`)
      .its("status")
      .should("eq", 400);
    apiWrite("POST", `/starsky/api/tenants/${tenantB}/purge?confirm=${tenantB}`)
      .its("status")
      .should("eq", 200);

    cy.request("/starsky/api/tenants").then((response) => {
      const tenant = (response.body as { tenantId: string; state: number }[]).find(
        (t) => t.tenantId === tenantB
      );
      expect(tenant.state, "Deleted").to.eq(4);
    });
    // and not twice, the default tenant never
    apiWrite("POST", `/starsky/api/tenants/${tenantB}/purge?confirm=${tenantB}`)
      .its("status")
      .should("eq", 409);
    apiWrite("POST", "/starsky/api/tenants/default/purge?confirm=default")
      .its("status")
      .should("eq", 400);
  });

  it("the default tenant can not be created, disabled or deleted (75)", () => {
    if (!config.isEnabled) return;
    loginAsAdmin();
    apiWrite("POST", "/starsky/api/tenants/default/disable").its("status").should("eq", 400);
    apiWrite("DELETE", "/starsky/api/tenants/default").its("status").should("eq", 400);
    apiWrite("POST", "/starsky/api/tenants", { id: "default", displayName: "x" })
      .its("status")
      .should("eq", 400);
    statusOf("/starsky/api/index?f=/").should("eq", 200);
  });
});
