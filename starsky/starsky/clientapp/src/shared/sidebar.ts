import { IUseLocation } from "../hooks/use-location/interfaces/IUseLocation";
import { URLPath } from "./url/url-path";

export class Sidebar {
  private readonly setSidebar: React.Dispatch<React.SetStateAction<boolean | undefined>> = () => {
    /* should do nothing */
  };
  private readonly history: IUseLocation;

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
  public toggleSidebar(state?: boolean) {
    const urlObject = new URLPath().StringToIUrl(this.history.location.search);
    if (state === undefined) {
      urlObject.sidebar = !urlObject.sidebar;
    } else {
      urlObject.sidebar = state;
    }

    this.setSidebar(urlObject.details);
    this.history.navigate(new URLPath().IUrlToString(urlObject), {
      replace: true
    });
  }
}
