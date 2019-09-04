import { IArchiveProps } from '../interfaces/IArchiveProps';
import { IDetailView } from '../interfaces/IDetailView';

export class DocumentTitle {
  public SetDocumentTitle = (archive: IArchiveProps | IDetailView): void => {
    if (!archive.breadcrumb || !archive.pageType) return;
    if (archive.subPath && (archive.pageType === "Archive" || archive.pageType === "DetailView")) {
      var folderName = archive.subPath.split("/")[archive.subPath.split("/").length - 1];
      if (folderName.length === 0) {
        folderName = "Home";
      }
      document.title = this.GetDocumentTitle(folderName);
    }
    else if (archive.pageType === "Search" && archive.breadcrumb) {
      var name = archive.breadcrumb[archive.breadcrumb.length - 1]
      document.title = this.GetDocumentTitle(name);
    }
  }

  public GetDocumentTitle = (prefix: string): string => {
    return prefix + " - Starsky App";
  }
}
export default DocumentTitle;

