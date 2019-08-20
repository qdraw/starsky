import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import { newIUrl } from '../interfaces/IUrl';
import { URLPath } from './url-path';


export class Query {

  private urlReplacePath(input: string): string {
    let output = input.replace("#", "");
    return output.replace(/\+/ig, "%2B");
  }


  public UrlQueryServerApi = (historyLocationHash: string) => {
    var requested = new URLPath().StringToIUrl(historyLocationHash);

    var urlObject = newIUrl();
    if (requested.f) {
      urlObject.f = requested.f;
    }
    urlObject.colorClass = requested.colorClass;

    var url = new URLPath().RemovePrefixUrl(new URLPath().IUrlToString(urlObject));
    return "/api/" + url;
  }

  public UrlQueryInfoApi = (subPath: string) => {
    var url = this.urlReplacePath(subPath);
    if (url === "") {
      url = "/";
    }
    return "/api/info?f=" + url + "&json=true";
  }

  private urlQueryUpdateApi = (subPath: string) => {
    return "/api/update";
  }

  queryUpdateApi = (subPath: string, type: string | undefined, value: string): Promise<IFileIndexItem[]> => {
    let location = this.urlQueryUpdateApi(subPath);
    let post = "f=" + this.urlReplacePath(subPath) + "&" + type + "=" + value;
    var controller = new AbortController();

    return new Promise(
      // The resolver function is called with the ability to resolve or
      // reject the promise
      function (resolve, reject) {
        if (!subPath || !type || !value) reject();

        fetch(location, {
          signal: controller.signal,
          method: "POST",
          headers: {
            "Content-Type": "application/x-www-form-urlencoded"
          },
          body: post,
          credentials: "include"
        })
          .then(function (response) {
            return response;
          })
          .then(function (response) {
            return response.json();
          })
          .then(function (data) {
            let detailView: IFileIndexItem[] = data;
            resolve(detailView);
          });
      });
  }

  queryInfoApi = (subPath: string): Promise<IFileIndexItem[]> => {

    let location = this.UrlQueryInfoApi(subPath);

    return new Promise(
      // The resolver function is called with the ability to resolve or
      // reject the promise
      function (resolve, reject) {
        fetch(location, {
          credentials: "include"
        })
          .then(function (response) {
            return response;
          })
          .then(function (response) {
            return response.json();
          })
          .then(function (data) {

            let detailView: IFileIndexItem[] = data;
            // data.status.
            resolve(detailView);

          });
      });
  }
}
