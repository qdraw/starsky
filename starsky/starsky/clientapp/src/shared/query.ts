import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import { newIUrl } from '../interfaces/IUrl';
import { URLPath } from './url-path';


export class Query {

  private urlReplacePath(input: string): string {
    let output = input.replace("#", "");
    return output.replace(/\+/ig, "%2B");
  }

  public UrlQuerySearchApi = (query: string, pageNumber = 0) => {
    return "/search?json=true&t=" + query + "&p=" + pageNumber;
  }

  public UrlQueryServerApi = (historyLocationHash: string) => {
    var requested = new URLPath().StringToIUrl(historyLocationHash);

    var urlObject = newIUrl();
    if (requested.f) {
      urlObject.f = requested.f;
    }
    if (requested.colorClass) {
      urlObject.colorClass = requested.colorClass;
    }
    // Not needed in API, but the context is used in detailview (without this the results in issues in the sidemenu)
    if (requested.details) {
      urlObject.details = requested.details;
    }
    var url = new URLPath().RemovePrefixUrl(new URLPath().IUrlToString(urlObject));
    return "/api/index" + url;
  }

  public UrlQueryInfoApi = (subPath: string): string => {
    if (!subPath) return "";
    var url = this.urlReplacePath(subPath);
    if (url === "") {
      url = "/";
    }
    return "/api/info?f=" + url + "&json=true";
  }

  private urlQueryUpdateApi = (subPath: string) => {
    return "/api/update";
  }

  public queryUpdateApi = (subPath: string, type: string | undefined, value: string): Promise<IFileIndexItem[]> => {
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

  public UrlQueryThumbnailApi = (fileHash: string) => {
    return "/api/thumbnail/" + fileHash + "?json=true";
  }

  // http://localhost:5000/api/downloadPhoto?f=%2F__starsky%2F0001-readonly%2F4.jpg&isThumbnail=True
  public UrlDownloadPhotoApi = (f: string, isThumbnail: boolean = true) => {
    return "/api/downloadPhoto?f=" + f + "&isThumbnail=" + isThumbnail;
  }

  public UrlExportPostZipApi = () => {
    return "/export/createZip/"
  }

  // export/zip/SR497519527.zip?json=true
  public UrlExportZipApi = (createZipId: string, json: boolean = true) => {
    return "/export/zip/" + createZipId + ".zip?json=" + json;
  }

  public queryInfoApi = (subPath: string): Promise<IFileIndexItem[]> => {

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
