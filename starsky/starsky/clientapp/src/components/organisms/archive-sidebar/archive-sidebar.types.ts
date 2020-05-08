import { PageType } from '../../../interfaces/IDetailView';
import { IFileIndexItem } from '../../../interfaces/IFileIndexItem';

export interface IArchiveSidebarProps {
  fileIndexItems: Array<IFileIndexItem>,
  colorClassUsage: Array<number>,
  subPath: string;
  isReadOnly: boolean;
  pageType: PageType;
}