import { IArchiveProps } from "../../../../interfaces/IArchiveProps.ts";
import { SelectCheckIfActive } from "../../../../shared/select-check-if-active.ts";
import { URLPath } from "../../../../shared/url/url-path.ts";

export function GetFilterUrlColorClass(
  item: number,
  historyLocationSearch: string,
  state: IArchiveProps
): string {
  const urlObject = new URLPath().StringToIUrl(historyLocationSearch);

  urlObject.colorClass ??= [];

  if (urlObject.colorClass?.includes(item)) {
    const index = urlObject.colorClass.indexOf(item);
    if (index !== -1) urlObject.colorClass.splice(index, 1);
  } else {
    urlObject.colorClass.push(item);
  }

  if (urlObject.select)
    urlObject.select = new SelectCheckIfActive().IsActive(
      urlObject.select,
      urlObject.colorClass,
      state.fileIndexItems
    );

  return new URLPath().IUrlToString(urlObject);
}
