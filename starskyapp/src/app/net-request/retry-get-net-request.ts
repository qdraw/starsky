import UrlQuery from "../../app/config/url-query";
import { GetNetRequest } from "./get-net-request";

/**
 * no slash at end
 * @param {*} domainUrl
 * @param {*} count
 * @param {*} maxCount
 */
export function RetryGetNetRequest(
  domainUrl: string,
  count: number,
  maxCount: number,
  callback: Function
): void {
  console.log('--fetch');
  
  GetNetRequest(domainUrl + new UrlQuery().HealthApi())
    .then((response) => {
      console.log('-any respone');
      
      if (response.statusCode === 200 || response.statusCode === 503) {
        callback(response.data.includes(new UrlQuery().HealthShouldContain()))
        return;
      }
      next();
    })
    .catch((error) => {
      console.log("error", error);
      next();
    });

  function next() {
    if (count <= maxCount) {
      count++;
      setTimeout(() => {
        RetryGetNetRequest(domainUrl, count, maxCount, callback);
      }, 200);
    } else {
      console.log(
        "no connection to the internal component, please restart the application"
      );
      callback(false);
    }
  }
}
