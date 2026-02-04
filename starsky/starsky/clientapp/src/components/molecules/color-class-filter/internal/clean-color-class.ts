import { URLPath } from "../../../../shared/url/url-path";

export function CleanColorClass(propsSubPath: string, historyLocationSearch: string): string {
  if (!propsSubPath) return "/";
  const urlObject = new URLPath().StringToIUrl(historyLocationSearch);
  urlObject.colorClass = [];
  return new URLPath().IUrlToString(urlObject);
}
