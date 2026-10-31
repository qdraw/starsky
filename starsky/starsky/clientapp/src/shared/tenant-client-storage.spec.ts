import { newIArchive } from "../interfaces/IArchive";
import { ClipboardHelper } from "./clipboard-helper";
import { FileListCache } from "./filelist-cache";

const goTo = (path: string) => globalThis.history.pushState({}, "", path);

describe("client storage per tenant", () => {
  beforeEach(() => sessionStorage.clear());
  afterEach(() => {
    goTo("/");
    sessionStorage.clear();
  });

  describe("FileListCache", () => {
    it("the default tenant keeps the original key", () => {
      goTo("/starsky/");
      const key = new FileListCache().CacheKeyGenerator({ f: "/" });
      expect(key.startsWith("starsky;c")).toBe(true);
      expect(key).not.toContain("t:");
    });

    it("a tenant has its own key", () => {
      goTo("/starsky/t/acme/");
      const key = new FileListCache().CacheKeyGenerator({ f: "/" });
      expect(key.startsWith("starsky;t:acme;")).toBe(true);
    });

    it("the cache of one tenant is not shown in another tenant (same path)", () => {
      const archive = { ...newIArchive(), subPath: "/", searchQuery: "default-content" };

      goTo("/starsky/");
      new FileListCache().CacheSet("?f=/", archive);
      expect(new FileListCache().CacheGet("?f=/")).not.toBeNull();

      goTo("/starsky/t/acme/");
      expect(new FileListCache().CacheGet("?f=/")).toBeNull();

      goTo("/starsky/t/beta/");
      expect(new FileListCache().CacheGet("?f=/")).toBeNull();

      goTo("/starsky/");
      expect((new FileListCache().CacheGet("?f=/") as { searchQuery: string }).searchQuery).toBe(
        "default-content"
      );
    });

    it("clean everything cleans all tenants", () => {
      goTo("/starsky/t/acme/");
      new FileListCache().CacheSet("?f=/", newIArchive());
      goTo("/starsky/");
      new FileListCache().CacheSet("?f=/", newIArchive());

      new FileListCache().CacheCleanEverything();

      expect(sessionStorage).toHaveLength(0);
    });
  });

  describe("ClipboardHelper", () => {
    const element = (text: string) =>
      ({ current: { innerText: text } }) as unknown as React.RefObject<HTMLDivElement>;

    it("what is copied in one tenant is not pasted in another", () => {
      goTo("/starsky/");
      new ClipboardHelper().Copy(element("tag"), element("description"), element("title"));
      expect(new ClipboardHelper().Read()?.tags).toBe("tag");

      goTo("/starsky/t/acme/");
      expect(new ClipboardHelper().Read()).toBeNull();

      new ClipboardHelper().Copy(element("acme-tag"), element("d"), element("t"));
      expect(new ClipboardHelper().Read()?.tags).toBe("acme-tag");

      goTo("/starsky/");
      expect(new ClipboardHelper().Read()?.tags).toBe("tag");
    });
  });
});
