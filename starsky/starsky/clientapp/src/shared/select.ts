import { IUseLocation } from '../hooks/use-location';
import { IArchiveProps } from '../interfaces/IArchiveProps';
import { URLPath } from './url-path';

export class Select {

  private select: string[] | undefined = [];
  private setSelect: React.Dispatch<React.SetStateAction<string[] | undefined>> = () => { };
  private history: IUseLocation = {} as IUseLocation;
  private state: IArchiveProps;

  constructor(select: string[] | undefined,
    setSelect: React.Dispatch<React.SetStateAction<string[] | undefined>>,
    state: IArchiveProps,
    history: IUseLocation) {
    this.select = select;
    this.setSelect = setSelect;
    this.state = state;
    this.history = history;
  }

  public undoSelection() {
    if (!this.select) return;
    var urlObject = new URLPath().updateSelection(this.history.location.search, []);
    this.setSelect(urlObject.select);
    this.history.navigate(new URLPath().IUrlToString(urlObject), { replace: true });
  }

  /**
   * Select All Items
   */
  public allSelection() {
    if (!this.select) return;
    var updatedSelect = new URLPath().GetAllSelection(this.select, this.state.fileIndexItems);

    var urlObject = new URLPath().updateSelection(this.history.location.search, updatedSelect);
    this.setSelect(urlObject.select);
    this.history.navigate(new URLPath().IUrlToString(urlObject), { replace: true });
  }

  public toggleSelection(fileName: string): void {
    var urlObject = new URLPath().toggleSelection(fileName, this.history.location.search);
    this.history.navigate(new URLPath().IUrlToString(urlObject), { replace: true });
    this.setSelect(urlObject.select);
  }

  /**
   * To remove the sidebar state and select state
   * In the menu Nothing selected and items selected
   */
  public removeSidebarSelection() {
    var urlObject = new URLPath().StringToIUrl(this.history.location.search);
    var selectVar: string[] = urlObject.select ? urlObject.select : [];
    if (!urlObject.select) {
      urlObject.select = [];
    }
    else {
      delete urlObject.sidebar;
      delete urlObject.select;
    }
    if (selectVar) {
      this.setSelect(selectVar);
    }
    this.history.navigate(new URLPath().IUrlToString(urlObject), { replace: true });
  }
}