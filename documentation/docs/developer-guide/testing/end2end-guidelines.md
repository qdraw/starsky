# End-to-end test guidelines

These guidelines are derived from patterns that caused flakiness in the Cypress
end-to-end suite and the fixes that resolved them. Follow them when writing new
tests or reviewing existing ones.

## 1. Poll for state; never assert immediately after a write

Backend writes (upload, trash, delete, update) are processed asynchronously by a
background queue. A fixed `cy.wait(N)` will pass on a fast machine and fail on a
slow one.

**Bad**
```ts
cy.get("[data-test=trash]").click();
cy.wait(500);
cy.request(urlApi).then(res =>
  expect(res.body.fileIndexItems.some(...)).to.be.false // may be too early
);
```

**Good** — write a retry loop that checks the actual condition:
```ts
function waitUntilFileAbsent(filePath: string, index = 0, max = 15) {
  cy.request(config.urlApiCollectionsFalse).then(response => {
    const items: Array<{ filePath: string }> = response.body.fileIndexItems ?? [];
    if (!items.some(i => i.filePath === filePath)) return;
    cy.wait(1500);
    if (++index < max) waitUntilFileAbsent(filePath, index, max);
    else expect(true, `${filePath} should be absent`).to.be.false;
  });
}
```

Use the same pattern for uploads (`waitUntilBothIndexed`) and for cache
propagation (`waitUntilCollectionReady`).

---

## 2. Check specific file paths, not total counts

Other suites leave files in the shared test folder. A count check (`length >= 3`)
passes with the wrong files or fails because extra files are present.

**Bad**
```ts
if (response.body.fileIndexItems.length >= 3) return;
```

**Good**
```ts
const allPresent = expectedPaths.every(fp =>
  items.some(i => i.filePath === fp)
);
if (allPresent) return;
```

---

## 3. Use content-based selectors, not positional ones

`eq(N)` is fragile: the position of an element changes when other tests leave
items behind or when the rendering order shifts.

**Bad**
```ts
cy.get("[data-test=collections]").eq(1).click();
```

**Good**
```ts
cy.contains("[data-test=collections]", fileNameMp4).scrollIntoView().click();
```

---

## 4. Wait for buttons to become enabled before clicking

Modal action buttons (Move, Submit, etc.) are disabled until async state
settles — a folder selection, a validation check, an API response. Clicking a
disabled button is silently ignored and the test hangs.

**Bad**
```ts
cy.get("[data-test=btn-child_folder]").click();
cy.get("[data-test=modal-move-file-btn-default]").click(); // may still be disabled
```

**Good**
```ts
cy.get("[data-test=btn-child_folder]", { timeout: 15000 }).click();
cy.get("[data-test=modal-move-file-btn-default]")
  .should("not.be.disabled")
  .click();
```

---

## 5. Give async-loaded modal content a real timeout

Folder lists, collection members, and sidebar content are fetched from the API
after the modal opens. The default Cypress retry timeout (4 s) is not enough on
Windows CI where the machine is slower.

Use at least `{ timeout: 15000 }` on any element that appears as the result of
an API call inside a modal:

```ts
cy.get("[data-test=parent]", { timeout: 15000 }).click();
cy.get("[data-test=collections]").should("have.length", 2);
```

---

## 6. Prefer DOM-based waits over `cy.intercept` for reads

`cy.intercept` on GET requests is unreliable across platforms: on Windows the
browser often serves the response from its own cache, so the network request
never fires and `cy.wait("@alias")` times out.

**Unreliable on Windows**
```ts
cy.intercept("GET", "**/api/info*").as("info");
cy.wait("@info"); // silently skipped when served from browser cache
```

**Reliable on all platforms**
```ts
// Assert the DOM element that depends on the data
cy.get("[data-test=collections]").should("have.length", 2);

// Or poll the API directly until the expected value appears
waitUntilCollectionReady(0);
```

Reserve `cy.intercept` + `cy.wait` for **write** operations (POST, DELETE,
PUT) where you need to confirm the server accepted the request before
proceeding.

---

## 7. Set up intercepts before triggering the action

An intercept registered after the click can miss the request if the browser
dispatches it synchronously.

**Bad**
```ts
cy.get("[data-test=trash]").click();
cy.intercept("**/api/trash/move-to-trash").as("trash"); // too late
cy.wait("@trash");
```

**Good**
```ts
cy.intercept("**/api/trash/move-to-trash").as("trash");
cy.get("[data-test=trash]").click();
cy.wait("@trash");
```

---

## 8. Assert each contenteditable field's value before moving to the next

React's `onBlur` handler updates component state after each field is left. That
state change triggers a re-render. On Windows CI, Cypress types fast enough
that the re-render from field N can race with Cypress focusing field N+1,
wiping the just-typed value.

Assert the current field's text after blur to ensure React has settled before
the next field is touched:

```ts
function typeInField(selector: string, value: string) {
  cy.get(selector).focus();
  cy.get(selector)
    .type("{selectall}")
    .type(value, { parseSpecialCharSequences: false });
  cy.get(selector).blur();
  cy.get(selector).should("have.text", value); // wait for React re-render
}
```

This is only necessary when filling multiple fields in sequence. Single-field
edits are not affected.

---

## 9. Clear sessionStorage before reloading to verify persistence

Starsky's `FileListCache` stores index responses in `sessionStorage` with a
3-minute TTL. A reload without clearing the cache can return stale data,
making a persistence check pass incorrectly or fail when the backend has not
yet written to disk.

```ts
cy.then(() => { sessionStorage.clear(); });
cy.reload();
```

`cy.resetStorage()` (called in `beforeEach`) clears both `localStorage` and
`sessionStorage`; the manual clear above is needed mid-test after a write.

---

## 10. Make cleanup API-based, not UI-based

If any step in the suite fails partway through, files may end up in unexpected
locations. A UI-based cleanup (visit folder → select → trash) will not find
them and the cleanup test itself will fail, polluting the next run.

Use a direct API DELETE that lists every possible location with
`failOnStatusCode: false`:

```ts
cy.request({
  failOnStatusCode: false,
  method: "DELETE",
  url: "/starsky/api/delete",
  qs: {
    f: [
      `/starsky-end2end-test/${fileName}`,
      `/starsky-end2end-test/child_folder/${fileName}`,
      `/starsky-end2end-test/child_folder`,
    ].join(";"),
  },
});
```
