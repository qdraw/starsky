import { IArchiveProps } from "../interfaces/IArchiveProps";
import { PageType } from "../interfaces/IDetailView";
import DocumentTitle from "./document-title";

describe("document-title", () => {
  describe("SetDocumentTitle", () => {
    it("do nothing / do not fail", () => {
      new DocumentTitle().SetDocumentTitle({} as IArchiveProps);
    });

    it("Archive Home", () => {
      var state = {
        pageType: PageType.Archive,
        subPath: "/",
        breadcrumb: ["/"]
      } as IArchiveProps;
      new DocumentTitle().SetDocumentTitle(state);
      expect(document.title).toContain("Home");
    });
    it("Archive Child folder", () => {
      var state = {
        pageType: PageType.Archive,
        subPath: "/test",
        breadcrumb: ["/", "/test"]
      } as IArchiveProps;
      new DocumentTitle().SetDocumentTitle(state);
      expect(document.title).toContain("test");
    });
    it("Detailview Child folder", () => {
      var state = {
        pageType: PageType.DetailView,
        subPath: "/test",
        breadcrumb: ["/", "/test"]
      } as IArchiveProps;
      new DocumentTitle().SetDocumentTitle(state);
      expect(document.title).toContain("test");
    });

    it("Search fallback", () => {
      var state = {
        pageType: PageType.Search,
        breadcrumb: ["/", "search"]
      } as IArchiveProps;
      new DocumentTitle().SetDocumentTitle(state);
      expect(document.title).toContain("search");
    });

    it("Search fallback 2", () => {
      var state = {
        pageType: PageType.Search,
        breadcrumb: ["/", "search"],
        searchQuery: undefined
      } as IArchiveProps;
      new DocumentTitle().SetDocumentTitle(state);
      expect(document.title).toContain("search");
    });

    it("Search with title", () => {
      var state = {
        pageType: PageType.Search,
        breadcrumb: ["/", "search"],
        searchQuery: "test"
      } as IArchiveProps;
      new DocumentTitle().SetDocumentTitle(state);
      expect(document.title).toContain("test");
    });

    it("Trash with trash", () => {
      var state = {
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

      var title = new DocumentTitle().GetDocumentTitle("test");

      expect(title).toContain("dummy.com");
    });
  });
});
