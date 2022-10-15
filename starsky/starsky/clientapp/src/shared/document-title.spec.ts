import { IArchiveProps } from "../interfaces/IArchiveProps";
import { PageType } from "../interfaces/IDetailView";
import DocumentTitle from "./document-title";

describe("document-title", () => {
  describe("SetDocumentTitle", () => {
    it("do nothing / do not fail", () => {
      new DocumentTitle().SetDocumentTitle({} as IArchiveProps);
    });

    it("Archive Home", () => {
      const state = {
        pageType: PageType.Archive,
        subPath: "/",
        breadcrumb: ["/"]
      } as IArchiveProps;
      new DocumentTitle().SetDocumentTitle(state);
      expect(document.title).toContain("Home");
    });
    it("Archive Child folder", () => {
      const state = {
        pageType: PageType.Archive,
        subPath: "/test",
        breadcrumb: ["/", "/test"]
      } as IArchiveProps;
      new DocumentTitle().SetDocumentTitle(state);
      expect(document.title).toContain("test");
    });
    it("Detailview Child folder", () => {
      const state = {
        pageType: PageType.DetailView,
        subPath: "/test",
        breadcrumb: ["/", "/test"]
      } as IArchiveProps;
      new DocumentTitle().SetDocumentTitle(state);
      expect(document.title).toContain("test");
    });

    it("Search fallback", () => {
      const state = {
        pageType: PageType.Search,
        breadcrumb: ["/", "search"]
      } as IArchiveProps;
      new DocumentTitle().SetDocumentTitle(state);
      expect(document.title).toContain("search");
    });

    it("Search fallback 2", () => {
      const state = {
        pageType: PageType.Search,
        breadcrumb: ["/", "search"],
        searchQuery: undefined
      } as IArchiveProps;
      new DocumentTitle().SetDocumentTitle(state);
      expect(document.title).toContain("search");
    });

    it("Search with title", () => {
      const state = {
        pageType: PageType.Search,
        breadcrumb: ["/", "search"],
        searchQuery: "test"
      } as IArchiveProps;
      new DocumentTitle().SetDocumentTitle(state);
      expect(document.title).toContain("test");
    });

    it("Trash with trash", () => {
      const state = {
        pageType: PageType.Trash,
        breadcrumb: ["/", "search"],
        searchQuery: "!delete!"
      } as IArchiveProps;
      new DocumentTitle().SetDocumentTitle(state);
      expect(document.title).toContain("Trash");
    });

    it("GetDocumentTitle title Electron", () => {
      const url = "https://dummy.com/";
      Object.defineProperty(window, "location", {
        value: new URL(url)
      });

      Object.defineProperty(navigator, "userAgent", {
        value: "Electron starsky/"
      });

      const title = new DocumentTitle().GetDocumentTitle("test");

      expect(title).toContain("dummy.com");
    });
  });
});
