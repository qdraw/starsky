import FetchPost from "../fetch/fetch-post";
import { URLPath } from "../url/url-path";
import { UrlQuery } from "../url/url-query";

/**
 * clear search cache * when you refresh the search page this is needed to display the correct labels
 * @param historyLocationSearch
 */
export function ClearSearchCache(historyLocationSearch: string) {
  // clear search cache * when you refresh the search page this is needed to display the correct labels
  const searchTag = new URLPath().StringToIUrl(historyLocationSearch).t;
  if (!searchTag) return;
  FetchPost(new UrlQuery().UrlSearchRemoveCacheApi(), `t=${searchTag}`);
}
