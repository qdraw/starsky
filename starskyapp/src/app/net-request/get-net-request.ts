import { net } from "electron";

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
      session,
      headers: {
        Accept: "*/*"
      }
    } as any);

    let body = "";
    request.on("response", (response) => {
      console.log(`HEADERS: ${JSON.stringify(response.headers)}`);

      response.on("data", (chunk) => {
        body += chunk.toString();
      });
      response.on("end", () => {
        if (
          response.headers &&
          response.headers["content-type"] &&
          response.headers["content-type"] === "text/plain"
        ) {
          resolve({
            data: body,
            statusCode: response.statusCode
          });
          return;
        }

        try {
          resolve({
            data: JSON.parse(body),
            statusCode: response.statusCode
          });
        } catch (error) {
          console.log(error);
          reject({ error: error.toString(), statusCode: response.statusCode });
        }
      });
    });

    request.on("error", (error) => {
      console.log(error);
      reject({ error: error, statusCode: 999 });
    });

    request.end();
  });
}
