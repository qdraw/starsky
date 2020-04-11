import { IUseLocation } from '../hooks/use-location';
import { URLPath } from './url-path';

export class Sidebar {

  private setSidebar: React.Dispatch<React.SetStateAction<boolean | undefined>> = () => { };
  private history: IUseLocation;
  private sidebar: boolean | undefined;

  constructor(sidebar: boolean | undefined,
    setSidebar: React.Dispatch<React.SetStateAction<boolean | undefined>>,
    history: IUseLocation) {

    this.sidebar = sidebar;
    this.setSidebar = setSidebar;
    this.history = history;
  }

  /**
   * Toggle the labels button
   */
  public toggleSidebar() {
    var urlObject = new URLPath().StringToIUrl(this.history.location.search);
    urlObject.sidebar = !urlObject.sidebar;

    this.setSidebar(urlObject.details);
    this.history.navigate(new URLPath().IUrlToString(urlObject), { replace: true })
  }
}