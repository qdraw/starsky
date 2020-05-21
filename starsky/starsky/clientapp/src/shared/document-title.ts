import { IArchiveProps } from '../interfaces/IArchiveProps';
import { IDetailView, PageType } from '../interfaces/IDetailView';
import BrowserDetect from './browser-detect';

export class DocumentTitle {
  public SetDocumentTitle = (archive: IArchiveProps | IDetailView): void => {
    if (!archive.breadcrumb || !archive.pageType) return;

    var name = archive.breadcrumb[archive.breadcrumb.length - 1];

    // The breadcrumb implementation of Archive/DetailView does not include the current item
    if (archive.subPath && (archive.pageType === PageType.Archive || archive.pageType === PageType.DetailView)) {
      name = archive.subPath.split("/")[archive.subPath.split("/").length - 1];
      if (name.length === 0) {
        name = "Home";
      }
    }

    document.title = this.GetDocumentTitle(name);
  }

  public SetDocumentTitlePrefix = (prefix: string): void => {
    document.title = this.GetDocumentTitle(prefix);
  }

  public GetDocumentTitle = (prefix: string): string => {
    if (!prefix) return "Starsky App";
    prefix += " - Starsky App";
    if (new BrowserDetect().IsElectronApp() && window.location.hostname !== 'localhost') {
      prefix += ` - ${window.location.hostname}`;
    }
    return prefix;
  }
}
export default DocumentTitle;

