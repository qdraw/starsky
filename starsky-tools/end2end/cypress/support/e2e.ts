// ***********************************************************
// This example support/index.js is processed and
// loaded automatically before your test files.
//
// This is a great place to put global configuration and
// behavior that modifies Cypress.
//
// You can change the location of this file or turn off
// automatically serving support files with the
// 'supportFile' configuration option.
//
// You can read more here:
// https://on.cypress.io/configuration
// ***********************************************************

// We don't need custom commands as we use TypeScript and can
// create simple modules that we can import with more typesafety

import "./commands";

// Every test logs in again, which the login rate limit (30 per minute per IP) blocks on a full run.
// Switch the limiter off for the spec and put the original value back afterwards.
let useRateLimitingBefore: boolean | null | undefined;

before("disable rate limiting for this spec", () => {
  cy.sendAuthenticationHeader();
  cy.request({ url: "/api/env", failOnStatusCode: false }).then((response) => {
    if (response.status !== 200) {
      return;
    }
    useRateLimitingBefore = response.body.useRateLimiting;
    if (useRateLimitingBefore === false) {
      return;
    }
    cy.request({
      url: "/api/env",
      method: "POST",
      form: true,
      failOnStatusCode: false,
      body: { useRateLimiting: false },
    });
  });
});

after("restore rate limiting", () => {
  if (useRateLimitingBefore === undefined || useRateLimitingBefore === false) {
    return;
  }
  cy.sendAuthenticationHeader();
  cy.request({
    url: "/api/env",
    method: "POST",
    form: true,
    failOnStatusCode: false,
    body: { useRateLimiting: true },
  });
});
