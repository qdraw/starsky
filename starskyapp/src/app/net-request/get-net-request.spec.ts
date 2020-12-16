import { net } from "electron";
import { GetNetRequest } from "./get-net-request";
jest.mock("electron", () => {
  return {
    app: {
      getVersion: () => "99.99.99",
      getPath: () => "tmp",
      getLocale: () => "en",
      on: () => "en"
    },
    net: {
      request: () => {}
    }
  };
});

describe("get net request", () => {
  describe("get net request", () => {
    it("should give status code and json content", async () => {
      jest.spyOn(net, "request").mockImplementationOnce((t) => {
        console.log("valid url ");
        console.log(t);
        return {
          on: (name: any, fun: Function) => {
            if (name === "response") {
              fun({
                on: (param: any, func: Function) => {
                  if (param === "data") {
                    func("{}");
                    return;
                  }
                  func();
                  console.log(param);
                },
                headers: {},
                statusCode: 200
              });
            }
          },
          end: jest.fn()
        } as any;
      });

      const result = await GetNetRequest("");
      expect(result.data).toStrictEqual({});
      expect(result.statusCode).toBe(200);
    });

    it("should give status code and plain text content", async () => {
      jest.spyOn(net, "request").mockImplementationOnce((t) => {
        console.log("valid url ");
        console.log(t);
        return {
          on: (name: any, fun: Function) => {
            if (name === "response") {
              fun({
                on: (param: any, func: Function) => {
                  if (param === "data") {
                    func("Healthy");
                    return;
                  }
                  func();
                },
                headers: {
                  "content-type": "text/plain"
                },
                statusCode: 200
              });
            }
          },
          end: jest.fn()
        } as any;
      });

      const result = await GetNetRequest("");
      expect(result.data).toStrictEqual("Healthy");
      expect(result.statusCode).toBe(200);
    });

    it("should give status code and plain text with charset utf 8 content", async () => {
      jest.spyOn(net, "request").mockImplementationOnce((t) => {
        console.log("valid url ");
        console.log(t);
        return {
          on: (name: any, fun: Function) => {
            if (name === "response") {
              fun({
                on: (param: any, func: Function) => {
                  if (param === "data") {
                    func("Healthy");
                    return;
                  }
                  func();
                },
                headers: {
                  "content-type": "text/plain; charset=utf-8"
                },
                statusCode: 200
              });
            }
          },
          end: jest.fn()
        } as any;
      });

      const result = await GetNetRequest("");
      expect(result.data).toStrictEqual("Healthy");
      expect(result.statusCode).toBe(200);
    });

    it("should give status code and invalid json content", async () => {
      jest.spyOn(net, "request").mockImplementationOnce((t) => {
        console.log("valid url ");
        console.log(t);
        return {
          on: (name: any, fun: Function) => {
            if (name === "response") {
              fun({
                on: (param: any, func: Function) => {
                  if (param === "data") {
                    func("{-------!!!!---}");
                    return;
                  }
                  func();
                  console.log(param);
                },
                headers: {},
                statusCode: 200
              });
            }
          },
          end: jest.fn()
        } as any;
      });

      console.log("--> should give json parse error -->");

      let error = undefined;
      try {
        await GetNetRequest("");
      } catch (err) {
        error = err;
      }

      expect(error.error).toContain(
        "SyntaxError: Unexpected number in JSON at position 1"
      );
      expect(error.statusCode).toBe(200);
    });
  });
});
