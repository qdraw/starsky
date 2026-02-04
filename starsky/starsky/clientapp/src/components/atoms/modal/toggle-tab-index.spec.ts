import { toggleTabIndex } from "./toggle-tab-index";

describe("toggleTabIndex", () => {
  it("test set tabIndex", () => {
    const div = document.createElement("div");
    const aHref = document.createElement("a");
    div.appendChild(aHref);

    toggleTabIndex("off", div);
    expect(aHref.tabIndex).toBe(-1);
  });

  it("test remove tabIndex", () => {
    const div = document.createElement("div");
    const aHref = document.createElement("a");
    div.appendChild(aHref);

    toggleTabIndex("off", div);
    expect(aHref.tabIndex).toBe(-1);

    toggleTabIndex("on", div);

    expect(aHref.tabIndex).toBe(0);
  });
});
