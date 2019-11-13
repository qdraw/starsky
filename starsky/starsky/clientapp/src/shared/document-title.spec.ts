import { IArchiveProps } from '../interfaces/IArchiveProps';
import { PageType } from '../interfaces/IDetailView';
import DocumentTitle from './document-title';

describe("document-title", () => {
  describe("SetDocumentTitle", () => {
    it("do nothing / do not fail", () => {
      new DocumentTitle().SetDocumentTitle({} as IArchiveProps)
    });

    it("Archive Home", () => {
      var state = { pageType: PageType.Archive, subPath: "/", breadcrumb: ["/"] } as IArchiveProps
      new DocumentTitle().SetDocumentTitle(state)
      expect(document.title).toContain("Home");
    });
    it("Archive Child folder", () => {
      var state = { pageType: PageType.Archive, subPath: "/test", breadcrumb: ["/", "/test"] } as IArchiveProps
      new DocumentTitle().SetDocumentTitle(state)
      expect(document.title).toContain("test");
    });
    it("Detailview Child folder", () => {
      var state = { pageType: PageType.DetailView, subPath: "/test", breadcrumb: ["/", "/test"] } as IArchiveProps
      new DocumentTitle().SetDocumentTitle(state)
      expect(document.title).toContain("test");
    });

    it("Search", () => {
      var state = { pageType: PageType.Search, breadcrumb: ["/", "search"] } as IArchiveProps
      new DocumentTitle().SetDocumentTitle(state)
      expect(document.title).toContain("search");
    });
  });
});