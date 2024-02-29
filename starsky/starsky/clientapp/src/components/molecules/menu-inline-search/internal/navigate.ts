import { IUseLocation } from "../../../../hooks/use-location/interfaces/IUseLocation";
import { UrlQuery } from "../../../../shared/url/url-query";

/** Go to different search page */
function Navigate(
  history: IUseLocation,
  setFormFocus: React.Dispatch<React.SetStateAction<boolean>>,
  inputFormControlReference: React.RefObject<HTMLInputElement>,
  query: string,
  callback?: (query: string) => void
) {
  // To do change to search page
  history.navigate(new UrlQuery().UrlSearchPage(query));
  setFormFocus(false);

  // force update input field after navigate to page (only the input item)
  (inputFormControlReference.current as HTMLInputElement).value = query;

  if (!callback) return;
  callback(query);
}

export default Navigate;
