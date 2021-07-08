import { net } from "electron";
import logger from "../logger/logger";

export interface IGetNetRequestResponse {
  data?: any;
  error?: any;
  statusCode: number;
}

export function GetNetRequest(
  url: string,
  session: Electron.Session = null
): Promise<IGetNetRequestResponse> {
  return new Promise(function (resolve, reject) {
    const request = net.request({
      url,
      useSessionCookies: session !== null,
      session,
      headers: {
        Accept: "*/*"
      }
    } as any);

    let body = "";
    request.on("response", (response) => {
      response.on("data", (chunk) => {
        body += chunk.toString();
      });
      response.on("end", () => {
        if (
          response.headers &&
          response.headers["content-type"] &&
          response.headers["content-type"].toString().startsWith("text/plain")
        ) {
          resolve({
            data: body,
            statusCode: response.statusCode
          } as IGetNetRequestResponse);
          return;
        }

        try {
          resolve({
            data: JSON.parse(body),
            statusCode: response.statusCode
          } as IGetNetRequestResponse);
        } catch (error) {
          logger.warn("GetNetRequest error");
          logger.warn(error);
          reject({
            error: error.toString(),
            statusCode: response.statusCode
          } as IGetNetRequestResponse);
        }
      });
    });

    request.on("error", (error) => {
      logger.info(error);
      reject({ error: error, statusCode: 999 });
    });

    request.end();
  });
}
