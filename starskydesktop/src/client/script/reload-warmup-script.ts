/* eslint-disable @typescript-eslint/ban-types */
import UrlQuery from "../../app/config/url-query";

/**
 * no slash at end
 * @param {*} domainUrl
 * @param {*} count
 * @param {*} maxCount
 */
export function warmupScript(
  domainUrl: string,
  count: number,
  maxCount: number,
  callback: Function
): void {
  function next() {
    if (count <= maxCount) {
      // eslint-disable-next-line no-plusplus, no-param-reassign
      count++;
      setTimeout(() => {
        warmupScript(domainUrl, count, maxCount, callback);
      }, 200);
    } else {
      console.log(
        "no connection to the internal component, please restart the application"
      );
      callback(false);
    }
  }

  fetch(domainUrl + new UrlQuery().HealthApi())
    .then((response) => {
      if (response.status === 200 || response.status === 503) {
        // eslint-disable-next-line @typescript-eslint/no-floating-promises
        response.text().then((text) => {
          console.log(text);
          callback(text.includes(new UrlQuery().HealthShouldContain()));
        });
        return;
      }
      next();
    })
    .catch((error) => {
      console.log("error", error);
      next();
    });
}
