import { IUseLocation } from "../hooks/use-location/interfaces/IUseLocation";
import { URLPath } from "./url-path";

export class Sidebar {
  private setSidebar: React.Dispatch<
    React.SetStateAction<boolean | undefined>
  > = () => {
    /* should do nothing */
  };
  private history: IUseLocation;

  constructor(
    setSidebar: React.Dispatch<React.SetStateAction<boolean | undefined>>,
    history: IUseLocation
  ) {
    this.setSidebar = setSidebar;
    this.history = history;
  }

  /**
   * Toggle the labels button
   */
  public toggleSidebar() {
    const urlObject = new URLPath().StringToIUrl(this.history.location.search);
    urlObject.sidebar = !urlObject.sidebar;

    this.setSidebar(urlObject.details);
    this.history.navigate(new URLPath().IUrlToString(urlObject), {
      replace: true
    });
  }
}
