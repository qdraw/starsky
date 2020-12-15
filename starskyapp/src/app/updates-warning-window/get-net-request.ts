import { net } from "electron";

export interface IGetNetRequestResponse {
  data?: any;
  error?: any;
  statusCode: number;
}

export function GetNetRequest(url: string): Promise<IGetNetRequestResponse> {
  return new Promise(function (resolve, reject) {
    const request = net.request({
      url,
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
        try {
          resolve({
            data: JSON.parse(body),
            statusCode: response.statusCode
          });
        } catch (error) {
          console.log(error);
          reject({ error, statusCode: response.statusCode });
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
